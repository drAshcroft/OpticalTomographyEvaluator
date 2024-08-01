using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using MathLibrary;

namespace ReconstructCells
{
    [Serializable]
    public class CellLocation
    {
        public PointF CellCenter;
        public int CellSize;
        public int Index;
        public double FV;

        public static int[] MaxY(CellLocation[] locations)
        {
            int[] maxY = new int[2];
            maxY[0] = int.MaxValue;
            maxY[1] = int.MinValue;
            foreach (CellLocation cl in locations)
            {
                double m = cl.CellCenter.Y - cl.CellSize / 2d;
                double M = cl.CellCenter.Y + cl.CellSize / 2d;
                if (m < maxY[0]) maxY[0] = (int)m;
                if (M > maxY[1]) maxY[1] = (int)M;
            }
            return maxY;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle((int)(CellCenter.X - CellSize / 2f), (int)(CellCenter.Y - CellSize / 2f), CellSize, CellSize);
        }

        public Rectangle ToFixedRectangle(int CellSize)
        {
            return new Rectangle((int)(CellCenter.X - CellSize / 2f), (int)(CellCenter.Y - CellSize / 2f), CellSize, CellSize);
        }

        public CellLocation(PointF Center, int cellSize, int index)
        {
            CellCenter = Center;
            CellSize = cellSize;
            Index = index;
        }

        public CellLocation(string line)
        {
            string[] parts = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //Index + ", " + CellCenter.X + ", " + CellCenter.Y + ", " + CellSize + ", " + FV;

            Index = int.Parse(parts[0]);
            CellCenter = new PointF(float.Parse(parts[1]), float.Parse(parts[2]));
            CellSize = int.Parse(parts[3]);
            FV = double.Parse(parts[4]);
        }

        public static void Save(string Filename, CellLocation[] Locations)
        {

            System.IO.StreamWriter logFile = new System.IO.StreamWriter(Filename);
            for (int i = 0; i < Locations.Length; i++)
            {
                logFile.WriteLine(Locations[i].ToString());
            }

            logFile.Close();

        }

        public static CellLocation[] Open(string Filename)
        {
            try
            {
                CellLocation[] Locations;
                // create reader & open file
                TextReader tr = new StreamReader(Filename);

                string[] lines = tr.ReadToEnd().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                tr.Close();

                Locations = new CellLocation[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    Locations[i] = new CellLocation(lines[i]);
                }
                return Locations;
            }
            catch (Exception ex)
            {

                Program.WriteTagsToLog("Error", ex.Message);
                throw ex;
            }
        }

        public static CellLocation[] SmoothKalman(CellLocation[] locations, float noisePercent, float gain)
        {


            float noisevar, predicted, predictedvar;
            float Kalman;

            noisevar = noisePercent;

            predicted = locations[0].CellCenter.Y;
            predictedvar = noisevar;


            for (int i = 1; i < locations.Length; ++i)
            {
                Kalman = predictedvar / (predictedvar + noisevar);
                predicted = (float)(gain * predicted + (1.0 - gain) * locations[i].CellCenter.Y + Kalman * (locations[i].CellCenter.Y - predicted));
                predictedvar = (float)(predictedvar * (1.0 - Kalman));

                locations[i].CellCenter.Y = predicted;
            }
            return locations;
        }


        public static CellLocation[] SmoothTrig(CellLocation[] locations)
        {

            double[] Yvalues = new double[locations.Length];
            for (int i = 0; i < locations.Length; i++)
            {
                Yvalues[i] = locations[i].CellCenter.Y;

            }
            Yvalues = MathLibrary.Regression.LMA.FitToSine(Yvalues);

            for (int i = 0; i < Yvalues.Length; i++)
                locations[i].CellCenter.Y = (float)Yvalues[i];
            return locations;
        }


        public static double[,] GetCenters(CellLocation[] locations)
        {
            double[,] Yvalues = new double[2, locations.Length];

            for (int i = 0; i < locations.Length; i++)
            {
                Yvalues[0, i] = locations[i].CellCenter.X;
                Yvalues[1, i] = locations[i].CellCenter.Y;
            }

            return Yvalues;
        }

        public static void SetCenters(CellLocation[] locations, double[,] newLocations)
        {


            for (int i = 0; i < locations.Length; i++)
            {
                locations[i].CellCenter.X = (float)newLocations[0, i];
                locations[i].CellCenter.Y = (float)newLocations[1, i];
            }


        }

        public static CellLocation[] SmoothPoly(CellLocation[] locations)
        {

            float  x1, x2, y1, y2;
            x1 = locations[0].CellCenter.X;
            x2 = locations[1].CellCenter.X;

            y1 = locations[0].CellCenter.Y;
            y2 = locations[1].CellCenter.Y;


            double[,] Yvalues = new double[2, locations.Length];
            for (int i = 0; i < locations.Length; i++)
            {
                Yvalues[0, i] = i;
                Yvalues[1, i] = locations[i].CellCenter.Y;
            }

            double[] a;

            MathLibrary.Regression.LinearRegression.LinearRegressionPoly(Yvalues, 11, out a);

            for (int i = 0; i < locations.Length; i++)
            {
                double y = a[0];
                for (int j = 1; j < a.Length; j++)
                    y = y + a[j] * Math.Pow(i, j);

                locations[i].CellCenter.Y = (float)y;
            }



            Yvalues = new double[2, locations.Length];
            for (int i = 0; i < locations.Length; i++)
            {
                Yvalues[0, i] = i;
                Yvalues[1, i] = locations[i].CellCenter.X;
            }



            MathLibrary.Regression.LinearRegression.LinearRegressionPoly(Yvalues, 21, out a);

            for (int i = 0; i < locations.Length; i++)
            {
                double y = a[0];
                for (int j = 1; j < a.Length; j++)
                    y = y + a[j] * Math.Pow(i, j);

                locations[i].CellCenter.X = (float)y;
            }

            locations[0].CellCenter.X = x1;
            locations[1].CellCenter.X = x2;

            locations[0].CellCenter.Y = y1;
            locations[1].CellCenter.Y = y2;

            return locations;
        }


        public static Bitmap GraphY(CellLocation[] Locations)
        {
            float[] Points = new float[Locations.Length];

            for (int i = 0; i < Locations.Length; i++)
                Points[i] = Locations[i].CellCenter.Y;


            return Points.DrawGraph();
        }

        public static Bitmap GraphX(CellLocation[] Locations)
        {
            float[] Points = new float[Locations.Length];

            for (int i = 0; i < Locations.Length; i++)
                Points[i] = Locations[i].CellCenter.X;


            return Points.DrawGraph();
        }

        public override string ToString()
        {
            return Index + ", " + CellCenter.X + ", " + CellCenter.Y + ", " + CellSize + ", " + FV;
        }
    }
}
