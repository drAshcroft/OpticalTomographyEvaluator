#if DEBUG
//  #define TestingDebug
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
    public class ZRegistration : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;
        private double[] ReferenceProfile = null;
        CellLocation[] Locations;
        int CellSize = 170;
        int halfCell = 170 / 2;
        int Buffer = 0;

        double[] Filter;

#if  TestingDebug
        double[,] WholeImage;
        double[,] WholeImageAfter;
        double[] testFilter;
#endif

        double sumWhole = 0;

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

            if (top < 0) top = 0;
            if (bottom >= image.Height) bottom = image.Height;
            for (int x = ROI.X; x < ROI.Right; x++)
            {
                val = 0;
                if (x >= 0 && x < image.Width)
                {
                    for (int y = top; y < bottom; y++)
                        val += image.Data[y, x, 0];

                    profile[x - ROI.X] = val / ROI.Height / 6000;
                }
            }

            return profile;
        }

        private void  ZeroProfile(double[] profile)
        {
            double start = 0;
            double end=0;
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

            if (left < 0) left = 0;
            if (right >= image.Height) right = image.Height;


            for (int y = ROI.Y; y < ROI.Bottom; y++)
            {
                val = 0;
                for (int x = left; x < right; x++)
                    val += image.Data[y, x, 0];
                profile[y - ROI.Y] = val / ROI.Width / 6000;
            }

            return profile;
        }

        private void SetupFilter()
        {
            Filter = new double[CellSize ];
            double sigma1 = 1;
            double sigma2 = 10;
            for (int i = 0; i < Filter.Length / 2; i++)
            {
                Filter[i] = 2 * (1 / (1 + Math.Exp(-1 * (i / sigma1) * (i / sigma1))) - .5);
                int j = Filter.Length / 2 - i;
                Filter[i] *= 2 * (1 / (1 + Math.Exp(-1 * (j / sigma2) * (j / sigma2))) - .5);
                Filter[Filter.Length - 1 - i] = Filter[i];
            }

            Filter[0] = 0;
            Filter[Filter.Length - 1] = 0;

#if  TestingDebug
            var b = Filter.DrawGraph();


            int reducedCellSize = CellSize;// (int)(CellSize * .95);
            testFilter = new double[reducedCellSize];

            for (int i = 0; i < Filter.Length / 2; i++)
            {
                testFilter[i] = 2 * (1 / (1 + Math.Exp(-1 * (i / sigma1) * (i / sigma1))) - .5);
                int j = testFilter.Length / 2 - i;
                testFilter[i] *= 2 * (1 / (1 + Math.Exp(-1 * (j / sigma2) * (j / sigma2))) - .5);
                testFilter[testFilter.Length - 1 - i] = testFilter[i];
            }

            int w = b.Width;
#endif

        }

        private void BatchFindZAlreadyBlack(int imageNumber)
        {

            var image = Library[imageNumber];
            Rectangle ROI = new Rectangle(0, 0, image.Width, image.Height);

            double[] xProfile = XProfile(image, ROI);


            double[] filtered = MathLibrary.FFT.MathFFTHelps.ShowFilter(xProfile, Filter);
            for (int i = 0; i < 10; i++)
            {
                filtered[i] = 0;
                filtered[filtered.Length - 1 - i] = 0;
            }

#if  TestingDebug

            var b1 = Filter.DrawGraph();

            var b = filtered.DrawGraph();
            int w = b.Width + b1.Width;
            for (int i = 0; i < xProfile.Length; i++)
            {
                WholeImage[imageNumber, i] = filtered[i];
            }
#endif

            int shift = (int)Math.Round(ImageProcessing._2D.MotionRegistration.PhaseCorrelationXOnly(ReferenceProfile, filtered));

           // if (shift > 8)
           //     shift = 0;

            Locations[imageNumber].CellCenter.X += shift;

            var cellLocation = Locations[imageNumber];
            int center = Library[imageNumber].Width / 2 + shift;
            int reducedCellSize = (int)(Library[imageNumber].Width * .95);


            int minCellX = (int)(center - reducedCellSize / 2);
            int minCellY = (int)(Library[imageNumber].Height / 2 - reducedCellSize / 2);
            ROI = new Rectangle(minCellX, minCellY, reducedCellSize, reducedCellSize);


            var cutDown = ImageManipulation.CopyROI(Library[imageNumber], ROI);
            Library[imageNumber] = cutDown;

#if  TestingDebug
            ROI = new Rectangle(0, 0, cutDown.Width, cutDown.Height);
            xProfile = XProfile(Library[imageNumber], ROI);
            filtered = MathLibrary.FFT.MathFFTHelps.ShowFilter(xProfile, testFilter);
            for (int i = 0; i < 10; i++)
            {
                filtered[i] = 0;
                filtered[filtered.Length - 1 - i] = 0;
            }
            for (int i = 0; i < xProfile.Length; i++)
            {
                WholeImageAfter[imageNumber, i] = filtered[i];
            }
#endif
        }

        private void ZRegisterCellAllreadyBlack()
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
            CellSize = Library[0].Width + Buffer ;
            halfCell = CellSize / 2;
            

            SetupFilter();

            maxIndex = 1;
            Rectangle ROI = new Rectangle(0, 0, Library[maxIndex].Width, Library[maxIndex].Height);
            ReferenceProfile = XProfile(Library[maxIndex], ROI);

            sumWhole = ReferenceProfile.Sum();

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindZAlreadyBlack(x));
        }



        private void BatchFindZ(int imageNumber)
        {

            var image = Library[imageNumber];

            Rectangle ROI = Rectangle.Empty;// new Rectangle(Locations[imageNumber].CellCenter.X - halfCell, Locations[imageNumber].CellCenter.Y - halfCell, CellSize, CellSize);
            //Rectangle ROI = new Rectangle(0, 0, image.Width, image.Height);

            double[] xProfile = XProfile(image, ROI);

            ZeroProfile(xProfile);

            double[] filtered = MathLibrary.FFT.MathFFTHelps.ShowFilter(xProfile, Filter);
            for (int i = 0; i < 10; i++)
            {
                filtered[i] = 0;
                filtered[filtered.Length - 1 - i] = 0;
            }

#if  TestingDebug

            var b1 = Filter.DrawGraph();

            var b = filtered.DrawGraph();
            int w = b.Width + b1.Width;
            for (int i = 0; i < xProfile.Length; i++)
            {
                WholeImage[imageNumber, i] = filtered[i];
            }
#endif

            int shift = (int)Math.Round(ImageProcessing._2D.MotionRegistration.PhaseCorrelationXOnly(ReferenceProfile, filtered));

            // if (shift > 8)
            //     shift = 0;

            Locations[imageNumber].CellCenter.X += shift;

            
          

#if  TestingDebug

            ROI = new Rectangle(Locations[imageNumber].CellCenter.X - halfCell, Locations[imageNumber].CellCenter.Y - halfCell, CellSize, CellSize);
            var cutDown = ImageManipulation.CopyROI(Library[imageNumber], ROI);

            xProfile = XProfile(Library[imageNumber], ROI);
            filtered = MathLibrary.FFT.MathFFTHelps.ShowFilter(xProfile, testFilter);
            for (int i = 0; i < 10; i++)
            {
                filtered[i] = 0;
                filtered[filtered.Length - 1 - i] = 0;
            }
            for (int i = 0; i < xProfile.Length; i++)
            {
                WholeImageAfter[imageNumber, i] = filtered[i];
            }
#endif
        }

        private void ZRegisterCell()
        {
            Buffer = 10;
            CellSize += Buffer;
            halfCell = CellSize / 2;


#if  TestingDebug
            int imageWidth = CellSize;
            int reducedCellSize = CellSize;

            WholeImage = new double[mPassData.Library.Count, imageWidth];
            WholeImageAfter = new double[mPassData.Library.Count, reducedCellSize];
#endif

           // SetupFilter();


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

            SetupFilter();

            maxIndex = 1;
            Rectangle ROI = new Rectangle((int)Locations[maxIndex].CellCenter.X - halfCell ,(int) Locations[maxIndex].CellCenter.Y - halfCell , CellSize , CellSize);
            ReferenceProfile = XProfile(Library[maxIndex], ROI);

            ZeroProfile(ReferenceProfile);

            sumWhole = ReferenceProfile.Sum();

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindZ(x));
        }

        #endregion

        protected override void RunNodeImpl()
        {

            Library = mPassData.Library;
            Locations = mPassData.Locations;

         
            CellSize =(int) mPassData.GetInformation("CellSize");


            ZRegisterCell();

#if  TestingDebug
            //Library.CreateAVIVideoEMGU(@"c:\temp\allImages.avi", 10);
            Bitmap b = WholeImage.MakeBitmap();
            var bAfter = WholeImageAfter.MakeBitmap();
            int w = b.Width + bAfter.Width;
#endif
        }
    }
}
