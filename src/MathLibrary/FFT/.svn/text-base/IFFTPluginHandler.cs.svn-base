using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibrary.FFT
{
  
        /// <summary>
        /// Used to make the fft libraries a little more interchangable.  FFTW is the best, but seems to be a little problematic.  This allows the lib to be changed out
        /// </summary>
        public interface IFFTPluginHandler
        {

            //FFTW allows any size array, and allows any of the following transformation.  it is best to simulate this behavior if possible
            #region 1D FFTs
            double[] FFTreal2real(double[] array);
            unsafe void FFTreal2real(double* pArray, int Length, double* pArrayOut);
            double[] iFFTreal2real(double[] array);
            unsafe void iFFTreal2real(double* pArray, int Length, double* pArrayOut);

            complex[] FFTcomplex2complex(complex[] array);
            unsafe void FFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut);

            complex[] iFFTcomplex2complex(complex[] array);
            unsafe void iFFTcomplex2complex(complex* pArray, int Length, complex* pArrayOut);
            complex[] FFTreal2complex(double[] array);
            unsafe void FFTreal2complex(double* pArray, int Length, complex* pOutArray);
            double[] iFFTcomplex2real(complex[] array);
            unsafe void iFFTcomplex2real(complex* pArray, int Length, double* pArrayOut);
            #endregion

            #region 2D FFTs

            double[,] FFTreal2real(double[,] array);

            double[,] iFFTreal2real(double[,] array);

            complex[,] FFTcomplex2complex(complex[,] array);

            complex[,] iFFTcomplex2complex(complex[,] array);

            complex[,] FFTreal2complex(double[,] array);

            double[,] iFFTcomplex2real(complex[,] array);

            #region Unsafes
            unsafe void FFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray);
            unsafe void iFFTreal2real(double* pArray, int Length1, int Length2, double* pOutArray);
            unsafe void FFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray);
            unsafe void iFFTcomplex2complex(complex* pArray, int Length1, int Length2, complex* pOutArray);

            unsafe void FFTreal2complex(double* pArray, int Length1, int Length2, complex* pOutArray);

            unsafe void iFFTcomplex2real(complex* pArray, int Length1, int Length2, double* pOutArray);
            #endregion
            #endregion
        }
    }



