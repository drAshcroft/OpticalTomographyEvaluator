﻿

#if DEBUG
// #define TESTING
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageProcessing._3D;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using MathLibrary;
using ImageProcessing;
using System.Diagnostics;
using MathLibrary.FFT;
using ILNumerics;
namespace ReconstructCells.Registration
{
    class mirrorAlignRegister : ReconstructNodeTemplate
    {

        public enum MergeMethodEnum { Average, SuperRes }

        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize;
        OnDemandImageLibrary Library;

        float[] ImageMinValues;
        bool MergeMirror = false;
        MergeMethodEnum _MergMethod = MergeMethodEnum.Average;
        #region Properties

        #region Set
        public void setMergeMethod(MergeMethodEnum mergeMethod)
        {
            _MergMethod = mergeMethod;
        }

        public void setSuggestedCellSize(int cellSize)
        {
            CellSize = cellSize;
        }

        public void setMergeMirror(bool doMerge)
        {
            MergeMirror = doMerge;
        }

        #endregion

        #endregion

        #region Code

        int scaleX = 1;
        int scaleY = 1;

        private void BatchMirrorRefine(int imageNumber)
        {
            try
            {
                int imageNumber2 = imageNumber + Library.Count / 2;

                double[] min, max;

                int Buffer = 80;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                //  int maxCellX = (int)(minCellX + CellSize + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                //  int maxCellY = (int)(minCellY + CellSize + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);
                var image4 = image.Mul(Math.Abs(1d / image.GetAverage().Intensity));
                ImageProcessing._2D.ImageManipulation.InvertImage(image4);

                cellLocation = Locations[imageNumber2];
                minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                // maxCellX = (int)(minCellX + CellSize + Buffer);
                minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                // maxCellY = (int)(minCellY + CellSize + Buffer);

                ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);

                var image2 = Library[imageNumber2];
                image2 = ImageProcessing._2D.ImageManipulation.CopyROI(image2, ROI).Flip(Emgu.CV.CvEnum.FLIP.VERTICAL); ;
                image2 = image2.Mul(Math.Abs(1d / image2.GetAverage().Intensity));
                ImageProcessing._2D.ImageManipulation.InvertImage(image2);

                const int padding = 15;
                var image5 = ImageProcessing._2D.ImageManipulation.CopyROI(image4, new Rectangle(-padding, -padding, image4.Width + 2 * padding, image4.Height + 2 * padding));
                Image<Gray, float> image3 = image5.MatchTemplate(image2, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED);

                Point[] point1, point2;
                image3.MinMax(out min, out max, out point1, out point2);
                Point point3 = new Point();
                point3.X = -1 * (point2[0].X - image3.Width / 2);
                point3.Y = -1 * (point2[0].Y - image3.Height / 2);

                int sX = point3.X;
                int sY = point3.Y;
                // System.Diagnostics.Debug.Print(point3.ToString());
                // image3 = image2.CopyBlank();
                //MathFFTHelps.FFT_cnv2(image4.Data, image2.Data, image3.Data);

                ////avoid weird spikes
                //image3 = image3.SmoothGaussian(5);
                //image3.MinMax(out min, out max, out pm, out pM);

                //int sX = -1 * (pM[0].X - image3.Width / 2);
                //int sY = -1 * (pM[0].Y - image3.Height / 2);

                //int sX2 = (int)Math.Floor(sX / 2d);
                //int sY2 = (int)Math.Floor(sY / 2d);

                //sX = sX - sX2;
                //sY = sY - sY2;

                Locations[imageNumber2].CellCenter.X -= scaleX * sX;
                Locations[imageNumber2].CellCenter.Y -= scaleY * sY;

                //Locations[imageNumber].CellCenter.X += scaleX * sX2;
                //Locations[imageNumber].CellCenter.Y += scaleY * sY2;

                ////now cut out the positioned cells
                //cellLocation = Locations[imageNumber];
                //minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                //maxCellX = (int)(minCellX + CellSize + Buffer);
                //minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                //maxCellY = (int)(minCellY + CellSize + Buffer);

                //ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);
                //image = Library[imageNumber];
                //image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);
                Library[imageNumber] = image;


                cellLocation = Locations[imageNumber2];
                minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                // maxCellX = (int)(minCellX + CellSize + Buffer);
                minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                // maxCellY = (int)(minCellY + CellSize + Buffer);

                ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);
                image2 = Library[imageNumber2];
                image2 = ImageProcessing._2D.ImageManipulation.CopyROI(image2, ROI);
                Library[imageNumber2] = image2;

                // image3 = image.Sub(image2.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL));
                MergeMirror = false;
                if (MergeMirror)
                {
                    float f1 = ImageProcessing._2D.FocusScores.F4(image) + .1f;
                    float f2 = ImageProcessing._2D.FocusScores.F4(image2) + .1f;
                    mPassData.Weights[imageNumber] = f1;
                    mPassData.Weights[imageNumber2] = f2;
                    float tf = f1 + f2;
                    f1 = f1 / tf;
                    f2 = f2 / tf;

                    image = Library[imageNumber].AddWeighted(image2.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL), f1, f2, 0);

