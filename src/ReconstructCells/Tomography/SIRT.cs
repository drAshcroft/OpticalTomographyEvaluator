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
    public class SIRTRecon : ReconTemplate
    {
        #region Properties

        #region Set

        #endregion

        #region Get


        #endregion

        #endregion

        #region Code



        private float[, ,] SecondGrid;
        private float[, ,] ErrorGrid;
        private float[, ,] AlphaWeighting;

        const double P = 1.1;
       
        const double sigma = .1;
        double factor = 1 / P / Math.Pow(sigma, P) / 26;
        const double alpha = 100;
        float calmFactor = .001f;
       // float errorWeight = (float)(1.0 / 50);
        double MaxR = 1;
        int batchStartIndex = 0;
        int batchSkip = 1;


        //  int[,] Mask;
        Random rnd = new Random();

        #region Parallel Math


        private double I(double delta)
        {
            delta = Math.Abs(delta);
            return Math.Pow(delta, P) + P * P * Math.Pow(delta, P - 1);

        }

        private double MRFRegularization(double centerValue, int x, int y, int z)
        {
            int x0 = x - 1;
            int y0 = y - 1;
            int z0 = z - 1;

            int x1 = x + 1;
            int y1 = y + 1;
            int z1 = z + 1;

            //get the corners
            double sum = I(DensityGrid[x0, y0, z0] - centerValue);
            sum += I(DensityGrid[x0, y1, z0] - centerValue);
            sum += I(DensityGrid[x0, y0, z1] - centerValue);
            sum += I(DensityGrid[x0, y1, z1] - centerValue);

            sum += I(DensityGrid[x1, y0, z0] - centerValue);
            sum += I(DensityGrid[x1, y1, z0] - centerValue);
            sum += I(DensityGrid[x1, y0, z1] - centerValue);
            sum += I(DensityGrid[x1, y1, z1] - centerValue);
            sum *= r;

            //then get the sides
            sum += I(DensityGrid[x0, y, z] - centerValue);
            sum += I(DensityGrid[x, y0, z] - centerValue);
            sum += I(DensityGrid[x, y, z0] - centerValue);
            sum += I(DensityGrid[x1, y, z] - centerValue);
            sum += I(DensityGrid[x, y1, z] - centerValue);
            sum += I(DensityGrid[x, y, z1] - centerValue);

            sum = factor * sum;

            return sum;
        }

        private void BatchAddAndZero(int sliceNumber)
        {
            float error, d;
            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    if (MaxR > (x - halfI) * (x - halfI) + (y - halfJ) * (y - halfJ) + (sliceNumber - halfK) * (sliceNumber - halfK))
                    {
                        {
                            error = ErrorGrid[sliceNumber, x, y];// *errorWeight;
                            d = DensityGrid[sliceNumber, x, y] + error;

                            if (d > 0)
                                SecondGrid[sliceNumber, x, y] = d;
                            else
                                SecondGrid[sliceNumber, x, y] = DensityGrid[sliceNumber, x, y];
                        }
                    }
                    else
                        SecondGrid[sliceNumber, x, y] = 0;

                    ErrorGrid[sliceNumber, x, y] = 0;
                    AlphaWeighting[sliceNumber, x, y] = 0;
                }
        }

        private void BatchMoveArray(int sliceNumber)
        {

            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    DensityGrid[sliceNumber, x, y] = SecondGrid[sliceNumber, x, y];
                }
        }

        #endregion

        #region Statistical


        private void BatchStatProject(int imageNumber)
        {
            //imageNumber *= 20;
            //imageNumber = (imageNumber * 100 + imageNumber) % Library.Count;
            imageNumber = (batchStartIndex + imageNumber * batchSkip) % Library.Count;

            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
            DoSIRTProjection_OneSlice(Library[imageNumber], Angle, Axis2D.YAxis);
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
        private void DoSIRTProjection_OneSlice(Image<Gray, float> Slice, double AngleRadians, Axis2D ConvolutionAxis)
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

            SmearArray2DWhole(AngleRadians, Slice, vec, Point3D.CrossProduct(vec, vRotationAxis));
        }

        private void SmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, Point3D Direction, Point3D FastScanDirection)
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

            int K_ = 0;
            int FinishedCount = 0;
            double sX, sY, sdotI;
            float val;
            float u;
            int lower_sI, lower_sJ;

            unsafe
            {
                //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
                unchecked
                {
                    //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it

                    int StartI = 0;// (int)halfK;// (new Random(DateTime.Now.Millisecond)).Next(LK);

                    #region Calculate all the weights
                    float[,] Weights = new float[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
                    int[,] Coord = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
                    for (int J_ = 0; J_ < LJ; J_++)
                    {
                        //tranform to slice index coords
                        sY = (J_ - halfI) * VolToSliceY;
                        for (int I_ = 0; I_ < LI; I_++)
                        {

                            //tranform to slice index coords
                            sX = (I_ - halfI) * VolToSliceX;

                            sdotI = halfIs + (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                            //make sure that we are still in the recon volumn
                            if (sdotI > 0 && sdotI < LsI_1)
                            {
                                lower_sI = (int)Math.Floor(sdotI);
                                u = (float)(sdotI - lower_sI);
                                Weights[J_, I_] = u;
                                Coord[J_, I_] = lower_sI;
                                counts[lower_sI] += (1 - u);
                                counts[lower_sI + 1] += (u);
                            }
                            else
                                Weights[J_, I_] = -1;
                        }
                    }
                    #endregion


                    #region UnSmear slice
                    for (K_ = 0; K_ < LK; K_++)
                    {

                        for (int J_ = 0; J_ < LJ; J_++)
                        {

                            for (int I_ = 0; I_ < LI; I_++)
                            {
                                if (Weights[J_, I_] != -1)
                                {
                                    lower_sI = Coord[J_, I_];
                                    u = Weights[J_, I_];
                                    val = DensityGrid[K_, J_, I_];

                                    difference[lower_sI, K_, 0] += val * (1 - u);
                                    difference[lower_sI + 1, K_, 0] += val * u;
                                }
                            }
                        }
                    }
                    #endregion

                    int halfIndex = (int)halfJ;


                    #region DoDifference

                    for (int x = 0; x < LsJ; x++)
                    {
                        float dd, v;
                        for (int y = 0; y < LsI; y++)
                        {
                            dd = difference[y, x, 0];
                            v = PaintingArray[y, x, 0];
                            difference[y, x, 0] = calmFactor * (v - dd);
                        }
                    }

                    #endregion

                    float df, df2;
                    StartI = rnd.Next(LK);
                    for (int i = 0; i < LK; i++)
                    {
                        K_ = (i + StartI) % LK;
                        LockIndicator[K_] = true;
                        FinishedCount++;


                        //indicate that the thread is locked
                        lock (LockArray[K_])
                        {
                            lower_sJ = K_;

                            #region Smear Slice


                            for (int J_ = 0; J_ < LJ; J_++)
                            {
                                for (int I_ = 0; I_ < LI; I_++)
                                {
                                    if (Weights[J_, I_] != -1)
                                    {
                                        lower_sI = Coord[J_, I_];
                                        u = Weights[J_, I_];

                                        df = difference[lower_sI + 1, lower_sJ, 0];
                                        df2 = difference[lower_sI, lower_sJ, 0];

                                        ErrorGrid[K_, J_, I_] += df2 * (1 - u) + df * (u);
                                        AlphaWeighting[K_, J_, I_] += u * u + (1 - u) * (1 - u);
                                    }
                                }
                            }

                            #endregion
                        }
                        LockIndicator[K_] = false;
                    }

                    // ErrorGrid.SaveCross(@"c:\temp\error.bmp");

                }
            }
        }
        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            DoProjections(mPassData.DensityGrid);
        }

        int ProjectionNumber = 0;
        public float[, ,] DoProjections(float[, ,] densityGrid)
        {
            int GridSize = Library[0].Width;

            DensityGrid = densityGrid;
            SetConstants(Library[0]);


            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            int numberOfImages = Library.Count;

            CleanProjections();
            Mask = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

            MaxR = DensityGrid.GetLength(1) / 2;
            MaxR *= MaxR;

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


            //  DensityGrid.SaveCross(

            var b = DensityGrid.ShowCross();
            //b[0].Save(@"c:\temp\FBPview1.bmp");
            //b[1].Save(@"c:\temp\FBPview2.bmp");
            //b[2].Save(@"c:\temp\FBPview3.bmp");

            b[0] = Mask.MakeBitmap();

            ErrorGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
            AlphaWeighting = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
            SecondGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

            ProjectionNumber++;
            int nIterations = 5;

            // BatchSIRTProject(0);
            double m = densityGrid.MaxArray();

            for (int j = 0; j < nIterations; j++)
            {

                calmFactor = .75f / DensityGrid.GetLength(0)*(float)((20 * batchSkip / (numberOfImages)) / Math.Sqrt(j + 2));

                batchStartIndex = j;

                Parallel.For(0, (int)(numberOfImages / (double)batchSkip), Program.threadingParallelOptions, x => BatchStatProject(x));

                float sum = ErrorGrid.AverageArrayWithThreshold(-500);
                float s2 = ErrorGrid.MinArray();
                float s3 = ErrorGrid.MaxArray();

                //  for (int i = 120; i < 130; i++)
                //    BatchAddAndZero(i);
               // errorWeight = ;
                sum += s2 + s3;
                Parallel.For(0, (int)DensityGrid.GetLength(0), Program.threadingParallelOptions, x => BatchAddAndZero(x));

                double m2 = DensityGrid.MaxArray();
                double m3 = SecondGrid.MaxArray();

                float[, ,] tempGrid = DensityGrid;

                Program.ShowBitmaps(SecondGrid);
             
                DensityGrid = SecondGrid;
                SecondGrid = tempGrid;

                ProjectionNumber++;
                m = m + m2 + m3;
            }


            return DensityGrid;
        }
        #endregion
    }
}





