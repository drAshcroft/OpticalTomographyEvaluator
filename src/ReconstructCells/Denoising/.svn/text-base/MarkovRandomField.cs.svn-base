using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLibrary;
using System.Threading.Tasks;

namespace ReconstructCells.Denoising
{
    public class MarkovRandomField
    {
        float[, ,] Data;
        double Covar;
        float Max_Diff;
        double Weight_Diff;
        double V_max;

        int height, width, depth;

        private float[, ,] Buffer1;
        private float[, ,] Buffer2;

        private float[, ,] buffSrc, buffDest;//used at pointers to buffer1 and buffer2 to be able to switch them easily

        private float min(float a, float b)
        {

            return a < b ? a : b;
        }

        private void BatchCleanSlice(int X)
        {

            //% Vary each pixel individually to find the
            // % values that minimise the local potentials.

           // for (int X = 0; X < width; X++)
            {
                float srcVal, V_local, V_data, V_diff, V_current, min_val, v;
                for (int Y = 0; Y < height; Y++)
                {
                    for (int Z = 0; Z < depth; Z++)
                    {
                        srcVal = Data[X, Y, Z];
                        V_local = (float)V_max;
                        min_val = -1;
                        for (float val = srcVal - 5000; val < srcVal + 5000; val += 1000)
                        {
                            //  % The component of the potential due to the known data.
                            V_data = (float)(Math.Pow(val - srcVal, 2) / (2 * Covar));

                            // % The component of the potential due to the
                            // % difference between neighbouring pixel values.
                            V_diff = 0;
                            if (X > 1)
                            {
                                v = min((val - buffSrc[X - 1, Y, Z]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }

                            if (X+1 < width)
                            {
                                v = min((val - buffSrc[X + 1, Y, Z]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }

                            if (Y > 1)
                            {
                                v = min((val - buffSrc[X, Y - 1, Z]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }

                            if (Y+1 < height)
                            {
                                v = min((val - buffSrc[X, Y + 1, Z]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }

                            if (Z > 1)
                            {
                                v = min((val - buffSrc[X, Y, Z - 1]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }

                            if (Z+1 < depth)
                            {
                                v = min((val - buffSrc[X, Y, Z + 1]), Max_Diff);
                                V_diff = V_diff + v * v;
                            }


                            V_current = (float)(V_data + Weight_Diff * V_diff);

                            if (V_current < V_local)
                            {
                                min_val = val;
                                V_local = V_current;
                            }
                        }

                        buffDest[X, Y, Z] = (srcVal + 3 * min_val) / 4;
                    }
                }
            }
        }

        public float[, ,] CleanSinogram(float[, ,] data, double covar, double max_Diff, double weight_Diff, int iterations)
        {
            Data = data;
            Covar = covar;
            Max_Diff = (float)Math.Sqrt(max_Diff);
            Weight_Diff = weight_Diff / 6;

            /* % Use ICM to remove the noise from the given image.
 % * covar is the known covariance of the Gaussian noise.
 % * max_diff is the maximum contribution to the potential
 %   of the difference between two neighbouring pixel values.
 % * weight_diff is the weighting attached to the component of the potential
 %   due to the difference between two neighbouring pixel values.
 % * iterations is the number of iterations to perform.

 function dst = restore_volume_MRF(src, covar, max_diff, weightdiff, iterations)*/
            Buffer1 = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];
            Buffer2 = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];

            Buffer.BlockCopy(data, 0, Buffer1, 0, Buffer.ByteLength(data));

            buffSrc = Buffer1;
            buffDest = Buffer2;

            //% This value is guaranteed to be larger than the
            //% potential of any configuration of pixel values.
            double mVal = data.MaxArray();
            V_max = (data.GetLength(0) * data.GetLength(0)) * (mVal * mVal / (2 * covar) + 4 * Weight_Diff * Weight_Diff);

            width = data.GetLength(0);
            height = data.GetLength(1);
            depth = data.GetLength(2);

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = width;

            for (int i = 0; i < iterations; i++)
            {
                Parallel.For(0, numberOfImages, po, x => BatchCleanSlice(x));

                float[,,] temp = buffDest;
                buffDest = buffSrc;
                buffSrc = temp;
            }

            return buffSrc;
        }
    }
}
