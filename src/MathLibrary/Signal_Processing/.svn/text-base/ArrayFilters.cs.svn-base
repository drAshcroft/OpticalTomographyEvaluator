using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILNumerics;

namespace MathLibrary.Signal_Processing
{
    public static class ArrayFilters
    {
        /// <summary>
        /// performs simple box car filtering of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Period">number of values that are used for the filter</param>
        /// <returns></returns>
        public static double[] SmoothArrayBoxCar(this double[] array, int Period)
        {
            double[] Smoothed = new double[array.Length];
          

            int Cut = (int)(Period / 2.0f);

            double sum = array[0];
            for (int i = 0; i < Cut; i++)
            {
                Smoothed[i] = sum / (i + 1);
                sum += array[i + 1];
            }

            sum = array[array.Length - 1];
            for (int i = 0; i <= Cut; i++)
            {
                Smoothed[array.Length - 1 - i] = sum / (i + 1);
                sum += array[array.Length - i - 1];
            }

            for (int i = Cut; i < array.Length - Cut - 1; i++)
            {
                for (int j = i - Cut; j < i + Cut; j++)
                {
                    Smoothed[i] += array[j];
                }

                Smoothed[i] = Smoothed[i] / Period;
            }
            return Smoothed;
        }

       
        /// <summary>
        /// performs simple box car filtering of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Period">number of values that are used for the filter</param>
        /// <returns></returns>
        public static float[] SmoothArrayBoxCar(this float[] array, int Period)
        {
            float[] Smoothed = new float[array.Length];
            int Count = 0;

            int Cut = (int)(Period / 2.0f);

            float sum = array[0];
            Smoothed [0]=sum;
            for (int i = 1; i < Cut; i++)
            {
                sum=0;
                Count = 0;
                for (int j = 0; j < i * 2; j++)
                {
                    sum += array[j];
                    Count++;
                }

                Smoothed[i] = sum / Count ;
            }
            Smoothed[array.Length - 1] = array[array.Length - 1];

            sum = array[array.Length - 1];
            for (int i = 1; i <= Cut; i++)
            {
                sum = 0;
                Count = 0;
                for (int j = 0; j < i * 2; j++)
                {
                    sum += array[array.Length -1- j];
                    Count++;
                }

                Smoothed[array.Length - 1-i] = sum / Count;
            }

            for (int i = Cut; i < array.Length - Cut - 1; i++)
            {
                Count = 0;
                for (int j = i - Cut; j < i + Cut; j++)
                {
                    Smoothed[i] += array[j];
                    Count++;
                }
                if (Count > 0)
                    Smoothed[i] = Smoothed[i] / Count;
            }
            return Smoothed;
        }

        public static float[] SmoothArrayHighPoints(this float[] array, int fastPeriod, int slowPeriod)
        {
            float[] SmoothedFast = SmoothArrayBoxCar(array, fastPeriod);
            float[] SmoothedSlow = SmoothArrayBoxCar(array, slowPeriod);

            float maxValue = SmoothedSlow.Max();
            float minValue = SmoothedSlow.Min();


            float[] Smoothed = new float[array.Length];

            float u;
            for (int i = 0; i < array.Length; i++)
            {
                u = (SmoothedSlow[i] - maxValue) / (minValue - maxValue);
                Smoothed[i] = SmoothedFast[i] * u + SmoothedSlow[i] * (1 - u);
            }

            return Smoothed;
        }


        public static float[] SmoothArrayMedianFilter(this float[] array, int period)
        {
            List<float> lsamp = new List<float>(new float[period]);
            float[] arrayOut = new float[array.Length];
            int LL = 0;
            int Cut = (int)(period / 2.0);
            int Length = array.GetLength(0) - 1;
            unchecked
            {
                arrayOut [0]=array[0];
                arrayOut[arrayOut.Length -1]=array[array.Length-1];

                for (int i = 1; i < Cut; i++)
                {
                     List<float> lsamp2 = new List<float>(new float[i+Cut]);
                     for (int j = 0; j <  i+ Cut ; j++)
                     {
                         lsamp2[j] = array[i];
                     }
                     lsamp2.Sort();
                     arrayOut[i] = lsamp2[lsamp2.Count / 2];


                     for (int j = 0; j <  i+Cut; j++)
                     {
                         lsamp2[j] = array[Length-j];
                     }
                     lsamp2.Sort();
                     arrayOut[Length - i] = lsamp2[lsamp2.Count / 2];
                }


                for (int i = Cut; i < Length - Cut+1; i++)
                {
                    int X = i - Cut;
                    for (LL = 0; LL < period; LL++)
                    {
                        lsamp[LL] = array[X];
                        X++;
                    }

                    lsamp.Sort();
                    arrayOut[i] = (lsamp[Cut]);
                }
            }
            return arrayOut;
        }


    
    }
}