//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Emgu.CV;
//using Emgu.CV.Structure;
//using ImageProcessing._3D;
//using System.Drawing;
//using MathLibrary;
//using ImageProcessing;

//namespace ReconstructCells.Tomography
//{
//    public class SIRT : ReconTemplate
//    {
//        float alpha = 1;

//       // int[,] Mask;
//        Random rnd = new Random();

//        public SIRT()
//        {

//        }

//        private float[, ,] ErrorGrid;

//        #region Properties

//        #region Set

//        #endregion

//        #region Get


//        #endregion

//        #endregion

//        #region Code

//        #region Parallel Math

//        private void BatchAddAndZero(int sliceNumber)
//        {
//            float error, d;
//            for (int x = 0; x < DensityGrid.GetLength(1); x++)
//                for (int y = 0; y < DensityGrid.GetLength(2); y++)
//                {
//                    error = ErrorGrid[sliceNumber, x, y];
//                    d = DensityGrid[sliceNumber, x, y];
//                    if (error > -50000)
//                        DensityGrid[sliceNumber, x, y] += error;
//                    else
//                        DensityGrid[sliceNumber, x, y] = 0;
//                    if (DensityGrid[sliceNumber, x, y] < 0) DensityGrid[sliceNumber, x, y] = 0;
//                    ErrorGrid[sliceNumber, x, y] = 0;
//                }
//        }

