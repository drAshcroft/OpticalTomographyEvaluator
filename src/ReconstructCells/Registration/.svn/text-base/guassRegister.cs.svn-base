

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
    class guassRegister : ReconstructNodeTemplate
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

        private PointF GuassFinder(ref Image<Gray, float> image)
        {

            float xx = 0, yy = 0;
            double  tx, ty;
            float cc = 0;
            for (int i = 0; i < 1; i++)
            {
                //Bitmap b = image.ScaledBitmap;
                //MathLibrary.Signal_Processing.ImageFilters.AddJitter(ref b);

                //Image<Gray, float> image2 = new Image<Gray, float>(b);

                //var moments = image.GetMoments(false);
                var image2 = image.SmoothGaussian(31);
                float thresh = image2.Data.MaxArray() ;
                double  d;
                tx = 0;
                ty = 0;
                double cW = 0;
                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        d = image2.Data[y, x, 0];
                        d = Math.Floor(d / thresh * 50);
                        if (d < 2)
                        {
                            d = 0;
                            image2.Data[y, x, 0] = 0;
                        }
                        tx += d * x;
                        ty += d * y;
                        cW += d;
                       
                    }

                xx += (float)(tx / cW);
                yy += (float)(ty / cW);
                cc++;
            }

            return new PointF(xx / cc, yy / cc);
        }

        private void BatchNoisyRefine(int imageNumber)
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


                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false );

                #region refine cutting
                PointF p = GuassFinder(ref image);

                mPassData.Locations[imageNumber].CellCenter.X = minCellX + p.X;
                mPassData.Locations[imageNumber].CellCenter.Y = minCellY + p.Y;


                //cellLocation = mPassData.Locations[imageNumber];
                //minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                //maxCellX = (int)(minCellX + CellSize + Buffer);
                //minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                //maxCellY = (int)(minCellY + CellSize + Buffer);

                //ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                //cellHalf = (int)((ROI.Width) / 2d);

                //image = Library[imageNumber];
                //image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);



                //if (imageNumber % 3 == 0)
                //    image.Bitmap.Save(@"c:\temp\aligned\image" + string.Format("{0:000}.tif", imageNumber / 3));

                #endregion

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
            CellSize = (int)(suggestedCellSize * 1.5);// (int)(ImprovedCellSize(suggestedCellSize) * 1.1);

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchNoisyRefine(x));

        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            Locations = mPassData.Locations;
            BackGround = mPassData.theBackground;
            ImageMinValues = new float[Library.Count];

            RefineRegistration();
        }
    }
}
