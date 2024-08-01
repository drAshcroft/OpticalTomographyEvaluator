using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ReconstructCells.Tomography
{
    class Convolution
    {
        public static Image<Gray, float> ConvoluteImage(Image<Gray, float> image, float[] kernal)
        {
            Image<Gray, float> convolved = image.CopyBlank();

            float[, ,] data = convolved.Data;//workaround to c# rules
            Convolve(image.Data, kernal, ref data);
            return convolved;
        }

        private  static unsafe void Convolve(float[, ,] DataIn, float[] Kernal, ref float[, ,] DataOut)
        {
            fixed (float* pArrayIn = DataIn)
            {
                fixed (float* pKernal = Kernal)
                {
                    fixed (float* pOut = DataOut)
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
        }

        private  static unsafe void ConvoluteChop(float* image, int width, float* pImpulse, int impulseWidth, float* pArrayOut)
        {

            int LengthWhole = width + impulseWidth;

            int StartI = (int)Math.Truncate((double)LengthWhole / 2d - (double)width / 2d);
            int EndI = (int)Math.Truncate((double)LengthWhole / 2d + (double)width / 2d);
            if (EndI - StartI > width)
                EndI--;
            int sI, eI;


            float p1;
            float* p2;
            float* pOut;

            unchecked
            {
                for (int x = 0; x < width; x++)
                {
                    p1 = image[x];
                    sI = StartI - x;
                    eI = EndI - x;
                    if (eI > impulseWidth) eI = impulseWidth;
                    if (sI < 0) sI = 0;
                    if (sI < eI)
                    {
                        p2 = pImpulse + sI;
                        pOut = pArrayOut + x + sI - StartI;
                        for (int j = sI; j < eI; j++)
                        {
                            *pOut += p1 * (*p2);
                            pOut++;
                            p2++;
                        }
                    }
                }
            }
        }

    }
}