//        #endregion

//        #region SIRT


//        private void BatchSIRTProject(int imageNumber)
//        {
//            //imageNumber *= 20;
//            //imageNumber = (imageNumber * 100 + imageNumber) % Library.Count;

//            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
//            DoSIRTProjection_OneSlice(Library[imageNumber], Angle, Axis2D.YAxis);
//        }

//        /// <summary>
//        /// calculates the needed vectors to turn the requested angle into projection vectors
//        /// </summary>
//        /// <param name="Slice"></param>
//        /// <param name="PaintingWidth">Physical dimension of the pseudo projection (vs the recon size)</param>
//        /// <param name="PaintingHeight">Physical dimension of the pseudo projection (vs the recon size)</param>
//        /// <param name="DensityGrid"></param>
//        /// <param name="AngleRadians"></param>
//        /// <param name="ConvolutionAxis">Usually set to YAxis. This is the direction that carries the rotation, or the fast axis for the convolution</param>
//        private void DoSIRTProjection_OneSlice(Image<Gray, float> Slice, double AngleRadians, Axis2D ConvolutionAxis)
//        {
//            //AngleRadians = 0;
//            Axis RotationAxis = Axis.ZAxis;

//            Point3D vRotationAxis = new Point3D();
//            Point3D axis = new Point3D();

