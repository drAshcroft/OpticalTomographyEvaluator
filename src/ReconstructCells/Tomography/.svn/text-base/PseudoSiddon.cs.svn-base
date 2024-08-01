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
    public class PseudoSiddon : ReconTemplate
    {
        private float[] Kernal;
        private ConvolutionKernelF cvKernal;
        private string FilterType = "Han";
        private int FilterLength = 512;
        private bool Halfsies = false; 
        int CutWidth;
        int CutStart;

        int SkipProjections = 1;
        #region Properties

        #region Set
        public void setFilter(string filterName, int filterLength)
        {
            FilterType = filterName;
            FilterLength = filterLength;
        }
        public void setHalfProjections(bool halfProjectionsON)
        {
            Halfsies = halfProjectionsON;
        }

        public void setSkipProjections(int nSkipped)
        {
            SkipProjections = nSkipped;
        }
        #endregion

        #region Get


        #endregion

        #endregion

        #region Code

        private Image<Gray, float> Convolve(Image<Gray, float> image)
        {
           // if (mPassData.FluorImage ==false )
                image = image.Rotate(90, new Gray(0));

            var image2 = new Image<Gray, float>(ImageProcessing._2D.Convolution.Convolve(image.Data, Kernal));

            return image2.Rotate(-90, new Gray(0));
        }

        private void BatchBackProject(int imageNumber)
        {
            imageNumber *= SkipProjections;
            Image<Gray, float> convolved = Convolve(Library[imageNumber]);//.Convolution(cvKernal);
            // convolved= convolved.Rotate(90, new Gray(0));
            convolved.ROI = new Rectangle(CutStart, CutStart, CutWidth, CutWidth);
            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;

            convolved = convolved.Copy().Mul(mPassData.Weights[imageNumber]);

            DoBackProjection_OneSlice(convolved, 1, 1, Angle, Axis2D.YAxis);
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
        private void DoBackProjection_OneSlice(Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, double AngleRadians, Axis2D ConvolutionAxis)
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
            //  double VolToSliceX, VolToSliceY, VolToSliceZ;

            halfI = (double)LI / 2;
            halfJ = (double)LJ / 2;
            halfK = (double)LK / 2;

            halfIs = (double)LsI / 2;
            halfJs = (double)LsJ / 2;

            //VolToSliceX = (xMax - xMin) / LI * (double)Slice.Height / (PaintingWidth);
            //VolToSliceY = (yMax - yMin) / LJ * (double)Slice.Height / (PaintingWidth);
            //VolToSliceZ = (zMax - zMin) / LK * (double)Slice.Width / (PaintingHeight);

            int K_ = 0;
            int FinishedCount = 0;
            #endregion

            double sX, sY, sdotI;
            float u;
            int lower_sI, lower_sJ;
            bool SliceFound = false;
            int LUT_Index;
            unsafe
            {
                //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
                unchecked
                {
                    fixed (float* mipDataWhole = DensityGrid)
                    {
                        //for (int zI = 0; zI < LZ; zI++)
                        // while (FinishedCount < LK)
                        {
                            #region Find open slice
                            //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it
                            SliceFound = false;
                            int StartI = (new Random(DateTime.Now.Millisecond)).Next(LK);
                            for (int i = 0; i < LK; i++)
                            {
                                //lock (FindLock)
                                {
                                    //  if (DensityGrid.LockIndicator[i] == false)
                                    {
                                        K_ = (i + StartI) % LK;
                                        LockIndicator[K_] = true;
                                        FinishedCount++;
                                        SliceFound = true;
                                        //  break;
                                    }
                                }

                            #endregion

                                if (SliceFound == true)
                                {
                                    //indicate that the thread is locked
                                    lock (LockArray[K_])
                                    {
                                        #region Process slice
                                        float* mipData = mipDataWhole + K_ * LI * LJ; //mDataDouble[K_]
                                        {
                                          
                                            for (int J_ = 0; J_ < LJ; J_++)
                                            {
                                                //tranform to slice index coords
                                                sY = (J_ - halfI) * VolToSliceY;
                                                for (int I_ = 0; I_ < LI; I_++)
                                                {

                                                    //tranform to slice index coords
                                                    sX = (I_ - halfI) * VolToSliceX;

                                                    sdotI = sX * FastScanDirection.X + sY * FastScanDirection.Y + halfIs;
                                                    //make sure that we are still in the recon volumn
                                                    if (sdotI > 0 && sdotI < LsI_1)
                                                        if (K_ > 0 && K_ < LsJ_1)
                                                        {
                                                          //  POut = (double*)mipData + I_ * LI + J_;
                                                            lower_sI = (int)Math.Floor(sdotI);
                                                            u = (float)sdotI - lower_sI;
                                                            lower_sJ = (int)((K_ - halfK) * VolToSliceZ + halfJs);

                                                            LUT_Index = (int)(u / 1.2);
                                                            if (LUT_Index > 49)
                                                                LUT_Index = 49;


                                                            DensityGrid[K_, J_, I_] += PaintingArray[lower_sI, lower_sJ, 0] * (1 - u)
                                                                                   + PaintingArray[lower_sI + 1, lower_sJ, 0] * u;
                                                        }
                                                }
                                            }
                                        }

                                        #endregion

                                    }
                                }
                                //release the programatic handle to 
                                LockIndicator[K_] = false;
                            }
                        }
                    }
                }
                //SmearArray2DQueue = null;
            }



        }

        private float[, ,] DoProjections()
        {
            for (int i = 0; i < Library.Count; i++)
            {
                //    Library[i] = Library[i].Resize(2, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            }

            float SizeFactor = mPassData.DataScaling;

            int firstWidth = (int)(Library[0].Width / Math.Sqrt(2));

            CutWidth = (int)(Library[0].Width -5);
            CutStart = (int)((Library[0].Width - CutWidth) / 2);
            int reducedSize = (int)(CutWidth / Math.Sqrt(2));
            DensityGrid = new float[(int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor)];
            SetConstants(new Image<Gray, float>((int)(reducedSize), (int)(reducedSize)));

            //now create the kernal
            double[] kernal = MathLibrary.Signal_Processing.ConvolutionFilters.GetRealSpaceFilter(FilterType, FilterLength, FilterLength, (double)FilterLength / 2d);
            //format for this system, or for the openCV method
            Kernal = new float[kernal.Length];
            float[,] fKernal = new float[kernal.Length, 1];
            for (int i = 0; i < kernal.Length; i++)
            {
                Kernal[i] = (float)kernal[i];
                fKernal[i, 0] = Kernal[i];
            }
            cvKernal = new ConvolutionKernelF(fKernal);

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count/SkipProjections;
            if (Halfsies)
                numberOfImages /= 2;

          //  BatchBackProject(125);
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchBackProject(x));

            return DensityGrid;
        }

        private float[, ,] DoHalfProjections()
        {
            int GridSize = Library[0].Width;

            //set up the output grid and the locks that will be needed for the library system
            DensityGrid = new float[GridSize, GridSize, GridSize];
            LockArray = new object[GridSize];
            LockIndicator = new bool[GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                LockArray[i] = new object();
            }

            //now create the kernal
            double[] kernal = MathLibrary.Signal_Processing.ConvolutionFilters.GetRealSpaceFilter(FilterType, FilterLength, FilterLength, (double)FilterLength / 2d);
            //format for this system, or for the openCV method
            Kernal = new float[kernal.Length];
            float[,] fKernal = new float[kernal.Length, 1];
            for (int i = 0; i < kernal.Length; i++)
            {
                Kernal[i] = (float)kernal[i];
                fKernal[i, 0] = Kernal[i];
            }
            cvKernal = new ConvolutionKernelF(fKernal);

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count / 2;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchBackProject(x));

            return DensityGrid;
        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            mPassData.DensityGrid = DoProjections();
        }
    }
}
