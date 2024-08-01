using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibrary.Regression
{
    /// <summary>
    /// A class which implements the <i>Levenberg-Marquardt Algorithm</i> (LMA) fit for non-linear,
    /// multidimensional parameter space. The algorithm is described in
    /// Numerical Recipes in FORTRAN, 2nd edition, p. 676-679, ISBN 0-521-43064X, 1992.
    /// 
    /// author Janne Holopainen (jaolho@utu.fi, tojotamies@gmail.com)
    /// java version 1.1, 24.03.2006
    /// .......................................................................
    /// Ported to .NET from the above listed version by Kris Kniaz
    /// .NET version 1.0 28.10.2006
    /// </summary>
    public class LMA
    {

        public static double[] FitToSine(double[] Yvalues)
        {

            double[] xValues = new double[Yvalues.Length];
           

            for (int i = 0; i < xValues.Length; i++)
            {
                xValues[i] = (double)i/(double)Yvalues.Length * 2d*Math.PI;
               
            }

            double[][] dataPoints = new double[][] { xValues, Yvalues };

            MathLibrary.Regression.SineFunction sine = new MathLibrary.Regression.SineFunction();
            // a[0] * System.Math.Sin(x * a[1] + a[2]) + a[3];

            double[] a=new double[4];
            double m=Yvalues.Max();
            double M =Yvalues.Min();
            a[0]=Math.Abs(m-M);

            int mIndex =0;
            for (int i=0;i<Yvalues.Length ;i++)
                if (Yvalues[i]==m)
                    mIndex=i;
            a[1] = 1;
            a[2] = (Yvalues.Length /4 - mIndex) * Math.PI /(Yvalues.Length/2);
            a[3]=Yvalues.Average();


            LMA algorithm = new LMA(sine, a, dataPoints, null, new MathNet.Numerics.LinearAlgebra.Matrix(a.Length, a.Length), 1d - 20, 100);

            algorithm.Fit();

            for (int i = 0; i < a.Length; i++)
            {
                a[i] = algorithm.Parameters[i];
            }

            // string junk = "";
            for (int i = 0; i < Yvalues.Length; i++)
            {

                Yvalues[i] = sine.GetY(dataPoints[0][i], a);
                //   junk += outArray[1, i] + "\n";
            }

            return Yvalues;
        }


        //interface of the function to be fitted
        private ILMAFunction function;

        //The array of fit parameters (a.k.a, the a-vector).
        private double[] parameters;

        //Parameters incremented by value of lambda
        private double[] incrementedParameters;

        //Measured data points for which the model function is to be fitted.
        //double[0 = x, 1 = y][data point index] = data value
        private double[][] dataPoints;

        //Weights for each data point. The merit function is: chi2 = sum[ (y_i - y(x_i;a))^2 * w_i ].
        //For gaussian errors in datapoints, set w_i = 1 / sigma_i.
        private double[] weights;

        //Hessian
        private MathNet.Numerics.LinearAlgebra.Matrix alpha;

        //gradient
        private double[] beta;

        private double[] da;

        private double chi2;
        private double lambda;

        private double incrementedChi2;
        private int iterationCount;

        // default end conditions
        private double minDeltaChi2 = 1e-30;
        private int maxIterations = 100;

        /// <summary>
        ///Ctor. In the LMA fit N is the number of data points, M is the number of fit parameters.
        ///Call <code>fit()</code> to start the actual fitting.
        /// </summary>
        /// <param name="function">The model function to be fitted. Must be able to take M input parameters.</param>
        /// <param name="parameters">The initial guess for the fit parameters, length M.</param>
        /// <param name="dataPoints">The data points in an array, <code>double[0 = x, 1 = y][point index]</code>. Size must be <code>double[2][N]</code>.</param>
        /// <param name="weights">The weights, normally given as: <code>weights[i] = 1 / sigma_i^2</code>. 
        /// If you have a bad data point, set its weight to zero. If the given array is null,
        /// a new array is created with all elements set to 1.</param>
        /// <param name="alpha">A Matrix instance. Must be initiated to (M x M) size. 
        /// In this case we are using the GeneralMatrix type from the open source JAMA library</param>
        /// <param name="argDeltaChi2">delta chi square</param>
        /// <param name="argMaxIter">maximum number of iterations</param>
        public LMA(LMAFunction function, double[] parameters,
            double[][] dataPoints, double[] weights,
            MathNet.Numerics.LinearAlgebra.Matrix alpha,
            double argDeltaChi2, int argMaxIter)
        {
            if (dataPoints[0].Length != dataPoints[1].Length)
                throw new ArgumentException("ImageData must have the same number of x and y points.");

            if (dataPoints.Length != 2)
                throw new ArgumentException("ImageData point array must be 2 x N");

            this.function = function;
            this.parameters = parameters;
            this.dataPoints = dataPoints;
            this.weights = CheckWeights(dataPoints[0].Length, weights);
            this.incrementedParameters = new double[parameters.Length];
            this.alpha = alpha;
            this.beta = new double[parameters.Length];
            this.da = new double[parameters.Length];
            minDeltaChi2 = argDeltaChi2;
            maxIterations = argMaxIter;
            lambda = Constants.lambda;

        }

        #region Accessors
        public double[] Parameters
        {
            get { return this.parameters; }
        }

        public int Iterations
        {
            get { return this.iterationCount; }
        }

        public double Chi2
        {
            get { return this.chi2; }
        }
        #endregion

        /// <summary>
        /// The default fit. If used after calling fit(lambda, minDeltaChi2, maxIterations),
        /// uses those values. The stop condition is fetched from <code>this.stop()</code>.
        /// Override <code>this.stop()</code> if you want to use another stop condition.
        /// </summary>
        public void Fit()
        {
            iterationCount = 0;

            do
            {
                chi2 = CalculateChi2();
                UpdateAlpha();
                UpdateBeta();


                SolveIncrements();
                incrementedChi2 = CalculateIncrementedChi2();
                // The guess results to worse chi2 - make the step smaller
                if (incrementedChi2 >= chi2)
                {
                    lambda *= 10;
                }
                // The guess results to better chi2 - move and make the step larger
                else
                {
                    lambda /= 10;
                    UpdateParameters();
                }
                iterationCount++;
            } while (!this.Stop());
        }

        /// <summary>
        /// Initializes and starts the fit. The stop condition is fetched from <code>this.stop()</code>.
        /// Override <code>this.stop()</code> if you want to use another stop condition.
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="minDeltaChi2"></param>
        /// <param name="maxIterations"></param>
        public void Fit(double lambda, double minDeltaChi2, int maxIterations)
        {
            this.lambda = lambda;
            this.minDeltaChi2 = minDeltaChi2;
            this.maxIterations = maxIterations;
            Fit();
        }

        /// <summary>
        ///The stop condition for the fit.
        ///Override this if you want to use another stop condition.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            return System.Math.Abs(chi2 - incrementedChi2) < minDeltaChi2 || iterationCount > maxIterations;
        }

        /// <summary>
        /// Updates parameters from incrementedParameters.
        /// </summary>
        protected void UpdateParameters()
        {
            System.Array.Copy(incrementedParameters, 0, parameters, 0, parameters.Length);
        }

        /// <summary>
        /// Solves the increments array (<code>this.da</code>) using alpha and beta.
        /// Then updates the <code>this.incrementedParameters</code> array.
        /// NOTE: Inverts alpha. Call at least <code>updateAlpha()</code> before calling this.
        /// </summary>
        protected void SolveIncrements()
        {
            try
            {
                //use the GeneralMatrix package to invert alpha
                //one could also use 
                //double[] da = DoubleMatrix.solve(alpha, beta);

                MathNet.Numerics.LinearAlgebra.Matrix m = alpha.Inverse();
                //set alpha with inverted matrix
                alpha.SetMatrix(0, alpha.RowCount - 1, 0, alpha.ColumnCount - 1, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            for (int i = 0; i < alpha.RowCount; i++)
            {
                da[i] = 0;
                for (int j = 0; j < alpha.ColumnCount; j++)
                {
                    da[i] += alpha[i, j] * beta[j];
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                //if (!Double.isNaN(da[i]) && !Double.isInfinite(da[i]))
                incrementedParameters[i] = parameters[i] + da[i];
            }
        }


        /// <summary>
        /// Calculates value of the function for given parameter array
        /// </summary>
        /// <param name="a">input parameters</param>
        /// <returns>value of the function</returns>
        protected double CalculateChi2(double[] a)
        {
            double result = 0;
            for (int i = 0; i < dataPoints[0].Length; i++)
            {
                double dy = dataPoints[1][i] - function.GetY(dataPoints[0][i], a);
                result += weights[i] * dy * dy;
            }
            return result;
        }

        /// <summary>
        /// Calculates function value for the current fit parameters
        /// Does not change the value of chi2
        /// </summary>
        /// <returns>value of the function</returns>
        protected double CalculateChi2()
        {
            return CalculateChi2(parameters);
        }

        /// <summary>
        /// Calculates function value for the incremented parameters (da + a).
        /// Does not change the value of chi2.
        /// </summary>
        /// <returns></returns>
        protected double CalculateIncrementedChi2()
        {
            return CalculateChi2(incrementedParameters);
        }

        // 
        /// <summary>
        /// Calculates all elements for <code>this.alpha</code>.
        /// </summary>
        protected void UpdateAlpha()
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                for (int j = 0; j < parameters.Length; j++)
                {
                    alpha[i, j] = CalculateAlphaElement(i, j);
                }
            }
        }

        /// <summary>
        /// Calculates lambda weighted element for the alpha-matrix.
        /// NOTE: Does not change the value of alpha-matrix.
        /// </summary>
        /// <param name="row">row of the Hessian</param>
        /// <param name="col">column of the Hessian</param>
        /// <returns></returns>
        protected double CalculateAlphaElement(int row, int col)
        {
            double result = 0;
            for (int i = 0; i < dataPoints[0].Length; i++)
            {
                result +=
                    weights[i] *
                    function.GetPartialDerivative(dataPoints[0][i], parameters, row) *
                    function.GetPartialDerivative(dataPoints[0][i], parameters, col);
            }
            if (row == col) result *= (1 + lambda);
            return result;
        }

        /// <summary>
        /// Calculates all elements for <code>this.beta</code>.
        /// </summary>
        protected void UpdateBeta()
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                beta[i] = CalculateBetaElement(i);
            }
        }


        /// <summary>
        /// Calculates element of the beta (gradient) matrix
        /// NOTE: Does not change the value of beta-matrix.
        /// </summary>
        /// <param name="row"></param>
        /// <returns>Value of the gradient point</returns>
        protected double CalculateBetaElement(int row)
        {
            double result = 0;
            for (int i = 0; i < dataPoints[0].Length; i++)
            {
                result +=
                    weights[i] *
                    (dataPoints[1][i] - function.GetY(dataPoints[0][i], parameters)) *
                    function.GetPartialDerivative(dataPoints[0][i], parameters, row);
            }
            return result;
        }

        /// <summary>
        /// Checks if the matrix of weights for each point is a 
        /// matrix of positive elements. Otherwise it initializes
        /// a new matrix and sets each value to 1
        /// </summary>
        /// <param name="length"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        protected double[] CheckWeights(int length, double[] weights)
        {
            bool damaged = false;
            // check for null
            if (weights == null)
            {
                //Trace.WriteLine("weights matrix was null");
                damaged = true;
                weights = new double[length];
            }
            // check if all elements are zeros or if there are negative, NaN or Infinite elements
            else
            {
                bool allZero = true;
                bool illegalElement = false;
                for (int i = 0; i < weights.Length && !illegalElement; i++)
                {
                    if (weights[i] < 0 || Double.IsNaN(weights[i]) || Double.IsInfinity(weights[i])) illegalElement = true;
                    allZero = (weights[i] == 0) && allZero;
                }
                damaged = allZero || illegalElement;
            }

            if (damaged)
            {
                //Trace.WriteLine("Weights were not well defined. All elements set to 1.");
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] = 1;
                }
            }

            return weights;
        }// end of weights
    }


    /// <summary>
    /// function represents sinusoidal aplitude
    /// </summary>
    public class SineFunctionFixedFrequency : LMAFunction
    {

        public double Frequency = 1;// 2 * Math.PI;
        /// <summary>
        /// Returns value of the function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public override double GetY(double x, double[] a)
        {
            return a[0] * System.Math.Sin(x * Frequency + a[1]) + a[2];
        }

        /// <summary>
        /// Returns derivative
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public override double GetPartialDerivative(double x, double[] a, int parameterIndex)
        {
            double result = 0;
            switch (parameterIndex)
            {
                case 0://amplitude
                    result = System.Math.Sin(x * Frequency + a[1]);
                    break;
                case 1://phase
                    result = a[0] * System.Math.Cos(x * Frequency + a[2]);
                    break;
                case 2://offset
                    result = 1;
                    break;

                default:
                    throw new ArgumentException("No such parameter index: " + parameterIndex);
            }

            return result;
        }
    }


    /// <summary>
    /// function represents sinusoidal aplitude
    /// </summary>
    public class SineFunction : LMAFunction
    {

        /// <summary>
        /// Returns value of the function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public override double GetY(double x, double[] a)
        {
            return a[0] * System.Math.Sin(x * a[1] + a[2]) + a[3];
        }

        /// <summary>
        /// Returns derivative
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public override double GetPartialDerivative(double x, double[] a, int parameterIndex)
        {
            double result = 0;
            switch (parameterIndex)
            {
                case 0:
                    result = System.Math.Sin(x * a[1] + a[2]);
                    break;
                case 1:
                    result = a[0] * System.Math.Cos(x * a[1] + a[2]) * (x);
                    break;
                case 2:
                    result = a[0] * System.Math.Cos(x * a[1] + a[2]);
                    break;
                case 3:
                    result = 1;
                    break;

                default:
                    throw new ArgumentException("No such parameter index: " + parameterIndex);
            }

            return result;
        }
    }


    /// <summary>
    /// function represents sinusoidal aplitude
    /// </summary>
    public class GuassFunction : LMAFunction
    {

        /// <summary>
        /// Returns value of the function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public override double GetY(double x, double[] a)
        {
            double xr = x - a[1];
            return a[0] * System.Math.Exp(-1 * xr * xr / a[2]) + a[3];
        }

        /// <summary>
        /// Returns derivative
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public override double GetPartialDerivative(double x, double[] a, int parameterIndex)
        {
            double result = 0;
            double xr = x - a[1];
            switch (parameterIndex)
            {
                case 0:
                    result = System.Math.Exp(-1 * xr * xr / a[2]);
                    break;
                case 1:
                    result = 2 * a[0] * xr / a[2] * System.Math.Exp(-1 * xr * xr / a[2]);
                    break;
                case 2:
                    result = a[0] * xr * xr / a[2] / a[2] * System.Math.Exp(-1 * xr * xr / a[2]);
                    break;
                case 3:
                    result = 1;
                    break;
                default:
                    throw new ArgumentException("No such parameter index: " + parameterIndex);
            }

            return result;
        }
    }

   


    /// <summary>
    /// function represents sinusoidal aplitude
    /// </summary>
    public class GuassDiffusionFunction : LMAFunction
    {
        double A = 1 / Math.Sqrt(Math.PI * 2);
        /// <summary>
        /// Returns value of the function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public override double GetY(double x, double[] a)
        {
            double xr = x - a[1];
            return A / a[0] * System.Math.Exp(-1 * xr * xr / (2 * a[0] * a[0])) + a[2];
        }

        /// <summary>
        /// Returns derivative
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public override double GetPartialDerivative(double x, double[] a, int parameterIndex)
        {
            double result = 0;
            double xr = x - a[1];
            switch (parameterIndex)
            {
                case 0:
                    result = A * System.Math.Exp(-1 * xr * xr / (2 * a[0] * a[0])) * (x * x - 2 * x * a[1] + a[1] * a[1] - a[0] * a[0]) / Math.Pow(a[0], 4);
                    break;
                case 1:
                    result = A * System.Math.Exp(-1 * xr * xr / (2 * a[0] * a[0])) * (xr) / Math.Pow(a[0], 3);
                    break;
                case 2:
                    result = 1;
                    break;
                default:
                    throw new ArgumentException("No such parameter index: " + parameterIndex);
            }

            return result;
        }
    }




    /// <summary>
    /// Summary description for Constants.
    /// </summary>
    public class Constants
    {
        public const double lambda = 0.001;
    }

    /// <summary>
    /// Defines Function of a[] parameters that will be fitted 
    /// </summary>
    interface ILMAFunction
    {
        /// <summary>
        /// Returns the y value of the function for
        /// the given x and vector of parameters
        /// </summary>
        /// <param name="x">The <i>x</i>-value for which the <i>y</i>-value is calculated.</param>
        /// <param name="a">The fitting parameters. </param>
        /// <returns></returns>
        double GetY(double x, double[] a);

        /// <summary>
        /// The method which gives the partial derivates used in the LMA fit.
        /// If you can't calculate the derivate, use a small <code>a</code>-step (e.g., <i>da</i> = 1e-20)
        /// and return <i>dy/da</i> at the given <i>x</i> for each fit parameter.
        /// </summary>
        /// <param name="x">The <i>x</i>-value for which the partial derivate is calculated.</param>
        /// <param name="a">The fitting parameters.</param>
        /// <param name="parameterIndex">The parameter index for which the partial derivate is calculated.</param>
        /// <returns>The partial derivate of the function with respect to parameter <code>parameterIndex</code> at <i>x</i>.</returns>
        double GetPartialDerivative(double x, double[] a, int parameterIndex);

    }

    /// <summary>
    /// Abstract class implementing the LMAFunction interface
    /// </summary>
    public abstract class LMAFunction : ILMAFunction
    {
        /// <summary>
        /// Returns the y value of the function for
        /// the given x and vector of parameters
        /// </summary>
        /// <param name="x">The <i>x</i>-value for which the <i>y</i>-value is calculated.</param>
        /// <param name="a">The fitting parameters. </param>
        /// <returns></returns>
        public abstract double GetY(double x, double[] a);

        /// <summary>
        /// The method which gives the partial derivates used in the LMA fit.
        /// If you can't provide the functional derivative, use a small <code>a</code>-step (e.g., <i>da</i> = 1e-20)
        /// and return <i>dy/da</i> at the given <i>x</i> for each fit parameter.
        /// This is provided in the method below as a default implementation
        /// </summary>
        /// <param name="x">The <i>x</i>-value for which the partial derivate is calculated.</param>
        /// <param name="a">The fitting parameters.</param>
        /// <param name="parameterIndex">The parameter index for which the partial derivate is calculated.</param>
        /// <returns>The partial derivative of the function with respect to parameter <code>parameterIndex</code> at <i>x</i>.</returns>
        public virtual double GetPartialDerivative(double x, double[] a, int parameterIndex)
        {
            //kk 25 Jun 2010
            //this value has been changed to 1*10-9 from 1*10-14 after a hint by a user
            //who was having issues with convergence on some gaussian function

            double delta = 0.000000001;
            double[] newParam = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
                newParam[i] = a[i];

            newParam[parameterIndex] = a[parameterIndex] + delta;
            double dplusResult = GetY(x, newParam);

            newParam[parameterIndex] = a[parameterIndex] - delta;
            double dminusResult = GetY(x, newParam);

            double result = (dplusResult - dminusResult) / (2 * delta);

            return result;
        }


        /// <summary>
        /// Returns array of x,y values, given x and fitting parameters
        /// used by all tests to generate test data for exact fits
        /// </summary>
        /// <param name="xValues">x values</param>
        /// <param name="a">fitting parameters</param>
        /// <returns>point values</returns>
        public double[][] GenerateData(double[] a, double[] xValues)
        {
            double[] yValues = new double[xValues.Length];

            for (int i = 0; i < xValues.Length; i++)
            {
                yValues[i] = GetY(xValues[i], a);
            }

            return new double[][] { xValues, yValues };
        }

    }
}