                    ImageProcessing._2D.ImageManipulation.InvertImage(image);

                    mPassData.Weights[imageNumber] = .5f;
                    mPassData.Weights[imageNumber + Library.Count / 2] = .5f;
                    Library[imageNumber] = image;
                    Library[imageNumber + Library.Count / 2] = image.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                }
                else
                {
                    mPassData.Weights[imageNumber] = 1;
                    mPassData.Weights[imageNumber2] = 1;
                    ImageProcessing._2D.ImageManipulation.InvertImage(Library[imageNumber]);
                    ImageProcessing._2D.ImageManipulation.InvertImage(Library[imageNumber2]);
                }
            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                //Library[imageNumber] = Library[0].CopyBlank();

            }

            //  Library[imageNumber] = image;
        }


        private void RefineRegistration()
        {
            // CellSize = (int)(suggestedCellSize * 1.5);// (int)(ImprovedCellSize(suggestedCellSize) * 1.1);

         //   ParallelOptions po = new ParallelOptions();
          //  //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;
            BatchMirrorRefine(0);

            if (_MergMethod == MergeMethodEnum.Average)
                Parallel.For(0, numberOfImages / 2, Program.threadingParallelOptions, x => BatchMirrorRefine(x));
            else
            {
              //  //po.MaxDegreeOfParallelism = Environment.ProcessorCount / 2;

                for (int imageNumber = 0; imageNumber < Library.Count / 2; imageNumber++)
                {
                    BatchSuperResMirrorRefine(imageNumber);
                    Console.WriteLine(imageNumber.ToString());
                }

                //Thread[] ThreadPool = new Thread[//po.MaxDegreeOfParallelism ];
                //for (int i = 0; i < ThreadPool.Length; i++)
                //{
                //    ThreadPool[i] = new Thread(delegate(object imageBlock)
                //        {
                //            int halfLib = Library.Count / 2;
                //            int step = (int)Math.Ceiling(halfLib / (double)//po.MaxDegreeOfParallelism);
                //            int startBlock = step * (int)imageBlock;
                //for (int imageNumber = startBlock; imageNumber < startBlock + step && imageNumber < halfLib; imageNumber++)
                //{
                //    BatchSuperResMirrorRefine(imageNumber);
                //    Console.WriteLine(imageNumber.ToString());
                //            }
                //        }
                //    );
                //    ThreadPool[i].Start(i);
                //}

                //for (int i = 0; i < ThreadPool.Length; i++)
                //    ThreadPool[i].Join();
                // Parallel.For(0, numberOfImages / 2, Program.threadingParallelOptions, x => BatchSuperResMirrorRefine(x));
            }
        }

        #endregion

        protected override void RunNodeImpl()
        {
            CellSize = (int)mPassData.GetInformation("CellSize");
            Library = mPassData.Library;
            Locations = mPassData.Locations;
            BackGround = mPassData.theBackground;
            ImageMinValues = new float[Library.Count];


            if (mPassData.Weights == null)
                mPassData.Weights = new float[Library.Count];

            RefineRegistration();
        }


        #region SuperResolution

        private void BatchSuperResMirrorRefine(int imageNumber)
        {
            try
            {
                int imageNumber2 = imageNumber + Library.Count / 2;

                int Buffer = 80;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);
                ImageProcessing._2D.ImageManipulation.InvertImage(image);

                cellLocation = Locations[imageNumber2];
                minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);

                ROI = new Rectangle(minCellX, minCellY, CellSize + Buffer * 2, CellSize + Buffer * 2);

                var image2 = Library[imageNumber2];
                image2 = ImageProcessing._2D.ImageManipulation.CopyROI(image2, ROI);
                ImageProcessing._2D.ImageManipulation.InvertImage(image2);
                image2 = image2.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

                float f1 = ImageProcessing._2D.FocusScores.F4(image) + .1f;
                float f2 = ImageProcessing._2D.FocusScores.F4(image2) + .1f;
                mPassData.Weights[imageNumber] = f1;
                mPassData.Weights[imageNumber2] = f2;

                ImageProcessing._2D.Deconvolution dc = new ImageProcessing._2D.Deconvolution();
                image = image.Add(new Gray( 1000));
                image2 = image2.Add(new Gray(1000));
                Image<Gray, float> combinedImage;
                if (f1 > f2)
                    combinedImage = dc.obDeconvolve(new Image<Gray, float>[] { image2, image }, 150, 1);
                else
                    combinedImage = dc.obDeconvolve(new Image<Gray, float>[] {  image, image2 }, 150, 1);
                //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\cut\image" + string.Format("{0:000}", imageNumber) + ".tif", image);
                //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\cut\image" + string.Format("{0:000}", imageNumber2) + ".tif", image2);

                //return;

                mPassData.Weights[imageNumber] = .5f;
                mPassData.Weights[imageNumber + Library.Count / 2] = .5f;
                Library[imageNumber] = combinedImage;
                Library[imageNumber + Library.Count / 2] = combinedImage.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                //Library[imageNumber] = Library[0].CopyBlank();

            }

            //  Library[imageNumber] = image;
        }


        #endregion
    }
}
