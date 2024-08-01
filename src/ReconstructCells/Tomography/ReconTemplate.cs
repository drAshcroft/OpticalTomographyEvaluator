using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using ImageProcessing._3D;
using System.Threading.Tasks;

namespace ReconstructCells.Tomography
{
    public class ReconTemplate : ReconstructNodeTemplate
    {
        #region ReconVars
        //declare once to avoid having to recalculate all of these 
        protected int LI;
        protected int LJ;
        protected int LK;

        protected int LsI;
        protected int LsJ;
        protected int LsI_1;
        protected int LsJ_1;

        protected double halfI, halfJ, halfK, halfIs, halfJs;
        protected double VolToSliceX, VolToSliceY, VolToSliceZ;

        protected double RX;
        protected double RY;
        protected double RZ;

        protected double HalfI;
        protected double HalfJ;
        protected double HalfK;
        protected int yLower;
        protected int yUpper;

        protected int yLowerMax;
        protected int yUpperMin;

        protected double radiusCyl;
        protected int countRow;

        protected double PaintingWidth = 1;
        protected double PaintingHeight = 1;
        protected double r = Math.Sqrt(2 * .5 * .5);

        protected double xMax = .5;
        protected double xMin = -.5;
        protected double yMax = .5;
        protected double yMin = -.5;
        protected double zMax = .5;
        protected double zMin = -.5;

        protected int[,] Mask;

        /// <summary>
        /// Used to keep all the threads properly locked
        /// </summary>
        protected object[] LockArray;

        /// <summary>
        /// Used to help distribute the working threads amoung all the slices.  the lock indicator is used to do a non blocking check if the 
        /// slice is already in use
        /// </summary>
        protected bool[] LockIndicator;
        #endregion

        protected OnDemandImageLibrary Library;

        protected float[, ,] DensityGrid;

        #region class Defs
        /// <summary>
        /// Axis specification for 3D arrays and transforms
        /// </summary>
        protected enum Axis
        {
            XAxis = 0, YAxis = 1, ZAxis = 2
        }
        /// <summary>
        ///  Axis specification for 2D arrays and transforms
        /// </summary>
        protected enum Axis2D
        {
            XAxis = 0, YAxis = 1
        }
        #endregion


        #region Properties

        #region Set

        #endregion

        #region Get


        #endregion

        #endregion

        #region Code

