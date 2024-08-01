using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fftwlib;

namespace MathLibrary.FFT
{
    /// <summary>
    /// Performs all the needed transformation.   keep in mind that fftw is not threadsafe, so this adapter only allows 1 at a time.
    /// </summary>
    public class FFTWLib : IFFTPluginHandler
    {
        static object CriticalSectionLock = new object();


        #region 1D FFTs
        public double[] FFTreal2real(double[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[] fout = new double[array.Length];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.r2r_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void FFTreal2real(double* pArray, int Length, double* pArrayOut)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                fplan2 = fftw.r2r_1d(Length, (IntPtr)pArray, (IntPtr)pArrayOut, fftw_kind.DHT, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public double[] iFFTreal2real(double[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[] fout = new double[array.Length];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.r2r_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void iFFTreal2real(double* pArray, int Length, double* pArrayOut)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;

                fplan2 = fftw.r2r_1d(Length, (IntPtr)pArray, (IntPtr)pArrayOut, fftw_kind.DHT, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public complex[] FFTcomplex2complex(complex[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[] fout = new complex[array.Length];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_direction.Forward, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void FFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;

                fplan2 = fftw.dft_1d(Length, (IntPtr)pArray, (IntPtr)pArrayOut, fftw_direction.Forward, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }

        }

        public complex[] iFFTcomplex2complex(complex[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[] fout = new complex[array.Length];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_direction.Backward, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void iFFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;

                fplan2 = fftw.dft_1d(Length, (IntPtr)pArray, (IntPtr)pArrayOut, fftw_direction.Backward, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }


        public complex[] FFTreal2complex(double[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[] fout = new complex[array.Length];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_r2c_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void FFTreal2complex(double* pArray, int Length, complex* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                fplan2 = fftw.dft_r2c_1d(Length, (IntPtr)pArray, (IntPtr)pOutArray, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public double[] iFFTcomplex2real(complex[] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[] fout = new double[array.Length];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.dft_c2r_1d(array.Length, (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }
        public unsafe void iFFTcomplex2real(complex* pArray, int Length, double* pArrayOut)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                fplan2 = fftw.dft_c2r_1d(Length, (IntPtr)pArray, (IntPtr)pArrayOut, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }
        #endregion

        #region 2D FFTs


        public double[,] FFTreal2real(double[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[,] fout = new double[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.r2r_2d(array.GetLength(0), array.GetLength(1), (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_kind.DHT, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }

        public double[,] iFFTreal2real(double[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[,] fout = new double[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.r2r_2d(array.GetLength(0), array.GetLength(1), (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_kind.DHT, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }

        public complex[,] FFTcomplex2complex(complex[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[,] fout = new complex[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_2d(array.GetLength(0), array.GetLength(1), (double*)hin, (double*)hout, fftw_direction.Forward, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }

        public complex[,] iFFTcomplex2complex(complex[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[,] fout = new complex[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_2d(array.GetLength(0), array.GetLength(1), (double*)hin, (double*)hout, fftw_direction.Backward, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }

        public complex[,] FFTreal2complex(double[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex[,] fout = new complex[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (double* hin = array)
                    {
                        fixed (complex* hout = fout)
                        {
                            fplan2 = fftw.dft_r2c_2d(array.GetLength(0), array.GetLength(1), (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }


        }

        public double[,] iFFTcomplex2real(complex[,] array)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double[,] fout = new double[array.GetLength(0), array.GetLength(1)];
                unsafe
                {
                    fixed (complex* hin = array)
                    {
                        fixed (double* hout = fout)
                        {
                            fplan2 = fftw.dft_c2r_2d(array.GetLength(0), array.GetLength(1), (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                            fftw.execute(fplan2);
                            fftw.destroy_plan(fplan2);
                        }
                    }
                }
                return fout;
            }
        }

        #region Unsafes
        public unsafe void FFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;

                double* hin = pArray;
                double* hout = pOutArray;
                {
                    fplan2 = fftw.r2r_2d(Length1, Length2, (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_kind.DHT, fftw_flags.Estimate);
                    fftw.execute(fplan2);
                    fftw.destroy_plan(fplan2);
                }
            }

        }

        public unsafe void iFFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double* hin = pArray;
                double* hout = pOutArray;
                {
                    fplan2 = fftw.r2r_2d(Length1, Length2, (IntPtr)hin, (IntPtr)hout, fftw_kind.DHT, fftw_kind.DHT, fftw_flags.Estimate);
                    fftw.execute(fplan2);
                    fftw.destroy_plan(fplan2);
                }
            }
        }

        public unsafe void FFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex* hin = pArray;
                complex* hout = pOutArray;
                fplan2 = fftw.dft_2d(Length1, Length2, (double*)hin, (double*)hout, fftw_direction.Forward, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public unsafe void iFFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex* hin = pArray;
                complex* hout = pOutArray;
                fplan2 = fftw.dft_2d(Length1, Length2, (double*)hin, (double*)hout, fftw_direction.Backward, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public unsafe void FFTreal2complex(double* pArray, int Length1, int Length2, complex* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                double* hin = pArray;
                complex* hout = pOutArray;
                fplan2 = fftw.dft_r2c_2d(Length1, Length2, (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }

        public unsafe void iFFTcomplex2real(complex* pArray, int Length1, int Length2, double* pOutArray)
        {
            lock (CriticalSectionLock)
            {
                IntPtr fplan2;
                complex* hin = pArray;
                double* hout = pOutArray;
                fplan2 = fftw.dft_c2r_2d(Length1, Length2, (IntPtr)hin, (IntPtr)hout, fftw_flags.Estimate);
                fftw.execute(fplan2);
                fftw.destroy_plan(fplan2);
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// cleans up the unwanted output format from the fft.  (i.e. only 1/2 the quadrands have data
        /// </summary>
        /// <param name="iFFTArray"></param>
        /// <returns></returns>
        public static double[,] Correct2DRealOutput(double[,] iFFTArray)
        {
            double[,] iFFTArrayO = iFFTArray.MakeFFTHumanReadable();
            int HalfX = iFFTArrayO.GetLength(0) / 2;
            int HalfY = iFFTArrayO.GetLength(1) / 2;
            for (int i = 0; i < HalfX; i++)
            {
                for (int j = 0; j < HalfY; j++)
                {
                    iFFTArrayO[i, j] = -1 * iFFTArrayO[i, j];
                }
            }

            for (int i = HalfX + 1; i < iFFTArrayO.GetLength(0); i++)
            {
                for (int j = HalfY + 1; j < iFFTArrayO.GetLength(1); j++)
                {
                    iFFTArrayO[i, j] = -1 * iFFTArrayO[i, j];
                }
            }
            return iFFTArrayO;
        }
    }
}
