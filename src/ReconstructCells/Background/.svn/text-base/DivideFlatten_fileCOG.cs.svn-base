

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
    class DivideFlatten_fileCOG : ReconstructNodeTemplate
    {
        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        int suggestedCellSize;
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
        OnDemandImageLibrary Library2;
        string OBDFolder = "OBD";
       

       

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
        public void setFileFolder(string OBDFolder)
        {
            this.OBDFolder = OBDFolder;

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
               

                Rectangle ROI ;
                int cellHalf;

                Image<Gray,float> image;

                image = Library2[imageNumber];
                //Library[imageNumber] = image;
                //return;
                //image = image.SubR(new Gray(60000));
                //////}
                //////else if (Program.TestMode == "6")
                //////{
                ////string index = string.Format("  {0}", imageNumber);
                ////index = index.Substring(index.Length - 3);
                ////image = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\testbad\data\safirFiltered\image" + index + ".tif");


                ROI = new Rectangle(0, 0, image.Width, image.Height);
                cellHalf = (int)((ROI.Width) / 2d);

                image.MinMax(out min, out max, out pm, out pM);

                int sizeIncrease = mPassData.DataScaling;
                var image2 = image.Copy();
                //ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                //ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                #region refine cutting
                PointF p = NoisyFinder(image2);
              //  PointF p = Locations[imageNumber].CellCenter;

                int nCellSize = Locations[imageNumber].CellSize;//(int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

             
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

            }

            //  Library[imageNumber] = image;
        }


        private void RemoveBackgrounds()
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
           // ParallelOptions po = new ParallelOptions();
           // po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = 500;// Library.Count;

           // Library2 = new OnDemandImageLibrary(Program.dataFolder + "\\OBD_15_2_7_t_t", true, "c:\temp", false);
            string folder = Program.DataFolder + "\\" + OBDFolder;
            Library2 = new OnDemandImageLibrary(folder , true, "c:\temp", false);
            Library2.LoadLibrary();

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

          //  ImageMinValues = new float[Library.Count];

            RemoveBackgrounds();


        }
    }
}