        protected void SetConstants2(float scaleFactor, int CutSize)
        {

            Image<Gray, float> ExampleProjection = Library[0];

            int GridSize = DensityGrid.GetLength(0);
            LockArray = new object[GridSize];
            LockIndicator = new bool[GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                LockArray[i] = new object();
            }

            // var Slice = Library[0];

            //get all the dimensions
            LI = DensityGrid.GetLength(2);
            LJ = DensityGrid.GetLength(1);
            LK = DensityGrid.GetLength(0);

            LsI = Library[0].Width;
            LsJ = Library[0].Height;
            LsI_1 = LsI - 1;
            LsJ_1 = LsJ - 1;


            halfI = (double)LI / 2;
            halfJ = (double)LJ / 2;
            halfK = (double)LK / 2;

            halfIs = (double)LsI / 2;
            halfJs = (double)LsJ / 2;

            VolToSliceX = scaleFactor;// (xMax - xMin) / LI * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceY = scaleFactor;// (yMax - yMin) / LJ * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceZ = scaleFactor;// (zMax - zMin) / LK * (double)ExampleProjection.Width / (PaintingHeight);


            RX = DensityGrid.GetLength(0) / 2d - 15;
            RY = DensityGrid.GetLength(1) / 2d - 15;
            RZ = DensityGrid.GetLength(2) / 2d - 15;

            HalfI = DensityGrid.GetLength(0) / 2d;
            HalfJ = DensityGrid.GetLength(1) / 2d;
            HalfK = DensityGrid.GetLength(2) / 2d;
            yLower = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 10));
            yUpper = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 10));

            yLowerMax = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 15));
            yUpperMin = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 15));

            radiusCyl = (LsI - 20) / 2;
            countRow = (int)halfI;

        }



        protected void SetConstants(Image<Gray, float> ExampleProjection)
        {
            int GridSize = DensityGrid.GetLength(0);
            LockArray = new object[GridSize];
            LockIndicator = new bool[GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                LockArray[i] = new object();
            }

            // var Slice = Library[0];

            //get all the dimensions
            LI = DensityGrid.GetLength(2);
            LJ = DensityGrid.GetLength(1);
            LK = DensityGrid.GetLength(0);

            LsI = ExampleProjection.Width;
            LsJ = ExampleProjection.Height;
            LsI_1 = LsI - 1;
            LsJ_1 = LsJ - 1;


            halfI = (double)LI / 2;
            halfJ = (double)LJ / 2;
            halfK = (double)LK / 2;

            halfIs = (double)LsI / 2;
            halfJs = (double)LsJ / 2;

            VolToSliceX = (xMax - xMin) / LI * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceY = (yMax - yMin) / LJ * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceZ = (zMax - zMin) / LK * (double)ExampleProjection.Width / (PaintingHeight);


            RX = DensityGrid.GetLength(0) / 2d - 15;
            RY = DensityGrid.GetLength(1) / 2d - 15;
            RZ = DensityGrid.GetLength(2) / 2d - 15;

            HalfI = DensityGrid.GetLength(0) / 2d;
            HalfJ = DensityGrid.GetLength(1) / 2d;
            HalfK = DensityGrid.GetLength(2) / 2d;
            yLower = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 10));
            yUpper = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 10));

            yLowerMax = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 15));
            yUpperMin = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 15));

            radiusCyl = (LsI - 20) / 2;
            countRow = (int)halfI;

            Mask = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

            for (int j = 0; j < DensityGrid.GetLength(1); j++)
                for (int k = 0; k < DensityGrid.GetLength(2); k++)
                {
                    double y = (j - HalfJ) / RY;
                    double z = (k - HalfK) / RZ;
                    if ((y * y + z * z) > 1)
                    {
                        Mask[j, k] = 1;
                    }
                }
        }

        protected void SetConstants(Image<Gray, float> ExampleProjection, float scaling)
        {
            int GridSize = DensityGrid.GetLength(0);
            LockArray = new object[GridSize];
            LockIndicator = new bool[GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                LockArray[i] = new object();
            }

            // var Slice = Library[0];

            //get all the dimensions
            LI = DensityGrid.GetLength(2);
            LJ = DensityGrid.GetLength(1);
            LK = DensityGrid.GetLength(0);

            LsI = ExampleProjection.Width;
            LsJ = ExampleProjection.Height;
            LsI_1 = LsI - 1;
            LsJ_1 = LsJ - 1;


            halfI = (double)LI / 2;
            halfJ = (double)LJ / 2;
            halfK = (double)LK / 2;

            halfIs = (double)LsI / 2;
            halfJs = (double)LsJ / 2;

            VolToSliceX = scaling;// (xMax - xMin) / LI * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceY = scaling;// (yMax - yMin) / LJ * (double)ExampleProjection.Height / (PaintingWidth);
            VolToSliceZ = scaling;// (zMax - zMin) / LK * (double)ExampleProjection.Width / (PaintingHeight);


            RX = DensityGrid.GetLength(0) / 2d - 15;
            RY = DensityGrid.GetLength(1) / 2d - 15;
            RZ = DensityGrid.GetLength(2) / 2d - 15;

            HalfI = DensityGrid.GetLength(0) / 2d;
            HalfJ = DensityGrid.GetLength(1) / 2d;
            HalfK = DensityGrid.GetLength(2) / 2d;
            yLower = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 10));
            yUpper = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 10));

            yLowerMax = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 15));
            yUpperMin = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 15));

            radiusCyl = (LsI - 20) / 2;
            countRow = (int)halfI;

            Mask = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

            for (int j = 0; j < DensityGrid.GetLength(1); j++)
                for (int k = 0; k < DensityGrid.GetLength(2); k++)
                {
                    double y = (j - HalfJ) / RY;
                    double z = (k - HalfK) / RZ;
                    if ((y * y + z * z) > 1)
                    {
                        Mask[j, k] = 1;
                    }
                }
        }

        protected void BatchCleanProjection(int imageNumber)
        {

            var imageData = Library[imageNumber].Data;

            for (int y = 0; y < imageData.GetLength(0); y++)
                for (int x = 0; x < imageData.GetLength(1); x++)
                {
                    if (imageData[y, x, 0] < -500)
                        imageData[y, x, 0] = 0;
                }
        }
        protected void CleanProjections()
        {

            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            int numberOfImages = Library.Count;

            Parallel.For(0, (int)(numberOfImages), Program.threadingParallelOptions, x => BatchCleanProjection(x));
        }

        #region Forward Projection
        protected Image<Gray, float> BatchForwardProject(int imageNumber)
        {
            //imageNumber *= 20;
            imageNumber = (imageNumber * 100 + imageNumber) % Library.Count;

            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
            return DoForwardProjection_OneSlice(Library[imageNumber], 1, 1, Angle, Axis2D.YAxis);



        }
        private Image<Gray, float> DoForwardProjection_OneSlice(Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, double AngleRadians, Axis2D ConvolutionAxis)
        {
            //AngleRadians = 0;
            Axis RotationAxis = Axis.ZAxis;

            Point3D vRotationAxis = new Point3D();
            Point3D axis = new Point3D();

            if (RotationAxis == Axis.XAxis)
            {
                vRotationAxis = new Point3D(1, 0, 0);
                axis = new Point3D(0, 1, 0);
            }
            else if (RotationAxis == Axis.YAxis)
            {
                vRotationAxis = new Point3D(0, 1, 0);
                axis = new Point3D(0, 0, 1);
            }
            else if (RotationAxis == Axis.ZAxis)
            {
                vRotationAxis = new Point3D(0, 0, 1);
                axis = new Point3D(0, 1, 0);
            }

            Point3D vec = Point3D.RotateAroundAxis(axis, vRotationAxis, AngleRadians);

            return UnSmearArray2DWhole(AngleRadians, Slice, PaintingWidth, PaintingHeight, vec, Point3D.CrossProduct(vec, vRotationAxis));
        }

        private Image<Gray, float> UnSmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, Point3D Direction, Point3D FastScanDirection)
        {
            float[, ,] PaintingArray = Slice.Data;
            Image<Gray, float> diffImage = Slice.CopyBlank();
            float[, ,] difference = diffImage.Data;
            //make sure all the vectors are the right size
            Direction.Normalize();
            FastScanDirection.Normalize();
            //determine all the important planes in the 3D space and all the step
            Point3D SlowScanAxis = Point3D.CrossProduct(Direction, FastScanDirection);

            ///get the existing recon data.  the jagged array is used so the data can be checked out by each thread

            double r = Math.Sqrt(2 * .5 * .5);
            double[] LUT = new double[50];
            #region Look up table
            double[] p_X = new double[4];
            double[] p_Y = new double[4];


            p_X[0] = Math.Cos(Math.PI / 4d + AngleRadians) * r;
            p_Y[0] = Math.Sin(Math.PI / 4d + AngleRadians) * r;

            p_X[1] = Math.Cos(Math.PI * 3d / 4d + AngleRadians) * r;
            p_Y[1] = Math.Sin(Math.PI * 3d / 4d + AngleRadians) * r;

            p_X[2] = Math.Cos(Math.PI * 5d / 4d + AngleRadians) * r;
            p_Y[2] = Math.Sin(Math.PI * 5d / 4d + AngleRadians) * r;

            p_X[3] = Math.Cos(Math.PI * 7d / 4d + AngleRadians) * r;
            p_Y[3] = Math.Sin(Math.PI * 7d / 4d + AngleRadians) * r;

            double[] o_X = new double[4];
            double[] o_Y = new double[4];
            double d;
            const double step = 1.2 / 50d;
            int cc = 0;
            for (double u2 = 0; u2 < 1.2; u2 += step)
            {
                double l1_X = -1.2;
                double l1_Y = u2;

                double l2_X = 1.2;
                double l2_Y = u2;

                for (int j = 0; j < 4; j++)
                {
                    int j2 = (j + 1) % 4;
                    d = (l1_X - l2_X) * (p_Y[j] - p_Y[j2]) - (l1_Y - l2_Y) * (p_X[j] - p_X[j2]);
                    if (d == 0)
                    {
                        o_X[j] = 1000;
                        o_Y[j] = 10000;
                    }
                    else
                    {
                        o_X[j] = ((l1_X * l2_Y - l1_Y * l2_X) * (p_X[j] - p_X[j2]) - (l1_X - l2_X) * (p_X[j] * p_Y[j2] - p_Y[j] * p_X[j2])) / d;
                        o_Y[j] = ((l1_X * l2_Y - l1_Y * l2_X) * (p_Y[j] - p_Y[j2]) - (l1_Y - l2_Y) * (p_X[j] * p_Y[j2] - p_Y[j] * p_X[j2])) / d;
                    }
                }

                double mX1 = 0, mY1 = 0, mX2 = 0, mY2;
                double minP = 1000;
                double minN = 1000;
                for (int j = 0; j < 4; j++)
                {
                    d = Math.Abs(o_X[j]);
                    if (o_X[j] < 0 && d < minN)
                    {
                        mX2 = o_X[j];
                        mY2 = o_Y[j];
                        minN = d;
                    }
                    if (o_X[j] > 0 && d < minP)
                    {
                        mX1 = o_X[j];
                        mY1 = o_Y[j];
                        minP = d;
                    }
                }
                /* double min2 = 1000;
                 for (int j = 0; j < 4; j++)
                 {
                     d=Math.Abs(o_X[j]) ;
                     if (d < min2 && d>min)
                     {
                         mX2 = o_X[j];
                         mY2 = o_Y[j];
                         min2 = d;
                     }
                 }*/
                LUT[cc] = Math.Abs(mX1 - mX2);
                cc++;
            }
            #endregion
            #region constants
            //get all the dimensions
            int LI = DensityGrid.GetLength(2);
            int LJ = DensityGrid.GetLength(1);
            int LK = DensityGrid.GetLength(0);

            int LsI = Slice.Width;
            int LsJ = Slice.Height;
            int LsI_1 = LsI - 1;
            int LsJ_1 = LsJ - 1;

            double halfI, halfJ, halfK, halfIs, halfJs;
            double VolToSliceX, VolToSliceY, VolToSliceZ;

            halfI = (double)LI / 2;
            halfJ = (double)LJ / 2;
            halfK = (double)LK / 2;

            halfIs = (double)LsI / 2;
            halfJs = (double)LsJ / 2;

            VolToSliceX = (xMax - xMin) / LI * (double)Slice.Height / (PaintingWidth);
            VolToSliceY = (yMax - yMin) / LJ * (double)Slice.Height / (PaintingWidth);
            VolToSliceZ = (zMax - zMin) / LK * (double)Slice.Width / (PaintingHeight);

            int K_ = 0;
            int FinishedCount = 0;
            #endregion

            double sX, sY, sdotI;
            float u;
            int lower_sI, lower_sJ;
            bool SliceFound = false;

            unsafe
            {
                //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
                unchecked
                {

                    //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it
                    SliceFound = false;
                    int StartI = (int)halfK;// (new Random(DateTime.Now.Millisecond)).Next(LK);
                    for (int i = 0; i < LK; i++)
                    {
                        K_ = (i + StartI) % LK;
                        LockIndicator[K_] = true;
                        FinishedCount++;
                        SliceFound = true;

                        if (SliceFound == true)
                        {
                            //indicate that the thread is locked
                            lock (LockArray[K_])
                            {
                                lower_sJ = (int)((halfK - K_) * VolToSliceZ + halfJs) - 1;

                                #region UnSmear slice


                                for (int J_ = 0; J_ < LJ; J_++)
                                {
                                    //tranform to slice index coords
                                    sY = (J_ - halfI) * VolToSliceY;
                                    for (int I_ = 0; I_ < LI; I_++)
                                    {
                                        if (DensityGrid[K_, J_, I_] != 0)
                                        {
                                            //tranform to slice index coords
                                            sX = (I_ - halfI) * VolToSliceX;

                                            sdotI = halfIs - (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                                            //make sure that we are still in the recon volumn
                                            if (sdotI > 0 && sdotI < LsI_1)
                                                if (K_ > 0 && K_ < LsJ_1)
                                                {

                                                    lower_sI = (int)Math.Floor(sdotI);
                                                    u = (float)sdotI - lower_sI;
                                                    difference[lower_sI, lower_sJ, 0] += DensityGrid[K_, J_, I_];// *(1 - u);
                                                    //difference[(lower_sI + 1), lower_sJ,0] += DensityGrid[K_, J_, I_] * u;
                                                }
                                        }
                                    }
                                }


                                #endregion
                            }
                        }
                    }
                }
            }

            return diffImage;
        }
        #endregion

        #endregion

        protected override void RunNodeImpl()
        {

        }
    }
}
