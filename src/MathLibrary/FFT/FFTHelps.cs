using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using MathLibrary.Signal_Processing;


namespace MathLibrary.FFT
{
    public static class MathFFTHelps
    {

        /// <summary>
        /// Returns the nearest power of 2 that is equal or greater to the current value
        /// </summary>
        /// <param name="testNumber"></param>
        /// <returns></returns>
        public static double NearestPowerOf2(int testNumber)
        {
            double denom = Math.Log(testNumber) / Math.Log(2);
            if (denom - Math.Floor(denom) == 0)
                return testNumber;
            else
                return Math.Pow(2, Math.Floor(denom) + 1);

        }


        private static IFFTPluginHandler CurrentFFTLib;
        private static object CriticalSection = new object();

        static MathFFTHelps()
        {
            //check if there is an error when the assembly is loaded.  If there is not, use the fftw lib as it is really fast
            // string path = Path.GetDirectoryName(Application.ExecutablePath) + "\\fftwlib.dll";

            // Assembly fftwAsm = Assembly.LoadFrom(path);
            CurrentFFTLib = new FFTWLib();
        }

        #region General easy fft statements
        public static double[] iFFT(complex[] array)
        {
            return iFFTcomplex2real(array);
        }
        public static complex[] FFT(double[] array)
        {
            return FFTreal2complex(array);
        }
        public static double[,] iFFT(complex[,] array)
        {
            return iFFTcomplex2real(array);
        }
        public static complex[,] FFT(double[,] array)
        {
            return FFTreal2complex(array);
        }
        public static complex[] FFT(complex[] array)
        {
            return FFTcomplex2complex(array);
        }
        public static complex[,] FFT(complex[,] array)
        {
            return FFTcomplex2complex(array);
        }

        #endregion

        #region 1D FFTs
        public static double[] FFTreal2real(double[] array)
        {
            return CurrentFFTLib.FFTreal2real(array);
        }
        public static unsafe void FFTreal2real(double* pArray, int Length, double* pArrayOut)
        {
            CurrentFFTLib.FFTreal2real(pArray, Length, pArrayOut);
        }

        public static double[] iFFTreal2real(double[] array)
        {
            return CurrentFFTLib.iFFTreal2real(array);
        }

        public static unsafe void iFFTreal2real(double* pArray, int Length, double* pArrayOut)
        {
            CurrentFFTLib.iFFTreal2real(pArray, Length, pArrayOut);
        }

        public static complex[] FFTcomplex2complex(complex[] array)
        {
            return CurrentFFTLib.FFTcomplex2complex(array);
        }
        public static unsafe void FFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut)
        {
            CurrentFFTLib.FFTcomplex2complex(pArray, Length, pArrayOut);
        }

