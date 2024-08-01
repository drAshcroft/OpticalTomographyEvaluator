

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
    class DivideFlatten_COG : ReconstructNodeTemplate
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
        public void setBackground(Image<Gray, float> backGround)
        {
            BackGround = backGround;
        }

        #endregion

        #endregion

        #region Code



        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;
                if (!mPassData.FluorImage)
                    Buffer = 20;
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
                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);

                image.MinMax(out min, out max, out pm, out pM);

                int sizeIncrease = mPassData.DataScaling;
               
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

              

                Library[imageNumber] = image;
              

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

       
        }
    }
}
