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
using MathLibrary.FFT;

namespace ReconstructCells.Tomography
{
    public class BI_IRT : ReconTemplate
    {
        #region Properties
        private bool Realign = false;
        #region Set
        public void setRealign(bool realign)
        {
            Realign = realign;
        }
        #endregion

        #region Get


        #endregion

        #endregion

        #region Code

        private float[, ,] predictedGrid;
        private float[, ,] ErrorGrid;
        private float[, ,] PredictedVariance;


        float calmFactor = .001f;

        double MaxR = 1;
        int batchStartIndex = 0;
        int batchSkip = 1;
        float noiseVar = 4;
        float gain = .8f;
        float lambda = .6f;
        //  int[,] Mask;
        Random rnd = new Random();

        #region Parallel Math

        private void BatchAddAndZero(int sliceNumber)
        {
            float error, observed;
            float kalman, predicted, theta1;
            float weight = (1 - lambda) / 6f;
            if (sliceNumber > 0 && sliceNumber < DensityGrid.GetLength(0)-1)
            {
                for (int x = 1; x < DensityGrid.GetLength(1) - 1; x++)
                    for (int y = 1; y < DensityGrid.GetLength(2) - 1; y++)
                    {
                        {
                            {

                                observed = DensityGrid[sliceNumber, x, y] + ErrorGrid[sliceNumber, x, y];

                                //   if (observed < 0)
                                //       observed = DensityGrid[sliceNumber, x, y];

                                //error = PredictedVariance[sliceNumber, x, y];

                                //kalman = error / (error + noiseVar);
                                //predicted = (float)(gain * predictedGrid[sliceNumber, x, y] + (1.0 - gain) * observed + kalman * (observed - predictedGrid[sliceNumber, x, y]));
                                //PredictedVariance[sliceNumber, x, y] = (float)(error * (1.0 - kalman));

                                //predictedGrid[sliceNumber, x, y] = predicted;

                                theta1 = DensityGrid[sliceNumber, x + 1, y] 
                                    + DensityGrid[sliceNumber, x - 1, y] 
                                    + DensityGrid[sliceNumber, x, y + 1] 
                                    + DensityGrid[sliceNumber, x, y - 1]
                                    + DensityGrid[sliceNumber + 1, x, y] 
                                    + DensityGrid[sliceNumber - 1, x, y];

                                DensityGrid[sliceNumber, x, y] = observed * lambda + theta1 * weight;

                                // DensityGrid[sliceNumber, x, y] = observed;
                            }
                        }
                        ErrorGrid[sliceNumber, x, y] = 0;
                    }
            }
        }

        private void BatchMoveArray(int sliceNumber)
        {

            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    DensityGrid[sliceNumber, x, y] = predictedGrid[sliceNumber, x, y];
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
            DoSIRTProjection_OneSlice(Library[imageNumber], Angle, Axis2D.YAxis, imageNumber);
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
        private void DoSIRTProjection_OneSlice(Image<Gray, float> Slice, double AngleRadians, Axis2D ConvolutionAxis, int imageNumber)
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

            SmearArray2DWhole(AngleRadians, Slice, vec, Point3D.CrossProduct(vec, vRotationAxis), imageNumber);
        }

