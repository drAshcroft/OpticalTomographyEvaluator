using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using MathLibrary;

namespace ImageProcessing._2D
{
    public class FocusScores
    {
        public static float F4(Image<Gray, float> image)
        {
            int MaxX = image.Width - 2;
            int MaxY = image.Height - 2;

            double Max = double.MinValue;
            double Min = double.MaxValue;
            double d = 0;

            for (int x = 2; x < MaxX; x++)
            {
                for (int y = 2; y < MaxY; y++)
                {
                    d = image.Data[y, x, 0];
                    if (d > Max)
                    {
                        Max = d;
                    }
                    else if (d < Min)
                        Min = d;

                }
            }

            double sum1 = 0;
            double sum2 = 0;
            double sumAve = 0;

            double cut = (Max - Min) * 1d / 3d + Min;
            long cc = 0;

            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    d = image.Data[y, x, 0];
                    if (d > cut)
                    {
                        sum1 += d * image.Data[y + 1, x, 0];
                        sum2 += d * image.Data[y + 2, x, 0];
                        sumAve += d * d;
                        cc++;
                    }
                }
            }

            sumAve = sumAve / cc;
            if (cc == 0) { sumAve = 1; cc = 1; }
            // Bitmap b = SourceImage.ToBitmap();
            // int w = b.Width;
            sum1 /= (sumAve * cc);
            sum2 /= (sumAve * cc);

            double f4 = Math.Abs(sum1 - sum2);
            // if (f4 < 0)
            //    System.Diagnostics.Debug.Print(" ");
            return (float)(f4 * 1000);// *(Max - Min) / Max;


        }


        public static double F4(double[,] SourceImage)
        {

            double Max = SourceImage.MaxArray();
            double Min = SourceImage.MinArray();
            int MaxI = SourceImage.GetLength(0) - 2;
            int MaxJ = SourceImage.GetLength(1) - 2;

            double sum1 = 0;
            double sum2 = 0;
            double sumAve = 0;
            double d = 0;
            double cut = (Max - Min) * 1d / 3d + Min;
            long cc = 0;

            for (int i = 0; i < MaxI; i++)
            {
                for (int j = 0; j < MaxJ; j++)
                {
                    if (SourceImage[i, j] > cut)
                    {
                        d = SourceImage[i, j];
                        sum1 += d * SourceImage[i, j + 1];
                        sum2 += d * SourceImage[i, j + 2];
                        sumAve += d * d;

                        cc++;
                    }
                }
            }

            sumAve = sumAve / cc;
            if (cc == 0) { sumAve = 1; cc = 1; }
            // Bitmap b = SourceImage.ToBitmap();
            // int w = b.Width;
            sum1 /= (sumAve * cc);
            sum2 /= (sumAve * cc);

            double f4 = Math.Abs(sum1 - sum2);
            // if (f4 < 0)
            //    System.Diagnostics.Debug.Print(" ");
            return f4 * 1000;// *(Max - Min) / Max;

        }

        public static double F4(float[, ,] SourceImage)
        {

            double Max = SourceImage.MaxArray();
            double Min = SourceImage.MinArray();
            int MaxI = SourceImage.GetLength(0) - 2;
            int MaxJ = SourceImage.GetLength(1) - 2;

            double sum1 = 0;
            double sum2 = 0;
            double sumAve = 0;
            double d = 0;
            double cut = (Max - Min) * 1d / 3d + Min;
            long cc = 0;

            for (int i = 0; i < MaxI; i++)
            {
                for (int j = 0; j < MaxJ; j++)
                {
                    if (SourceImage[i, j, 0] > cut)
                    {
                        d = SourceImage[i, j, 0];
                        sum1 += d * SourceImage[i, j + 1, 0];
                        sum2 += d * SourceImage[i, j + 2, 0];
                        sumAve += d * d;

                        cc++;
                    }
                }
            }

            sumAve = sumAve / cc;
            if (cc == 0) { sumAve = 1; cc = 1; }
            // Bitmap b = SourceImage.ToBitmap();
            // int w = b.Width;
            sum1 /= (sumAve * cc);
            sum2 /= (sumAve * cc);

            double f4 = Math.Abs(sum1 - sum2);
            // if (f4 < 0)
            //    System.Diagnostics.Debug.Print(" ");
            return f4 * 1000;// *(Max - Min) / Max;

        }


    }
}
