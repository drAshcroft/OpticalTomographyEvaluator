using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLibrary;
using System.Threading.Tasks;

namespace ReconstructCells.Denoising
{
    public class MRFVolume
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
            float srcVal, V_local, V_data, V_diff, V_current, min_val, v;
            for (int Y = 0; Y < height; Y++)
            {
                for (int Z = 0; Z < depth; Z++)
                {
                    srcVal = Data[X, Y, Z];
                    if (srcVal != 0)
                    {
                        V_local = (float)V_max;
                        min_val = -1;
                        for (float val = srcVal - 2000; val < srcVal + 2000; val += 100)
                        {
                            //  % The component of the potential due to the known data.
                            v = val - srcVal;
                            V_data = (float)(v * v / (2 * Covar));

                            // % The component of the potential due to the
                            // % difference between neighbouring pixel values.
                            V_diff = 0;
                            if (X > 1)
                            {
                                v=(val - buffSrc[X - 1, Y, Z]);
                                v = min(v*v, Max_Diff);
                                V_diff = V_diff + v ;
                            }

                            if (X + 1 < width)
                            {
                                v=(val - buffSrc[X + 1, Y, Z]);
                                v = min(v*v, Max_Diff);
                                V_diff = V_diff + v;
                            }

                            if (Y > 1)
                            {
                                v=(val - buffSrc[X, Y - 1, Z]);
                                v = min(v*v, Max_Diff);
                                V_diff = V_diff + v ;
                            }

                            if (Y + 1 < height)
                            {
                                v=(val - buffSrc[X, Y + 1, Z]);
                                v = min(v*v, Max_Diff);
                                V_diff = V_diff + v ;
                            }

                            if (Z > 1)
                            {
                                v=(val - buffSrc[X, Y, Z - 1]);
                                v = min(v*v, Max_Diff);
                                V_diff = V_diff + v ;
                            }

                            if (Z + 1 < depth)
                            {
                                v=(val - buffSrc[X, Y, Z + 1]);
                                v = min(v*v , Max_Diff);
                                V_diff = V_diff + v ;
                            }


                            V_current = (float)(V_data + Weight_Diff * V_diff);

                            if (V_current < V_local)
                            {
                                min_val = val;
                                V_local = V_current;
                            }
                        }

                        buffDest[X, Y, Z] = (srcVal + min_val) / 2;
                    }
                }
            }

        }

        public OnDemandImageLibrary CleanSinogram(OnDemandImageLibrary library, double covar, double max_Diff, double weight_Diff, int iterations)
        {
            Data = new float[library.Count, library[10].Width, library[10].Height];

            Covar = covar;
            Max_Diff = (float)(max_Diff);
            Weight_Diff = weight_Diff / 6;

            /* % Use ICM to remove the noise from the given image.
 % * covar is the known covariance of the Gaussian noise.
 % * max_diff is the maximum contribution to the potential
 %   of the difference between two neighbouring pixel values.
 % * weight_diff is the weighting attached to the component of the potential
 %   due to the difference between two neighbouring pixel values.
 % * iterations is the number of iterations to perform.

 function dst = restore_volume_MRF(src, covar, max_diff, weightdiff, iterations)*/

            for (int i = 0; i < library.Count; i++)
            {
                Buffer.BlockCopy(library[i].Data, 0, Data, i * library[0].Width * library[0].Height * sizeof(float), Buffer.ByteLength(library[i].Data));
            }

            Buffer1 = new float[Data.GetLength(0), Data.GetLength(1), Data.GetLength(2)];
            Buffer2 = new float[Data.GetLength(0), Data.GetLength(1), Data.GetLength(2)];

            Buffer.BlockCopy(Data, 0, Buffer1, 0, Buffer.ByteLength(Data));

            buffSrc = Buffer1;
            buffDest = Buffer2;

            //% This value is guaranteed to be larger than the
            //% potential of any configuration of pixel values.
            double mVal = Data.MaxArray();
            V_max = (Data.GetLength(0) * Data.GetLength(0)) * (mVal * mVal / (2 * covar) + 4 * Weight_Diff * Weight_Diff);

            width = Data.GetLength(0);
            height = Data.GetLength(1);
            depth = Data.GetLength(2);

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = width;

            for (int i = 0; i < iterations; i++)
            {
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCleanSlice(x));
                float[, ,] temp = buffDest;
                buffDest = buffSrc;
                buffSrc = temp;
            }

            for (int i = 0; i < library.Count; i++)
            {
                Buffer.BlockCopy(Data, i * library[0].Width * library[0].Height * sizeof(float), library[i].Data, 0, Buffer.ByteLength(library[i].Data));
            }

            return library;
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

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = width;

            for (int i = 0; i < iterations; i++)
            {
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCleanSlice(x));

                float[, ,] temp = buffDest;
                buffDest = buffSrc;
                buffSrc = temp;
            }

            return buffSrc;
        }
    }
}