        private void SmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, Point3D Direction, Point3D FastScanDirection, int imageNumber)
        {
            // var Slice2 = Slice.Copy();// Slice.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
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
            double sX, sY, sdotI, sdotJ;
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
                        // sdotJ=halfJs + sY;
                        // if (sdotJ > 0 && sdotJ < LsJ_1)
                        {
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
                    }
                    #endregion

                    float maxCount = counts.Max();
                    for (int i = 0; i < counts.Length; i++)
                        counts[i] /= maxCount;

                    #region UnSmear slice
                    int s_K;
                    for (K_ = 0; K_ < LK; K_++)
                    {
                        s_K = (int)(halfIs + (K_ - halfK) * VolToSliceY);
                        if (s_K<difference.GetLength(1)-1 && s_K>0)
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

                                    difference[lower_sI, s_K, 0] += val * (1 - u);
                                    difference[lower_sI + 1, s_K, 0] += val * u;
                                }
                            }
                        }
                        }
                    }
                    #endregion

                    int halfIndex = (int)halfJ;


                    //Image<Gray, float> test = new Image<Gray, float>(diffImage.Width, diffImage.Height * 2);
                    //test.ROI = new Rectangle(0, 0, diffImage.Width, diffImage.Height);
                    //diffImage.CopyTo(test );
                    //test.ROI = new Rectangle(0, diffImage.Height, diffImage.Width, diffImage.Height);
                    //Slice.CopyTo(test);
                    #region DoDifference
                    float d;
                    for (int x = 0; x < LsJ; x++)
                    {
                        // float dd, v;
                        // int t = PaintingArray.GetLength(0) - 1;
                        for (int y = 0; y < LsI; y++)
                        {
                            d = difference[y, x, 0];
                            if (d != 0)
                                difference[y, x, 0] = calmFactor * (PaintingArray[y, x, 0] - d);
                        }
                    }

                    //   ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\diffs\diff" + imageNumber + ".tif", diffImage);
                    //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\slice.tif", Slice);
                    #endregion

                    //   double d = difference.MaxArray();

                    float df, df2, weight;
                    df = 0;// (float)d;
                    df = 0;
                    StartI = rnd.Next(LK);
                    for (int i = 0; i < LK; i++)
                    {
                        K_ = (i + StartI) % LK;
                        LockIndicator[K_] = true;
                        FinishedCount++;


                        //indicate that the thread is locked
                        lock (LockArray[K_])
                        {
                            //s_K = (int)(K_ - halfK + halfIs);
                            s_K = (int)(halfIs + (K_ - halfK) * VolToSliceY);
                            lower_sJ = s_K;

                            #region Smear Slice
                            if (lower_sJ > 0 && lower_sJ < difference.GetLength(1)-1)
                            {
                                for (int J_ = 0; J_ < LJ; J_++)
                                {
                                    for (int I_ = 0; I_ < LI; I_++)
                                    {
                                        if (Weights[J_, I_] != -1)
                                        {
                                            lower_sI = Coord[J_, I_];
                                            u = Weights[J_, I_];

                                            weight = counts[lower_sI];
                                            df = difference[lower_sI + 1, lower_sJ, 0] * weight;
                                            df2 = difference[lower_sI, lower_sJ, 0] * weight;

                                            ErrorGrid[K_, J_, I_] += df2 * (u) + df * (1 - u);
                                            //AlphaWeighting[K_, J_, I_] += u * u + (1 - u) * (1 - u);
                                        }
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
            DensityGrid = mPassData.DensityGrid;
            mPassData.DensityGrid = DoProjections();
        }

        int ProjectionNumber = 0;
        public float[, ,] DoProjections()
        {

            int GridSize = Library[0].Width;

           int CutWidth = (int)(Library[0].Width * .95);
            int CutStart = (int)((Library[0].Width - CutWidth) / 2);
            int reducedSize = (int)(CutWidth / Math.Sqrt(2));
            // DensityGrid = new float[(int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor)];
            SetConstants(new Image<Gray, float>((int)(reducedSize), (int)(reducedSize)));
            /////////////////////////////////////because it needs to be fixed, just not right now


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


            VolToSliceX = mPassData.DataScaling;
            VolToSliceY = mPassData.DataScaling;
            VolToSliceZ = mPassData.DataScaling;

            /////////////////////////////////
            CleanProjections();


            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 2;

            int numberOfImages = Library.Count;

            //  return null;
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



            var b = DensityGrid.ShowCross();

            b[0] = Mask.MakeBitmap();

            ErrorGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
            PredictedVariance = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
            predictedGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

            ProjectionNumber++;
            float averageGrid = DensityGrid.AverageArray();


            for (int j = 0; j < DensityGrid.GetLength(1); j++)
                for (int k = 0; k < DensityGrid.GetLength(2); k++)
                    for (int n = 0; n < DensityGrid.GetLength(0); n++)
                    {
                        if (DensityGrid[n, j, k] < 0)
                            DensityGrid[n, j, k] = 0;
                        PredictedVariance[n, j, k] = noiseVar;
                    }

            Buffer.BlockCopy(DensityGrid, 0, predictedGrid, 0, Buffer.ByteLength(DensityGrid));

            int nIterations =8;

            double m = 0;
            int hx = DensityGrid.GetLength(1) / 2;
            for (int i = 0; i < DensityGrid.GetLength(0); i++)
                m += DensityGrid[i, hx, hx];


            double mP = Library[1].Data[Library[1].Height / 2, Library[1].Width / 2, 0];

            DensityGrid.DivideInPlace((float)(m / mP));
            predictedGrid.DivideInPlace((float)(m / mP));

            var CrossSections = ErrorGrid.ShowCrossHigh();
            // Program.ShowBitmaps(CrossSections);

            //CrossSections[0].Save(Program.dataFolder + "\\CrossSections_FBP_X.tif");
            //CrossSections[1].Save(Program.dataFolder + "\\CrossSections_FBP_Y.tif");
            //CrossSections[2].Save(Program.dataFolder + "\\CrossSections_FBP_Z.tif");

            // float m1 = DensityGrid.MaxArray();

            //float m3 = m1 + m2;


            float adjustment = 1f / numberOfImages / DensityGrid.GetLength(0);
            for (int j = 0; j < nIterations; j++)
            {
                batchStartIndex = j;
               // lambda = .6f;// .3f * (1 - j / (float)nIterations) + .4f;
                calmFactor = (float)(adjustment / Math.Sqrt(j + 1));// 3f / DensityGrid.GetLength(0);


                //     BatchStatProject(Library.Count / 8);

                Parallel.For(0, (int)(numberOfImages / 2 / (double)batchSkip), po, x => BatchStatProject(x));

                if (j == 0)
                {
                    float aveError = ErrorGrid.AverageArray();
                    averageGrid = DensityGrid.AverageArray();
                    float adjustmentC = Math.Abs(.5f * averageGrid / aveError);

                    ErrorGrid.MultiplyInPlace(adjustmentC);

                    adjustment *= adjustmentC;
                }

                //CrossSections = ErrorGrid.ShowCrossHigh();
                //// Program.ShowBitmaps(CrossSections);

                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Xe_OT" + j + ".tif", CrossSections[0]);
                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Ye_OT" + j + ".tif", CrossSections[1]);
                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Ze_OT" + j + ".tif", CrossSections[2]);


                Parallel.For(0, (int)DensityGrid.GetLength(0), po, x => BatchAddAndZero(x));

                //CrossSections = DensityGrid.ShowCrossHigh();
                //// Program.ShowBitmaps(CrossSections);

                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_X_" + j + ".tif", CrossSections[0]);
                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Y_" + j + ".tif", CrossSections[1]);
                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Z_" + j + ".tif", CrossSections[2]);

                //CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_OT" + j + ".tif");
                //CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_OT" + j + ".tif");
                //CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_OT" + j + ".tif");


                ProjectionNumber++;

            }


            return DensityGrid;
        }
        #endregion


    }
}


