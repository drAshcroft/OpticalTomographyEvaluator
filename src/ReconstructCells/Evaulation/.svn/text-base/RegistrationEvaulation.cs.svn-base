#if DEBUG
#define TESTING
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using ImageProcessing._2D;
using ImageProcessing.Statistics;
using MathLibrary;

namespace ReconstructCells.Evaulation
{
    class RegistrationEvaulation
    {
        private float[] CovarianceCoeff;
        private float[] MaxCovariance;
        private OnDemandImageLibrary Library;

        private float AutoCovarianceCompare(int imageNumber)
        {
            var image1 = Library[imageNumber];
         
          //  image2.ROI = image1.ROI;
            image1.NormalizedImageByStandardDeviation();
            var image2 = image1.Copy();

            var imageOut = new Image<Gray, float>(MathLibrary.FFT.MathFFTHelps.CrossCorrelationFFT(image1.Data, image2.Data));

            return imageOut.Data[imageOut.Height / 2, imageOut.Width / 2, 0] / imageOut.Data.LongLength;

        }
        private float AutoCovarianceCompareMax(int imageNumber)
        {
            var image1 = Library[imageNumber];
          
            image1.NormalizedImageByStandardDeviation();
            var image2 = image1.Copy();

            var imageOut = new Image<Gray, float>(MathLibrary.FFT.MathFFTHelps.CrossCorrelationFFT(image1.Data, image2.Data));

            return imageOut.Data.MaxArray();
        }


        private void BatchCovarianceCompareCut(int imageNumber)
        {
            var image1 = Library[imageNumber];
            var image2 = Library[imageNumber + Library.Count / 2];

            int Buffer = 10;
            var cellLocation = mPassData.Locations[imageNumber];
            int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
            int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
            int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
            int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

            Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
            image1.ROI = ROI;
            image1 = image1.Copy();

            cellLocation = mPassData.Locations[imageNumber + 250];
            minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
            maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
            minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
            maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

            ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
            image2.ROI = ROI;
            image2 = image2.Copy();

            image2 = image2.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

            image1.ROI = new Rectangle(30, 30, image1.Width - 60, image2.Width - 60);
            image2.ROI = image1.ROI;
            image1 = image1.Copy();
            image2 = image2.Copy();

            image1.NormalizedImageByStandardDeviation();
            image2.NormalizedImageByStandardDeviation();

            var imageOut = new Image<Gray, float>(MathLibrary.FFT.MathFFTHelps.CrossCorrelationFFT(image1.Data, image2.Data));

            CovarianceCoeff[imageNumber] = imageOut.Data[imageOut.Height / 2, imageOut.Width / 2, 0] / imageOut.Data.LongLength;
            MaxCovariance[imageNumber] = imageOut.Data.MaxArray();
        }

        PassData mPassData;
        int CellSize;
        public float[] MirrorEvaluationCut(PassData passData, string ExpTag)
        {
            mPassData = passData;
            CellSize = (int)(mPassData.Locations[0].CellSize * 1.7);
            Library = passData.Library;

            CovarianceCoeff = new float[Library.Count / 2];
            MaxCovariance = new float[Library.Count / 2];

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count / 2;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCovarianceCompareCut(x));

            float norm = AutoCovarianceCompare(10);
            float normMax = AutoCovarianceCompareMax(10);
            for (int i = 0; i < CovarianceCoeff.Length; i++)
            {
                CovarianceCoeff[i] /= norm;
                MaxCovariance[i] /= normMax;
            }

            var g=CovarianceCoeff.DrawGraph();
            int w = g.Width;
#if TESTING
            try
            {
                Program.WriteLine(ExpTag + "Correlation |||");
                string junk = ExpTag + "\n";
                for (int i = 0; i < CovarianceCoeff.Length; i++)
                {
                    junk += CovarianceCoeff[i] + "\n";
                    Program.WriteLine(CovarianceCoeff[i].ToString());
                }


                System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + ExpTag + ".csv", junk);
                Program.WriteLine("Registration Qual - Max " + ExpTag);
                junk = ExpTag + "\n";
                for (int i = 0; i < CovarianceCoeff.Length; i++)
                {
                    junk += MaxCovariance[i] + "\n";
                    Program.WriteLine(MaxCovariance[i].ToString());
                }

                System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + ExpTag + ".csv", junk);
                Program.WriteLine("|||");
            }
            catch (Exception ex)
            {

                Program.WriteLine(ex.Message);
                Program.WriteLine(ex.StackTrace);

            }
#endif
            return CovarianceCoeff;
        }


        private void BatchCovarianceCompare(int imageNumber)
        {
            var image1 = Library[imageNumber];
            var image2 = Library[imageNumber + Library.Count / 2];

            image2 = image2.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);

            image1.ROI = new Rectangle(30, 30, image1.Width - 60, image2.Width - 60);
            image2.ROI = image1.ROI;
            var image3 = image1;
            var image4 = image2;

            image3.NormalizedImageByStandardDeviation();
            image4.NormalizedImageByStandardDeviation();

            var imageOut = new Image<Gray, float>(MathLibrary.FFT.MathFFTHelps.CrossCorrelationFFT(image3.Data, image4.Data));

            CovarianceCoeff[imageNumber] = imageOut.Data[imageOut.Height / 2, imageOut.Width / 2, 0] / imageOut.Data.LongLength;
            MaxCovariance[imageNumber] = imageOut.Data.MaxArray();
        }
        public float[] MirrorEvaluation(OnDemandImageLibrary library, string ExpTag)
        {
            Library = library;

            CovarianceCoeff = new float[Library.Count / 2];
            MaxCovariance = new float[Library.Count / 2];

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count / 2;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCovarianceCompare(x));

            float norm = AutoCovarianceCompare(10);
            float normMax = AutoCovarianceCompareMax(10);
            for (int i = 0; i < CovarianceCoeff.Length; i++)
            {
                CovarianceCoeff[i] /= norm;
                MaxCovariance[i] /= normMax;
            }
            var g = CovarianceCoeff.DrawGraph();
            int w = g.Width;
#if TESTING
            try
            {
                Program.WriteLine(ExpTag + "Correlation |||");
                string junk = ExpTag + "\n";
                for (int i = 0; i < CovarianceCoeff.Length; i++)
                {
                    junk += CovarianceCoeff[i] + "\n";
                    Program.WriteLine(CovarianceCoeff[i].ToString());
                }


                System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + ExpTag + ".csv", junk);
                Program.WriteLine("Registration Qual - Max " + ExpTag);
                junk = ExpTag + "\n";
                for (int i = 0; i < CovarianceCoeff.Length; i++)
                {
                    junk += MaxCovariance[i] + "\n";
                    Program.WriteLine(MaxCovariance[i].ToString());
                }

                System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + ExpTag + ".csv", junk);
                Program.WriteLine("|||");
            }
            catch (Exception ex)
            {

                Program.WriteLine(ex.Message);
                Program.WriteLine(ex.StackTrace);

            }
#endif
            return CovarianceCoeff;
        }
    }
}
