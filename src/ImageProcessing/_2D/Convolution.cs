using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageProcessing._2D
{
    public class Convolution
    {
       

        public static unsafe float[,,] Convolve(float[, ,] DataIn, float[] Kernal)
        {
            float[,,] ArrayOut = new float[DataIn.GetLength(0), DataIn.GetLength(1),1];

            fixed (float* pArrayIn = DataIn)
            {
                fixed (float* pKernal = Kernal)
                {
                    fixed (float* pOut = ArrayOut)
                    {
                        int Width = DataIn.GetLength(0);
                        int Height = DataIn.GetLength(1);
                        for (int i = 0; i < Height; i++)
                        {
                            ConvoluteChop(pArrayIn + i * Width, Width, pKernal, Kernal.Length, pOut + i * Width);
                        }
                    }
                }
            }
            return ArrayOut;
        }

        /// <summary>
        /// Does the convolution along the fast memory access direction
        /// </summary>
        /// <param name="Array1"></param>
        /// <param name="Length1"></param>
        /// <param name="pImpulse"></param>
        /// <param name="Length2"></param>
        /// <param name="pArrayOut"></param>
        public static unsafe void ConvoluteChop(float* Array1, int Length1, float* pImpulse, int Length2, float* pArrayOut)
        {

            int LengthWhole = Length1 + Length2;

            int StartI = (int)Math.Truncate((double)LengthWhole / 2d - (double)Length1 / 2d);
            int EndI = (int)Math.Truncate((double)LengthWhole / 2d + (double)Length1 / 2d);
            if (EndI - StartI > Length1)
                EndI--;
            int sI, eI;


            double p1;
            float* p2;
            float* pOut;

            unchecked
            {
                for (int i = 0; i < Length1; i++)
                {
                    p1 = Array1[i];
                    sI = StartI - i;
                    eI = EndI - i;
                    if (eI > Length2) eI = Length2;
                    if (sI < 0) sI = 0;
                    if (sI < eI)
                    {
                        p2 = pImpulse + sI;
                        pOut = pArrayOut + i + sI - StartI;
                        for (int j = sI; j < eI; j++)
                        {
                            *pOut += (float)(p1 * (*p2));
                            pOut++;
                            p2++;
                        }
                    }
                }
            }
        }



    }
}