        public static complex[] iFFTcomplex2complex(complex[] array)
        {
            return CurrentFFTLib.iFFTcomplex2complex(array);
        }
        public static unsafe void iFFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut)
        {
            CurrentFFTLib.iFFTcomplex2complex(pArray, Length, pArrayOut);
        }


        public static complex[] FFTreal2complex(double[] array)
        {
            return CurrentFFTLib.FFTreal2complex(array);
        }
        public static unsafe void FFTreal2complex(double* pArray, int Length, complex* pOutArray)
        {
            CurrentFFTLib.FFTreal2complex(pArray, Length, pOutArray);
        }

        public static double[] iFFTcomplex2real(complex[] array)
        {
            return CurrentFFTLib.iFFTcomplex2real(array);
        }
        public static unsafe void iFFTcomplex2real(complex* pArray, int Length, double* pArrayOut)
        {
            CurrentFFTLib.iFFTcomplex2real(pArray, Length, pArrayOut);
        }
        #endregion

        #region 2D FFTs

        public static double[,] FFTreal2real(double[,] array)
        {
            return CurrentFFTLib.FFTreal2real(array);
        }

        public static double[,] iFFTreal2real(double[,] array)
        {
            return CurrentFFTLib.iFFTreal2real(array);
        }

        public static complex[,] FFTcomplex2complex(complex[,] array)
        {
            return CurrentFFTLib.FFTcomplex2complex(array);
        }

        public static complex[,] iFFTcomplex2complex(complex[,] array)
        {
            return CurrentFFTLib.iFFTcomplex2complex(array);
        }

        public static complex[,] FFTreal2complex(double[,] array)
        {
            return CurrentFFTLib.FFTreal2complex(array);
        }

        public static double[,] iFFTcomplex2real(complex[,] array)
        {
            return CurrentFFTLib.iFFTcomplex2real(array);
        }

        #region Unsafes
        public static unsafe void FFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray)
        {
            CurrentFFTLib.FFTreal2real(pArray, Length1, Length2, pOutArray);
        }

        public static unsafe void iFFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray)
        {
            CurrentFFTLib.iFFTreal2real(pArray, Length1, Length2, pOutArray);
        }

        public static unsafe void FFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray)
        {
            CurrentFFTLib.FFTcomplex2complex(pArray, Length1, Length2, pOutArray);
        }

        public static unsafe void iFFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray)
        {
            CurrentFFTLib.iFFTcomplex2complex(pArray, Length1, Length2, pOutArray);
        }

        public static unsafe void FFTreal2complex(double* pArray, int Length1, int Length2, complex* pOutArray)
        {
            CurrentFFTLib.FFTreal2complex(pArray, Length1, Length2, pOutArray);
        }

        public static unsafe void iFFTcomplex2real(complex* pArray, int Length1, int Length2, double* pOutArray)
        {
            CurrentFFTLib.iFFTcomplex2real(pArray, Length1, Length2, pOutArray);
        }
        #endregion
        #endregion

        #region ImageQuality Metrics

        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static double[,] CrossCorrelationFFTNormalized(double[,] Function, double[,] impulse)
        {

            int Length = 0;
            double[,] array1;
            double[,] array2;
            double[,] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
            {
                if (Function.GetLength(0) > Function.GetLength(1))
                    Length = Function.GetLength(0);
                else
                    Length = Function.GetLength(1);
            }
            else
            {
                if (Function.GetLength(0) > impulse.GetLength(1))
                    Length = impulse.GetLength(0);
                else
                    Length = impulse.GetLength(1);
            }

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = Function.ZeroPadArray2D(Length);
                array2 = impulse.ZeroPadArray2D(Length);
            }
            else
            {
                array1 = Function;
                array2 = impulse;
            }

            lock (CriticalSection)
            {
                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);
                complex[,] cArray2 = FFTreal2complex(array2);

                //remove the DC offset
                cArray1[0, 0] = new complex(0, 0);
                cArray2[0, 0] = new complex(0, 0);

                double intensity1 = cArray1.Abs().SumArrayCareful();
                double intensity2 = cArray2.Abs().SumArrayCareful();

                cArray1.DivideInPlace(intensity1);
                cArray2.DivideInPlace(intensity2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();
            }
            //normalize the return data
            double dL = Length;
            arrayOut.MultiplyInPlace(1d / dL);

            return arrayOut;

        }


        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static float[, ,] FFT_cnv2(float[, ,] Function, float[, ,] impulse)
        {

            int Length = 0;
            double[,] array1;
            double[,] array2;
            double[,] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
            {
                if (Function.GetLength(0) > Function.GetLength(1))
                    Length = Function.GetLength(0);
                else
                    Length = Function.GetLength(1);
            }
            else
            {
                if (Function.GetLength(0) > impulse.GetLength(1))
                    Length = impulse.GetLength(0);
                else
                    Length = impulse.GetLength(1);
            }


            array1 = new double[Function.GetLength(0), Function.GetLength(1)];
            array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

            for (int i = 0; i < Function.GetLength(0); i++)
                for (int j = 0; j < Function.GetLength(1); j++)
                    array1[i, j] = Function[i, j, 0];

            for (int i = 0; i < impulse.GetLength(0); i++)
                for (int j = 0; j < impulse.GetLength(1); j++)
                    array2[i, j] = impulse[i, j, 0];

            int origLength = Function.GetLength(0);

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = array1.ZeroPadArray2D(Length);
                array2 = array2.ZeroPadArray2D(Length);
            }


            lock (CriticalSection)
            {
                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                // cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL;

                float[, ,] arrayOutf = new float[origLength, origLength, 1];
                int startI = (int)Math.Floor((arrayOut.GetLength(0) - origLength) / 2d);
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i + startI, j + startI] * Multiplicant);
                    }
                }

                return arrayOutf;
            }
        }

        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static float[, ,] FFT_cnv2(float[, ,] Function, float[, ,] impulse,float[,,] arrayOutf)
        {

            int Length = 0;
            double[,] array1;
            double[,] array2;
            double[,] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
            {
                if (Function.GetLength(0) > Function.GetLength(1))
                    Length = Function.GetLength(0);
                else
                    Length = Function.GetLength(1);
            }
            else
            {
                if (Function.GetLength(0) > impulse.GetLength(1))
                    Length = impulse.GetLength(0);
                else
                    Length = impulse.GetLength(1);
            }


            array1 = new double[Function.GetLength(0), Function.GetLength(1)];
            array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

            for (int i = 0; i < Function.GetLength(0); i++)
                for (int j = 0; j < Function.GetLength(1); j++)
                    array1[i, j] = Function[i, j, 0];

            for (int i = 0; i < impulse.GetLength(0); i++)
                for (int j = 0; j < impulse.GetLength(1); j++)
                    array2[i, j] = impulse[i, j, 0];

            int origLength = Function.GetLength(0);

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = array1.ZeroPadArray2D(Length);
                array2 = array2.ZeroPadArray2D(Length);
            }


            lock (CriticalSection)
            {
                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

              

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL/dL;

               
                int startI = (int)Math.Floor((arrayOut.GetLength(0) - origLength) / 2d);
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i + startI, j + startI] * Multiplicant);
                    }
                }

                return arrayOutf;
            }
        }

        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <param name="paddingScale">This must be a factor of 2</param>
        /// <returns></returns>
        public static float[, ,] FFT_cnv2(float[, ,] Function, float[, ,] impulse, float[, ,] arrayOutf,int paddingScale)
        {

            int Length = 0;
            double[,] array1;
            double[,] array2;
            double[,] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
            {
                if (Function.GetLength(0) > Function.GetLength(1))
                    Length = Function.GetLength(0);
                else
                    Length = Function.GetLength(1);
            }
            else
            {
                if (Function.GetLength(0) > impulse.GetLength(1))
                    Length = impulse.GetLength(0);
                else
                    Length = impulse.GetLength(1);
            }


            array1 = new double[Function.GetLength(0), Function.GetLength(1)];
            array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

            for (int i = 0; i < Function.GetLength(0); i++)
                for (int j = 0; j < Function.GetLength(1); j++)
                    array1[i, j] = Function[i, j, 0];

            for (int i = 0; i < impulse.GetLength(0); i++)
                for (int j = 0; j < impulse.GetLength(1); j++)
                    array2[i, j] = impulse[i, j, 0];

            int origLength = Function.GetLength(0) * paddingScale;

            ///make sure the images are a power of 2
            //if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length)*paddingScale;
                array1 = array1.ZeroPadArray2D(Length);
                array2 = array2.ZeroPadArray2D(Length);
            }


            lock (CriticalSection)
            {
                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL;


                int startI = (int)Math.Floor((arrayOut.GetLength(0) - origLength) / 2d);
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i + startI, j + startI] * Multiplicant);
                    }
                }

                return arrayOutf;
            }
        }


        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static float[, ,] CrossCorrelationFFT(float[, ,] Function, float[, ,] impulse)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                //take the dimensions of the larger array
                if (Function.Length > impulse.Length)
                {
                    if (Function.GetLength(0) > Function.GetLength(1))
                        Length = Function.GetLength(0);
                    else
                        Length = Function.GetLength(1);
                }
                else
                {
                    if (Function.GetLength(0) > impulse.GetLength(1))
                        Length = impulse.GetLength(0);
                    else
                        Length = impulse.GetLength(1);
                }


                array1 = new double[Function.GetLength(0), Function.GetLength(1)];
                array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

                for (int i = 0; i < Function.GetLength(0); i++)
                    for (int j = 0; j < Function.GetLength(1); j++)
                        array1[i, j] = Function[i, j, 0];

                for (int i = 0; i < impulse.GetLength(0); i++)
                    for (int j = 0; j < impulse.GetLength(1); j++)
                        array2[i, j] = impulse[i, j, 0];

                int origLength = Function.GetLength(0);
                ///make sure the images are a power of 2
                if (Function.Length != impulse.Length)
                {
                    Length = (int)NearestPowerOf2(Length);
                    array1 = array1.ZeroPadArray2D(Length);
                    array2 = array2.ZeroPadArray2D(Length);
                }



                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL;

                float[, ,] arrayOutf = new float[origLength, origLength, 1];
                int startI = (int)Math.Floor((arrayOut.GetLength(0) - origLength) / 2d);
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i + startI, j + startI] * Multiplicant);
                    }
                }

                return arrayOutf;
            }
        }

        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static float[, ,] CrossCorrelationFFT(float[, ,] Function, float[, ,] impulse, int ArrayWidth)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                //take the dimensions of the larger array
                if (Function.Length > impulse.Length)
                {
                    if (Function.GetLength(0) > Function.GetLength(1))
                        Length = Function.GetLength(0);
                    else
                        Length = Function.GetLength(1);
                }
                else
                {
                    if (Function.GetLength(0) > impulse.GetLength(1))
                        Length = impulse.GetLength(0);
                    else
                        Length = impulse.GetLength(1);
                }


                array1 = new double[Function.GetLength(0), Function.GetLength(1)];
                array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

                for (int i = 0; i < Function.GetLength(0); i++)
                    for (int j = 0; j < Function.GetLength(1); j++)
                        array1[i, j] = Function[i, j, 0];

                for (int i = 0; i < impulse.GetLength(0); i++)
                    for (int j = 0; j < impulse.GetLength(1); j++)
                        array2[i, j] = impulse[i, j, 0];


                int diff = (int)(Math.Abs(Function.GetLength(0) - impulse.GetLength(0)) - 1);

                Length = (int)ArrayWidth;
                array1 = array1.ZeroPadArray2D(ArrayWidth);
                array2 = array2.ZeroPadArray2D(ArrayWidth);




                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                 arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();
                //arrayOut = iFFTcomplex2real(cArray1);

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL;

                float[, ,] arrayOutf = new float[arrayOut.GetLength(0), arrayOut.GetLength(1), 1];

                for (int i = 0; i < arrayOut.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOut.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i, j] * Multiplicant);
                    }
                }

                return arrayOutf;
            }
        }


        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static float[, ,] conv2(float[, ,] Function, float[, ,] impulse, int ArrayWidth)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                //take the dimensions of the larger array
                if (Function.Length > impulse.Length)
                {
                    if (Function.GetLength(0) > Function.GetLength(1))
                        Length = Function.GetLength(0);
                    else
                        Length = Function.GetLength(1);
                }
                else
                {
                    if (Function.GetLength(0) > impulse.GetLength(1))
                        Length = impulse.GetLength(0);
                    else
                        Length = impulse.GetLength(1);
                }


                array1 = new double[Function.GetLength(0), Function.GetLength(1)];
                array2 = new double[impulse.GetLength(0), impulse.GetLength(1)];

                for (int i = 0; i < Function.GetLength(0); i++)
                    for (int j = 0; j < Function.GetLength(1); j++)
                        array1[i, j] = Function[i, j, 0];

                for (int i = 0; i < impulse.GetLength(0); i++)
                    for (int j = 0; j < impulse.GetLength(1); j++)
                        array2[i, j] = impulse[i, j, 0];


                int diff = (int)(Math.Abs(Function.GetLength(0) - impulse.GetLength(0)) + 1);

                if (array1.GetLength(0) < Length)
                    array1 = array1.ZeroPadArray2D(Length);
                if (array2.GetLength(0) < Length)
                    array2 = array2.ZeroPadArray2D(Length);




                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                 arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();
               // arrayOut = iFFTcomplex2complex(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                var Multiplicant = 1d / dL;

                float[, ,] arrayOutf = new float[diff, diff, 1];


                int startI = (int)Math.Floor((arrayOut.GetLength(0) - diff) / 2d);
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(arrayOut[i + startI, j + startI]*Multiplicant);
                    }
                }

              

                return arrayOutf;
            }
        }

        /// <summary>
        /// Does the cross correlation of two images using the fft method
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="impulse"></param>
        /// <returns></returns>
        public static double[,] CrossCorrelationFFT(double[,] Function, double[,] impulse)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                //take the dimensions of the larger array
                if (Function.Length > impulse.Length)
                {
                    if (Function.GetLength(0) > Function.GetLength(1))
                        Length = Function.GetLength(0);
                    else
                        Length = Function.GetLength(1);
                }
                else
                {
                    if (Function.GetLength(0) > impulse.GetLength(1))
                        Length = impulse.GetLength(0);
                    else
                        Length = impulse.GetLength(1);
                }

                ///make sure the images are a power of 2
                if (Function.Length != impulse.Length)
                {
                    Length = (int)NearestPowerOf2(Length);
                    array1 = Function.ZeroPadArray2D(Length);
                    array2 = impulse.ZeroPadArray2D(Length);
                }
                else
                {
                    array1 = Function;
                    array2 = impulse;
                }


                //comvert both to freq. space
                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);

                //do the correlation
                cArray2.ConjugateInPlace();
                cArray1.MultiplyInPlace(cArray2);

                //convert them back
                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                //normalize the return data
                double dL = Length;
                arrayOut.MultiplyInPlace(1d / dL);

                return arrayOut;
            }
        }

        public static System.Drawing.PointF FindShift(float[, ,] Function, float[, ,] impulse)
        {

            int Length = 0;
            double[,] array1;
            double[,] array2;
            double[,] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
            {
                if (Function.GetLength(0) > Function.GetLength(1))
                    Length = Function.GetLength(0);
                else
                    Length = Function.GetLength(1);
            }
            else
            {
                if (Function.GetLength(0) > impulse.GetLength(1))
                    Length = impulse.GetLength(0);
                else
                    Length = impulse.GetLength(1);
            }

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = Function.ZeroPadArray2D(Length);
                array2 = impulse.ZeroPadArray2D(Length);
            }
            else
            {
                array1 = Function.ToDouble();
                array2 = impulse.ToDouble();
            }
            //move to the right format
            array2.MakeFFTHumanReadable();


            complex[,] img1;

            complex[,] img2;
            lock (CriticalSection)
            {
                //comvert both to freq. space
                img1 = FFTreal2complex(array1);

                img2 = FFTreal2complex(array2);
            }
            //do the correlation
            img2.ConjugateInPlace();
            img1.MultiplyInPlace(img2);



            lock (CriticalSection)
            {
                //convert them back
                arrayOut = iFFTcomplex2real(img1);
            }
            int X, Y;

            arrayOut = arrayOut.MakeFFTHumanReadable();

            //  MathHelpLib.Convolution.AverageConvolutionKernal aKernal = new Convolution.AverageConvolutionKernal(7);
            // arrayOut = MathHelpLib.Convolution.ConvolutionFilterImplented.ConvolutionFilter(arrayOut, aKernal);

            arrayOut.MaxArray(out X, out Y);

            // arrayOut = arrayOut.LogErrorless();
            //  System.Drawing.Bitmap b = MathHelpLib.ImageProcessing.MathImageHelps.MakeBitmap(arrayOut);

            // int jj = b.Width;
            return new System.Drawing.PointF(X - arrayOut.GetLength(0) / 2, Y - arrayOut.GetLength(1) / 2);
        }


        public static float FindShift(double[] Function, double[] impulse)
        {
            int Length = Function.Length;
            double[] array1;
            double[] array2;
            double[] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
                Length = Function.Length;
            else
                Length = impulse.Length;

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = Function.ZeroPadArray(Length);
                array2 = impulse.ZeroPadArray(Length);
            }
            else
            {
                array1 = Function;
                array2 = impulse;
            }
            //move to the right format
            array2.MakeFFTHumanReadable();


            complex[] img1;

            complex[] img2;
            lock (CriticalSection)
            {
                //comvert both to freq. space
                img1 = FFTreal2complex(array1);

                img2 = FFTreal2complex(array2);
            }
            //do the correlation
            img2.ConjugateInPlace();
            img1.MultiplyInPlace(img2);

            lock (CriticalSection)
            {
                //convert them back
                arrayOut = iFFTcomplex2real(img1);
            }
            int X;

            arrayOut = arrayOut.MakeFFTHumanReadable();

            //  var test =  arrayOut.DrawGraph();

            // Signal_Processing.ArrayFilters.SmoothArrayBoxCar(arrayOut, 10);
            // test = arrayOut.DrawGraph();
            arrayOut.MaxArray(out X);

            return arrayOut.Length / 2 - X;
        }

        public static float FindShift(double[] Function, double[] impulse, double[] Filter)
        {
            int Length = Function.Length;
            double[] array1;
            double[] array2;
            double[] arrayOut;
            //take the dimensions of the larger array
            if (Function.Length > impulse.Length)
                Length = Function.Length;
            else
                Length = impulse.Length;

            ///make sure the images are a power of 2
            if (Function.Length != impulse.Length)
            {
                Length = (int)NearestPowerOf2(Length);
                array1 = Function.ZeroPadArray(Length);
                array2 = impulse.ZeroPadArray(Length);
            }
            else
            {
                array1 = Function;
                array2 = impulse;
            }
            //move to the right format
            array2.MakeFFTHumanReadable();


            complex[] img1;

            complex[] img2;
            lock (CriticalSection)
            {
                //comvert both to freq. space
                img1 = FFTreal2complex(array1);

                img2 = FFTreal2complex(array2);
            }
            //do the correlation
            img2.ConjugateInPlace();
            img1.MultiplyInPlace(img2);
            img1.MultiplyInPlace(Filter);

            lock (CriticalSection)
            {
                //convert them back
                arrayOut = iFFTcomplex2real(img1);
            }
            int X;

            arrayOut = arrayOut.MakeFFTHumanReadable();

            //  var test =  arrayOut.DrawGraph();

            // Signal_Processing.ArrayFilters.SmoothArrayBoxCar(arrayOut, 10);
            // test = arrayOut.DrawGraph();
            arrayOut.MaxArray(out X);

            return arrayOut.Length / 2 - X;
        }

        public static double[] ShowFilter(double[] Function, double[] Filter)
        {
            int Length = Function.Length;
            double[] array1;
            double[] array2;
            double[] arrayOut;
            //take the dimensions of the larger array

            Length = Function.Length;

            array1 = Function;
            array2 = Filter;

            //move to the right format
            array2 = array2.MakeFFTHumanReadable();

            complex[] img1;
            lock (CriticalSection)
            {
                //comvert both to freq. space
                img1 = FFTreal2complex(array1);
            }
            //do the correlation
            //img2.ConjugateInPlace();
            //img1.MultiplyInPlace(img2);
            img1.MultiplyInPlace(Filter);

            lock (CriticalSection)
            {
                //convert them back
                arrayOut = iFFTcomplex2real(img1);
            }

            //  arrayOut = arrayOut.MakeFFTHumanReadable();

            return arrayOut;
        }


        public static double[] CreateFilter(double[] Function)
        {
            int Length = Function.Length;
            double[] array1;

            double[] arrayOut;
            //take the dimensions of the larger array

            Length = Function.Length;
            array1 = Function;
            array1 = array1.MakeFFTHumanReadable();

            complex[] img1;
            lock (CriticalSection)
            {
                //comvert both to freq. space
                img1 = FFTreal2complex(array1);
            }

            arrayOut = img1.Abs();

            return arrayOut;
        }

        public static double[,] CrossCorrelationFFTFiltered(double[,] Function, double[,] impulse, double percentagelow, double percentagehigh, out double SumFreq)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                if (Function.Length > impulse.Length)
                    Length = Function.GetLength(0);
                else
                    Length = impulse.GetLength(0);

                if (Function.Length != impulse.Length)
                {
                    Length = (int)NearestPowerOf2(Length);
                    array1 = Function.ZeroPadArray2D(Length);
                    array2 = impulse.ZeroPadArray2D(Length);
                }
                else
                {
                    array1 = Function;
                    array2 = impulse;
                }
                array2.MakeFFTHumanReadable();

                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);
                cArray2.ConjugateInPlace();

                cArray1.MultiplyInPlace(cArray2);
                int halfX = (int)Math.Truncate((cArray1.GetLength(0)) / 2d);
                int halfY = (int)Math.Truncate((cArray1.GetLength(1)) / 2d);

                double max = Math.Sqrt(Math.Pow(halfX, 2) + Math.Pow(halfY, 2));
                double min = 0;
                double lower = ((max - min) * percentagelow) + min;
                lower = lower * lower;
                double higher = ((max - min) * percentagehigh) + min;// max - ((max - min) * percentagehigh);
                higher = higher * higher;
                complex zero = new complex(0, 0);

                int LenX = cArray1.GetLength(0);
                int LenY = cArray1.GetLength(1);
                double x, y, R;
                SumFreq = 0;
                for (int i = 0; i < LenX; i++)
                    for (int j = 0; j < LenY; j++)
                    {
                        x = (i - halfX);
                        y = (j - halfY);
                        R = (x * x + y * y);
                        if (R < lower || R > higher)
                        {
                            // cArray1[halfX, halfY] += cArray1[i, j];
                            cArray1[i, j] = zero;
                        }
                        else
                        {
                            SumFreq += cArray1[i, j].Abs();
                        }
                    }
                SumFreq = SumFreq / (3.1415 * Math.Abs(higher - lower));

                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                double dL = Length;
                arrayOut.MultiplyInPlace(1d / dL);

                return arrayOut;
            }
        }

        public static double UQI(double[,] imag1, double[,] imag2)
        {

            double avg1 = 0;
            double avg2 = 0;
            double std1 = 0;
            double std2 = 0;
            double d1;
            double d2;

            for (int i = 0; i < imag1.GetLength(0); i++)
                for (int j = 0; j < imag1.GetLength(1); j++)
                {
                    d1 = imag1[i, j];
                    avg1 += d1;

                    d2 = imag2[i, j];
                    avg2 += d2;

                }

            avg1 /= (imag1.GetLength(0) * imag1.GetLength(1));
            avg2 /= (imag2.GetLength(0) * imag2.GetLength(1));


            for (int i = 0; i < imag1.GetLength(0); i++)
                for (int j = 0; j < imag1.GetLength(1); j++)
                {
                    d1 = (avg1 - imag1[i, j]);
                    std1 += d1 * d1;

                    d2 = (avg2 - imag2[i, j]);
                    std2 += d2 * d2;
                }

            std1 = Math.Sqrt(std1 / (imag1.GetLength(0) * imag1.GetLength(1) - 1));
            std2 = Math.Sqrt(std2 / (imag2.GetLength(0) * imag2.GetLength(1) - 1));

            double[,] imag11 = imag1.SubtractFromArray(avg1);
            double[,] imag22 = imag2.SubtractFromArray(avg2);
            double cov = 0;


            for (int i = 0; i < imag11.GetLength(0); i++)
                for (int j = 0; j < imag11.GetLength(1); j++)
                {
                    cov += imag11[i, j] * imag22[i, j];
                }
            cov = cov / (imag11.GetLength(0) * imag11.GetLength(1) - 1);
            double UQI = (4 * cov * avg1 * avg2) / ((std1 * std1 + std2 * std2) * (avg1 * avg1 + avg2 * avg2));
            return UQI;

        }

        public static double MI(double[,] imag1, double[,] imag2)
        {
            double max1 = imag1.MaxArray();
            double max2 = imag2.MaxArray();
            double max = Math.Max(max1, max2);

            double min1 = imag1.MinArray();
            double min2 = imag2.MinArray();
            double min = Math.Min(min1, min2);

            double binhist = imag1.GetLength(0) * imag1.GetLength(1);
            double binjointhist = 2 * binhist;

            double[] hist1 = new double[256];
            double[] hist2 = new double[256];

            for (int i = 0; i < imag1.GetLength(0); i++)
                for (int j = 0; j < imag1.GetLength(1); j++)
                {
                    double ii = (255 * (imag1[i, j] - min) / (max - min));
                    double jj = (255 * (imag2[i, j] - min) / (max - min));


                    hist1[(int)ii] += 1;
                    hist2[(int)jj] += 1;


                }
            double[] jointhist = new double[hist1.Length];

            for (int i = 0; i < hist1.Length; i++)
            {
                jointhist[i] = hist1[i] + hist2[i];
            }
            double MI = 0;

            double d;
            for (int i = 0; i < hist1.Length; i++)
            {
                d = (jointhist[i] / binjointhist) * Math.Log((jointhist[i] / binjointhist) / ((hist1[i] / binhist) * (hist2[i] / binhist)));
                if (double.IsNaN(d) == false && double.IsInfinity(d) == false)
                    MI += d;
            }

            return MI;


        }

        public static double CNR(double avegS, double avegB, double stdS, double stdB)
        {
            double CNR = 2 * Math.Abs(avegS - avegB) / (stdS + stdB);
            return CNR;


        }
        public static List<double[,]> ROISelection(double[,] image)
        {

            List<double[,]> ROIList = new List<double[,]>();
            int height = image.GetLength(0) / 3;
            int length = image.GetLength(1) / 3;


            for (int i = 0; i < image.GetLength(0); i += height)
            {
                for (int j = 0; j < image.GetLength(1); j += length)
                {


                    double[,] ROI = new double[height, length];

                    for (int k = i; (k < i + height) && (k < image.GetLength(0)); k++)
                    {
                        for (int l = j; (l < j + length) && (l < image.GetLength(1)); l++)
                        {
                            ROI[k - i, l - j] = image[k, l];
                        }
                    }
                    ROIList.Add(ROI);





                }
            }
            return ROIList;


        }


        public static double[] QualityByPowerSpectrum(double[,] ROI, out double MaxFreqRealitive)
        {

            //   subtracted.MultiplyInPlace(subtracted);
            complex[,] freqSpace = FFTreal2complex(ROI);

            //  freqSpace= freqSpace.MakeFFTHumanReadable();
            ROI = freqSpace.Modulos();
            // Bitmap b = MathHelpLib.ImageProcessing.MathImageHelps.ConvertToBitmap(ROI);
            //int w = b.Width;
            double[] Spec = Turn2Dto1D(ROI);


            Spec[0] = 0;
            Spec[1] = 0;
            Spec[2] = 0;
            Spec[Spec.Length - 1] = 0;
            int nBins = 6;
            double[] ReducedSpec = new double[nBins];
            int binSize = (int)(Spec.Length / (float)nBins);
            double MaxFreq = 0;
            int MaxFreqI = 0;
            for (int i = 0; i < Spec.GetLength(0); i++)
            {
                int binNumber = (int)Math.Truncate(i / (float)binSize);
                if (binNumber < nBins)
                {
                    ReducedSpec[binNumber] += Spec[i];
                    if (Spec[i] > MaxFreq && i > Spec.Length / 2f)
                    {
                        MaxFreq = Spec[i];
                        MaxFreqI = i;
                    }
                }
            }



            MaxFreqRealitive = MaxFreqI / (double)Spec.Length;

            return ReducedSpec;
        }

        public static double[] Turn2Dto1D(double[,] Sum)
        {
            double[] Spec = new double[Sum.GetLength(0) / 2];
            double HalfX = Sum.GetLength(0) / 2d;
            double HalfY = Sum.GetLength(1) / 2d;
            double R = 0, dx = 0, dy = 0;

            double d;
            for (int i = 0; i < Sum.GetLength(0); i++)
                for (int j = 0; j < Sum.GetLength(1); j++)
                {
                    dx = i;
                    dy = j;
                    R = Math.Sqrt(dx * dx + dy * dy);
                    d = Sum[i, j];
                    if (R < HalfX && double.IsNaN(d) == false && double.IsInfinity(d) == false)
                        Spec[(int)Math.Truncate(R)] += d;
                }

            return Spec;
        }


        public static double[,] PowerSpectrum(List<double[,]> ROIs)
        {
            double[,] Sum = new double[ROIs[0].GetLength(0), ROIs[0].GetLength(0)];

            double[,] aveg = new double[ROIs[0].GetLength(0), ROIs[0].GetLength(0)];

            for (int i = 0; i < ROIs.Count; i++)
                for (int j = 0; j < ROIs[i].GetLength(0); j++)
                    for (int k = 0; k < ROIs[i].GetLength(1); k++)
                        aveg[j, k] += ROIs[i][j, k];
            aveg.DivideInPlace(ROIs.Count);

            double[,] arrayOut = new double[aveg.GetLength(0), aveg.GetLength(1)];


            for (int i = 0; i < ROIs.Count; i++)
            {
                double[,] subtracted = new double[aveg.GetLength(0), aveg.GetLength(1)];

                for (int j = 0; j < ROIs[i].GetLength(0); j++)
                    for (int k = 0; k < ROIs[i].GetLength(1); k++)
                        subtracted[j, k] = ROIs[i][j, k] - aveg[j, k];


                double[] XWindow = ConvolutionFilters.HanWindow(aveg.GetLength(0), aveg.GetLength(0));
                double[] YWindow = ConvolutionFilters.HanWindow(aveg.GetLength(1), aveg.GetLength(1));

                for (int j = 0; j < subtracted.GetLength(0); j++)
                {
                    for (int k = 0; k < subtracted.GetLength(1); k++)
                    {
                        subtracted[j, k] = subtracted[j, k] * YWindow[k];
                    }
                }
                for (int j = 0; j < subtracted.GetLength(1); j++)
                {
                    for (int k = 0; k < subtracted.GetLength(0); k++)
                    {
                        subtracted[k, j] = subtracted[k, j] * XWindow[k];
                    }
                }


                subtracted.MultiplyInPlace(subtracted);
                complex[,] freqSpace = FFTreal2complex(subtracted);

                Sum.AddInPlace(freqSpace.Modulos());

            }
            Sum.DivideInPlace(ROIs.Count);
            return Sum;
        }

        public static double Turn1D(double[,] Sum, double CutPercent)
        {
            double[] Spec = new double[Sum.GetLength(0) / 2];
            double HalfX = Sum.GetLength(0) / 2d;
            double HalfY = Sum.GetLength(1) / 2d;
            double R = 0, dx = 0, dy = 0;

            double d;
            for (int i = 0; i < Sum.GetLength(0); i++)
                for (int j = 0; j < Sum.GetLength(1); j++)
                {
                    dx = i - HalfX;
                    dy = j - HalfY;
                    R = Math.Sqrt(dx * dx + dy * dy);
                    d = Sum[i, j];
                    if (R < HalfX && double.IsNaN(d) == false && double.IsInfinity(d) == false)
                        Spec[(int)Math.Truncate(R)] += d;
                }

            double OutValue = 0;
            for (int i = (int)(Spec.Length * CutPercent); i < Spec.Length; i++)
            {
                d = Spec[i];
                if (double.IsNaN(d) == false && double.IsInfinity(d) == false)
                    OutValue += d;
            }

            OutValue /= (Sum.GetLength(0) * Sum.GetLength(1));
            return OutValue;
        }


        public static double DetectionTask(double[,] signal, double[,] background)
        {
            List<double[,]> BGRois = ROISelection(background);

            // if (signal.GetLength(0) != BGRois[0].GetLength(0) || signal.GetLength(1) != BGRois[0].GetLength(1) )
            //{

            int LengthX;
            int LengthY;

            if (signal.GetLength(0) > BGRois[0].GetLength(0) || signal.GetLength(1) > BGRois[0].GetLength(1))
            {
                LengthX = BGRois[0].GetLength(0);
                LengthY = BGRois[0].GetLength(1);
                double[,] SignalChopped = new double[LengthX, LengthY];

                for (int i = (signal.GetLength(0) - LengthX) / 2; i < (signal.GetLength(0) - LengthX) / 2 + LengthX; i++)
                {
                    for (int j = (signal.GetLength(1) - LengthY) / 2; j < (signal.GetLength(1) - LengthY) / 2 + LengthY; j++)
                    {
                        SignalChopped[i - (signal.GetLength(0) - LengthX) / 2, j - (signal.GetLength(1) - LengthY) / 2] = signal[i, j];
                    }
                }
                signal = SignalChopped;
            }

            //}



            complex[,] SignalDFT = FFTreal2complex(signal);
            double[,] ModulosSignal = SignalDFT.Modulos();

            background = PowerSpectrum(BGRois);
            ModulosSignal.DivideInPlaceErrorless(background);


            double sum = ModulosSignal.SumArrayCareful();
            sum /= (signal.GetLength(0) * signal.GetLength(1));



            return sum;

        }

        public static double[,] CrossCorrelationFFTFilteredReordered(double[,] Function, double[,] impulse, double percentagelow, double percentagehigh, out double SumFreq)
        {
            lock (CriticalSection)
            {
                int Length = 0;
                double[,] array1;
                double[,] array2;
                double[,] arrayOut;
                if (Function.Length > impulse.Length)
                    Length = Function.GetLength(0);
                else
                    Length = impulse.GetLength(0);

                if (Function.Length != impulse.Length)
                {
                    Length = (int)NearestPowerOf2(Length);
                    array1 = Function.ZeroPadArray2D(Length);
                    array2 = impulse.ZeroPadArray2D(Length);
                }
                else
                {
                    array1 = Function;
                    array2 = impulse;
                }
                array2.MakeFFTHumanReadable();

                complex[,] cArray1 = FFTreal2complex(array1);

                complex[,] cArray2 = FFTreal2complex(array2);
                cArray2.ConjugateInPlace();

                cArray1.MultiplyInPlace(cArray2);

                cArray1.MakeFFTHumanReadable();

                int halfX = (int)Math.Truncate((cArray1.GetLength(0)) / 2d);
                int halfY = (int)Math.Truncate((cArray1.GetLength(1)) / 2d);

                double max = Math.Sqrt(Math.Pow(halfX, 2) + Math.Pow(halfY, 2));
                double min = 0;
                double lower = ((max - min) * percentagelow) + min;
                lower = lower * lower;
                double higher = ((max - min) * percentagehigh) + min;// max - ((max - min) * percentagehigh);
                higher = higher * higher;
                complex zero = new complex(0, 0);

                int LenX = cArray1.GetLength(0);
                int LenY = cArray1.GetLength(1);
                double x, y, R;
                SumFreq = 0;
                for (int i = 0; i < LenX; i++)
                    for (int j = 0; j < LenY; j++)
                    {
                        x = (i - halfX);
                        y = (j - halfY);
                        R = (x * x + y * y);
                        if (R < lower || R > higher)
                        {
                            // cArray1[halfX, halfY] += cArray1[i, j];
                            cArray1[i, j] = zero;
                        }
                        else
                        {
                            SumFreq += cArray1[i, j].Abs();
                        }
                    }

                SumFreq = SumFreq / (3.1415 * Math.Abs(higher - lower));

                cArray1.MakeFFTMachineReadable();

                arrayOut = iFFTcomplex2real(cArray1).MakeFFTHumanReadable();

                double dL = Length;
                arrayOut.MultiplyInPlace(1d / dL);

                return arrayOut;
            }
        }

        #endregion

        #region Readouts
        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[] MakeFFTHumanReadable(this double[] MachineArray)
        {
            int d1 = MachineArray.Length;
            double[] HumanReadable = new double[d1];
            int Length = d1;
            int hLength = d1 / 2;
            for (int i = 0; i < hLength; i++)
            {
                HumanReadable[i] = MachineArray[hLength + i];
                HumanReadable[i + hLength] = MachineArray[i];
            }
            return HumanReadable;
        }

        /// <summary>
        /// Moves the origin from the center of the image to the 4 courners
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[] MakeFFTMachineReadable(this double[] MachineArray)
        {
            int d1 = MachineArray.Length;
            double[] HumanReadable = new double[d1];
            int Length = d1;
            int hLength = d1 / 2;
            for (int i = 0; i < hLength; i++)
            {
                HumanReadable[i] = MachineArray[hLength + i];
                HumanReadable[i + hLength] = MachineArray[i];
            }
            return HumanReadable;
        }

        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center.
        /// adds the frequency information to the 1D data
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[,] MakeFFTHumanReadableAndGraphable(this double[] MachineArray)
        {
            int d1 = MachineArray.Length;
            double[,] HumanReadable = new double[2, d1];
            int Length = d1;
            int hLength = d1 / 2;
            for (int i = 0; i < hLength; i++)
            {
                HumanReadable[1, i] = MachineArray[hLength + i];
                HumanReadable[1, i + hLength] = MachineArray[i];
                HumanReadable[0, i] = i - hLength;
                HumanReadable[0, i + hLength] = i;
            }
            return HumanReadable;
        }

        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center.
        /// takes the magnitude at each frequency
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[] MakeFFTPowerSpectrum(this double[] MachineArray)
        {
            int d1 = MachineArray.Length;
            int Length = d1;
            int hLength = d1 / 2;

            double[] HumanReadable = new double[hLength];
            for (int i = 0; i < hLength; i++)
            {
                HumanReadable[i] = (MachineArray[Length - 2 - i] + MachineArray[i]) / 2d;
            }
            return HumanReadable;
        }

        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center.
        /// takes the magnitude at each frequency, adds the frequency axis
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[,] MakeFFTPowerSpectrumAndGraphable(this double[] MachineArray)
        {
            int d1 = MachineArray.Length;
            int Length = d1;
            int hLength = d1 / 2;

            double[,] HumanReadable = new double[2, hLength];
            for (int i = 0; i < hLength; i++)
            {
                HumanReadable[1, i] = (/*MachineArray[LengthX-2 - X] +*/ MachineArray[i] * MachineArray[i]) / 2d;
                HumanReadable[0, i] = i;
            }
            return HumanReadable;
        }

        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center.
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[,] MakeFFTHumanReadable(this double[,] MachineArray)
        {
            int d1 = MachineArray.GetLength(0);
            int d2 = MachineArray.GetLength(1);
            double[,] HumanReadable = new double[d1, d2];
            int hCols = d1 / 2;
            int hRows = d2 / 2;

            for (int i = 0; i < hCols; i++)
            {
                for (int j = 0; j < hRows; j++)
                {
                    HumanReadable[i, j] = MachineArray[i + hCols, j + hRows];
                    HumanReadable[i + hCols, j + hRows] = MachineArray[i, j];
                    HumanReadable[i + hCols, j] = MachineArray[i, j + hRows];
                    HumanReadable[i, j + hRows] = MachineArray[i + hCols, j];
                }
            }

            return HumanReadable;
        }


        /// <summary>
        /// FFTW returns data with the origin distributed to the 4 courners.  this adjusts it so the origin is at the center.
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static float[,,] MakeFFTHumanReadable(this float[,,] MachineArray)
        {
            int d1 = MachineArray.GetLength(0);
            int d2 = MachineArray.GetLength(1);
            float[,,] HumanReadable = new float[d1, d2,1];
            int hCols = d1 / 2;
            int hRows = d2 / 2;

            for (int i = 0; i < hCols; i++)
            {
                for (int j = 0; j < hRows; j++)
                {
                    HumanReadable[i, j,0] = MachineArray[i + hCols, j + hRows,0];
                    HumanReadable[i + hCols, j + hRows,0] = MachineArray[i, j,0];
                    HumanReadable[i + hCols, j,0] = MachineArray[i, j + hRows,0];
                    HumanReadable[i, j + hRows,0] = MachineArray[i + hCols, j,0];
                }
            }

            return HumanReadable;
        }

        /// <summary>
        /// Moves the origin from the center of the image to the 4 courners
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static double[,] MakeFFTMachineReadable(this double[,] MachineArray)
        {
            int d1 = MachineArray.GetLength(0);
            int d2 = MachineArray.GetLength(1);
            double[,] HumanReadable = new double[d1, d2];
            int hCols = d1 / 2;
            int hRows = d2 / 2;

            for (int i = 0; i < hCols; i++)
            {
                for (int j = 0; j < hRows; j++)
                {
                    HumanReadable[i, j] = MachineArray[i + hCols, j + hRows];
                    HumanReadable[i + hCols, j + hRows] = MachineArray[i, j];
                    HumanReadable[i + hCols, j] = MachineArray[i, j + hRows];
                    HumanReadable[i, j + hRows] = MachineArray[i + hCols, j];
                }
            }

            return HumanReadable;
        }
        #endregion
    }
}
