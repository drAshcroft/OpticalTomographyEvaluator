using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathLibrary;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ReconstructCells.Denoising
{
    public class MedianVolume
    {
        float[, ,] Data;

       
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
            X = X + 1;
            float[] vals = new float[7];
            List<float> lvals = new List<float>(vals);
            for (int Y = 1; Y < height; Y++)
            {
                for (int Z = 1; Z < depth; Z++)
                {

                    lvals[0] = buffSrc[X, Y, Z];
                    lvals[1] = buffSrc[X - 1, Y, Z];
                    lvals[2] = buffSrc[X + 1, Y, Z];
                    lvals[3] = buffSrc[X, Y - 1, Z];
                    lvals[4] = buffSrc[X, Y + 1, Z];
                    lvals[5] = buffSrc[X, Y, Z - 1];
                    lvals[6] = buffSrc[X, Y, Z + 1];

                    lvals.Sort();

                    buffDest[X, Y, Z] = lvals[3];
                }
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="iterations"></param>
        /// <param name="despeckle">check for salt and pepper noise and clean</param>
        /// <param name="maxDiff">only used if despeckle has been selected</param>
        /// <returns></returns>
        public float[, ,] CleanSinogram(float[, ,] data, int iterations)
        {


            Data = data;

            Buffer1 = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];
            Buffer2 = new float[data.GetLength(0), data.GetLength(1), data.GetLength(2)];

            Buffer.BlockCopy(data, 0, Buffer1, 0, Buffer.ByteLength(data));

            buffSrc = Buffer1;
            buffDest = Buffer2;

            //% This value is guaranteed to be larger than the
            //% potential of any configuration of pixel values.
            double mVal = data.MaxArray();

            width = data.GetLength(0) - 1;
            height = data.GetLength(1) - 1;
            depth = data.GetLength(2) - 1;

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = width - 1;

            for (int i = 0; i < iterations; i++)
            {

                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCleanSlice(x));

                float[, ,] temp = buffDest;
                buffDest = buffSrc;
                buffSrc = temp;
            }

            return buffSrc;
        }

        public OnDemandImageLibrary CleanSinogram(OnDemandImageLibrary lib, int iterations)
        {


            var data = lib[10].Data;
            Buffer1 = new float[lib.Count, data.GetLength(0), data.GetLength(1)];
            Buffer2 = new float[lib.Count, data.GetLength(0), data.GetLength(1)];

            for (int i = 0; i < lib.Count; i++)
            {
                data = lib[i].Data;
                Buffer.BlockCopy(data, 0, Buffer1, Buffer.ByteLength(data) * i, Buffer.ByteLength(data));
            }

            Image<Gray, float> test = new Image<Gray, float>(data.GetLength(0), data.GetLength(1));
            for (int i = 0; i < test.Width; i++)
                for (int j = 0; j < test.Height; j++)
                {
                    test.Data[j, i, 0] = Buffer1[20, i, j];
                }

            buffSrc = Buffer1;
            buffDest = Buffer2;

            //% This value is guaranteed to be larger than the
            //% potential of any configuration of pixel values.
            double mVal = data.MaxArray();

            width = lib.Count - 1;
            height = data.GetLength(0) - 1;
            depth = data.GetLength(1) - 1;

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = width - 1;

            for (int i = 0; i < iterations; i++)
            {
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchCleanSlice(x));

                float[, ,] temp = buffDest;
                buffDest = buffSrc;
                buffSrc = temp;
            }


            //Image<Gray, float> test = new Image<Gray, float>(data.GetLength(0), data.GetLength(1));
            for (int i = 0; i < test.Width; i++)
                for (int j = 0; j < test.Height; j++)
                {
                    test.Data[j, i, 0] = buffSrc[20, i, j];
                }

            for (int i = 0; i < lib.Count; i++)
            {
                data = lib[i].Data;
                Buffer.BlockCopy(buffSrc, Buffer.ByteLength(data) * i, data, 0, Buffer.ByteLength(data));
            }


            return lib;
        }
    }
}
