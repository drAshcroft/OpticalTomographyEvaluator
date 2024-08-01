using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MathLibrary.FFT
{
    /// <summary>
    /// Allows C# to have a mostly useful complex dataset.  there are only two real data pieces in the 
    /// struct and it has to remain that way or all the calculations will fail.
    /// the struct is set up to communicate with the FFT libs.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Ansi)]
    public struct complex
    {
        [FieldOffset(0)]
        public double real;
        [FieldOffset(8)]
        public double imag;
        public complex(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }
        /// <summary>
        /// returns the magnitide of the complex number
        /// </summary>
        /// <returns></returns>
        public double Abs()
        {
            return Math.Sqrt(real * real + imag * imag);
        }

        #region Operators
        public static complex operator +(complex c1, complex c2)
        {
            return new complex(c1.real + c2.real, c1.imag + c2.imag);
        }

        public static complex operator +(complex c1, double c2)
        {
            return new complex(c1.real + c2, c1.imag);
        }

        public static complex operator -(complex c1, complex c2)
        {
            return new complex(c1.real - c2.real, c1.imag - c2.imag);
        }

        public static complex operator -(complex c1, double c2)
        {
            return new complex(c1.real - c2, c1.imag);
        }

        public static complex operator *(complex c1, complex c2)
        {
            return new complex(c1.real * c2.real - c1.imag * c2.imag, c1.real * c2.imag + c1.imag * c2.real);
        }

        public static complex operator *(complex c1, double c2)
        {
            return new complex(c1.real * c2, c1.imag * c2);
        }

        public static complex operator /(complex c1, complex c2)
        {
            double denom = c2.real * c2.real + c2.imag * c2.imag;
            return new complex((c1.real * c2.real + c1.imag * c2.imag) / denom, (c1.real * c2.imag - c1.imag * c2.real) / denom);
        }

        public static complex operator /(complex c1, double c2)
        {
            return new complex(c1.real / c2, c1.imag / c2);
        }

        public static complex operator +(double c2, complex c1)
        {
            return new complex(c1.real + c2, c1.imag);
        }

        public static complex operator -(double c2, complex c1)
        {
            return new complex(c2 - c1.real, c1.imag);
        }

        public static complex operator *(double c2, complex c1)
        {
            return new complex(c1.real * c2, c1.imag * c2);
        }

        public static complex operator /(double c2, complex c1)
        {
            double denom = c1.real * c1.real + c1.imag * c1.imag;
            return new complex((c2 * c1.real) / denom, (c2 * c1.imag) / denom);
        }

        public static bool operator ==(double c2, complex c1)
        {
            return (c2 == c1.real && c1.imag == 0);
        }
        public static bool operator !=(double c2, complex c1)
        {
            return !(c2 == c1.real && c1.imag == 0);
        }

        public static bool operator ==(complex c1, double c2)
        {
            return (c2 == c1.real && c1.imag == 0);
        }
        public static bool operator !=(complex c1, double c2)
        {
            return !(c2 == c1.real && c1.imag == 0);
        }

        public static bool operator ==(complex c1, complex c2)
        {
            return (c2.real == c1.real && c1.imag == c2.imag);
        }
        public static bool operator !=(complex c1, complex c2)
        {
            return !(c2.real == c1.real && c1.imag == c2.imag);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(complex))
            {
                return this == (complex)obj;
            }
            else if (obj.GetType() == typeof(double))
            {
                return this == (double)obj;
            }
            else if (obj.GetType() == typeof(int))
            {
                return this == (double)(int)obj;
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        public override string ToString()
        {
            return (String.Format("{0} + {1}i ", real, imag));
        }
    }

    public static class MathComplexHelps
    {

        /// <summary>
        /// takes an array and performs a conjugation on every element
        /// </summary>
        /// <param name="array"></param>
        public static void ConjugateInPlace(this complex[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j].imag = -1 * array[i, j].imag;
                }
            }
        }

        /// <summary>
        /// takes an array and performs a conjugation on every element
        /// </summary>
        /// <param name="array"></param>
        public static void ConjugateInPlace(this complex[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i].imag = -1 * array[i].imag;
            }
        }

        /// <summary>
        /// takes an array and performs a conjugation on every element
        /// </summary>
        /// <param name="array"></param>
        public static double[,] Modulos(this complex[,] array)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j].real * array[i, j].real + array[i, j].imag * array[i, j].imag;
                }
            }
            return OutArray;
        }

        /// <summary>
        /// takes an array and performs a conjugation on every element
        /// </summary>
        /// <param name="array"></param>
        public static double[,] Abs(this complex[,] array)
        {
            double[,] OutArray = new double[array.GetLength(0), array.GetLength(1)];
            double r = 0, I = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    r = array[i, j].real;
                    I = array[i, j].imag;
                    OutArray[i, j] = Math.Sqrt(r * r + I * I);
                }
            }
            return OutArray;
        }


        /// <summary>
        /// takes an array and performs a conjugation on every element
        /// </summary>
        /// <param name="array"></param>
        public static double[] Abs(this complex[] array)
        {
            double[] OutArray = new double[array.Length];
            double r = 0, I = 0;
            for (int i = 0; i < array.Length; i++)
            {
                    r = array[i].real;
                    I = array[i].imag;
                    OutArray[i] = Math.Sqrt(r * r + I * I);
            }
            return OutArray;
        }

        /// <summary>
        /// creates a new complex array with all the doubles from the last array becoming
        /// the real components
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static complex[] ConvertToComplex(this double[] array)
        {
            complex[] outArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[i] = new complex(array[i], 0);
            }
            return outArray;
        }

        /// <summary>
        /// creates a new complex array with all the doubles from the last array becoming
        /// the real components
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static complex[,] ConvertToComplex(this double[,] array)
        {
            complex[,] outArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(0); j++)
                {
                    outArray[i, j] = new complex(array[i, j], 0);
                }
            }
            return outArray;
        }

        /// <summary>
        /// Creates a new double array where each element is the magnitude of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] ConvertToDoubleMagnitude(this complex[] array)
        {
            double[] outArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[i] = array[i].Abs();
            }
            return outArray;

        }

        /// <summary>
        /// Creates a new double array where each element is the magnitude of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[,] ConvertToDoubleMagnitude(this complex[,] array)
        {
            double[,] outArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    outArray[i, j] = array[i, j].Abs();
                }
            }
            return outArray;

        }

        /// <summary>
        /// Creates a new double array where each element is the real component of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] ConvertToDoubleReal(this complex[] array)
        {
            double[] outArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[i] = array[i].real;
            }
            return outArray;

        }

        /// <summary>
        /// Creates a new double array where each element is the imaginary component of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] ConvertToDoubleImag(this complex[] array)
        {
            double[] outArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outArray[i] = array[i].imag;
            }
            return outArray;

        }
        /// <summary>
        /// Creates a new double array where each element is the real component of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[,] ConvertToDoubleReal(this complex[,] array)
        {
            double[,] outArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    outArray[i, j] = array[i, j].real;
                }
            }
            return outArray;

        }
        /// <summary>
        /// Creates a new double array where each element is the imagine component of the complex array element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[,] ConvertToDoubleImag(this complex[,] array)
        {
            double[,] outArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    outArray[i, j] = array[i, j].imag;
                }
            }
            return outArray;

        }
        /// <summary>
        /// Moves the fft origin from the 4 corners, where the machine likes it to the center
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static complex[] MakeFFTHumanReadable(this complex[] MachineArray)
        {
            int d1 = MachineArray.Length;
            complex[] HumanReadable = new complex[d1];
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
        /// moves the fft origin from the center of the graph to the 4 edges
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static complex[] MakeFFTMachineReadable(this complex[] MachineArray)
        {
            return MakeFFTHumanReadable(MachineArray);
        }

        /// <summary>
        /// Moves the fft origin from the 4 corners, where the machine likes it to the center
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static complex[,] MakeFFTHumanReadable(this complex[,] MachineArray)
        {
            int d1 = MachineArray.GetLength(0);
            int d2 = MachineArray.GetLength(1);
            complex[,] HumanReadable = new complex[d1, d2];
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
        /// moves the fft origin from the center of the graph to the 4 edges
        /// </summary>
        /// <param name="MachineArray"></param>
        /// <returns></returns>
        public static complex[,] MakeFFTMachineReadable(this complex[,] MachineArray)
        {
            return MakeFFTHumanReadable(MachineArray);
        }

        #region Array_Arithmetic

        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[] array, double addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] AddToArray(this complex[] array, double addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[] array, double addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] SubtractFromArray(this complex[] array, double addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[] array, double Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] MultiplyToArray(this complex[] array, double Multiplicant)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[] array, double Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] DivideToArray(this complex[] array, double Divisor)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[] array, complex[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] AddToArray(this complex[] array, complex[] addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[] array, complex[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] SubtractFromArray(this complex[] array, complex[] addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[] array, complex[] Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] MultiplyToArray(this complex[] array, complex[] Multiplicant)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[] array, complex[] Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] DivideToArray(this complex[] array, complex[] Divisor)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, double addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, double addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, double addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] SubtractFromArray(this complex[,] array, double addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, double Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, double Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, double Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, double Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, complex[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, complex[,] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, complex[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] SubtractFromArray(this complex[,] array, complex[,] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, complex[,] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, complex[,] Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, complex[,] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, complex[,] Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, complex[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, complex[] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, complex[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] SubtractFromArray(this complex[,] array, complex[] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, complex[] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, complex[] Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, complex[] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, complex[] Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[] array, complex addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] AddToArray(this complex[] array, complex addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[] array, complex addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] SubtractFromArray(this complex[] array, complex addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[] array, complex Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] MultiplyToArray(this complex[] array, complex Multiplicant)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[] array, complex Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor;
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] DivideToArray(this complex[] array, complex Divisor)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor;
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[] array, double[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += addValue[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] AddToArray(this complex[] array, double[] addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] + addValue[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[] array, double[] addValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] -= addValue[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] SubtractFromArray(this complex[] array, double[] addValue)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] - addValue[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[] array, double[] Multiplicant)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= Multiplicant[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] MultiplyToArray(this complex[] array, double[] Multiplicant)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] * Multiplicant[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[] array, double[] Divisor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] /= Divisor[i];
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[] DivideToArray(this complex[] array, double[] Divisor)
        {
            complex[] OutArray = new complex[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                OutArray[i] = array[i] / Divisor[i];
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, complex addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, complex addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, complex addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] SubtractFromArray(this complex[,] array, complex addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, complex Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, complex Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, complex Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor;
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, complex Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor;
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, double[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, double[,] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, double[,] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] SubtractFromArray(this complex[,] array, double[,] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, double[,] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, double[,] Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, double[,] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i, j];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, double[,] Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i, j];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void AddInPlace(this complex[,] array, double[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] += addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] AddToArray(this complex[,] array, double[] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] + addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void SubtractInPlace(this complex[,] array, double[] addValue)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] -= addValue[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>

        public static complex[,] SubtractFromArray(this complex[,] array, double[] addValue)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] - addValue[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void MultiplyInPlace(this complex[,] array, double[] Multiplicant)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= Multiplicant[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] MultiplyToArray(this complex[,] array, double[] Multiplicant)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] * Multiplicant[i];
                }
            }
            return OutArray;
        }
        /// <summary>
        /// Performs operation on inputed array, changing its value. There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        public static void DivideInPlace(this complex[,] array, double[] Divisor)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] /= Divisor[i];
                }
            }
        }
        /// <summary>
        /// Creates a new array that is the result of performing the operation on each element of the original array
        /// There is no error checking
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public static complex[,] DivideToArray(this complex[,] array, double[] Divisor)
        {
            complex[,] OutArray = new complex[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    OutArray[i, j] = array[i, j] / Divisor[i];
                }
            }
            return OutArray;
        }

        #endregion


        /// <summary>
        /// Performs full(slow) convolution of two arrays
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] Convolute(complex[] Array1, double[] Impulse)
        {
            complex[] ArrayOut = new complex[Array1.Length + Impulse.Length];
            int L1 = Array1.Length;
            int L2 = Impulse.Length;

            unsafe
            {
                complex p1;
                double* p2;
                complex* pOut;
                fixed (double* pArray2 = Impulse)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int i = 0; i < L1; i++)
                        {
                            p1 = Array1[i];
                            p2 = pArray2;
                            pOut = pArrayOut + i;
                            for (int j = 0; j < L2; j++)
                            {
                                *pOut += p1 * (*p2);
                                p2++;
                                pOut++;
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }

        /// <summary>
        /// Performs  full(slow) convolution of two arrays
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static unsafe void Convolute(complex* Array1, int Length1, double* Impulse, int Length2, complex* pArrayOut)
        {
            complex p1;
            double* p2;
            complex* pOut;
            for (int i = 0; i < Length1; i++)
            {
                p1 = Array1[i];
                p2 = Impulse;
                pOut = pArrayOut + i;
                for (int j = 0; j < Length2; j++)
                {
                    *pOut += p1 * (*p2);
                    p2++;
                    pOut++;
                }
            }
        }

        /// <summary>
        /// Performs realspace convolution of two arrays
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] ConvoluteChop(complex[] Array1, double[] Impulse)
        {
            int Length;
            int Length2;

            if (Array1.Length < Impulse.Length)
            {
                Length = Array1.Length;
            }
            else
            {
                Length = Impulse.Length;
            }
            complex[] ArrayOut = new complex[Length];

            Length2 = Array1.Length + Impulse.Length;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;


            int L1 = Array1.Length;
            int L2 = Impulse.Length;
            int sI, eI;

            unsafe
            {
                complex p1;
                double* p2;
                complex* pOut;
                fixed (double* pArray2 = Impulse)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int i = 0; i < L1; i++)
                        {
                            p1 = Array1[i];
                            sI = StartI - i;
                            eI = EndI - i;
                            if (eI > L2) eI = L2;
                            if (sI < 0) sI = 0;
                            if (sI < eI)
                            {
                                p2 = pArray2 + sI;
                                pOut = pArrayOut + i + sI - StartI;
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
            return ArrayOut;
        }
        /// <summary>
        /// Performs realspace convolution of two arrays
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static unsafe void ConvoluteChop(complex* Array1, int Length1, double* pImpulse, int Length2, complex* pArrayOut)
        {
            complex[] ArrayOut = new complex[Length1];

            int LengthOut = Length1 + Length2;
            int StartI = LengthOut / 2 - Length1 / 2;
            int EndI = LengthOut / 2 + Length1 / 2;
            int sI, eI;

            complex p1;
            double* p2;
            complex* pOut;

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
                        *pOut += p1 * (*p2);
                        pOut++;
                        p2++;
                    }
                }
            }
        }
        /// <summary>
        /// Performs realspace convolution of two arrays
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static unsafe void ConvoluteChop(complex* Array1, int Length1, double* pImpulse, int Length2, complex* pArrayOut, int Stride)
        {

            int LengthWhole = Length1 + Length2;

            int StartI = LengthWhole / 2 - Length1 / 2;
            int EndI = LengthWhole / 2 + Length1 / 2;

            int sI, eI;


            complex p1;
            double* p2;
            complex* pOut;


            for (int i = 0; i < Length1; i++)
            {
                p1 = Array1[i * Stride];
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
                        *pOut += p1 * (*p2);
                        pOut += Stride;
                        p2++;
                    }
                }
            }
        }




        /// <summary>
        /// Performs realspace convolution of two arrays, along only one axis.  The means that the lines of the image do not influence each other
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        private static complex[,] ConvoluteChopXAxis(complex[,] Array1, double[] Array2)
        {
            int L1 = Array1.GetLength(0);
            int L2 = Array2.Length;

            int Length;
            int Length2;

            if (L1 < L2)
            {
                Length = L1;
            }
            else
            {
                Length = L2;
            }
            complex[,] ArrayOut = new complex[Length, Array1.GetLength(1)];

            Length2 = L1 + L2;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;

            int Array1LY = Array1.GetLength(1);
            int sI, eI;
            unsafe
            {
                complex p1;
                double* p2;
                complex* pOut;
                fixed (double* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int m = 0; m < Array1.GetLength(1); m++)
                        {
                            for (int i = 0; i < L1; i++)
                            {
                                p1 = Array1[i, m];
                                sI = StartI - i;
                                eI = EndI - i;
                                if (eI > L2) eI = L2;
                                if (sI < 0) sI = 0;
                                if (sI < eI)
                                {
                                    p2 = pArray2 + sI;
                                    pOut = pArrayOut + m;
                                    for (int j = sI; j < eI; j++)
                                    {
                                        //ArrayOut[X + y - StartIX, z] += CenterValue * (*p2);
                                        *pOut += p1 * (*p2);
                                        p2++;
                                        pOut += Array1LY;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }

        /// <summary>
        /// Performs realspace convolution of two arrays, along only one axis.  The means that the lines of the image do not influence each other
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        private static complex[,] ConvoluteChopYAxis(complex[,] Array1, double[] Array2)
        {
            int L1 = Array1.GetLength(1);
            int L2 = Array2.Length;

            int Length;
            int Length2;

            if (L1 < L2)
            {
                Length = L1;
            }
            else
            {
                Length = L2;
            }
            complex[,] ArrayOut = new complex[Array1.GetLength(0), Length];

            Length2 = L1 + L2;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;

            int Array1LY = Array1.GetLength(0);
            int sI, eI;
            unsafe
            {
                complex p1;
                double* p2;
                complex* pOut;
                fixed (double* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int m = 0; m < Array1.GetLength(0); m++)
                        {
                            for (int i = 0; i < L1; i++)
                            {
                                p1 = Array1[m, i];
                                sI = StartI - i;
                                eI = EndI - i;
                                if (eI > L2) eI = L2;
                                if (sI < 0) sI = 0;
                                if (sI < eI)
                                {
                                    p2 = pArray2 + sI;
                                    pOut = pArrayOut + m * Array1LY;
                                    for (int j = sI; j < eI; j++)
                                    {
                                        *pOut += p1 * (*p2);
                                        p2++;
                                        pOut++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }
        /// <summary>
        /// Performs realspace convolution of two arrays
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] ConvoluteChopSlow(complex[] Array1, double[] Impulse)
        {
            complex[] ArrayOut = Convolute(Array1, Impulse);

            int Length;

            if (Array1.Length < Impulse.Length)
            {
                Length = Array1.Length;
            }
            else
            {
                Length = Impulse.Length;
            }
            complex[] ArrayOut2 = new complex[Length];
            int cc = 0;
            int Length2 = ArrayOut.Length / 2 + Length / 2;
            for (int i = (int)(ArrayOut.Length / 2 - Length / 2); i < Length2; i++)
            {
                ArrayOut2[cc] = ArrayOut[i];
                cc++;
            }

            return ArrayOut2;
        }


        /// <summary>
        /// Performs full(slow) convolution of two arrays
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] Convolute(complex[] Array1, complex[] Array2)
        {
            complex[] ArrayOut = new complex[Array1.Length + Array2.Length];
            int L1 = Array1.Length;
            int L2 = Array2.Length;

            unsafe
            {
                complex p1;
                complex* p2;
                complex* pOut;
                fixed (complex* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int i = 0; i < L1; i++)
                        {
                            p1 = Array1[i];
                            p2 = pArray2;
                            pOut = pArrayOut + i;
                            for (int j = 0; j < L2; j++)
                            {
                                *pOut += p1 * (*p2);
                                p2++;
                                pOut++;
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }

        /// <summary>
        /// Performs realspace convolution of two arrays
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] ConvoluteChop(complex[] Array1, complex[] Array2)
        {
            int Length;
            int Length2;

            if (Array1.Length < Array2.Length)
            {
                Length = Array1.Length;
            }
            else
            {
                Length = Array2.Length;
            }
            complex[] ArrayOut = new complex[Length];

            Length2 = Array1.Length + Array2.Length;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;


            int L1 = Array1.Length;
            int L2 = Array2.Length;
            int sI, eI;

            unsafe
            {
                complex p1;
                complex* p2;
                complex* pOut;
                fixed (complex* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int i = 0; i < L1; i++)
                        {
                            p1 = Array1[i];
                            sI = StartI - i;
                            eI = EndI - i;
                            if (eI > L2) eI = L2;
                            if (sI < 0) sI = 0;
                            if (sI < eI)
                            {
                                p2 = pArray2 + sI;
                                pOut = pArrayOut + i + sI - StartI;
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
            return ArrayOut;
        }



        private static complex[,] ConvoluteChopXAxis(complex[,] Array1, complex[] Array2)
        {
            int L1 = Array1.GetLength(0);
            int L2 = Array2.Length;

            int Length;
            int Length2;

            if (L1 < L2)
            {
                Length = L1;
            }
            else
            {
                Length = L2;
            }
            complex[,] ArrayOut = new complex[Length, Array1.GetLength(1)];

            Length2 = L1 + L2;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;

            int Array1LY = Array1.GetLength(1);
            int sI, eI;
            unsafe
            {
                complex p1;
                complex* p2;
                complex* pOut;
                fixed (complex* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int m = 0; m < Array1.GetLength(1); m++)
                        {
                            for (int i = 0; i < L1; i++)
                            {
                                p1 = Array1[i, m];
                                sI = StartI - i;
                                eI = EndI - i;
                                if (eI > L2) eI = L2;
                                if (sI < 0) sI = 0;
                                if (sI < eI)
                                {
                                    p2 = pArray2 + sI;
                                    pOut = pArrayOut + m;
                                    for (int j = sI; j < eI; j++)
                                    {
                                        //ArrayOut[X + y - StartIX, z] += CenterValue * (*p2);
                                        *pOut += p1 * (*p2);
                                        p2++;
                                        pOut += Array1LY;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }

        /// <summary>
        /// Performs realspace convolution of two arrays, only each line is considered seperately
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        private static complex[,] ConvoluteChopYAxis(complex[,] Array1, complex[] Array2)
        {
            int L1 = Array1.GetLength(1);
            int L2 = Array2.Length;

            int Length;
            int Length2;

            if (L1 < L2)
            {
                Length = L1;
            }
            else
            {
                Length = L2;
            }
            complex[,] ArrayOut = new complex[Array1.GetLength(0), Length];

            Length2 = L1 + L2;

            int StartI = Length2 / 2 - Length / 2;
            int EndI = Length2 / 2 + Length / 2;

            int Array1LY = Array1.GetLength(0);
            int sI, eI;
            unsafe
            {
                complex p1;
                complex* p2;
                complex* pOut;
                fixed (complex* pArray2 = Array2)
                {
                    fixed (complex* pArrayOut = ArrayOut)
                    {
                        for (int m = 0; m < Array1.GetLength(0); m++)
                        {
                            for (int i = 0; i < L1; i++)
                            {
                                p1 = Array1[m, i];
                                sI = StartI - i;
                                eI = EndI - i;
                                if (eI > L2) eI = L2;
                                if (sI < 0) sI = 0;
                                if (sI < eI)
                                {
                                    p2 = pArray2 + sI;
                                    pOut = pArrayOut + m * Array1LY;
                                    for (int j = sI; j < eI; j++)
                                    {
                                        *pOut += p1 * (*p2);
                                        p2++;
                                        pOut++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ArrayOut;
        }
        /// <summary>
        /// Performs realspace convolution of two arrays, 
        /// returns an array with length equal to the shorter array (results are centered)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Impulse"></param>
        /// <returns></returns>
        public static complex[] ConvoluteChopSlow(complex[] Array1, complex[] Array2)
        {
            complex[] ArrayOut = Convolute(Array1, Array2);

            int Length;

            if (Array1.Length < Array2.Length)
            {
                Length = Array1.Length;
            }
            else
            {
                Length = Array2.Length;
            }
            complex[] ArrayOut2 = new complex[Length];
            int cc = 0;
            int Length2 = ArrayOut.Length / 2 + Length / 2;
            for (int i = (int)(ArrayOut.Length / 2 - Length / 2); i < Length2; i++)
            {
                ArrayOut2[cc] = ArrayOut[i];
                cc++;
            }

            return ArrayOut2;
        }

        /// <summary>
        /// Pulls a array from the larger array with the ROI indicated
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startX"></param>
        /// <param name="endX"></param>
        /// <param name="startY"></param>
        /// <param name="endY"></param>
        /// <returns></returns>
        public static complex[,] SubArray(this complex[,] array, int startX, int endX, int startY, int endY)
        {
            complex[,] OutArray = new complex[endX - startX, endY - startY];
            int cX = 0, cy = 0;
            for (int x = startX; x < endX; x++)
            {
                cy = 0;
                for (int y = startY; y < endY; y++)
                {
                    OutArray[cX, cy] = array[x, y];
                    cy++;
                }
                cX++;
            }
            return OutArray;
        }
    }
}
