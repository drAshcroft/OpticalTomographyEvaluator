using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ImageProcessing._2D;
using Emgu.CV.Structure;
using Emgu.CV;
using MathLibrary.Signal_Processing;


namespace ReconstructCells.Background
{
    public class AveragingBackground : ReconstructNodeTemplate
    {
        private OnDemandImageLibrary Library = null;
        private Image<Gray, float> sum;
        private float[,] Counts;
        private bool FluorCells=false;
        public AveragingBackground(OnDemandImageLibrary library, bool fluorCells)
        {
            Library = library;
            FluorCells = fluorCells;
        }

        private void AddImages(Image<Gray, float> image, CellLocation cellLocation)
        {
            const int Buffer = 45;
            int minCellX = (int)(cellLocation.CellCenter.X - cellLocation.CellSize / 2 - Buffer);
            int maxCellX = (int)(cellLocation.CellCenter.X + cellLocation.CellSize / 2 + Buffer);
            int minCellY = (int)(cellLocation.CellCenter.Y - cellLocation.CellSize / 2 - Buffer);
            int maxCellY = (int)(cellLocation.CellCenter.Y + cellLocation.CellSize / 2 + Buffer);

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (!(x > minCellX && x < maxCellX && y > minCellY && y < maxCellY))
                    {
                        sum.Data[y, x, 0] += image.Data[y, x, 0];
                        Counts[y, x]++;
                    }
                }

        }

        public Image<Gray, float> GetFineBackground(int nSlices, CellLocation[] Locations)
        {
            sum = new Image<Gray, float>(Library[0].Width, Library[0].Height);
            Counts = new float[sum.Height, sum.Width];

            int frameSkip = (int)(Library.Count / (double)nSlices);

            for (int i = 0; i < Library.Count; i += frameSkip)
            {
                AddImages(Library[i], Locations[i]);
            }

            float c = 0;
           
            double averageSum = 0;
            double averageCount = 0;
            for (int x = 0; x < sum.Width; x++)
                for (int y = 0; y < sum.Height; y++)
                {
                    c = Counts[y, x];
                    if (c > 0)
                    {
                        sum.Data[y, x, 0] /= c;
                        averageSum += sum.Data[y, x, 0];
                        averageCount++;
                    }
                   
                }

            sum = sum.Mul(averageCount / averageSum);

            return sum;
        }

       
        public int CellSize;


        protected override void RunNodeImpl()
        {
            throw new NotImplementedException();
        }

    }
}
