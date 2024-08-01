using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILNumerics;

namespace MathLibrary.Signal_Processing
{
    public static  class ilFilters //: ILNumerics.ILMath
    {
        private static ILArray<fcomplex> Kernal = null;
        private static int KernalSize;
        private static void CreateGuassianFilter(int imageWidth,int imageHeight, int Radius)
        {
            KernalSize = Radius;
            ILArray<float> YLines = ILMath.empty<float>();
            ILArray<float> XLines = ILMath.meshgrid(ILMath.linspace<float>(-1 * Radius, Radius, 2 * Radius), ILMath.linspace<float>(-1 * Radius, Radius, 2 * Radius), YLines);

            ILArray<float> summed = ILMath.multiplyElem(XLines, XLines) +  ILMath.multiplyElem(YLines, YLines);

            ILArray<float> kernal = ILMath.exp(-4*summed / ( Radius * Radius));
            kernal = kernal / ILMath.sumall(kernal);
            Kernal = ILMath.fft2(kernal,imageWidth,imageHeight);

            KernalSize = imageHeight;
        }


        public  static ILArray<float> CreateGuassianFilterI(int ImageSize, int Radius)
        {
            KernalSize = Radius;
            ILArray<float> YLines = ILMath.empty<float>();
            ILArray<float> XLines = ILMath.meshgrid(ILMath.linspace<float>(-1 * Radius, Radius, 2 * Radius), ILMath.linspace<float>(-1 * Radius, Radius, 2 * Radius), YLines);

            ILArray<float> summed = ILMath.multiplyElem(XLines, XLines) + ILMath.multiplyElem(YLines, YLines);

            ILArray<float> kernal = ILMath.exp(-4*summed / ( Radius * Radius));

            Kernal = ILMath.fft2(kernal,1600,900);

            ILArray<float> R = ILMath.abs(Kernal);

            return R;

        }


        private static object lockObject = new object();

        /// <summary>
        /// You must provide an already created array for the results
        /// </summary>
        /// <param name="oArray"></param>
        /// <param name="outArray"></param>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public static float[, ,] SmoothGuassian(this float[, ,] oArray, int Radius)
        {
           

            if (Kernal == null || oArray.GetLength(0) != KernalSize)
            {
                lock (lockObject)
                {
                    if (Kernal == null || oArray.GetLength(0) != KernalSize)
                    {
                        CreateGuassianFilter(oArray.GetLength(1), oArray.GetLength(0) , Radius);
                    }
                }
            }

            ILArray<float> image = oArray;

            ILSize size = new ILSize(oArray.GetLength(1), oArray.GetLength(0));
            image = image.Reshape(size);

            ILArray<fcomplex> F = ILMath.fft2(image);
            ILArray<fcomplex> F2 = ILMath.multiplyElem(F, Kernal);

            ILArray<float> Smooth = ILMath.real(ILMath.ifft2(F2)) / 1e8f;
          

            float[] buffer =( Smooth).GetArrayForRead();
            float[, ,] smoothed = new float[Smooth.Size[1], Smooth.Size[0], 1];

            Buffer.BlockCopy(buffer, 0, smoothed, 0, Buffer.ByteLength(smoothed));
            return smoothed;
        }


        public static ILArray<float> SmoothGuassianT(this float[, ,] oArray, int Radius)
        {


            if (Kernal == null || oArray.GetLength(0) != KernalSize)
            {
                lock (lockObject)
                {
                    if (Kernal == null || oArray.GetLength(0) != KernalSize)
                    {
                        CreateGuassianFilter(oArray.GetLength(1), oArray.GetLength(0), Radius);
                    }
                }
            }

            ILArray<float> image = oArray;

            ILSize size = new ILSize(oArray.GetLength(1), oArray.GetLength(0));
            image = image.Reshape(size);

            ILArray<fcomplex> F = ILMath.fft2(image);
            ILArray<fcomplex> F2 = ILMath.multiplyElem(F, Kernal);

            ILArray<float> Smooth = ILMath.divide( ILMath.real(ILMath.ifft2(F2)), (float)Math.Pow(10,2) );

            return Smooth;
        }


        /// <summary>
        /// You must provide an already created array for the results
        /// </summary>
        /// <param name="oArray"></param>
        /// <param name="outArray"></param>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public static float [, ,] SmoothGuassian(this byte[, ,] oArray, int Radius)
        {
            int paddedSize;
            if (Radius * 2 < oArray.GetLength(0))
                paddedSize = oArray.GetLength(0);
            else
                paddedSize = Radius * 2;

            if (Kernal == null || paddedSize != KernalSize)
            {
                lock (lockObject)
                {
                    if (Kernal == null || paddedSize != KernalSize)
                    {
                   //     CreateGuassianFilter(paddedSize, Radius);
                    }
                }
            }

            ILArray<float> image = oArray;
            ILSize size = new ILSize(oArray.GetLength(0), oArray.GetLength(1));
            ILSize size2 = Kernal.Size;
            image = image.Reshape(size);

            ILArray<fcomplex> F = ILMath.fft2(image, paddedSize, paddedSize);
            ILArray<fcomplex> F2 = ILMath.multiplyElem(F, Kernal);

            ILArray<float> Smooth = ILMath.real(ILMath.ifft2(F2));

           // float[] buffer = Smooth.Reshape(oArray.GetLength(0), oArray.GetLength(1), 1);

            float[, ,] smoothed = new float[oArray.GetLength(0), oArray.GetLength(1), 1];

            Buffer.BlockCopy(Smooth.GetArrayForRead(), 0, smoothed, 0, Buffer.ByteLength(smoothed));
            return smoothed;
        }
    }
}
