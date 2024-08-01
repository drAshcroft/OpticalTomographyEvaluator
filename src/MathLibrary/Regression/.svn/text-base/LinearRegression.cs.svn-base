using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics.Models.Regression.Linear;


namespace MathLibrary.Regression
{
    public class LinearRegression
    {

        public bool TestPlaneRegression()
        {
            List<double[]> test = new List<double[]>();

            Random rnd = new Random();

            for (int i = 0; i < 30; i++)
            {
                double x = rnd.NextDouble();
                double y = rnd.NextDouble();

                test.Add(new double[] { x, y, x * 2 + y * 3 + 5 });
            }
            double[] coeff;
            MathLibrary.Regression.LinearRegression.PlaneLinearRegression(test, out coeff);

            double diff = 2 - coeff[0] + 3 - coeff[1] + 5 - coeff[2];
            return diff < .1;
        }

        public static void PlaneLinearRegression(List<double[]> DataPoints, out double[] Coeff)
        {
            double[,] A = new double[3, 3];
            double[,] B = new double[3, 1];

            for (int i = 0; i < DataPoints.Count; i++)
            {
                double x = DataPoints[i][0];
                double y = DataPoints[i][1];
                double z = DataPoints[i][2];

                A[0, 0] = A[0, 0] + x * x;
                A[1, 0] = A[1, 0] + x * y;
                A[2, 0] = A[2, 0] + x;
                A[0, 1] = A[0, 1] + x * y;
                A[1, 1] = A[1, 1] + y * y;
                A[2, 1] = A[2, 1] + y;
                A[0, 2] = A[0, 2] + x;
                A[1, 2] = A[1, 2] + y;
                A[2, 2] = A[2, 2] + 1;

                B[0, 0] = B[0, 0] + x * z;
                B[1, 0] = B[1, 0] + y * z;
                B[2, 0] = B[2, 0] + z;
            }

            var X = A.Solve(B, true);


            Coeff = new double[] { X[0, 0], X[1, 0], X[2, 0] };
        }

        public static double[] GetCoeffofLine(double[] data)
        {

            double[] x = new double[data.Length]; // Extract the independent variable
            double[] y = new double[data.Length]; // Extract the independent variable

            for (int i = 0; i < data.Length; i++)
            {
                x[i] = i;
                y[i] = data[i];
            }
            // Create a simple linear regression
            SimpleLinearRegression slr = new SimpleLinearRegression();
            slr.Regress(x, y);

            // Compute the simple linear regression output
            var line = slr.Compute(x);

            return line;
        }

        public static void LinearRegressionPoly(double[,] ScatterData, int PolyOrder, out double[] coeff)
        {
            double[,] X = new double[ScatterData.GetLength(1), PolyOrder + 1];
            double[,] Y = new double[1, ScatterData.GetLength(1)];
            for (int i = 0; i < ScatterData.GetLength(1); i++)
            {
                double xV = 1;
                for (int j = 0; j <= PolyOrder; j++)
                {
                    X[i, j] = xV;
                    xV *= ScatterData[0, i];
                }
                Y[0, i] = ScatterData[1, i];
            }



            MathNet.Numerics.LinearAlgebra.Matrix mX = new MathNet.Numerics.LinearAlgebra.Matrix(X);
            MathNet.Numerics.LinearAlgebra.Matrix mY = new MathNet.Numerics.LinearAlgebra.Matrix(Y);
            mY.Transpose();
            MathNet.Numerics.LinearAlgebra.Matrix a = mX.Solve(mY);
            //System.Diagnostics.Debug.Print(a.ToString());
            coeff = new double[PolyOrder + 1];
            for (int i = 0; i <= PolyOrder; i++)
            {
                coeff[i] = a[i, 0];
            }
        }
    }
}
