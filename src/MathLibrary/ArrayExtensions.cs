using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MathLibrary
{
    public static class ArrayExtensions
    {
        public static Bitmap DrawGraph(this double[] array)
        {
            Bitmap b = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            double max = array.Max();
            double min = array.Min();
            float lx = 0, ly = 0;
            for (int i = 0; i < array.Length; i++)
            {
                float x = (float)i / array.Length * 400f;
                float y = (float)(400 - (array[i] - min) / (max - min) * 400);
                g.DrawLine(Pens.White, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            return b;
        }

        public static Bitmap DrawGraph(this double[][] array)
        {
            Bitmap b = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            double max = array[0].Max();
            double min = array[0].Min();


            float lx = 0, ly = 0;
            for (int i = 0; i < array[0].Length; i++)
            {
                float x = (float)i / array[0].Length * 400f;
                float y = (float)(400 - (array[0][i] - min) / (max - min) * 400);
                g.DrawLine(Pens.White, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            max = array[1].Max();
            min = array[1].Min();

            lx = 0;
            ly = 0;
            for (int i = 0; i < array[1].Length; i++)
            {
                float x = (float)i / array[1].Length * 400f;
                float y = (float)(400 - (array[1][i] - min) / (max - min) * 400);
                g.DrawLine(Pens.Green, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            return b;
        }

        public static void CopyData(this float[] array)
        {
            string data = "";
            for (int i = 0; i < array.Length && i < 1000; i++)
                data = data + "\n" + array[i].ToString();
            Clipboard.SetText(data);
        }

        public static Bitmap DrawGraph(this float[] array)
        {
            Bitmap b = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            double max = array.Max();
            double min = array.Min();
            float lx = 0, ly = 0;
            for (int i = 0; i < array.Length; i++)
            {
                float x = (float)i / array.Length * 400f;
                float y = (float)(400 - (array[i] - min) / (max - min) * 400);
                g.DrawLine(Pens.White, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            return b;
        }

        public static Bitmap DrawGraph(this float[][] array)
        {
            Bitmap b = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            double max = array[0].Max();
            double min = array[0].Min();


            float lx = 0, ly = 0;
            for (int i = 0; i < array[0].Length; i++)
            {
                float x = (float)i / array[0].Length * 400f;
                float y = (float)(400 - (array[0][i] - min) / (max - min) * 400);
                g.DrawLine(Pens.White, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            max = array[1].Max();
            min = array[1].Min();

            lx = 0;
            ly = 0;
            for (int i = 0; i < array[1].Length; i++)
            {
                float x = (float)i / array[1].Length * 400f;
                float y = (float)(400 - (array[1][i] - min) / (max - min) * 400);
                g.DrawLine(Pens.Green, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            return b;
        }

        public static Bitmap DrawGraph(this int[] array)
        {
            Bitmap b = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            double max = array.Max();
            double min = array.Min();
            float lx = 0, ly = 0;
            for (int i = 0; i < array.Length; i++)
            {
                float x = (float)i / array.Length * 400f;
                float y = (float)(400 - (array[i] - min) / (max - min) * 400);
                g.DrawLine(Pens.White, lx, ly, x, y);
                lx = x;
                ly = y;

            }

            return b;
        }


        /// <summary>
        /// unpads a bigger array, using the center as the fixed point and cuttting away the edges
        /// </summary>
        /// <param name="array"></param>
        /// <param name="NewLength"></param>
        /// <returns></returns>
        public static double[] CenterShortenArray(this double[] array, int NewLength)
        {
            double[] ArrayOut2 = new double[NewLength];
            int cc = 0;
            int Length2 = array.Length / 2 + NewLength / 2;
            for (int i = (int)(array.Length / 2 - NewLength / 2); i < Length2; i++)
            {
                ArrayOut2[cc] = array[i];
                cc++;
            }
            return ArrayOut2;
        }

        /// <summary>
        /// Pads out an array, with the zeros going on the outer edges and the array remaining in the center
        /// </summary>
        /// <param name="array"></param>
        /// <param name="NewLength"></param>
        /// <returns></returns>
        public static double[] ZeroPadArray(this double[] array, int NewLength)
        {
            double[] ArrayOut2 = new double[NewLength];
            int cc = 0;
            int Length2 = array.Length / 2 + NewLength / 2;
            for (int i = (int)(NewLength / 2 - array.Length / 2); i < Length2; i++)
            {
                ArrayOut2[i] = array[cc];
                cc++;
            }
            return ArrayOut2;
        }

        /// <summary>
        /// Pads out an array, with the zeros going on the outer edges and the array remaining in the center. only pads on the second index
        /// </summary>
        /// <param name="array"></param>
        /// <param name="NewLength"></param>
        /// <returns></returns>
        public static double[,] ZeroPadArray1D(this double[,] array, int NewLength)
        {
            double[,] ArrayOut2 = new double[2, NewLength];
            int cc = 0;
            int Length2 = array.GetLength(1) / 2 + NewLength / 2;
            for (int i = (int)(NewLength / 2 - array.GetLength(1) / 2); i < Length2; i++)
            {
                ArrayOut2[0, i] = array[0, cc];
                ArrayOut2[1, i] = array[1, cc];
                cc++;
            }
            return ArrayOut2;
        }

        /// <summary>
        /// Pads out an array, with the zeros going on the outer edges and the array remaining in the center
        /// </summary>
        /// <param name="array"></param>
        /// <param name="NewLength"></param>
        /// <returns></returns>
        public static double[,] ZeroPadArray2D(this double[,] array, int NewLength)
        {
            double[,] ArrayOut2 = new double[NewLength, NewLength];
            int cc = 0;
            int OffSetX = (NewLength - array.GetLength(0)) / 2;
            int OffSetY = (NewLength - array.GetLength(1)) / 2;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    ArrayOut2[i + OffSetX, j + OffSetY] = array[i, j];
                    cc++;
                }
            }
            return ArrayOut2;
        }



        public static double[,] ToDouble(this float[, ,] array)
        {
            double[,] arrayout = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    arrayout[i, j] = array[i, j, 0];
                }
            }
            return arrayout;
        }
        /// <summary>
        /// Pads out an array, with the zeros going on the outer edges and the array remaining in the center
        /// </summary>
        /// <param name="array"></param>
        /// <param name="NewLength"></param>
        /// <returns></returns>
        public static double[,] ZeroPadArray2D(this float[, ,] array, int NewLength)
        {
            double[,] ArrayOut2 = new double[NewLength, NewLength];
            int cc = 0;
            int OffSetX = (NewLength - array.GetLength(0)) / 2;
            int OffSetY = (NewLength - array.GetLength(1)) / 2;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    ArrayOut2[i + OffSetX, j + OffSetY] = array[i, j, 0];
                    cc++;
                }
            }
            return ArrayOut2;
        }


        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void DivideInPlaceErrorless(this double[,] array, double[,] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    try
                    {
                        array[i, j] /= Divisor[i, j];
                    }
                    catch
                    {
                        array[i, j] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// returns the sum of all the elements in an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double SumArrayCareful(this double[,] array)
        {
            double sum = 0;
            unsafe
            {
                fixed (double* pArray = array)
                {
                    double* ppArray = pArray;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (double.IsNaN(*ppArray) == false && double.IsInfinity(*ppArray) == false)
                        {
                            sum += *ppArray;
                        }
                        ppArray++;

                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// Makes the array graphable by setting up an X axis for the array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="MinX"></param>
        /// <param name="StepX"></param>
        /// <returns></returns>
        public static double[,] MakeGraphableArray(this double[] array, double MinX, double StepX)
        {
            double[,] outArray = new double[2, array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[0, i] = MinX + StepX * i;
                outArray[1, i] = array[i];
            }
            return outArray;
        }

        /// <summary>
        /// Makes the array graphable by setting up an X axis for the array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="MinX"></param>
        /// <param name="StepX"></param>
        /// <returns></returns>
        public static double[,] MakeGraphableArray(this int[] array, double MinX, double StepX)
        {
            double[,] outArray = new double[2, array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[0, i] = MinX + StepX * i;
                outArray[1, i] = array[i];
            }
            return outArray;
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[, ,] array)
        {
            double max = double.MinValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut)
                        max = *pAOut;
                    pAOut++;
                }
            }
            return max;
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MaxArray(this float[, ,] array)
        {
            float max = float.MinValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut)
                        max = *pAOut;
                    pAOut++;
                }
            }
            return max;
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float AverageArray(this float[, ,] array)
        {
            double max = 0;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    max += *pAOut;
                    pAOut++;
                }
            }
            return (float)(max / (double)(array.LongLength));
        }

        /// <summary>
        /// divides the array bu its average
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe void NormalizeArray(this float[, ,] array)
        {
            double max = 0;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    max += *pAOut;
                    pAOut++;
                }

                float ave = (float)(max / (double)(array.LongLength));

                pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    *pAOut /= ave;
                    pAOut++;
                }
            }
        }

        /// <summary>
        /// divides the array bu its average
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float  StandardDeviation(this float[, ,] array)
        {
            double max = 0;
             double std = 0;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    max += *pAOut;
                    pAOut++;
                }

                float ave = (float)(max / (double)(array.LongLength));

                pAOut = pArray;
               
                for (int i = 0; i < array.Length; i++)
                {
                    std += (*pAOut - ave) * (*pAOut - ave);
                    pAOut++;
                }

                std = Math.Sqrt(std / array.LongLength);
            }

            return (float)std;
        }

        public static unsafe void AddToArray(this float[, ,] array, float additive)
        {
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    *pAOut += additive;
                    pAOut++;
                }


            }
        }

        public static unsafe void AddToArrayUnZero(this float[, ,] array, float additive)
        {
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    if (*pAOut <= 0)
                        *pAOut = .001f;
                    else 
                        *pAOut += additive;
                    pAOut++;
                }


            }
        }

        public static unsafe void AddToArray(this float[, ,] array, float[, ,] additive)
        {
            fixed (float* pArray = array, pAdd = additive)
            {
                float* pAOut = pArray;
                float* pAIn = pAdd;
                for (int i = 0; i < array.Length; i++)
                {
                    *pAOut += (*pAIn);
                    pAOut++;
                    pAIn++;
                }
            }
        }

        public static unsafe void DivideToArray(this float[, ,] array, float[, ,] numerator, float[, ,] denominator)
        {
            fixed (float* pArray = array, pNomerator = numerator, pDenom = denominator)
            {
                float* pAOut = pArray;
                float* pNom = pNomerator;
                float* pDe = pDenom;
                for (int i = 0; i < array.Length; i++)
                {
                    *pAOut = *pNom / (*pDe);
                    pAOut++;
                    pNom++;
                    pDe++;
                }
            }
        }

        public static float[, ,] CopyThis(this float[, ,] array)
        {
            float[, ,] newArray = new float[array.GetLength(0), array.GetLength(1), array.GetLength(2)];
            Buffer.BlockCopy(array, 0, newArray, 0, Buffer.ByteLength(array));
            return newArray;

        }


        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe void EnforcePositive(this float[, ,] array)
        {

            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    if (*pAOut < 0) *pAOut = 0;
                    pAOut++;
                }


            }
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float AverageArrayWithThreshold(this float[, ,] array, float Threshold)
        {
            double max = 0;
            long CC = 0;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;
                for (int i = 0; i < array.Length; i++)
                {
                    if ((*pAOut) > Threshold)
                    {
                        max += *pAOut;
                        CC++;
                    }
                    pAOut++;
                }
            }
            CC++;
            return (float)(max / (double)(CC));
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[][,] array)
        {
            double max = double.MinValue;
            for (int l = 0; l < array.Length; l++)
                fixed (double* pArray = array[l])
                {
                    double* pAOut = pArray;
                    int Length = array[l].Length;
                    for (int i = 0; i < Length; i++)
                    {
                        if (max < *pAOut)
                            max = *pAOut;
                        pAOut++;
                    }
                }
            return max;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MinArray(this double[][,] array)
        {
            double min = double.MaxValue;
            for (int l = 0; l < array.Length; l++)
                fixed (double* pArray = array[l])
                {
                    double* pAOut = pArray;
                    int Length = array[l].Length;
                    for (int i = 0; i < Length; i++)
                    {
                        if (min > *pAOut)
                            min = *pAOut;
                        pAOut++;
                    }
                }
            return min;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MinArray(this float[][,] array)
        {
            float min = float.MaxValue;
            for (int l = 0; l < array.Length; l++)
                fixed (float* pArray = array[l])
                {
                    float* pAOut = pArray;
                    int Length = array[l].Length;
                    for (int i = 0; i < Length; i++)
                    {
                        if (min > *pAOut)
                            min = *pAOut;
                        pAOut++;
                    }
                }
            return min;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MinArray(this double[, ,] array)
        {
            double min = double.MaxValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MinArray(this float[, ,] array)
        {
            float min = float.MaxValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut && -100000 != *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MinArrayUnZero(this float[, ,] array)
        {
            float min = float.MaxValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut && *pAOut != 0) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }


        #region ArrayArithmetic
        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void AddInPlace(this double[] array, double addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue;
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] AddToArray(this double[] array, double addValue)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue;
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void SubtractInPlace(this double[] array, double addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue;
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] SubtractFromArray(this double[] array, double addValue)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue;
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void MultiplyInPlace(this double[] array, double Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant;
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] MultiplyToArray(this double[] array, double Multiplicant)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant;
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void DivideInPlace(this double[] array, double Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor;
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] DivideToArray(this double[] array, double Divisor)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor;
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void AddInPlace(this double[] array, double[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue[i];
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] AddToArray(this double[] array, double[] addValue)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue[i];
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value
        /// </summary>
        public static void SubtractInPlace(this double[] array, double[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue[i];
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] SubtractFromArray(this double[] array, double[] addValue)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue[i];
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void MultiplyInPlace(this double[] array, double[] Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant[i];
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] MultiplyToArray(this double[] array, double[] Multiplicant)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant[i];
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void DivideInPlace(this double[] array, double[] Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor[i];
            }
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static double[] DivideInPlaceErrorless(this double[] array, double[] Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                try
                {
                    array[i] /= Divisor[i];
                }
                catch
                {
                    try
                    {
                        array[i] = array[i - 1];
                    }
                    catch { }
                }
            }
            return array;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void LogInPlaceErrorless(this double[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                try
                {
                    array[i] = Math.Log(array[i]);
                }
                catch
                {
                    try
                    {
                        array[i] = array[i - 1];
                    }
                    catch { array[i] = 0; }
                }
            }
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void LogInPlaceErrorless(this double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    try
                    {
                        array[i, j] = Math.Log(array[i, j]);
                    }
                    catch
                    {
                        try
                        {
                            array[i, j] = array[i - 1, j];
                        }
                        catch { array[i, j] = 0; }
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void LogInPlaceErrorlessImage(this double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] == 0)
                    {
                        array[i, j] = 0;
                    }
                    else
                    {
                        try
                        {
                            array[i, j] = Math.Log(array[i, j]);
                        }
                        catch
                        {
                            try
                            {
                                array[i, j] = array[i - 1, j];
                            }
                            catch { array[i, j] = 0; }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[,] LogErrorless(this double[,] array)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    try
                    {
                        OutArray[i, j] = Math.Log(array[i, j]);
                    }
                    catch
                    {
                        try
                        {
                            OutArray[i, j] = array[i - 1, j];
                        }
                        catch { OutArray[i, j] = 0; }
                    }
                }
            }
            return OutArray;
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[] DivideToArray(this double[] array, double[] Divisor)
        {
            double[] OutArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor[i];
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void AddInPlace(this double[,] array, double addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue;
                }
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[,] AddToArray(this double[,] array, double addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue;
                }
            }
            return OutArray;
        }

        /// <summary>
        /// performs cast on whole array to convert from double to float
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static float[] ConvertToFloat(this double[] array)
        {
            float[] OutArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = (float)array[i];
            }
            return OutArray;
        }

        /// <summary>
        /// performs cast on whole array to convert from double to float
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static float[,] ConvertToFloat(this double[,] array)
        {
            float[,] OutArray = new float[array.GetLength(0), array.GetLength(1)];
            unsafe
            {
                fixed (float* pOut = OutArray)
                {
                    fixed (double* pIn = array)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            pOut[i] = (float)pIn[i];
                        }
                    }
                }
            }
            return OutArray;
        }

        /// <summary>
        /// performs cast on whole array to convert from double to float
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static float[, ,] ConvertToFloat(this double[, ,] array)
        {
            float[, ,] OutArray = new float[array.GetLength(0), array.GetLength(1), array.GetLength(2)];
            unsafe
            {
                fixed (float* pOut = OutArray)
                {
                    fixed (double* pIn = array)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            pOut[i] = (float)pIn[i];
                        }
                    }
                }
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void SubtractInPlace(this double[,] array, double addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue;
                }
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[,] SubtractFromArray(this double[,] array, double addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue;
                }
            }
            return OutArray;
        }

        /// <summary>
        /// Adjusts the value of the given array by the given value, there is no error checking
        /// </summary>
        public static void MultiplyInPlace(this double[,] array, double Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant;
                }
            }
        }

        /// <summary>
        /// creates a new array that is the sum of the given array and the second value
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static double[,] MultiplyToArray(this double[,] array, double Multiplicant)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant;
                }
            }
            return OutArray;
        }

        /// <summary>
        /// returns the sum of all the elements in an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double SumArray(this double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                sum += array[i];
            }
            return sum;
        }
        /// <summary>
        /// returns the sum of all the elements in an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double SumArray(this double[,] array)
        {
            double sum = 0;
            unsafe
            {
                fixed (double* pArray = array)
                {
                    double* ppArray = pArray;
                    for (int i = 0; i < array.Length; i++)
                    {
                        sum += *ppArray;
                        ppArray++;
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// returns the sum of all the elements in an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double SumArray(this double[, ,] array)
        {
            double sum = 0;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    sum += *pAOut;

                    pAOut++;
                }
            }
            return sum;
        }

        /// <summary>
        /// returns the sum of all the elements in an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double SumArray(this float[, ,] array)
        {
            double sum = 0;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    sum += *pAOut;

                    pAOut++;
                }
            }
            return sum;
        }
        /*
                /// <summary>
                /// returns the max of all the elements of an array
                /// </summary>
                /// <param name="array"></param>
                /// <returns></returns>
                public static unsafe double MaxArray(this double[, ,] array)
                {
                    double max = double.MinValue;
                    fixed (double* pArray = array)
                    {
                        double* pAOut = pArray;

                        for (int i = 0; i < array.Length; i++)
                        {
                            if (max < *pAOut)
                                max = *pAOut;
                            pAOut++;
                        }
                    }
                    return max;
                }
                /// <summary>
                /// returns the min of all the elements of an array
                /// </summary>
                /// <param name="array"></param>
                /// <returns></returns>
                public static unsafe double MinArray(this double[, ,] array)
                {
                    double min = double.MaxValue;
                    fixed (double* pArray = array)
                    {
                        double* pAOut = pArray;

                        for (int i = 0; i < array.Length; i++)
                        {
                            if (min > *pAOut) min = *pAOut;
                            pAOut++;
                        }
                    }
                    return min;
                }

                /// <summary>
                /// returns the max of all the elements of an array
                /// </summary>
                /// <param name="array"></param>
                /// <returns></returns>
                public static unsafe double MaxArray(this double[,] array)
                {
                    double max = double.MinValue;
                    fixed (double* pArray = array)
                    {
                        double* pAOut = pArray;

                        for (int i = 0; i < array.Length; i++)
                        {
                            if (max < *pAOut) max = *pAOut;
                            pAOut++;
                        }
                    }
                    return max;
                }

                /// <summary>
                /// returns the min of all the elements of an array
                /// </summary>
                /// <param name="array"></param>
                /// <returns></returns>
                public static unsafe double MinArray(this double[,] array)
                {
                    double min = double.MaxValue;
                    fixed (double* pArray = array)
                    {
                        double* pAOut = pArray;

                        for (int i = 0; i < array.Length; i++)
                        {
                            if (min > *pAOut) min = *pAOut;
                            pAOut++;
                        }
                    }
                    return min;
                }
                */
        /// <summary>
        /// returns the stdev of the array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double StandardDeviation(this double[] array)
        {
            double average = array.Average();
            double sum = 0, x = 0;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    x = ((*pAOut) - average);
                    sum += x * x;
                    pAOut++;
                }
            }

            return Math.Sqrt(sum / (array.Length - 1));
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[] array)
        {
            double max = double.MinValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut) max = *pAOut;
                    pAOut++;
                }
            }
            return max;
        }
        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MinArray(this double[] array)
        {
            double min = double.MaxValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }


        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[,] array)
        {
            double max = double.MinValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut) max = *pAOut;
                    pAOut++;
                }
            }
            return max;
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[,] array, out int X, out int Y)
        {
            double max = double.MinValue;
            long MaxI = 0;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut)
                    {
                        max = *pAOut;
                        MaxI = i;
                    }
                    pAOut++;
                }
            }

            Y = (int)(MaxI / (double)array.GetLength(0));
            X = (int)(MaxI % array.GetLength(0));

            return max;
        }

        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MaxArray(this double[] array, out int X)
        {
            double max = double.MinValue;
            long MaxI = 0;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut)
                    {
                        max = *pAOut;
                        MaxI = i;
                    }
                    pAOut++;
                }
            }
            X = (int)(MaxI % array.Length);
            return max;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MinArray(this float[] array)
        {
            float min = float.MaxValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }


        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MaxArray(this float[,] array)
        {
            float max = float.MinValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (max < *pAOut) max = *pAOut;
                    pAOut++;
                }
            }
            return max;
        }


        /// <summary>
        /// returns the max of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        /* public static unsafe float MaxArray(this float[, ,] array)
         {
             float max = float.MinValue;
             fixed (float* pArray = array)
             {
                 float* pAOut = pArray;

                 for (int i = 0; i < array.Length; i++)
                 {
                     if (max < *pAOut) max = *pAOut;
                     pAOut++;
                 }
             }
             return max;
         }

         /// <summary>
         /// returns the max of all the elements of an array
         /// </summary>
         /// <param name="array"></param>
         /// <returns></returns>
         public static unsafe float MinArray(this float[, ,] array)
         {
             float min = float.MaxValue;
             fixed (float* pArray = array)
             {
                 float* pAOut = pArray;

                 for (int i = 0; i < array.Length; i++)
                 {
                     if (min > *pAOut) min = *pAOut;
                     pAOut++;
                 }
             }
             return min;
         }*/

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe double MinArray(this double[,] array)
        {
            double min = double.MaxValue;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }

        /// <summary>
        /// returns the min of all the elements of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static unsafe float MinArray(this float[,] array)
        {
            float min = float.MaxValue;
            fixed (float* pArray = array)
            {
                float* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    if (min > *pAOut) min = *pAOut;
                    pAOut++;
                }
            }
            return min;
        }


        /// <summary>
        /// Scales an array while keeping the same average
        /// </summary>
        /// <param name="array"></param>
        /// <param name="ScaleFactor"></param>
        /// <returns></returns>
        public static unsafe double[] RescaleArrayInPlace(this double[] array, double ScaleFactor)
        {
            double average = array.Average();
            double x;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    x = ((*pAOut) - average) * ScaleFactor;
                    *pAOut = x + average;
                    pAOut++;
                }
            }
            return array;
        }


        public static double AverageArray(this double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                sum += array[i];
            }
            return sum / (double)array.Length;
        }

        public static double AverageArray(this double[,] array)
        {
            double sum = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    sum += array[i, j];
                }
            }
            return sum / (double)array.Length;
        }

        public static double AverageArray(this Int32[,] array)
        {
            double sum = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    sum += array[i, j];
                }
            }
            return sum / (double)array.Length;
        }
        public static double AverageArray(this byte[,] array)
        {
            double sum = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    sum += array[i, j];
                }
            }
            return sum / (double)array.Length;
        }

        public static unsafe double AverageArray(this double[, ,] array)
        {
            double sum = 0;
            fixed (double* pArray = array)
            {
                double* pAOut = pArray;

                for (int i = 0; i < array.Length; i++)
                {
                    sum += *pAOut;
                    pAOut++;
                }
            }
            return sum / (double)array.Length;
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void DivideInPlace(this double[,] array, double Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor;
                }
            }
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void DivideInPlace(this float[, ,] array, float Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                        array[i, j, k] /= Divisor;
                }
            }
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void MultiplyInPlace(this float[, ,] array, float mult)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                        array[i, j, k] *= mult;
                }
            }
        }

        public static void MultiplyInPlace(this float[, ,] array, float[, ,] mult)
        {

            unsafe
            {
                fixed (float* pIn = array)
                {
                    fixed (float* pMul = mult)
                    {
                        float* PIn = pIn, pMult = pMul;
                        for (int i = 0; i < array.Length; i++)
                        {
                            *PIn *= *pMult;
                            PIn++;
                            pMult++;
                        }
                    }
                }
            }

        }

        public static void DivideInPlace(this float[, ,] array, float[, ,] div)
        {

            unsafe
            {
                fixed (float* pIn = array)
                {
                    fixed (float* pMul = div)
                    {
                        float* PIn = pIn, pMult = pMul;
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (*pMul != 0)
                                *PIn /= *pMult;
                            else
                                *PIn = 0;
                            PIn++;
                            pMult++;
                        }
                    }
                }
            }

        }

        public static void SubtractInPlace(this float[, ,] array, float[, ,] sub)
        {

            unsafe
            {
                fixed (float* pIn = array)
                {
                    fixed (float* pf_Sub = sub)
                    {
                        float* PIn = pIn, pSub = pf_Sub;
                        for (int i = 0; i < array.Length; i++)
                        {
                            *PIn -= *pSub;
                            PIn++;
                            pSub++;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// this subtracts sub array from array and puts the result in resultant, which must be already created
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sub"></param>
        /// <param name="resultant"></param>
        public static void SubtractFrom_To(this float[, ,] array, float[, ,] sub, float[, ,] resultant)
        {

            unsafe
            {
                fixed (float* pIn = array, pf_Sub = sub, pf_Result = resultant)
                {
                    float* PIn = pIn, pSub = pf_Sub;
                    float* pREsult = pf_Result;

                    for (int i = 0; i < array.Length; i++)
                    {
                        *pREsult = *PIn - *pSub;
                        PIn++;
                        pSub++;
                        pREsult++;
                    }

                }
            }

        }

        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] DivideToArray(this double[,] array, double Divisor)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            unsafe
            {
                fixed (double* pIn = array)
                {
                    fixed (double* pOut = OutArray)
                    {
                        double* PIn = pIn, POut = pOut;
                        for (int i = 0; i < array.Length; i++)
                        {
                            *PIn = (*POut) / Divisor;
                            PIn++;
                            POut++;
                        }
                    }
                }
            }
            return OutArray;
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static double[, ,] DivideToArray(this double[, ,] array, double Divisor)
        {
            double[, ,] OutArray = new double[array.GetLength(0), array.GetLength(1), array.GetLength(2)];
            unsafe
            {
                fixed (double* pIn = array)
                {
                    fixed (double* pOut = OutArray)
                    {
                        double* PIn = pIn, POut = pOut;
                        for (int i = 0; i < array.Length; i++)
                        {
                            *POut = (*PIn) / Divisor;
                            PIn++;
                            POut++;
                        }
                    }
                }
            }
            return OutArray;
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void AddInPlace(this double[,] array, double[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i, j];
                }
            }
        }

        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] AddToArray(this double[,] array, double[,] addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i, j];
                }
            }
            return OutArray;
        }

        public static void SubtractInPlace(this double[,] array, double[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i, j];
                }
            }
        }

        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] SubtractFromArray(this double[,] array, double[,] addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void MultiplyInPlace(this double[,] array, double[,] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] MultiplyToArray(this double[,] array, double[,] Multiplicant)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void DivideInPlace(this double[,] array, double[,] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] DivideToArray(this double[,] array, double[,] Divisor)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i, j];
                }
            }
            return OutArray;
        }

        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void AddInPlace(this double[,] array, double[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] AddToArray(this double[,] array, double[] addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void SubtractInPlace(this double[,] array, double[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] SubtractFromArray(this double[,] array, double[] addValue)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void MultiplyInPlace(this double[,] array, double[] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] MultiplyToArray(this double[,] array, double[] Multiplicant)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// performs a operation on every elements in the array, no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        public static void DivideInPlace(this double[,] array, double[] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array using the original array and the value.  each element is considered individually. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Divisor"></param>
        /// <returns></returns>
        public static double[,] DivideToArray(this double[,] array, double[] Divisor)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i];
                }
            }
            return OutArray;
        }
        #endregion

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j] && ImageArray[i, j] != 0) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }


        public static void SaveBinary(this double[,] ImageArray, string filename)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);

            FileStream BinaryFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BinaryWriter Writer = new BinaryWriter(BinaryFile);

            for (int y = 0; y < iHeight; y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    Writer.Write((double)(ImageArray[x, y]));
                }
            }


        }


        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap24FlipHorizonal(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            int fWidth = iWidth - 1;
            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = (byte*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[fWidth - x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline += 3;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap24(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = (byte*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline += 3;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// Uses the corners to set the min of the bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmapCorner(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }

            iMin = ImageArray[5, 5];

            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this double[,] ImageArray, double MinContrast, double MaxContrast)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = MaxContrast;
            double iMin = MinContrast;
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this float[,] ImageArray, float MinContrast, float MaxContrast)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = MaxContrast;
            double iMin = MinContrast;
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this float[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }


        public static float[, ,] InsideSphere(this float[, ,] data)
        {
            int nSize = (int)(data.GetLength(0) / Math.Sqrt(2) * .9);
            float[, ,] outArray = new float[nSize, nSize, nSize];

            int StartI = (int)Math.Floor((data.GetLength(0) - nSize) / 2d);

            for (int z = 0; z < nSize; z++)
                for (int y = 0; y < nSize; y++)
                    for (int x = 0; x < nSize; x++)
                        outArray[z, y, x] = data[z + StartI, y + StartI, x + StartI];
            return outArray;
        }
    }
}
