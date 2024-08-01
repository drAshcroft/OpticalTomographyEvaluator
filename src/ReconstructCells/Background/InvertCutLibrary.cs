

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

namespace ReconstructCells.Background
{
    class InvertAndCut : ReconstructNodeTemplate
    {
        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        int suggestedCellSize;
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
        

        float[] ImageMinValues;


        int Buffer = 30;



        #region Properties

        #region Set

        public void setSuggestedCellSize(int cellSize)
        {
            suggestedCellSize = cellSize;
        }
       
        #endregion

        #endregion

        #region Code

       
        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
              //  double[] min, max;
              //  Point[] pm, pM;
                if (!mPassData.FluorImage)
                    Buffer = 30;
                else
                    Buffer = 25;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(minCellX + CellSize + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(minCellY + CellSize + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                //   image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);
                image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);
                ImageProcessing._2D.ImageManipulation.InvertImage(image);


                mPassData.Weights[imageNumber] = ImageProcessing._2D.FocusScores.F4(image);


                if (mPassData.FluorImage)
                {
                    //image = image.Resize(2, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR );
                    mPassData.DataScaling = 1;
                }
                else
                    mPassData.DataScaling = 1;

                Library[imageNumber] = image;
                return;

                //  Library[imageNumber] = image;
            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }
        }


        /*
             // image = image.SmoothMedian(3);
               // image.MinMax(out min, out max, out pm, out pM);
               
                int sizeIncrease = mPassData.DataScaling;
               // image = image.Resize(2, Emgu.CV.CvEnum.INTER.CV_INTER_AREA );
            //   image = image.PyrUp();
               // var image2 = image.Copy();
              //  ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                #region refine cutting
               // PointF p = NoisyFinder(image2);
                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

              //  ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

               // cellLocation.CellCenter.X += (int)(p.X - cellHalf);
               // cellLocation.CellCenter.Y += (int)(p.Y - cellHalf);

                // beforeRoughness[imageNumber] = cellLocation.CellCenter.X;
                // afterRoughness[imageNumber] = cellLocation.CellCenter.Y;
                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    
                    image = image  .Copy(ROI);
                }
                else
                {
                    #region boundry Problems ___ lazy problem
                    Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                    var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
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

                    // nImage.ROI = ROIdest;
                    // image.ROI = ROIsrc;

                    //nImage = image.Copy();
                    int xx = ROIdest.X;
                    for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                    {
                        int yy = ROIdest.Y;
                        for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                            yy++;
                        }
                        xx++;
                    }

                    //image.CopyTo(nImage);
                    image = nImage;
                    #endregion
                }

                Library[imageNumber] = image;//.PyrDown();
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }

         * */

        private void InvertImages()
        {
            CellSize = (int)(suggestedCellSize * 1.5);// (int)(ImprovedCellSize(suggestedCellSize) * 1.1);


            //  BatchRemoveBackground(357);

            //List<double> averages = new List<double>();
            //averages.AddRange(EstimateBackgroundValue(1));
            //averages.AddRange(EstimateBackgroundValue(125));
            //averages.AddRange(EstimateBackgroundValue(250));
            //averages.AddRange(EstimateBackgroundValue(375));
            //averages.AddRange(EstimateBackgroundValue(490));

            //averages.Sort();
            //BackgroundAverage = (float)averages[averages.Count / 2];
            //now that the cell positions are estimated, fill in all the other cells
            mPassData.Weights = new float[mPassData.Library.Count];

          //  ParallelOptions po = new ParallelOptions();
          //  po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;
            //  BatchRemoveBackground(336);
#if TESTING
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackgroundTest(x));
#else
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackground(x));
#endif

            float sum = 0;
            for (int i = 0; i < mPassData.Weights.Length; i++)
            {
                sum += mPassData.Weights[i];
            }
            float ave = sum / mPassData.Weights.Length;


            float[] weights = new float[mPassData.Weights.Length];
            for (int i = 0; i < mPassData.Weights.Length; i++)
            {
                weights[i] = 1 - Math.Abs(ave - mPassData.Weights[i]) / (Math.Abs(mPassData.Weights[i] - ave) + Math.Abs(mPassData.Weights[(i + mPassData.Weights.Length/2) % mPassData.Weights.Length] - ave));
            }

            mPassData.Weights = weights;

            //for (int i = 0; i < Library.Count / 2; i++)
            //{
            //    var image2 = Library[i+Library.Count/2].Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

            //    Library[i] = Library[i].Add(image2);
            //    Library[i + Library.Count / 2] = Library[i].Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
            //}

        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            Locations = mPassData.Locations;
            BackGround = mPassData.Library[10].CopyBlank().Add(new Gray(1));
            ImageMinValues = new float[Library.Count];

            InvertImages();


           
        }
    }
}
