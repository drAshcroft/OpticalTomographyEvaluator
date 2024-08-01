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
    public class RoughRecon : ReconTemplate
    {
        int nProjections;
        double reduction = .5;
        bool scaled = false;
      

        #region Properties

        #region Set

        public void setScaleData(bool scale, double reduction)
        {
            this.reduction = reduction;
            scaled = scale;
        }

        public void setNumberProjections(int nProjections)
        {
            this.nProjections = nProjections;
        }

        #endregion

        #region Get

        public float[, ,] getDensityGrid()
        {
            return DensityGrid;
        }
        #endregion

        #endregion

        #region Code

        int frameSkip;
        double frameReduction;

        private float[] Kernal;
        private ConvolutionKernelF cvKernal;

        #region ScaledRecon
        private void BatchBackProject(int imageNumber)
        {
            imageNumber *= frameSkip;
            Image<Gray, float> image = Library[imageNumber].Clone();
            Image<Gray, float> convolved = image.Resize(frameReduction, Emgu.CV.CvEnum.INTER.CV_INTER_NN).Convolution(cvKernal);
            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
            DoBackProjection_OneSlice(convolved, 1, 1, Angle, Axis2D.YAxis);
        }


        private float[, ,] DoScaledProjections()
        {
            frameSkip = (int)((double)Library.Count / 2d / nProjections);
            frameReduction = reduction;
            string FilterType = "Han";
            int FilterLength = 128;


            //set up the output grid and the locks that will be needed for the library system
            Image<Gray, float> exampleImage = Library[0].Clone();
            exampleImage = exampleImage.Resize(frameReduction, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
            int GridSize = exampleImage.Width;
            DensityGrid = new float[GridSize, GridSize, GridSize];

            SetConstants(exampleImage);

            frameReduction = reduction;

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

            Parallel.For(0, nProjections, Program.threadingParallelOptions, x => BatchBackProject(x));

            return DensityGrid;
        }

        #endregion


        #region Unscaled


        private float[, ,] DoProjections()
        {
            frameSkip = (int)((double)Library.Count / 2d / nProjections);

            string FilterType = "Han";
            int FilterLength = 128;


            //set up the output grid and the locks that will be needed for the library system
            int GridSize = Library[0].Width;
            DensityGrid = new float[GridSize, GridSize, GridSize];

            SetConstants(Library[0]);

            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            int numberOfImages = Library.Count;


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

            Parallel.For(0, nProjections, Program.threadingParallelOptions, x => BatchBackProjectUnScaled(x));

            ImageProcessing._3D.ScrubVolume.RemoveSphereToZero(ref DensityGrid);// DensityGrid.ScrubReconVolume(Library.Count);

            NormalizeFBP();

            //ImageProcessing.ImageFileLoader.Save_Tiff_Stack(@"C:\temp\New folder\testvol.tif", DensityGrid);

            return DensityGrid;
        }

        private void BatchBackProjectUnScaled(int imageNumber)
        {

            imageNumber *= frameSkip;
            Image<Gray, float> convolved = Library[imageNumber].Convolution(cvKernal);
            // convolved= convolved.Rotate(90, new Gray(0));

            double Angle = 2 * Math.PI * (double)imageNumber / Library.Count;
            DoBackProjection_OneSlice(convolved, 1, 1, Angle, Axis2D.YAxis);
        }



        #endregion

        #region FBP

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

            FBPSmearArray2DWhole(AngleRadians, Slice, PaintingWidth, PaintingHeight, vec, Point3D.CrossProduct(vec, vRotationAxis));
        }

        private void FBPSmearArray2DWhole(double AngleRadians, Image<Gray, float> Slice, double PaintingWidth, double PaintingHeight, Point3D Direction, Point3D FastScanDirection)
        {
            float[, ,] PaintingArray = Slice.Data;

            //make sure all the vectors are the right size
            Direction.Normalize();
            FastScanDirection.Normalize();
            //determine all the important planes in the 3D space and all the step
            Point3D SlowScanAxis = Point3D.CrossProduct(Direction, FastScanDirection);

            ///get the existing recon data.  the jagged array is used so the data can be checked out by each thread

            double sX, sY, sdotI;
            float u;
            int lower_sI, lower_sJ;
            int K_ = 0;
            unsafe
            {
                //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
                unchecked
                {

                    #region Precalc Indexs

                    float[,] Weights = new float[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
                    int[,] Coord = new int[DensityGrid.GetLength(1), DensityGrid.GetLength(2)];

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

                                sdotI = halfIs + (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                                //make sure that we are still in the recon volumn
                                if (sdotI > 0 && sdotI < LsI_1)
                                {
                                    lower_sI = (int)Math.Floor(sdotI);
                                    u = (float)(sdotI - lower_sI);
                                    Weights[J_, I_] = u;
                                    Coord[J_, I_] = lower_sI;
                                }
                                else
                                    Weights[J_, I_] = -1;
                            }
                            else
                                Weights[J_, I_] = -1;
                        }
                    }
                    #endregion

                    //  Weights.MakeBitmap().Save(@"c:\temp\weights.bmp");
                    #region Find open slice
                    //there is a lot of thread contention, so look through the whole stack and find an open slice, and work on it

                    int StartI = 0;// (new Random(DateTime.Now.Millisecond)).Next(LK);
                    float df, df2;
                    for (int i = 0; i < LK; i++)
                    {
                        K_ = (i + StartI) % LK;
                        LockIndicator[K_] = true;


                    #endregion


                        //indicate that the thread is locked
                        lock (LockArray[K_])
                        {
                            lower_sJ = K_;// (int)((halfK - K_) * VolToSliceZ + halfJs);
                            #region Process slice

                            for (int J_ = 0; J_ < LJ; J_++)
                            {
                                for (int I_ = 0; I_ < LI; I_++)
                                {

                                    if (Weights[J_, I_] != -1)
                                    {
                                        lower_sI = Coord[J_, I_];
                                        u = Weights[J_, I_];
                                        df = PaintingArray[lower_sI + 1, lower_sJ, 0];
                                        df2 = PaintingArray[lower_sI, lower_sJ, 0];

                                        DensityGrid[K_, J_, I_] += df2 * (1 - u) + df * (u);
                                    }
                                }
                            }


                            #endregion

                        }

                        //release the programatic handle to 
                        LockIndicator[K_] = false;
                    }
                }
            }
        }


        private void NormalizeFBP()
        {
            int halfK = DensityGrid.GetLength(0) / 2;
            int halfJ = DensityGrid.GetLength(1) / 2;

            double sum = 0;
            double cc = 0;
            for (int i = 0; i < DensityGrid.GetLength(2); i++)
            {
                var d = DensityGrid[halfK, halfJ, i] + DensityGrid[halfK - 20, halfK, i] + DensityGrid[halfK + 20, halfK, i];
                if (d != 0)
                {
                    sum += d;
                    cc++;
                }
            }

            var ave = sum / 3;

            float val = Library[0].Data[halfK, halfJ, 0];

            float normFactor = (float)(val / ave);

            for (int i = 0; i < DensityGrid.GetLength(0); i++)
                for (int j = 0; j < DensityGrid.GetLength(1); j++)
                    for (int k = 0; k < DensityGrid.GetLength(2); k++)
                    {
                        DensityGrid[i, j, k] *= normFactor;
                        if (DensityGrid[i, j, k] < 0)
                            DensityGrid[i, j, k] = 0;
                    }

        }

        #endregion


        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            if (scaled)
                DensityGrid = DoScaledProjections();
            else
                DensityGrid = DoProjections();
        }
    }
}