//            if (RotationAxis == Axis.XAxis)
//            {
//                vRotationAxis = new Point3D(1, 0, 0);
//                axis = new Point3D(0, 1, 0);
//            }
//            else if (RotationAxis == Axis.YAxis)
//            {
//                vRotationAxis = new Point3D(0, 1, 0);
//                axis = new Point3D(0, 0, 1);
//            }
//            else if (RotationAxis == Axis.ZAxis)
//            {
//                vRotationAxis = new Point3D(0, 0, 1);
//                axis = new Point3D(0, 1, 0);
//            }

//            Point3D vec = Point3D.RotateAroundAxis(axis, vRotationAxis, AngleRadians);

//            SmearArray2DWhole(AngleRadians, Slice, vec, Point3D.CrossProduct(vec, vRotationAxis));
//        }

//        /* private void SmearArray2DWholeSlow(double AngleRadians, Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, Point3D Direction, Point3D FastScanDirection)
//         {
//             float[, ,] PaintingArray = Slice.Data;
//             Image<Gray, float> diffImage = Slice.CopyBlank();
//             float[, ,] difference = diffImage.Data;
//             float[] counts = new float[diffImage.Width];
//             //make sure all the vectors are the right size
//             Direction.Normalize();
//             FastScanDirection.Normalize();
//             //determine all the important planes in the 3D space and all the step
//             Point3D SlowScanAxis = Point3D.CrossProduct(Direction, FastScanDirection);

//             ///get the existing recon data.  the jagged array is used so the data can be checked out by each thread





//             int K_ = 0;
//             int FinishedCount = 0;


//             double sX, sY, sdotI;
//             float val;
//             float u;
//             int lower_sI, lower_sJ;

//             unsafe
//             {
//                 //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
//                 unchecked
//                 {

//                     //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it

//                     int StartI = 0;// (int)halfK;// (new Random(DateTime.Now.Millisecond)).Next(LK);
//                     for (K_ = 0; K_ < LK; K_++)
//                     {
//                         #region UnSmear slice
//                         for (int J_ = 0; J_ < LJ; J_++)
//                         {
//                             //tranform to slice index coords
//                             sY = (J_ - halfI) * VolToSliceY;
//                             for (int I_ = 0; I_ < LI; I_++)
//                             {
//                                 if (Mask[J_, I_] != 1)
//                                 {
//                                     //tranform to slice index coords
//                                     sX = (I_ - halfI) * VolToSliceX;

//                                     sdotI = halfIs - (sX * FastScanDirection.X + sY * FastScanDirection.Y);
//                                     //make sure that we are still in the recon volumn
//                                     if (sdotI > 0 && sdotI < LsI_1)
//                                         if (K_ > 0 && K_ < LsJ_1)
//                                         {
//                                             // System.Diagnostics.Debug.Print(sdotI.ToString());
//                                             // lower_sI = (int)Math.Round(sdotI);
//                                             lower_sI = (int)Math.Floor(sdotI);
//                                             u = (float)(sdotI - lower_sI);
//                                             val = DensityGrid[K_, J_, I_];
//                                             // difference[lower_sI, lower_sJ, 0] += DensityGrid[K_, J_, I_];
//                                             difference[lower_sI, K_, 0] += val * (1 - u);
//                                             difference[lower_sI + 1, K_, 0] += val * u;
//                                             if (K_ == countRow)
//                                             {

//                                                 counts[lower_sI] += (1 - u);
//                                                 counts[lower_sI + 1] += (u);
//                                             }
//                                             //difference[(lower_sI + 1), lower_sJ, 0] += val * u;
//                                         }
//                                 }
//                             }
//                         }
//                         #endregion
//                     }

//                     int halfIndex = (int)halfJ;


//                     List<int> badRows = new List<int>();

//                     float fc, fcc;
//                     for (int y = 1; y < LsI_1; y++)
//                     {
//                         fcc = (float)((2 * radiusCyl * Math.Sqrt(1 - Math.Pow((halfI - y) / radiusCyl, 2))));
//                         if (double.IsNaN(fcc) == false && fcc > 1 && counts[y] != 0)
//                         {
//                             fc = counts[y] / fcc;
//                             for (int x = 0; x < LsJ; x++)
//                             {
//                                 difference[y, x, 0] /= (fc);
//                             }
//                         }
//                         else
//                         {
//                             for (int x = 0; x < LsJ; x++)
//                             {
//                                 difference[y, x, 0] = -100000;
//                             }
//                             badRows.Add(y);
//                         }
//                     }


