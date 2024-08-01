using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageProcessing._3D;
using System.Drawing;
using MathLibrary;
using ImageProcessing;

namespace ReconstructCells.Tomography
{
    public class SART : ReconTemplate
    {

        #region Properties

        #region Set

        #endregion

        #region Get


        #endregion

        #endregion

        #region Code
        public SART()
        {

        }

        #region SIRT
        float alpha = 1;

       

        private void BatchSIRTProject(int imageNumber)
        {
            //imageNumber *= 20;
            //imageNumber = (imageNumber * 100 + imageNumber) % Library.Count;

            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
            DoSIRTProjection_OneSlice(Library[imageNumber], 1, 1, Angle, Axis2D.YAxis);
        }

        /// <summary>
        /// calculates the needed vectors to turn the requested angle into projection vectors
        /// </summary>
        /// <param name="Slice"></param>
        /// <param name="PaintingWidth">Physical dimension of the pseudo projection (vs the recon size)</param>
        /// <param name="PaintingHeight">Physical dimension of the pseudo projection (vs the recon size)</param>
        /// <param name="DensityGrid"></param>
        /// <param name="AngleRadians"></param>
        /// <param name="ConvolutionAxis">Usually set to YAxis. This is the direction that carries the rotation, or the fast axis for the convolution</param>
        private void DoSIRTProjection_OneSlice(Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, double AngleRadians, Axis2D ConvolutionAxis)
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

            SmearArray2DWhole(AngleRadians, Slice, PaintingWidth, PaintingHeight, vec, Point3D.CrossProduct(vec, vRotationAxis));
        }


