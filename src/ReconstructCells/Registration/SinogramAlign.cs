#if DEBUG
  #define TestingDebug
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Threading.Tasks;
using MathLibrary;
using ImageProcessing._2D;
namespace ReconstructCells.Registration
{
    class SinogramAlign : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;
       
        CellLocation[] Locations;
        int CellSize = 170;
        int halfCell = 170 / 2;
        int Buffer = 0;

        double[,] WholeImage;
        double[] Filter;

#if  TestingDebug
        
       
       
#endif

       

        #region Properties

        #region Set


        #endregion

        #endregion

        #region Code


        private double[] XProfile(Image<Gray, float> image, Rectangle ROI)
        {
            double[] profile = new double[ROI.Width];
            double val = 0;
            int top = ROI.Y;
            int bottom = ROI.Bottom;
            int mid = (top + bottom) / 2;

            if (top < 0) top = 0;
            if (bottom >= image.Height) bottom = image.Height;
            for (int x = ROI.X; x < ROI.Right; x++)
            {
                val = 0;
                if (x >= 0 && x < image.Width)
                {
                    val += image.Data[mid, x, 0];
                    profile[x - ROI.X] = val / ROI.Height / 6000;
                }
            }

            return profile;
        }

        private void ZeroProfile(double[] profile)
        {
            double start = 0;
            double end = 0;
            for (int i = 0; i < 10; i++)
            {
                start += profile[i];
                end += profile[profile.Length - 1 - i];
            }
            start /= 10;
            end /= 10;

            double slope = (end - start) / profile.Length;
            double b = start;

            for (int i = 0; i < profile.Length; i++)
                profile[i] -= (slope * i + b);
        }

        private double[] YProfile(Image<Gray, float> image, Rectangle ROI)
        {
            double[] profile = new double[ROI.Height];
            double val = 0;
            int left = ROI.X;
            int right = ROI.Right;
            int mid = (left + right) / 2;

            if (left < 0) left = 0;
            if (right >= image.Height) right = image.Height;


            for (int y = ROI.Y; y < ROI.Bottom; y++)
            {
                val = 0;
                val += image.Data[y, mid, 0];
                profile[y - ROI.Y] = val / ROI.Width;
            }

            return profile;
        }

        private void SetupFilter()
        {
            Filter = new double[CellSize];
            double sigma1 = 5;
            double X = 0;
            for (int i = 0; i < Filter.Length / 2; i++)
            {
                if (i < 2)
                    Filter[i] = 0;
                else
                {
                    X = (i - 4) / sigma1;
                    Filter[i] = Math.Exp(-1 * X * X);
                }
                Filter[Filter.Length - 1 - i] = Filter[i];
            }

            Filter[0] = 0;
            Filter[Filter.Length - 1] = 0;

#if  TestingDebug
            var b = Filter.DrawGraph();
            int w = b.Width;
#endif

        }

        private void BatchFindAlign(int imageNumber)
        {
            var image = Library[imageNumber];
            var profile = YProfile(image, new Rectangle(0, 0, image.Width, image.Height));

            /*double[] filtered = MathLibrary.FFT.MathFFTHelps.ShowFilter(profile, Filter);
            for (int i = 0; i < 10; i++)
            {
                filtered[i] = 0;
                filtered[filtered.Length - 1 - i] = 0;
            }*/


            for (int i = 0; i < profile.Length; i++)
                WholeImage[imageNumber, i] = profile[i];
        }

        private void RegisterCell()
        {
            double maxF4 = double.MinValue;
            int maxIndex = 0;
            for (int i = 0; i < Locations.Length; i++)
            {
                if (Locations[i].FV > maxF4)
                {
                    maxF4 = Locations[i].FV;
                    maxIndex = i;
                }
            }

            Buffer = 0;
            CellSize = Library[0].Width + Buffer;
            halfCell = CellSize / 2;


            SetupFilter();
            WholeImage = new double[Library.Count, Library[0].Height];
            maxIndex = 1;


           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindAlign(x));



        }

        #endregion

        protected override void RunNodeImpl()
        {

            Library = mPassData.Library;
            Locations = mPassData.Locations;

           
            CellSize = (int)mPassData.GetInformation("CellSize");

            RegisterCell();

#if  TestingDebug
            //Library.CreateAVIVideoEMGU(@"c:\temp\allImages.avi", 10);
            WholeImage.SaveBinary(@"c:\temp\sinogram.bin");

            Bitmap b = WholeImage.MakeBitmap();
           // var bAfter = WholeImageAfter.MakeBitmap();
            int w = b.Width;// +bAfter.Width;
#endif
        }
    }
}

