using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageProcessing._3D
{
    public static class ScrubVolume
    {

        public static void ScrubReconVolumeCloseCut(this  float[, ,] Data, int nProjections)
        {
            double CutValue = -500;

            CutValue = GetBoundaryIntensity(ref Data);

            float cValue = (float)CutValue * 2;
            NormalizeFBPVolume(ref Data, nProjections, true, cValue);
        }


        public static void ScrubReconVolume(this  float[, ,] Data, int nProjections)
        {
            double CutValue = -500;

            CutValue = RemoveSphere(ref Data);

            float cValue = (float)CutValue ;
           // NormalizeFBPVolume(ref Data, nProjections, true, cValue);
        }

        public static double GetBoundaryIntensity(ref float[, ,] Data)
        {


            double min = double.MaxValue;
            float val;
            float sum = 0, count = 0;
            Random rnd = new Random();
            List<float> medianList = new List<float>(1000);
            for (int i = 0; i < Data.GetLength(0); i++)
                for (int j = 0; j < Data.GetLength(1); j++)
                    for (int k = 0; k < Data.GetLength(2); k++)
                    {
                        if (i > 10 || j > 10 || k > 10)
                            if (i < 20 || j < 20 || k < 20)
                            {
                                val = Data[i, j, k];
                                if (val < min) min = val;
                                sum += val;
                                if (val > 0 && medianList.Count < 1000 && rnd.NextDouble() < .01)
                                    medianList.Add(val);
                                count++;
                            }
                    }

            sum /= count;

            if (medianList.Count > 0)
            {
                medianList.Sort();
                sum = medianList[(int)(medianList.Count * 5f / 8f)];
            }

            if (sum < -500) sum = -500;

            return sum * 2;
        }


        public static double RemoveSphere(ref float[, ,] Data)
        {
            double RX = Data.GetLength(0) / 2d - 10;
            double RY = Data.GetLength(1) / 2d - 10;
            double RZ = Data.GetLength(2) / 2d - 10;
            double HalfI = Data.GetLength(0) / 2d;
            double HalfJ = Data.GetLength(1) / 2d;
            double HalfK = Data.GetLength(2) / 2d;

            double x;
            double y;
            double z;

            double min = double.MaxValue;
            float val;
            float sum = 0, count = 0;
            Random rnd = new Random();
            List<float> medianList = new List<float>(1000);
            for (int i = 0; i < Data.GetLength(0); i++)
                for (int j = 0; j < Data.GetLength(1); j++)
                    for (int k = 0; k < Data.GetLength(2); k++)
                    {
                        x = (i - HalfI);
                        y = RX * (j - HalfJ) / RY;
                        z = RX * (k - HalfK) / RZ;
                        if (RX - (x * x + y * y) < 2)
                        {
                            val = Data[i, j, k];
                            if (val < min) min = val;
                            sum += val;
                            if (val > 0 && medianList.Count < 1000 && rnd.NextDouble() < .01)
                                medianList.Add(val);
                            count++;
                        }
                    }


            sum /= count;

            if (medianList.Count > 0)
            {
                medianList.Sort();
                sum = medianList[(int)(medianList.Count * 5f / 8f)];
            }
            // sum /= count;
            if (sum < -500) sum = -500;
            // sum = min;
            // sum = 0;
            //  double sum = -500;


            for (int i = 0; i < Data.GetLength(0); i++)
                for (int j = 0; j < Data.GetLength(1); j++)
                    for (int k = 0; k < Data.GetLength(2); k++)
                    {
                        x = (i - HalfI) / RX;
                        y = (j - HalfJ) / RY;
                        z = (k - HalfK) / RZ;
                        if ((x * x + y * y + z * z) > 1)
                        {
                            Data[i, j, k] = sum;
                        }
                    }

            return sum * 2;
        }

        public static void NormalizeFBPVolume(ref float[, ,] SourceImage, int nProjections, bool ClipZeros, float CutValue)
        {
            // if (nProjections != 500)
            {
                float factor = nProjections / 500f;// *SourceImage.GetLength(0);
               
              
                unsafe
                {
                    fixed (float* pSource = SourceImage)
                    {
                        float* pOut = pSource;
                        if (ClipZeros)
                        {
                            for (int i = 0; i < SourceImage.LongLength; i++)
                            {

                                *pOut = (*pOut) / factor - CutValue;
                                if (*pOut < 0)
                                    *pOut = 0;

                                /* if (*pOut > 5)
                                 {
                                     ave += *pOut;
                                     cc++;
                                 }*/
                                pOut++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < SourceImage.LongLength; i++)
                            {
                                *pOut = (*pOut) / factor - CutValue;
                                if (*pOut < 0)
                                    *pOut = 0;

                                /* if (*pOut > 5)
                                 {
                                     ave += *pOut;
                                     cc++;
                                 }*/
                                pOut++;
                            }
                        }
                    }
                }

            }
        }


        public static double RemoveSphereToZero(ref float[, ,] Data)
        {
            double RX = Data.GetLength(0) / 2d - 10;
            double RY = Data.GetLength(1) / 2d - 10;
            double RZ = Data.GetLength(2) / 2d - 10;
            double HalfI = Data.GetLength(0) / 2d;
            double HalfJ = Data.GetLength(1) / 2d;
            double HalfK = Data.GetLength(2) / 2d;

            double x;
            double y;
            double z;

            double min = double.MaxValue;
            float val;
            float sum = 0, count = 0;
            Random rnd = new Random();
            List<float> medianList = new List<float>(1000);
            for (int i = 0; i < Data.GetLength(0); i++)
                for (int j = 0; j < Data.GetLength(1); j++)
                    for (int k = 0; k < Data.GetLength(2); k++)
                    {
                        x = (i - HalfI);
                        y = RX * (j - HalfJ) / RY;
                        z = RX * (k - HalfK) / RZ;
                        if (RX - (x * x + y * y) < 2)
                        {
                            val = Data[i, j, k];
                            if (val < min) min = val;
                            sum += val;
                            if (val > 0 && medianList.Count < 1000 && rnd.NextDouble() < .01)
                                medianList.Add(val);
                            count++;
                        }
                    }


            sum /= count;

            if (medianList.Count > 0)
            {
                medianList.Sort();
                sum = medianList[(int)(medianList.Count * 5f / 8f)];
            }
            // sum /= count;
            if (sum < -500) sum = -500;
            // sum = min;
            // sum = 0;
            //  double sum = -500;
            //  RX *= .8;
            //  RY *= .8;
            //  RZ *= .8;
            for (int i = 0; i < Data.GetLength(0); i++)
                for (int j = 0; j < Data.GetLength(1); j++)
                    for (int k = 0; k < Data.GetLength(2); k++)
                    {
                        x = (i - HalfI) / RX;
                        y = (j - HalfJ) / RY;
                        z = (k - HalfK) / RZ;
                        if ((y * y + z * z) > 1)
                        {
                            Data[i, j, k] = 0;
                        }
                        // else if (Data[i, j, k] < 0)
                        {
                            //  Data[i, j, k] = 0;
                        }

                    }

            return sum * 2;
        }

    }
}