        private void SmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, Point3D Direction, Point3D FastScanDirection)
        {
            float[, ,] PaintingArray = Slice.Data;
            Image<Gray, float> diffImage = Slice.CopyBlank();
            float[, ,] difference = diffImage.Data;
            float[] counts = new float[diffImage.Width];
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


            double RX = DensityGrid.GetLength(0) / 2d - 15;
            double RY = DensityGrid.GetLength(1) / 2d - 15;
            double RZ = DensityGrid.GetLength(2) / 2d - 15;

            double HalfI = DensityGrid.GetLength(0) / 2d;
            double HalfJ = DensityGrid.GetLength(1) / 2d;
            double HalfK = DensityGrid.GetLength(2) / 2d;
            int yLower = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 10));
            int yUpper = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 10));

            int yLowerMax = (int)(halfI - (DensityGrid.GetLength(0) / 2d - 15));
            int yUpperMin = (int)(halfI + (DensityGrid.GetLength(0) / 2d - 15));

            double radiusCyl = (LsI - 20) / 2;
            int countRow = (int)halfI;

            double sX, sY,  sdotI;
            float val;
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
                    int StartI = 0;// (int)halfK;// (new Random(DateTime.Now.Millisecond)).Next(LK);
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
                                lower_sJ = K_;// (int)((halfK - K_) * VolToSliceZ + halfJs) - 1;

                                #region UnSmear slice


                                for (int J_ = 0; J_ < LJ; J_++)
                                {
                                    //tranform to slice index coords
                                    sY = (J_ - halfI) * VolToSliceY;
                                    for (int I_ = 0; I_ < LI; I_++)
                                    {
                                        if (Mask[J_, I_] != 1)
                                        {
                                            //tranform to slice index coords
                                            sX = (I_ - halfI) * VolToSliceX;

                                            sdotI = halfIs - (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                                            //make sure that we are still in the recon volumn
                                            if (sdotI > 0 && sdotI < LsI_1)
                                                if (K_ > 0 && K_ < LsJ_1)
                                                {
                                                    // System.Diagnostics.Debug.Print(sdotI.ToString());
                                                    // lower_sI = (int)Math.Round(sdotI);
                                                    lower_sI = (int)Math.Floor(sdotI);
                                                    u = (float)(sdotI - lower_sI);
                                                    val = DensityGrid[K_, J_, I_];
                                                    // difference[lower_sI, lower_sJ, 0] += DensityGrid[K_, J_, I_];
                                                    difference[lower_sI, lower_sJ, 0] += val * (1 - u);
                                                    difference[lower_sI + 1, lower_sJ, 0] += val * u;
                                                    if (lower_sJ == countRow)
                                                    {

                                                        counts[lower_sI] += (1 - u);
                                                        counts[lower_sI + 1] += (u);
                                                    }
                                                    //difference[(lower_sI + 1), lower_sJ, 0] += val * u;
                                                }
                                        }
                                    }
                                }
                                #endregion


                            }
                        }
                    }

                    int halfIndex = (int)halfJ;


                    List<int> badRows = new List<int>();

                    float fc, fcc;
                    for (int y = 1; y < LsI_1; y++)
                    {
                        fcc = (float)((2 * radiusCyl * Math.Sqrt(1 - Math.Pow((halfI - y) / radiusCyl, 2))));
                        if (double.IsNaN(fcc) == false && fcc > 1 && counts[y] != 0)
                        {
                            fc = counts[y] / fcc;
                            for (int x = 0; x < LsJ; x++)
                            {
                                difference[y, x, 0] /= (fc);
                            }
                        }
                        else
                        {
                            for (int x = 0; x < LsJ; x++)
                            {
                                difference[y, x, 0] = -100000;
                            }
                            badRows.Add(y);
                        }
                    }



                    // var testmax = difference.MaxArray();
                    //var testmin = difference.MinArray();
                    // var cB = counts.MakeBitmap();
                    // int w = cB.Width;
                    //  diffImage.ScaledBitmap.Save(@"c:\temp\Forwardimages_" + ProjectionNumber + ".bmp");
                    #region DoDifference


                    for (int x = 0; x < LsJ; x++)
                    {
                        float dd, v;
                        for (int y = 0; y < LsI; y++)
                        {
                            dd = difference[y, x, 0];
                            if (dd != 0 && dd != -100000)
                            {
                                v = PaintingArray[y, x, 0];
                                if (v == 0)
                                {
                                    if (x == 0)
                                    {
                                        if (y < (yLowerMax))
                                            badRows.Add(y);
                                        else if (y > yUpperMin)
                                            badRows.Add(y);
                                    }

                                    difference[y, x, 0] = -100000;
                                }
                                else
                                    difference[y, x, 0] = alpha * (v - dd) / LsJ;
                            }
                        }
                    }

                    #endregion
                    var testmax = difference.MaxArray();
                    var testmin = difference.MinArray();

                    //    testmax = PaintingArray.MaxArray();
                    //   testmin = PaintingArray.MinArray();
                    //    

                    var w = testmax - testmin;

                    //the top and bottom lines are always screwed up
                    if (ProjectionNumber > 10)
                    {
                        var b = DensityGrid.ShowCross();
                        b[0].Save(@"c:\temp\SIRT0view1.bmp");
                        b[1].Save(@"c:\temp\SIRT0view2.bmp");
                        b[2].Save(@"c:\temp\SIRT0view3.bmp");
                    }
                    if (ProjectionNumber == 10)
                    {
                        diffImage = diffImage.SmoothMedian(5);
                        //  diffImage = diffImage.SmoothGaussian(5);
                        difference = diffImage.Data;
                        //System.Diagnostics.Debug.Print("");
                    }

                    for (int x = 0; x < LsJ; x++)
                    {
                        difference[yLower, x, 0] = -100000;
                        difference[yUpper, x, 0] = -100000;
                    }
                    foreach (int y in badRows)
                    {
                        for (int x = 0; x < LsJ; x++)
                        {
                            if (y > 1 && y < LsI_1)
                            {
                                difference[y - 1, x, 0] = -100000;
                                difference[y + 1, x, 0] = -100000;
                            }
                            difference[y, x, 0] = -100000;
                        }
                    }
                    diffImage.ScaledBitmap.Save(@"c:\temp\diffimages_" + ProjectionNumber + ".bmp");
                    float df, df2;

                    for (int i = 0; i < LK; i++)
                    {
                        K_ = (i + StartI) % LK;
                        LockIndicator[K_] = true;
                        FinishedCount++;
                        SliceFound = true;


                        //indicate that the thread is locked
                        lock (LockArray[K_])
                        {
                            lower_sJ = K_;


                            #region Smear Slice


                            for (int J_ = 0; J_ < LJ; J_++)
                            {
                                //tranform to slice index coords
                                sY = (J_ - halfI) * VolToSliceY;
                                for (int I_ = 0; I_ < LI; I_++)
                                {
                                    if (Mask[J_, I_] != 1)
                                    {
                                        //tranform to slice index coords
                                        sX = (I_ - halfI) * VolToSliceX;

                                        sdotI = halfIs - (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                                        //make sure that we are still in the recon volumn
                                        if (sdotI > 0 && sdotI < LsI_1)
                                            if (K_ > 0 && K_ < LsJ_1)
                                            {
                                                lower_sI = (int)Math.Floor(sdotI);
                                                u = (float)(sdotI - lower_sI);

                                                df = difference[lower_sI + 1, lower_sJ, 0];
                                                df2 = difference[lower_sI, lower_sJ, 0];
                                                if (df == -100000 || df2 == -100000)
                                                    DensityGrid[K_, J_, I_] = 0;
                                                else
                                                    DensityGrid[K_, J_, I_] += df2 * (1 - u) + df * (u);

                                            }
                                    }
                                }
                            }

                            #endregion
                        }
                        //release the programatic handle to 
                        LockIndicator[K_] = false;


                    }
                    // cB = counts.MakeBitmap();
                    // w = cB.Width;
                }
                //SmearArray2DQueue = null;
            }


        }
        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;

            DoProjections();
        }

        int ProjectionNumber = 0;
        public float[, ,] DoProjections()
        {
            int GridSize = Library[0].Width;

            //set up the output grid and the locks that will be needed for the library system
            DensityGrid = mPassData.DensityGrid;
            LockArray = new object[GridSize];
            LockIndicator = new bool[GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                LockArray[i] = new object();
            }


            CleanProjections();
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


            var b = DensityGrid.ShowCross();
            b[0].Save(@"c:\temp\FBPview1.bmp");
            b[1].Save(@"c:\temp\FBPview2.bmp");
            b[2].Save(@"c:\temp\FBPview3.bmp");
            //DensityGrid.ShowCross().SaveArray(@"c:\temp", "fbp");

            Library[0].ScaledBitmap.Save(@"C:\temp\slice.bmp");

            Image<Gray, float> b5 = null, b6 = null;

            var maxGrid = DensityGrid.MaxArray();

            alpha = 1f;
            #region TestProjections
            /* BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            BatchSIRTProject((0) % 250);
            ProjectionNumber++;
            b5 = BatchForwardProject(0);

            b = DensityGrid.ShowCross();
            b[0].Save(@"c:\temp\SIRTview1.bmp");
            b[1].Save(@"c:\temp\SIRTview2.bmp");
            b[2].Save(@"c:\temp\SIRTview3.bmp");

            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;
            BatchSIRTProject((125) % 250);
            ProjectionNumber++;

            b = DensityGrid.ShowCross();
            b[0].Save(@"c:\temp\SIRTview1.bmp");
            b[1].Save(@"c:\temp\SIRTview2.bmp");
            b[2].Save(@"c:\temp\SIRTview3.bmp");

            b6 = BatchForwardProject(125);
            BatchSIRTProject((110) % 250);
            BatchSIRTProject((220) % 250);
            BatchSIRTProject((50) % 250);

            ProjectionNumber++;

            BatchSIRTProject((0) % 250);
            BatchSIRTProject((110) % 250);
            BatchSIRTProject((220) % 250);
            BatchSIRTProject((50) % 250);

            b = DensityGrid.ShowCross();
            b[0].Save(@"c:\temp\SIRTview1.bmp");
            b[1].Save(@"c:\temp\SIRTview2.bmp");
            b[2].Save(@"c:\temp\SIRTview3.bmp");
            */
            #endregion
            ProjectionNumber++;
            Random rnd = new Random();
            int index;


            List<Tuple<double, int>> indexList = new List<Tuple<double, int>>();
            for (int i = 0; i < Library.Count; i += 4)
                indexList.Add(new Tuple<double, int>(rnd.NextDouble(), i));

            indexList.Sort((a, bb) => a.Item1.CompareTo(bb.Item1));

            int indexIndex = 0;
            for (int j = 0; indexIndex < indexList.Count; j += 5)
            {
                for (int i = 0; i < 2; i++)
                {
                    index = indexList[(indexIndex % indexList.Count)].Item2; //rnd.Next(Library.Count - 1);

                    BatchSIRTProject((index) % 250);
                    BatchSIRTProject((110 + index) % 250);

                    indexIndex++;
                    //ProjectionNumber++;
                }
                ProjectionNumber++;
                alpha = 1f * (float)Math.Exp(-1 * j / (7 * 2));
            }
            b5 = BatchForwardProject(0);
            b6 = BatchForwardProject(125);
            var w = b5.Width;
            var min = b5.Data[25, 25, 0];
            var max = b5.Data[b5.Width / 2, b5.Width / 2, 0];

            maxGrid = DensityGrid.MaxArray();
            var ww = min - max - maxGrid;

            b = DensityGrid.ShowCross();
            b[0].Save(@"c:\temp\SIRTview1.bmp");
            b[1].Save(@"c:\temp\SIRTview2.bmp");
            b[2].Save(@"c:\temp\SIRTview3.bmp");

            alpha = 1;
            for (int i = 0; i < 2; i++)
            {
                alpha = .25f * (float)Math.Exp(-1 * i / 7);
                //Parallel.For(0, numberOfImages, po, x => BatchSIRTProject(x));

                for (int j = 0; j < 60; j++)
                    BatchSIRTProject(j);
            }

            return DensityGrid;
        }
        #endregion
    }
}

