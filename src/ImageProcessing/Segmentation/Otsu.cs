using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLibrary;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageProcessing.Segmentation
{
    public class Otsu
    {
         const int NGRAY = 256;

        public static float[, ,] MultiOtsu(float[, ,] volume, int nClasses)
        {
            int MLEVEL = nClasses; // 3 level

            float stepValue = volume.MaxArray() / 255;

            int[] threshold = new int[MLEVEL]; // threshold
            int width = volume.GetLength(2);
            int height = volume.GetLength(1);
            int depth = volume.GetLength(0);
            ////////////////////////////////////////////
            // Build Histogram
            ////////////////////////////////////////////
            float[] h = new float[NGRAY];
            buildHistogram(ref h, volume, stepValue);

            /////////////////////////////////////////////
            // Build lookup tables from h
            ////////////////////////////////////////////
            float[][] P = new float[NGRAY][];
            float[][] S = new float[NGRAY][];
            float[][] H = new float[NGRAY][];

            for (int i = 0; i < NGRAY; i++)
            {
                P[i] = new float[NGRAY];
                S[i] = new float[NGRAY];
                H[i] = new float[NGRAY];
            }

            buildLookupTables(ref P, ref S, ref  H, h);

            ////////////////////////////////////////////////////////
            // now M level loop   MLEVEL dependent term
            ////////////////////////////////////////////////////////
            float maxSig = findMaxSigma(MLEVEL, H, ref  threshold);

            return showRegions(MLEVEL, threshold, volume,stepValue);
        }

        private static void buildHistogram(ref float[] h, float[, ,] pixels, float stepValue)
        {
            int width = pixels.GetLength(2);
            int height = pixels.GetLength(1);
            int depth = pixels.GetLength(0);

            // note Java byte is signed. in order to make it 0 to 255 you have to
            // do int pix = 0xff & pixels[i];

            for (int x = 0; x < depth; x++)
                for (int y = 0; y < height; y++)
                    for (int z = 0; z < width; z++)
                        h[(int)Math.Floor(pixels[x, y, z] / stepValue)]++;

            // note the probability of grey i is h[i]/(width*height)
            float[] bin = new float[NGRAY];
            float hmax = 0.0f;
            for (int i = 0; i < NGRAY; ++i)
            {
                bin[i] = (float)i;
                h[i] /= ((float)(width * height * depth));
                if (hmax < h[i])
                    hmax = h[i];
            }
        }

        private static void buildLookupTables(ref float[][] P, ref  float[][] S, ref  float[][] H, float[] h)
        {
            // initialize
            for (int j = 0; j < NGRAY; j++)
                for (int i = 0; i < NGRAY; ++i)
                {
                    P[i][j] = 0.0f;
                    S[i][j] = 0.0f;
                    H[i][j] = 0.0f;
                }
            // diagonal 
            for (int i = 1; i < NGRAY; ++i)
            {
                P[i][i] = h[i];
                S[i][i] = ((float)i) * h[i];
            }
            // calculate first row (row 0 is all zero)
            for (int i = 1; i < NGRAY - 1; ++i)
            {
                P[1][i + 1] = P[1][i] + h[i + 1];
                S[1][i + 1] = S[1][i] + ((float)(i + 1)) * h[i + 1];
            }
            // using row 1 to calculate others
            for (int i = 2; i < NGRAY; i++)
                for (int j = i + 1; j < NGRAY; j++)
                {
                    P[i][j] = P[1][j] - P[1][i - 1];
                    S[i][j] = S[1][j] - S[1][i - 1];
                }
            // now calculate H[i][j]
            for (int i = 1; i < NGRAY; ++i)
                for (int j = i + 1; j < NGRAY; j++)
                {
                    if (P[i][j] != 0)
                        H[i][j] = (S[i][j] * S[i][j]) / P[i][j];
                    else
                        H[i][j] = 0.0f;
                }

        }

        private static float findMaxSigma(int mlevel, float[][] H, ref  int[] t)
        {
            t[0] = 0;
            float maxSig = 0.0f;
            switch (mlevel)
            {
                case 2:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                    {
                        float Sq = H[1][i] + H[i + 1][255];
                        if (maxSig < Sq)
                        {
                            t[1] = i;
                            maxSig = Sq;
                        }
                    }
                    break;
                case 3:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                        {
                            float Sq = H[1][i] + H[i + 1][j] + H[j + 1][255];
                            if (maxSig < Sq)
                            {
                                t[1] = i;
                                t[2] = j;
                                maxSig = Sq;
                            }
                        }
                    break;
                case 4:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                            for (int k = j + 1; k < NGRAY - mlevel + 2; k++) // t3
                            {
                                float Sq = H[1][i] + H[i + 1][j] + H[j + 1][k] + H[k + 1][255];
                                if (maxSig < Sq)
                                {
                                    t[1] = i;
                                    t[2] = j;
                                    t[3] = k;
                                    maxSig = Sq;
                                }
                            }
                    break;
                case 5:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                            for (int k = j + 1; k < NGRAY - mlevel + 2; k++) // t3
                                for (int m = k + 1; m < NGRAY - mlevel + 3; m++) // t4
                                {
                                    float Sq = H[1][i] + H[i + 1][j] + H[j + 1][k] + H[k + 1][m] + H[m + 1][255];
                                    if (maxSig < Sq)
                                    {
                                        t[1] = i;
                                        t[2] = j;
                                        t[3] = k;
                                        t[4] = m;
                                        maxSig = Sq;
                                    }
                                }
                    break;
            }
            return maxSig;
        }

        private static float[, ,] showRegions(int mlevel, int[] t, float[, ,] pixels, float stepValue)
        {

            float[, ,] regions = new float[pixels.GetLength(0), pixels.GetLength(1), pixels.GetLength(2)];

            int width = pixels.GetLength(2);
            int height = pixels.GetLength(1);
            int depth = pixels.GetLength(0);

            for (int x = 0; x < depth; x++)
                for (int y = 0; y < height; y++)
                    for (int z = 0; z < width; z++)
                    {
                        int val = (int)(pixels[x, y, z] / stepValue);

                        for (int k = 0; k < mlevel; k++)
                        {
                            if (k < mlevel - 1)
                            {
                                if (val < t[k + 1] && val > t[k]) // k-1 region
                                    regions[x, y, z] = k;
                            }
                            else // k= mlevel-1 last region
                            {
                                if (val > t[k])
                                    regions[x, y, z] = k;
                            }
                        }
                    }

            return regions;
        }



        public static Image<Gray, float> MultiOtsu(Image<Gray, float> image, int nClasses)
        {
            int MLEVEL = nClasses; // 3 level

            float stepValue = image.Data.MaxArray() / 255;

            int[] threshold = new int[MLEVEL]; // threshold
            int width = image.Data.GetLength(2);
            int height = image.Data.GetLength(1);
         
            ////////////////////////////////////////////
            // Build Histogram
            ////////////////////////////////////////////
            float[] h = new float[NGRAY];
            buildHistogram(ref h, image.Data, stepValue);

            /////////////////////////////////////////////
            // Build lookup tables from h
            ////////////////////////////////////////////
            float[][] P = new float[NGRAY][];
            float[][] S = new float[NGRAY][];
            float[][] H = new float[NGRAY][];

            for (int i = 0; i < NGRAY; i++)
            {
                P[i] = new float[NGRAY];
                S[i] = new float[NGRAY];
                H[i] = new float[NGRAY];
            }

            buildLookupTables(ref P, ref S, ref  H, h);

            ////////////////////////////////////////////////////////
            // now M level loop   MLEVEL dependent term
            ////////////////////////////////////////////////////////
            float maxSig = findMaxSigma(MLEVEL, H, ref  threshold);

           return new Image<Gray,float> ( showRegions(MLEVEL, threshold, image.Data, stepValue) );
        }

    }
}

