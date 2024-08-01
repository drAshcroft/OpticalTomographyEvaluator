using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageProcessing._3D
{
    public static class Filters3D
    {
        public static float[, ,] GuassianBlur(this float[, ,] data)
        {
            float[, ,] newArray = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];

            float[, ,] kernal = new float[3, 3, 3];

            double d;
            double sum = 0;
            for (int i = 0; i < kernal.GetLength(0) ; i++)
                for (int j = 0; j < kernal.GetLength(1) ; j++)
                    for (int k = 0; k < kernal.GetLength(2) ; k++)
                    {
                        d = Math.Exp(-1 * ((i - 1f) * (i - 1f) + (j - 1f) * (j - 1f) + (k - 1f) * (k - 1f)) / 2 / 1.15); ;
                        kernal[i, j, k] = (float)d;
                        sum += d;
                    }
            float f = (float)(1 / sum);
            for (int i = 1; i < kernal.GetLength(0) - 1; i++)
                for (int j = 1; j < kernal.GetLength(1) - 1; j++)
                    for (int k = 1; k < kernal.GetLength(2) - 1; k++)
                        kernal[i, j, k] *= f;


            unchecked
            {
                for (int i = 1; i < data.GetLength(0) - 1; i++)
                    for (int j = 1; j < data.GetLength(1) - 1; j++)
                        for (int k = 1; k < data.GetLength(2) - 1; k++)
                        {
                            f = 0;
                            int ii = 0, jj = 0, kk = 0;

                            for (int x = i - 1; x < i + 1; x++)
                            {
                                jj = 0;
                                for (int y = j - 1; y < j + 1; y++)
                                {
                                    kk = 0;
                                    for (int z = k - 1; z < k + 1; z++)
                                    {
                                        f += data[x, y, z] * kernal[ii, jj, kk];
                                        kk++;
                                    }
                                    jj++;
                                }
                                ii++;
                            }
                            newArray[i, j, k] = f;
                        }
            }

            return newArray;
        }
        public static float[, ,] GuassianBlur555(this float[, ,] data)
        {
            float[, ,] newArray = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];

            float[, ,] kernal = new float[5,5,5];

            double d;
            double sum = 0;
            for (int i = 0; i < kernal.GetLength(0); i++)
                for (int j = 0; j < kernal.GetLength(1); j++)
                    for (int k = 0; k < kernal.GetLength(2); k++)
                    {
                        d = Math.Exp(-1 * ((i - 3f) * (i - 3f) + (j - 3f) * (j - 3f) + (k - 3f) * (k - 3f)) / 2f /1.4f); ;
                        kernal[i, j, k] = (float)d;
                        sum += d;
                    }
            float f = (float)(1 / sum);
            for (int i = 1; i < kernal.GetLength(0) - 1; i++)
                for (int j = 1; j < kernal.GetLength(1) - 1; j++)
                    for (int k = 1; k < kernal.GetLength(2) - 1; k++)
                        kernal[i, j, k] *= f;


            unchecked
            {
                for (int i = 2; i < data.GetLength(0) - 2; i++)
                    for (int j = 2; j < data.GetLength(1) - 2; j++)
                        for (int k = 2; k < data.GetLength(2) - 2; k++)
                        {
                            f = 0;
                            int ii = 0, jj = 0, kk = 0;

                            for (int x = i - 2; x < i + 2; x++)
                            {
                                jj = 0;
                                for (int y = j - 2; y < j + 2; y++)
                                {
                                    kk = 0;
                                    for (int z = k - 2; z < k + 2; z++)
                                    {
                                        f += data[x, y, z] * kernal[ii, jj, kk];
                                        kk++;
                                    }
                                    jj++;
                                }
                                ii++;
                            }
                            newArray[i, j, k] = f;
                        }
            }

            return newArray;
        }
    }
}
