using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;


namespace ImageProcessing._2D
{
    public class MotionRegistration
    {
        public static Image<Gray, float> PhaseCorrelation(Image<Gray, float> StationaryImage, Image<Gray, float> CompareImage, out  PointF shiftAmount)
        {
            shiftAmount = MathLibrary.FFT.MathFFTHelps.FindShift(StationaryImage.Data, CompareImage.Data);

            Image<Gray, float> temp = CompareImage.CopyBlank();

            int sX = (int)(shiftAmount.X);
            int sY = (int)(shiftAmount.Y);
            int dX, dY;
            for (int x = 0; x < CompareImage.Width; x++)
            {
                dX = (int)(x + sX);
                if (dX > 0 && dX < CompareImage.Width)
                {
                    for (int y = 0; y < CompareImage.Height; y++)
                    {
                        dY = (int)(y + sY);
                        if (dY > 0 && dY < CompareImage.Height)
                            temp.Data[dY, dX, 0] = CompareImage.Data[y, x, 0];
                    }
                }
            }

            return temp;
        }

        private static double[] XProfile(Image<Gray, float> image)
        {
            double[] profile = new double[image.Width];
            double val = 0;
            for (int x = 0; x < image.Width; x++)
            {
                val = 0;
                for (int y = 0; y < image.Height; y++)
                    val += image.Data[y, x, 0];
                profile[x] = val;
            }

            return profile;
        }


        public static Image<Gray, float> PhaseCorrelationXOnly(double[] StationaryProfile, Image<Gray, float> CompareImage)
        {

            double[] xProfile = XProfile(CompareImage);

            float shiftAmount = MathLibrary.FFT.MathFFTHelps.FindShift(StationaryProfile, xProfile);

            Image<Gray, float> temp = CompareImage.CopyBlank();

            int sX = (int)(Math.Round(shiftAmount));
            int sY = (int)(0);
            int dX, dY;
            for (int x = 0; x < CompareImage.Width; x++)
            {
                dX = (int)(x + sX);
                if (dX > 0 && dX < CompareImage.Width)
                {
                    for (int y = 0; y < CompareImage.Height; y++)
                    {
                        dY = (int)(y + sY);
                        if (dY > 0 && dY < CompareImage.Height)
                            temp.Data[dY, dX, 0] = CompareImage.Data[y, x, 0];
                    }
                }
            }

            return temp;
        }

        public static float PhaseCorrelationXOnly(double[] StationaryProfile, double[] CompareImage)
        {
            return MathLibrary.FFT.MathFFTHelps.FindShift(StationaryProfile, CompareImage);
        }

        public static float PhaseCorrelationXOnly(double[] StationaryProfile, double[] CompareImage, double [] Filter)
        {
            return MathLibrary.FFT.MathFFTHelps.FindShift(StationaryProfile, CompareImage,Filter );
        }

    }
}
