using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessing.BlobFinding;

namespace ImageProcessing._2D
{
    public static class ImageManipulation
    {

        public static void DivideImage(this Image<Gray, float> image, Image<Gray, float> divisor)
        {
            var num = image.Data;
            var denom = divisor.Data;

            unsafe
            {
                fixed (float* PNum = image.Data, PDenom = divisor.Data)
                {
                    float* pNum = PNum, pDenom = PDenom;
                    unchecked
                    {
                        for (int i = 0; i < image.Data.LongLength; i++)
                        {
                            *pNum = (*pNum) / (*pDenom);
                            pNum++; pDenom++;
                        }
                    }
                }
            }
            //for (int x = 0; x < image.Width; x++)
            //    for (int y = 0; y < image.Height; y++)
            //        num[y, x, 0] /= denom[y, x, 0];
        }

        public static Image<Gray, float> DivideImage2(this Image<Gray, float> image, Image<Gray, float> divisor)
        {
            var outI = image.CopyBlank();


            var num = image.Data;
            var denom = divisor.Data;

            unsafe
            {
                fixed (float* PNum = image.Data, PDenom = divisor.Data, POUT = outI.Data)
                {
                    float* pNum = PNum, pDenom = PDenom;
                    float* pOutI = POUT;
                    unchecked
                    {
                        for (int i = 0; i < image.Data.LongLength; i++)
                        {
                            *pOutI = (*pNum) / (*pDenom);
                            pNum++; pDenom++; pOutI++;
                        }
                    }
                }
            }

            //var num = image.Data;
            //var denom = divisor.Data;
            //var outD = outI.Data;

            //float val = 1, d;
            //for (int x = 0; x < image.Width; x++)
            //    for (int y = 0; y < image.Height; y++)
            //    {
            //        d = denom[y, x, 0];
            //        if (d != 0)
            //        {
            //            val = num[y, x, 0] / d;
            //            outD[y, x, 0] = val;
            //        }
            //        else
            //            outD[y, x, 0] = val;

            //    }
            return outI;

        }

        public static Image<Gray, float> DivideImage2(this Image<Gray, float> image, float divisor)
        {
            var outI = image.CopyBlank();
            var num = image.Data;

            var outD = outI.Data;

            float val = 1, d;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    d = divisor;
                    if (d != 0)
                    {
                        val = num[y, x, 0] / d;
                        outD[y, x, 0] = val;
                    }
                    else
                        outD[y, x, 0] = val;

                }
            return outI;

        }


        public static Image<Gray, float> UnSharp(Image<Gray, float> image, int Radius, float factor)
        {
            var smoothed = image.SmoothGaussian(Radius);
            smoothed = smoothed.Mul(factor);
            var unsharped = image.Sub(smoothed);
            unsharped = unsharped.Add(new Gray(1));
            return unsharped;
        }

        public static Emgu.CV.Image<Gray, float> LoadBrokenVisiongate(string Filename)
        {
            Emgu.CV.Image<Gray, float> VisionGateImage = new Emgu.CV.Image<Gray, float>(Filename);
            Emgu.CV.Image<Gray, float> newHolder = new Emgu.CV.Image<Gray, float>(VisionGateImage.Width / 2, VisionGateImage.Height / 2);


            int iWidth = VisionGateImage.Width;
            int iHeight = VisionGateImage.Height;

            float[, ,] dataOut = VisionGateImage.Data;
            float[, ,] DataIn = newHolder.Data;

            int ccY = 0;

            for (int y = 0; y < iHeight; y += 2)
            {
                for (int x = 0; x < iWidth; x += 2)
                {
                    DataIn[y / 2, x / 2, 0] = dataOut[y, x, 0];
                }
                ccY++;
            }


            return newHolder;
        }

        public static Emgu.CV.Image<Gray, float> FixBrokenVisiongate(Emgu.CV.Image<Gray, float> VisionGateImage)
        {
            Emgu.CV.Image<Gray, float> newHolder = new Emgu.CV.Image<Gray, float>(VisionGateImage.Width / 2, VisionGateImage.Height / 2);


            int iWidth = VisionGateImage.Width;
            int iHeight = VisionGateImage.Height;

            float[, ,] dataOut = VisionGateImage.Data;
            float[, ,] DataIn = newHolder.Data;

            int ccY = 0;

            for (int y = 0; y < iHeight; y += 2)
            {
                for (int x = 0; x < iWidth; x += 2)
                {
                    DataIn[y / 2, x / 2, 0] = dataOut[y, x, 0];
                }
                ccY++;
            }


            return newHolder;
        }

