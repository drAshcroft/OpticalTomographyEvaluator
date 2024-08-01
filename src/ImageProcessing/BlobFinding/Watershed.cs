using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV;

namespace ImageProcessing.BlobFinding
{
    public class Watershed
    {
        public static  Blob[] FindBlobs(Image<Gray, float> image,bool Sensitive)
        {
            Bgr Average;
           

            var imageThresh = image.Convert<Bgr, byte>();
            ///Gets average image intensity of background subtracted PP images
            Average = imageThresh.GetAverage();
            //Average.Blue = 10;
            //Average.Green = 10;
           // Average.Red = 10;   
            if (Sensitive)
            {
                imageThresh = imageThresh.SmoothGaussian(21);
            ///Performs adaptive thresholding of the PPs and applies a Gaussian filter with a Kernel specified
//              imageThresh=  imageThresh.ThresholdAdaptive(new Bgr(Color.White), Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY,5,new Bgr(3, 3, 3));
//              imageThresh = imageThresh.SmoothGaussian(11);
            }
           
            //Applies the Otsu thresholding algorithm using the average intensity of the PP
            imageThresh = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));
            int currentColor = 1;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (imageThresh.Data[y, x, 0] == 0)
                    {
                        //Using connectivity of 8, changes the color (intensity) of the pixels above threshold; these pixels should then represent the cell
                        CvInvoke.cvFloodFill(imageThresh, new Point(x, y), new MCvScalar(currentColor, 0, 0, 255),
                                new MCvScalar(6, 6, 6),
                                new MCvScalar(6, 6, 6), out comp,
                                Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                                Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);

                        currentColor = (currentColor + 1) % 253;
                        if (currentColor == 0) currentColor = 1;
                    }
                }
            currentColor++;
            double[] minX = new double[currentColor -1];
            double[] minY = new double[currentColor - 1];
            double[] maxX = new double[currentColor - 1];
            double[] maxY = new double[currentColor - 1];
            double[] aveX = new double[currentColor - 1];
            double[] aveY = new double[currentColor - 1];

            //Finds min and max values of intensity?
            for (int i = 0; i < minX.Length; i++)
            {
                minX[i] = double.MaxValue;
                minY[i] = double.MaxValue;
                maxX[i] = double.MinValue;
                maxY[i] = double.MinValue;
            }
            double[] cc = new double[currentColor - 1];
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    currentColor = imageThresh.Data[y, x, 0];
                    if (currentColor < 255)
                    {
                        if (minX[currentColor] > x) minX[currentColor] = x;
                        if (minY[currentColor] > y) minY[currentColor] = y;
                        if (maxX[currentColor] < x) maxX[currentColor] = x;
                        if (maxY[currentColor] < y) maxY[currentColor] = y;
                        aveX[currentColor] += x;
                        aveY[currentColor] += y;
                        cc[currentColor]++;
                    }
                }

            Blob[] outArray = new Blob[minX.Length];
            //Determines the center coordinates, length and width of the cell (blob)
            for (int i = 0; i < minX.Length; i++)
            {
                aveX[i] /= cc[i];
                aveY[i] /= cc[i];

 //               outArray[i] = new Blob(new Point((int)aveX[i], (int)aveY[i]), minX[i], maxX[i], minY[i], maxY[i]);
                outArray[i] = new Blob(new Point(image.Height / 2, image.Width/2), 0, image.Height, 0, image.Width);
            }
            return outArray;
        }
    }
}
