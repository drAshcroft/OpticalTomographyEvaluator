using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageProcessing._2D;
using System.Drawing.Imaging;
using Emgu.CV.Structure;
using System.IO;
using BitMiracle.LibTiff.Classic;
using System.Drawing;
using MathLibrary;
using Utilities;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;


namespace ImageProcessing.Statistics
{
    public static class ImageStatistics
    {
        /// <summary>
        /// returns the average and the standard deviation of the image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static float[] GetStatistics(this Image<Gray, float> image)
        {
            float[] moments = new float[2];
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

                    double average  = sum / image.Data.LongLength;

                    ppImage = pImage;
                    sum = 0;
                    for (long i = 0; i < image.Data.LongLength; i++)
                    {
                        sum += Math.Pow(average - *ppImage,2);
                        ppImage++;
                    }

                    double std = Math.Sqrt(sum / (image.Data.LongLength - 1));

                    moments[0] = (float)average;
                    moments[1] = (float)std;
                }
            }
            return moments;
        }
    }
}