        public static Emgu.CV.Image<Gray, float> ConvertToImage(double[,] imageData)
        {
            Emgu.CV.Image<Gray, float> ih = new Image<Gray, float>(imageData.GetLength(0), imageData.GetLength(1));

            unsafe
            {
                fixed (float* pDataOut = ih.Data)
                fixed (double* pDataIn = imageData)
                {
                    float* pOut = pDataOut;
                    double* pIn = pDataIn;
                    for (long i = 0; i < imageData.LongLength; i++)
                    {
                        *pOut = (float)(*pIn);
                        pOut++;
                        pIn++;
                    }
                }
            }
            return ih;
        }


        public static Image<Gray, float> CopyROI(Image<Gray, float> image, Rectangle ROI)
        {
            if (ROI.X > 0 && ROI.Y > 0 && ROI.Right < image.Width && ROI.Bottom < image.Height)
            {
                image.ROI = ROI;
                var newimage = image.Copy();
                image.ROI = Rectangle.Empty;
                return newimage;
            }
            else
            {
                var newI = ImageManipulation.CopyClipped(image, ROI);
                return newI;
            }
        }

        public static Image<Gray, float> CopyClipped(Image<Gray, float> image, Rectangle ROI)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);

            float backGround = 0;// = GetBackgroundValue(image, RoughVariance);
            int cc = 0;
            int xx = ROI.Left;
            for (int x = 0; x < ROI.Width; x++)
            {
                int yy = ROI.Top;
                for (int y = 0; y < ROI.Height; y++)
                {
                    if ((yy >= 0 && xx >= 0 && yy < image.Height && xx < image.Width))
                    {
                        if (xx == 0 || xx == image.Width-1 || yy == 0 || yy == image.Height-1)
                        {

                            backGround += image.Data[yy, xx, 0];
                            cc++;
                        }
                    }
                    yy++;
                }
                xx++;
            }
            if (cc == 0)
                System.Diagnostics.Debug.Print("");
            backGround = backGround / cc;

            xx = ROI.Left;
            for (int x = 0; x < ROI.Width; x++)
            {
                int yy = ROI.Top;
                for (int y = 0; y < ROI.Height; y++)
                {
                    if (yy >= 0 && xx >= 0 && yy < image.Height && xx < image.Width)
                        imageOut.Data[y, x, 0] = image.Data[yy, xx, 0];
                    else
                        imageOut.Data[y, x, 0] = backGround;
                    yy++;
                }
                xx++;
            }

            return imageOut;
        }


        public static Image<Gray, float> CopyClipped(Image<Gray, float> image, Rectangle ROI, float RoughVariance)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);

            float backGround = GetBackgroundValue(image, RoughVariance);

            int xx = ROI.Left;
            for (int x = 0; x < ROI.Width; x++)
            {
                int yy = ROI.Top;
                for (int y = 0; y < ROI.Height; y++)
                {
                    if (yy >= 0 && xx >= 0 && yy < image.Height && xx < image.Width)
                        imageOut.Data[y, x, 0] = image.Data[yy, xx, 0];
                    else
                        imageOut.Data[y, x, 0] = backGround;
                    yy++;
                }
                xx++;
            }

            return imageOut;
        }


        public static Image<Gray, float> RemoveClearBackground(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);
            int xx = ROI.X, yy;
            for (int x = 0; x < ROI.Width; x++)
            {
                yy = ROI.Y;
                for (int y = 0; y < ROI.Height; y++)
                {
                    imageOut.Data[y, x, 0] = image.Data[yy, xx, 0] / Background.Data[yy, xx, 0];
                    yy++;
                }
                xx++;
            }
            return imageOut;
        }

        public static float GetBackgroundValue(Image<Gray, float> image, float RoughVariance)
        {
            float max = 0;
            for (int x = 0; x < image.Width; x += 5)
            {
                for (int y = 0; y < image.Height; y += 5)
                {
                    if (max < image.Data[y, x, 0]) max = image.Data[y, x, 0];
                }
            }

            max = max - RoughVariance;

            double sum = 0;
            long count = 0;
            for (int x = 0; x < image.Width; x += 5)
            {
                for (int y = 0; y < image.Height; y += 5)
                {
                    if (max < image.Data[y, x, 0])
                    {
                        sum += image.Data[y, x, 0];
                        count++;
                    }
                }
            }

            return (float)(sum / count);

        }



        public static Image<Gray, float> RemoveClippedBackgroundMasked(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI, float RoughVariance)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);

            imageOut = imageOut.Add(new Gray(-1000));

            int sX = ROI.X, sY = ROI.Y, sR = ROI.Right, sB = ROI.Bottom;

            int dX = 0;
            int dY = 0;

            if (sX < 0)
            {
                dX = -1 * sX;
                sX = 0;
            }

            if (sY < 0)
            {
                dY = -1 * sY;
                sY = 0;
            }

            if (sR > image.Width)
            {
                sR = image.Width;
            }

            if (sB > image.Height)
            {
                sB = image.Height;
            }

            int nWidth = sR - sX + dX;
            int nHeight = sB - sY + dY;

            int xx = sX, yy;
            for (int x = dX; x < nWidth; x++)
            {
                yy = sY;
                for (int y = dY; y < nHeight; y++)
                {
                    imageOut.Data[y, x, 0] = image.Data[yy, xx, 0] / Background.Data[yy, xx, 0];
                    yy++;
                }
                xx++;
            }

            return imageOut;
        }


        public static Image<Gray, float> RemoveClippedBackground(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI, float RoughVariance)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);

            //double[] min, max;
            //Point[] mp, Mp;
            //image.MinMax(out min, out max, out mp, out Mp);



            int h = image.Height / 2;
            float[] Line = new float[image.Width];
            List<float> lLine = new List<float>(Line);
            for (int j = 0; j < image.Width; j++)
                lLine[j] = image.Data[h, j, 0];

            lLine.Sort();

            float ave = 0;
            int cc = 0;
            for (int j = (int)(Math.Round(image.Width / 4.0) * 3 - 5); j < image.Width; j++)
            {
                ave += lLine[j];
                cc++;
            }

            ave = (float)(ave / cc);

            float backGround = ave;// GetBackgroundValue(image, RoughVariance);


            imageOut = imageOut.Add(new Gray(backGround));

            int sX = ROI.X, sY = ROI.Y, sR = ROI.Right, sB = ROI.Bottom;

            int dX = 0;
            int dY = 0;

            if (sX < 0)
            {
                dX = -1 * sX;
                sX = 0;
            }

            if (sY < 0)
            {
                dY = -1 * sY;
                sY = 0;
            }

            if (sR > image.Width)
            {
                sR = image.Width;
            }

            if (sB > image.Height)
            {
                sB = image.Height;
            }

            int nWidth = sR - sX + dX;
            int nHeight = sB - sY + dY;

            int xx = sX, yy;
            for (int x = dX; x < nWidth; x++)
            {
                yy = sY;
                for (int y = dY; y < nHeight; y++)
                {
                    imageOut.Data[y, x, 0] = image.Data[yy, xx, 0] / Background.Data[yy, xx, 0];
                    yy++;
                }
                xx++;
            }

            return imageOut;
        }

        public static Image<Gray, float> RemoveClippedBackgroundO(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI, float RoughVariance)
        {
            Image<Gray, float> imageOut = new Image<Gray, float>(ROI.Width, ROI.Height);
            int nWidth = ROI.Width, nHeight = ROI.Height;
            int nX = ROI.X, nY = ROI.Y;

            if (nX < 0)
            {
                nX = 0;
                nWidth = ROI.Right - nX;
            }

            if (nY < 0)
            {
                nY = 0;
                nWidth = ROI.Right - nY;
            }

            if (ROI.Right > image.Width)
            {
                nWidth = image.Width - ROI.X;
            }

            if (ROI.Bottom > image.Height)
            {
                nHeight = image.Height - ROI.Y;
            }

            int xx = nX, yy;
            for (int x = 0; x < nWidth; x++)
            {
                yy = nY;
                for (int y = 0; y < nHeight; y++)
                {
                    if (yy >= 0 && xx >= 0 && yy < image.Height && xx < image.Width)
                        if (y >= 0 && x >= 0 && y < imageOut.Height && x < imageOut.Width)
                            imageOut.Data[y, x, 0] = image.Data[yy, xx, 0] / Background.Data[yy, xx, 0];
                    yy++;
                }
                xx++;
            }

            float backGround = GetBackgroundValue(imageOut, RoughVariance);
            for (int x = 0; x < imageOut.Width; x++)
            {
                for (int y = 0; y < imageOut.Height; y++)
                {
                    if (0 == imageOut.Data[y, x, 0])
                        imageOut.Data[y, x, 0] = backGround;
                }
            }

            return imageOut;
        }

        /// <summary>
        /// flips a dark image to a light image with the target being set at max.  It is best to use a value that is slightly less than max to handle negatives.
        /// if there are no negatives, set it at max
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxBackground"></param>
        public static void InvertToMax(Image<Gray, float> image, float maxBackground)
        {
            //float background = (float)(Int16.MaxValue * .9f);
            unsafe
            {
                fixed (float* pImage = image.Data)
                {
                    float* pOut = pImage;

                    for (long x = 0; x < image.Data.LongLength; x++)
                    {
                        if (*pOut != -1000f)
                            *pOut = maxBackground - *pOut;
                        else
                            *pOut = 0;
                        pOut++;

                    }
                }
            }
        }


        public static void InvertImage(Image<Gray, float> image)
        {
            double maxValue = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.Data[y, x, 0] > maxValue) maxValue = image.Data[y, x, 0];
                }

            maxValue *= .9;

            double sum = 0, count = 0;

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.Data[y, x, 0] > maxValue)
                    {
                        sum += image.Data[y, x, 0];
                        count++;
                    }
                }

            float background = (float)(sum / count);

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    image.Data[y, x, 0] = background - image.Data[y, x, 0];
                }
        }

        public static Image<Gray, float> InvertImageAndClipOtsu(Image<Gray, float> image, bool removeMaskedArea)
        {

          
            return CenterOfGravity.InvertAndApplyOTSUMask(image, removeMaskedArea);


        }




        public static void InvertImageAndClip(Image<Gray, float> image)
        {
            double maxValue = 0;
            float val;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    val = image.Data[y, x, 0];

                    if (val > maxValue && val != 1000000) maxValue = val;
                }

            maxValue *= .93;

            double sum = 0, count = 0;

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    val = image.Data[y, x, 0];
                    if (val > maxValue && val != 1000000)
                    {
                        sum += val;
                        count++;
                    }
                }

            float background = (float)(sum / count) * .97f;


            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    val = background - image.Data[y, x, 0];
                    //  if (val < 0) val = 0;
                    image.Data[y, x, 0] = val;
                }
        }

        public static void InvertFlattenImage(Image<Gray, float> image)
        {
            double maxValue = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.Data[y, x, 0] > maxValue) maxValue = image.Data[y, x, 0];
                }

            maxValue *= .9;

            List<double[]> planePoints = new List<double[]>();
            for (int x = 0; x < image.Width; x += 5)
                for (int y = 0; y < image.Height; y += 5)
                {
                    if (image.Data[y, x, 0] > maxValue)
                    {
                        planePoints.Add(new double[] { x, y, image.Data[y, x, 0] });
                    }
                }
            double[] coeff;
            MathLibrary.Regression.LinearRegression.PlaneLinearRegression(planePoints, out coeff);

            float background;

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    background = (float)(x * coeff[0] + y * coeff[1] + coeff[2]);
                    image.Data[y, x, 0] = background - image.Data[y, x, 0];
                }

        }


        public static void InvertFlattenNormalizeImage(Image<Gray, float> image, float TargetAverageEnergy)
        {
            double maxValue = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.Data[y, x, 0] > maxValue) maxValue = image.Data[y, x, 0];
                }

            maxValue *= .9;

            List<double[]> planePoints = new List<double[]>();
            for (int x = 0; x < image.Width; x += 5)
                for (int y = 0; y < image.Height; y += 5)
                {
                    if (image.Data[y, x, 0] > maxValue)
                    {
                        planePoints.Add(new double[] { x, y, image.Data[y, x, 0] });
                    }
                }
            double[] coeff;
            MathLibrary.Regression.LinearRegression.PlaneLinearRegression(planePoints, out coeff);


            float average;


            float background;

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    background = (float)(x * coeff[0] + y * coeff[1] + coeff[2]);
                    image.Data[y, x, 0] = background - image.Data[y, x, 0];
                }




            double averaged = 0;
            double cc = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (x < 5 || y < 5)
                    {
                        averaged += image.Data[y, x, 0];
                        cc++;
                    }
                }

            average = (float)(averaged / cc);
            image = image.Add(new Gray(-1 * average));

            average = (float)(TargetAverageEnergy / image.GetAverage().Intensity);

            image = image.Mul(average);
        }


        public static void NormalizedImageByStandardDeviation(this Image<Gray, float> image)
        {


            unsafe
            {
                fixed (float* pImage = image.Data)
                {
                    float* ppImage = pImage;
                    double sum = 0;
                    for (long i = 0; i < image.Data.LongLength; i++)
                    {
                        sum += *ppImage;
                        ppImage++;
                    }

                    double average = sum / image.Data.LongLength;

                    ppImage = pImage;
                    sum = 0;
                    for (long i = 0; i < image.Data.LongLength; i++)
                    {
                        sum += Math.Pow(average - *ppImage, 2);
                        ppImage++;
                    }

                    float std = (float)Math.Sqrt(sum / (image.Data.LongLength - 1));
                    float ave = (float)average;

                    ppImage = pImage;
                    for (long i = 0; i < image.Data.LongLength; i++)
                    {
                        *ppImage = (*ppImage - ave) / std;
                        ppImage++;
                    }

                }
            }


        }
    }
}
