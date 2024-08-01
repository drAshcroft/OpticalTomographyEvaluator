

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
    class DivideFlattenFileOut : ReconstructNodeTemplate
    {
        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        int suggestedCellSize;
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
      
        int TrimSize = 10;

        float[] ImageMinValues;

        float[] EdgeValues;
        float[] EdgeRange;

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

        private PointF NoisyFinder(Image<Gray, float> image)
        {

            float xx = 0, yy = 0;
            for (int i = 0; i < 5; i++)
            {
                Bitmap b = image.ScaledBitmap;
                MathLibrary.Signal_Processing.ImageFilters.AddJitter(ref b);

                Image<Gray, float> image2 = new Image<Gray, float>(b);

                var moments = image2.GetMoments(false);

                xx += (float)moments.GravityCenter.x;
                yy += (float)moments.GravityCenter.y;
            }

            return new PointF(xx / 5f, yy / 5f);
        }


        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;
                if (!mPassData.FluorImage)
                    Buffer = 30;
                else
                    Buffer = 5;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                ROI.Inflate(20, 20);
                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);


                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\brightImages\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);

                //string index = string.Format("  {0}", imageNumber);
                //index = index.Substring(index.Length - 3);
                //ImageProcessing.ImageFileLoader.Save_16bit_TIFF(Program.dataFolder + "\\brightImages16\\imageTIFs" + index + ".tif", image, 1, 0);


                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);
                //  ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\bigSino\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);



                ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                image = Library[imageNumber];
                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);

                image.MinMax(out min, out max, out pm, out pM);

                EdgeRange[imageNumber] = (float)(max[0] - min[0]);
                // image.MinMax(out min, out  max, out pm, out pM);

                int sizeIncrease = mPassData.DataScaling;
                var image2 = image.Copy();
                //image = image.SmoothMedian(3).PyrUp();
                //  image= image.PyrUp();
                //  ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                //   ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                //  return ;
                #region refine cutting
                PointF p = NoisyFinder(image2);
                //PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image2);

                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

                cellLocation.CellCenter.X = (p.X );
                cellLocation.CellCenter.Y = (p.Y );
                cellLocation.CellSize = nCellSize;

                // beforeRoughness[imageNumber] = cellLocation.CellCenter.X;
                // afterRoughness[imageNumber] = cellLocation.CellCenter.Y;
                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    image = image.Copy(ROI);
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
             
                Library[imageNumber] = image;
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();
                MessageBox.Show(ex.Message);
            }

        }


        private void RemoveBackgrounds()
        {
            if (mPassData.FluorImage)
                CellSize = (int)(suggestedCellSize * 1);
            else
                CellSize = (int)(suggestedCellSize * 1.5);

            //now that the cell positions are estimated, fill in all the other cells
         //   ParallelOptions po = new ParallelOptions();
          //  po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackground(x));
        }

     
        #endregion


        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            Locations = mPassData.Locations;


            string fold = Program.DataFolder + "\\brightImages";
            Directory.CreateDirectory(fold);
        
            EdgeValues = new float[Library.Count];
            EdgeRange = new float[Library.Count];


            ImageMinValues = new float[Library.Count];

            RemoveBackgrounds();

        }
    }
}
