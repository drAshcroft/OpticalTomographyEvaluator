

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

namespace ReconstructCells.Registration
{
    class CCRegister : ReconstructNodeTemplate
    {
        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        int suggestedCellSize;
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
        int TrimSize = 10;

        float[] ImageMinValues;


        int Buffer = 30;



        #region Properties

        #region Set

        public void setSuggestedCellSize(int cellSize)
        {
            suggestedCellSize = cellSize;
        }
        public void setBackground(Image<Gray, float> backGround)
        {
            BackGround = backGround;
        }

        #endregion

        #endregion

        #region Code

        private Image<Gray, float> CutImage(Image<Gray, float> image, Rectangle ROI)
        {
            if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
            {
                image = image.Copy(ROI);
            }
            else
            {
                #region boundry Problems ___ lazy problem
                Image<Gray, float> nImage = image.Copy();

                var ROIdest = new Rectangle(0, 0, ROI.Width, ROI.Width);
                var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                if (ROI.X < 0)
                {
                    ROIdest.X = -1 * ROI.X;
                    int right = ROI.Right;
                    ROIsrc.X = 0; ROIsrc.Width = right;
                    ROIdest.Width = right;
                }

                if (ROI.Y < 0)
                {
                    ROIdest.Y = -1 * ROI.Y;
                    int bottom = ROI.Bottom;
                    ROIsrc.Y = 0; ROIsrc.Height = bottom;
                    ROIdest.Height = bottom;
                }

                if (ROI.Right > image.Width)
                {
                    ROIsrc.Width = image.Width - ROI.X;
                    ROIdest.Width = ROIsrc.Width;
                }

                if (ROI.Bottom > image.Height)
                {
                    ROIsrc.Height = image.Height - ROI.Y;
                    ROIdest.Height = ROIsrc.Height;
                }


                int xx = ROIdest.X;
                for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                {
                    int yy = ROIdest.Y;
                    for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                    {
                        if (yy > 0 && xx > 0 && yy < image.Height && xx < image.Width)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                        }
                        yy++;
                    }
                    xx++;
                }


                image = nImage;
                #endregion
            }
            return image;
        }
        private void CrossCorrelateLib(ref OnDemandImageLibrary library)
        {
            TrimSize = 6;
            int nCellSize = library[10].Width;// (int)(Math.Floor(((double)library[10].Width - TrimSize) / 2));
            Rectangle ROI;
            for (int j = 0; j < library.Count; j += 11)
            {
                var compImage = library[j];
                for (int i = j + 1; i < 12; i++)
                {
                    var image = library[i];
                    PointF p = MathLibrary.FFT.MathFFTHelps.FindShift(compImage.Data, image.Data);
                    ROI = new Rectangle((int)(Math.Floor(p.X)), (int)(Math.Floor(p.Y)), 2 * nCellSize, 2 * nCellSize);
                    library[i] = CutImage(image, ROI);
                }
                ROI = new Rectangle((int)Math.Floor(TrimSize / 2d), (int)Math.Floor(TrimSize / 2d), 2 * nCellSize, 2 * nCellSize);
                library[j] = CutImage(library[j], ROI);
            }
        }

        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
                
                if (!mPassData.FluorImage)
                    Buffer = 20;
                else
                    Buffer = 15;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(minCellX + CellSize + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(minCellY + CellSize + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);


                mPassData.DataScaling = 1;
                int sizeIncrease = mPassData.DataScaling;

                var image2 = image.Copy();
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                #region refine cutting
                PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image2);
                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

                mPassData.Locations[imageNumber].CellCenter.X += (int)(p.X - cellHalf);
                mPassData.Locations[imageNumber].CellCenter.Y += (int)(p.Y - cellHalf);

              
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }

            //  Library[imageNumber] = image;
        }


        private void RemoveBackgrounds()
        {
            CellSize = (int)(suggestedCellSize * 1.5);// (int)(ImprovedCellSize(suggestedCellSize) * 1.1);

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;
            //  BatchRemoveBackground(336);
#if TESTING
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackgroundTest(x));
#else
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackground(x));
#endif
        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            Locations = mPassData.Locations;

            ImageMinValues = new float[Library.Count];

            RemoveBackgrounds();

            CrossCorrelateLib(ref Library);

            //Registration.AlignByRecon ar = new Registration.AlignByRecon();
            //ar.SetInput(mPassData);
            //ar.setNumberOfProjections(125);
            //ar.setScale(1);
            //ar.RunNode();
        }
    }
}