//                     #region DoDifference


//                     for (int x = 0; x < LsJ; x++)
//                     {
//                         float dd, v;
//                         for (int y = 0; y < LsI; y++)
//                         {
//                             dd = difference[y, x, 0];
//                             if (dd != 0 && dd != -100000)
//                             {
//                                 v = PaintingArray[y, x, 0];
//                                 if (x == 0)
//                                 {
//                                     if (y < (yLowerMax))
//                                         badRows.Add(y);
//                                     else if (y > yUpperMin)
//                                         badRows.Add(y);
//                                 }

//                                 /*  if (v == 0)
//                                   {
//                                       if (x == 0)
//                                       {
//                                           if (y < (yLowerMax))
//                                               badRows.Add(y);
//                                           else if (y > yUpperMin)
//                                               badRows.Add(y);
//                                       }

//                                       difference[y, x, 0] = -100000;
//                                   }
//                                   else*/

//        /*                        difference[y, x, 0] = alpha * (v - dd);
//                            }
//                        }
//                    }

//                    #endregion


//                    badRows.Add(yLower);
//                    badRows.Add(yUpper);
//                    foreach (int y in badRows)
//                    {
//                        for (int x = 0; x < LsJ; x++)
//                        {
//                            if (y > 1 && y < LsI_1)
//                            {
//                                difference[y - 1, x, 0] = -100000;
//                                difference[y + 1, x, 0] = -100000;
//                            }
//                            difference[y, x, 0] = -100000;
//                        }
//                    }


//                    float df, df2;
//                    StartI = rnd.Next(LK);
//                    for (int i = 0; i < LK; i++)
//                    {
//                        K_ = (i + StartI) % LK;
//                        LockIndicator[K_] = true;
//                        FinishedCount++;

//                        //indicate that the thread is locked
//                        lock (LockArray[K_])
//                        {
//                            lower_sJ = K_;


//                            #region Smear Slice


//                            for (int J_ = 0; J_ < LJ; J_++)
//                            {
//                                //tranform to slice index coords
//                                sY = (J_ - halfI) * VolToSliceY;
//                                for (int I_ = 0; I_ < LI; I_++)
//                                {
//                                    if (Mask[J_, I_] != 1)
//                                    {
//                                        //tranform to slice index coords
//                                        sX = (I_ - halfI) * VolToSliceX;

//                                        sdotI = halfIs - (sX * FastScanDirection.X + sY * FastScanDirection.Y);
//                                        //make sure that we are still in the recon volumn
//                                        if (sdotI > 0 && sdotI < LsI_1)
//                                            if (K_ > 0 && K_ < LsJ_1)
//                                            {
//                                                lower_sI = (int)Math.Floor(sdotI);
//                                                u = (float)(sdotI - lower_sI);

//                                                df = difference[lower_sI + 1, lower_sJ, 0];
//                                                df2 = difference[lower_sI, lower_sJ, 0];
//                                                if (df == -100000 || df2 == -100000 || ErrorGrid[K_, J_, I_] == -100000)
//                                                    ErrorGrid[K_, J_, I_] = -100000;
//                                                else
//                                                    ErrorGrid[K_, J_, I_] += df2 * (1 - u) + df * (u);

//                                            }
//                                    }
//                                }
//                            }

//                            #endregion
//                        }
//                        LockIndicator[K_] = false;
//                    }

//                }
//            }
//        }
//        */


//        private void SmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, Point3D Direction, Point3D FastScanDirection)
//        {
//            float[, ,] PaintingArray = Slice.Data;
//            Image<Gray, float> diffImage = Slice.CopyBlank();
//            float[, ,] difference = diffImage.Data;
//            float[] counts = new float[diffImage.Width];
//            //make sure all the vectors are the right size
//            Direction.Normalize();
//            FastScanDirection.Normalize();
//            //determine all the important planes in the 3D space and all the step
//            Point3D SlowScanAxis = Point3D.CrossProduct(Direction, FastScanDirection);

//            int K_ = 0;
//            int FinishedCount = 0;
//            double sX, sY, sdotI;
//            float val;
//            float u;
//            int lower_sI, lower_sJ;

//            unsafe
//            {
//                //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
//                unchecked
//                {
//                    //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it

//                    int StartI = 0;// (int)halfK;// (new Random(DateTime.Now.Millisecond)).Next(LK);

//                    #region Calculate all the weights
//                    float[,] Weights = new float[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
//                    int[,] Coord = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
//                    for (int J_ = 0; J_ < LJ; J_++)
//                    {
//                        //tranform to slice index coords
//                        sY = (J_ - halfI) * VolToSliceY;
//                        for (int I_ = 0; I_ < LI; I_++)
//                        {
//                            if (Mask[J_, I_] != 1)
//                            {
//                                //tranform to slice index coords
//                                sX = (I_ - halfI) * VolToSliceX;

//                                sdotI = halfIs + (sX * FastScanDirection.X + sY * FastScanDirection.Y);
//                                //make sure that we are still in the recon volumn
//                                if (sdotI > 0 && sdotI < LsI_1)
//                                {
//                                    lower_sI = (int)Math.Floor(sdotI);
//                                    u = (float)(sdotI - lower_sI);
//                                    Weights[J_, I_] = u;
//                                    Coord[J_, I_] = lower_sI;
//                                    counts[lower_sI] += (1 - u);
//                                    counts[lower_sI + 1] += (u);
//                                }
//                                else
//                                    Weights[J_, I_] = -1;
//                            }
//                            else
//                                Weights[J_, I_] = -1;
//                        }
//                    }
//                    #endregion

//                    //Weights.MakeBitmap().Save(@"c:\temp\weights.bmp");

//                    #region UnSmear slice
//                    for (K_ = 0; K_ < LK; K_++)
//                    {

//                        for (int J_ = 0; J_ < LJ; J_++)
//                        {

//                            for (int I_ = 0; I_ < LI; I_++)
//                            {
//                                if (Weights[J_, I_] != -1)
//                                {
//                                    lower_sI = Coord[J_, I_];
//                                    u = Weights[J_, I_];
//                                    val = DensityGrid[K_, J_, I_];

//                                    difference[lower_sI, K_, 0] += val * (1 - u);
//                                    difference[lower_sI + 1, K_, 0] += val * u;
//                                }
//                            }
//                        }
//                    }
//                    #endregion

//                    int halfIndex = (int)halfJ;

//                    #region Normalized Projection
//                    List<int> badRows = new List<int>();

//                    float fc, fcc;
//                    for (int y = 1; y < LsI_1; y++)
//                    {
//                        fcc = (float)((2 * radiusCyl * Math.Sqrt(1 - Math.Pow((halfI - y) / radiusCyl, 2))));
//                        if (double.IsNaN(fcc) == false && fcc > 1 && counts[y] != 0)
//                        {
//                            fc = counts[y] / fcc;
//                            for (int x = 0; x < LsJ; x++)
//                            {
//                                difference[y, x, 0] /= (fc);
//                            }
//                        }
//                        else
//                        {
//                            for (int x = 0; x < LsJ; x++)
//                            {
//                                difference[y, x, 0] = -100000;
//                            }
//                            badRows.Add(y);
//                        }
//                    }
//                    #endregion

//                    #region DoDifference

//                    for (int x = 0; x < LsJ; x++)
//                    {
//                        float dd, v;
//                        for (int y = 0; y < LsI; y++)
//                        {
//                            dd = difference[y, x, 0];
//                            if (dd != 0 && dd != -100000)
//                            {
//                                v = PaintingArray[y, x, 0];
//                                if (x == 0)
//                                {
//                                    if (y < (yLowerMax))
//                                        badRows.Add(y);
//                                    else if (y > yUpperMin)
//                                        badRows.Add(y);
//                                }
//                                difference[y, x, 0] = alpha * (v - dd);
//                            }
//                        }
//                    }

//                    #endregion

//                    #region CleanRows
//                    badRows.Add(yLower);
//                    badRows.Add(yUpper);
//                    foreach (int y in badRows)
//                    {
//                        for (int x = 0; x < LsJ; x++)
//                        {
//                            if (y > 1 && y < LsI_1)
//                            {
//                                difference[y - 1, x, 0] = -100000;
//                                difference[y + 1, x, 0] = -100000;
//                            }
//                            difference[y, x, 0] = -100000;
//                        }
//                    }
//                    #endregion

//                   // Weights.MakeBitmap().Save(@"c:\temp\weight.bmp");
//                   // diffImage.ScaledBitmap.Save(@"c:\temp\dif.bmp");
//                    float df, df2;
//                    StartI = rnd.Next(LK);
//                    for (int i = 0; i < LK; i++)
//                    {
//                        K_ = (i + StartI) % LK;
//                        LockIndicator[K_] = true;
//                        FinishedCount++;


//                        //indicate that the thread is locked
//                        lock (LockArray[K_])
//                        {
//                            lower_sJ = K_;

//                            #region Smear Slice


//                            for (int J_ = 0; J_ < LJ; J_++)
//                            {
//                                for (int I_ = 0; I_ < LI; I_++)
//                                {
//                                    if (Weights[J_, I_] != -1)
//                                    {
//                                        lower_sI = Coord[J_, I_];
//                                        u =  Weights[J_, I_];

//                                        df = difference[lower_sI + 1, lower_sJ, 0];
//                                        df2 = difference[lower_sI, lower_sJ, 0];
//                                        if (df == -100000 || df2 == -100000 || ErrorGrid[K_, J_, I_] == -100000)
//                                            ErrorGrid[K_, J_, I_] =  - 100000;
//                                        else
//                                            ErrorGrid[K_, J_, I_] += df2 * (1 - u) +df * (u);
//                                    }
//                                }
//                            }

//                            #endregion
//                        }
//                        LockIndicator[K_] = false;
//                    }

//                   // ErrorGrid.SaveCross(@"c:\temp\error.bmp");

//                }
//            }
//        }
//        #endregion

//        protected override void RunNodeImpl()
//        {
//            Library = mPassData.Library;
//            DoProjections(mPassData.DensityGrid);
//        }

//        int ProjectionNumber = 0;
//        public float[, ,] DoProjections(float[, ,] densityGrid)
//        {
//            int GridSize = Library[0].Width;

//            DensityGrid = densityGrid;
//            SetConstants(Library [0]);


//            ParallelOptions po = new ParallelOptions();
//            //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

//            int numberOfImages = Library.Count;

//            CleanProjections();
//            Mask = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

//            for (int j = 0; j < DensityGrid.GetLength(1); j++)
//                for (int k = 0; k < DensityGrid.GetLength(2); k++)
//                {
//                    double y = (j - HalfJ) / RY;
//                    double z = (k - HalfK) / RZ;
//                    if ((y * y + z * z) > 1)
//                    {
//                        Mask[j, k] = 1;
//                    }
//                }


//            //  DensityGrid.SaveCross(

//            var b = DensityGrid.ShowCross();
//            b[0].Save(@"c:\temp\FBPview1.bmp");
//            b[1].Save(@"c:\temp\FBPview2.bmp");
//            b[2].Save(@"c:\temp\FBPview3.bmp");


//            ErrorGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];


//            ProjectionNumber++;
//            int nIterations = 5;

//            // BatchSIRTProject(0);

//            for (int j = 0; j < nIterations; j++)
//            {
//                //if (j <2)
//                alpha = .25f / DensityGrid.GetLength(0)/50;
//                //else
//                //alpha = (float)(1f * (float)Math.Exp(-1 * j / (7f / 2)) / DensityGrid.GetLength(0) / numberOfImages);


//              //  DensityGrid.SaveCross(@"c:\temp\densityBefore");

//                Parallel.For(0, (int)numberOfImages, po, x => BatchSIRTProject(x));
//              //  for (int i = 0; i < 250; i += 30)
//              //      BatchSIRTProject(i);

//                ErrorGrid.SaveCross(@"c:\temp\error.bmp");
//               /* float sum = ErrorGrid.AverageArrayWithThreshold(-500);
//                float s2 = ErrorGrid.MinArray();
//                float s3 = ErrorGrid.MaxArray();*/

//              //  sum += s2 + s3;
//                Parallel.For(0, (int)DensityGrid.GetLength(0), po, x => BatchAddAndZero(x));

//               // DensityGrid.SaveCross(@"c:\temp\density");

//                DensityGrid.SaveCross(@"c:\temp\SIRTview.bmp");

//                ProjectionNumber++;
//            }


//            return DensityGrid;
//        }
//        #endregion
//    }
//}

