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
    public class OptRecon : ReconTemplate
    {
        public enum ReconTypes
        {
            FBP, SIRT, SART, TIKH, ForwardOnly, Old
        }

        protected ReconTypes ReconType = ReconTypes.FBP;

        protected float[] Kernal;
        protected ConvolutionKernelF cvKernal;
        protected string FilterType = "Han";
        protected int FilterLength = 512;


        protected float[, ,] SecondGrid;
        protected float[, ,] ErrorGrid;

        protected float lambda = .001f;
        protected bool Realign = false;
        protected bool mCleanProjections = false;
        protected int m_nIterations = 1;

        protected int batchStartIndex = 0;
        protected int batchSkip = 1;

        protected float alpha = .4f;
        protected static Random rnd = new Random();

        protected static ReconTypes lastReconType;
        protected static Image<Gray, float>[] Weights;
        protected static Image<Gray, int>[] Coords;
        protected static float[][] WeightSums;

        protected static OnDemandImageLibrary ForwardProjections;
        protected static OnDemandImageLibrary DiffProjections;

        protected int CutWidth, CutStart;

        #region Properties

        #region Set
        public void setReconType(ReconTypes reconType)
        {
            ReconType = reconType;
        }

        public void setFilter(string filterName, int filterLength)
        {
            FilterType = filterName;
            FilterLength = filterLength;
        }

        public void setRealign(bool realign)
        {
            Realign = realign;
        }

        public void setCleanProjections(bool clean)
        {
            mCleanProjections = clean;
        }

        public void setNumIterations(int nIterations)
        {
            m_nIterations = nIterations;
        }

        public void setSkipProjections(int nSkips)
        {
            batchSkip = nSkips;
        }
        #endregion

        #region Get

        public OnDemandImageLibrary getForwardProjections()
        {
            return ForwardProjections;
        }
        #endregion

        #endregion

        #region Code

        #region Parallel Math
        protected virtual void BatchAddAndZeroTIKH(int sliceNumber)
        {
            float error, d;
            float theta1;
            //double dU, testvalue, stepSize = alpha, d1, d2;
            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    if (x > 2 && x < ErrorGrid.GetLength(1) - 2 && y > 2 && y < ErrorGrid.GetLength(2) - 2 && sliceNumber > 2 && sliceNumber < ErrorGrid.GetLength(0) - 2)
                    {
                        {
                            error = ErrorGrid[sliceNumber, x, y];// *errorWeight;
                            //lambda was applied in the difference routine (cheaper there)
                            if (error > .05)
                            {
                                d = DensityGrid[sliceNumber, x, y] + error;

                                if (d > 0)
                                {
                                    theta1 = DensityGrid[sliceNumber, x + 1, y] + DensityGrid[sliceNumber, x - 1, y] + DensityGrid[sliceNumber, x, y + 1] + DensityGrid[sliceNumber, x, y - 1] + DensityGrid[sliceNumber + 1, x, y] + DensityGrid[sliceNumber - 1, x, y];
                                    theta1 = theta1 / 6;
                                    d = d * (1 - alpha) + theta1 * alpha;
                                    SecondGrid[sliceNumber, x, y] = d;
                                }
                                else
                                    SecondGrid[sliceNumber, x, y] = .0001f;// DensityGrid[sliceNumber, x, y];
                            }
                            else
                                SecondGrid[sliceNumber, x, y] = DensityGrid[sliceNumber, x, y];
                        }
                    }
                    else
                        SecondGrid[sliceNumber, x, y] = 0;

                    ErrorGrid[sliceNumber, x, y] = 0;
                }
        }

        protected virtual void BatchAddAndZeroOLD(int sliceNumber)
        {
            float error, d;
            float theta1;
            //double dU, testvalue, stepSize = alpha, d1, d2;
            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    if (x > 2 && x < ErrorGrid.GetLength(1) - 2 && y > 2 && y < ErrorGrid.GetLength(2) - 2 && sliceNumber > 2 && sliceNumber < ErrorGrid.GetLength(0) - 2)
                    {
                        {
                            error = ErrorGrid[sliceNumber, x, y];// *errorWeight;
                            //lambda was applied in the difference routine (cheaper there)
                            d = DensityGrid[sliceNumber, x, y] + error;

                            if (d > 0)
                            {
                                theta1 = DensityGrid[sliceNumber, x + 1, y] + DensityGrid[sliceNumber, x - 1, y] + DensityGrid[sliceNumber, x, y + 1] + DensityGrid[sliceNumber, x, y - 1] + DensityGrid[sliceNumber + 1, x, y] + DensityGrid[sliceNumber - 1, x, y];
                                theta1 = theta1 / 6;
                                d = d * (1 - alpha) + theta1 * alpha;
                                SecondGrid[sliceNumber, x, y] = d;
                            }
                            else
                                SecondGrid[sliceNumber, x, y] = DensityGrid[sliceNumber, x, y];
                        }
                    }
                    else
                        SecondGrid[sliceNumber, x, y] = 0;

                    ErrorGrid[sliceNumber, x, y] = 0;
                }
        }


        protected virtual void BatchAddAndZeroSIRT(int sliceNumber)
        {
            float error, d;
            //float theta1;
            //double dU, testvalue, stepSize = alpha, d1, d2;
            for (int x = 0; x < DensityGrid.GetLength(1); x++)
                for (int y = 0; y < DensityGrid.GetLength(2); y++)
                {
                    if (x > 2 && x < ErrorGrid.GetLength(1) - 2 && y > 2 && y < ErrorGrid.GetLength(2) - 2 && sliceNumber > 2 && sliceNumber < ErrorGrid.GetLength(0) - 2)
                    {
                        {
                            error = ErrorGrid[sliceNumber, x, y];// *errorWeight;
                            //lambda was applied in the difference routine (cheaper there)
                            d = DensityGrid[sliceNumber, x, y] + error;

                            if (d > 0)
                            {
                                // theta1 = DensityGrid[sliceNumber, x + 1, y] + DensityGrid[sliceNumber, x - 1, y] + DensityGrid[sliceNumber, x, y + 1] + DensityGrid[sliceNumber, x, y - 1] + DensityGrid[sliceNumber + 1, x, y] + DensityGrid[sliceNumber - 1, x, y];
                                // theta1 = theta1 / 6;
                                // d = d * (1 - alpha)  + theta1 * alpha;
                                SecondGrid[sliceNumber, x, y] = d;
                            }
                            else
                                SecondGrid[sliceNumber, x, y] = DensityGrid[sliceNumber, x, y];
                        }
                    }
                    else
                        SecondGrid[sliceNumber, x, y] = 0;

                    ErrorGrid[sliceNumber, x, y] = 0;
                }
        }

        #endregion


        #region Initialize

        protected void BatchWeightCalc(int imageNumber)
        {
            Image<Gray, float> image = Library[imageNumber];

            double AngleRadians = 2 * Math.PI * (double)imageNumber / Library.Count;
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
            //calculate rotated weights for each angle of PP with respect to the Z axis
            Point3D vec = Point3D.RotateAroundAxis(axis, vRotationAxis, AngleRadians);

            Point3D Direction = vec;
            Point3D FastScanDirection = Point3D.CrossProduct(vec, vRotationAxis);
            //make sure all the vectors are the right size
            Direction.Normalize();
            FastScanDirection.Normalize();
            //determine all the important planes in the 3D space and all the step
            Point3D SlowScanAxis = Point3D.CrossProduct(Direction, FastScanDirection);

            double sY, sX, sdotI;
            int lower_sI;
            float u;

            Image<Gray, float> weights = new Image<Gray, float>(DensityGrid.GetLength(2), DensityGrid.GetLength(1));
            Image<Gray, int> coords = new Image<Gray, int>(DensityGrid.GetLength(2), DensityGrid.GetLength(1));

            float[, ,] weightData = weights.Data;
            int[, ,] coordData = coords.Data;
            float[] counts = new float[image.Width];

            int volSliceLength = DensityGrid.GetLength(1) * DensityGrid.GetLength(2);



            for (int J_ = 0; J_ < LJ; J_++)
            {
                //tranform to slice index coords
                sY = (J_ - halfJ) * VolToSliceY;
                for (int I_ = 0; I_ < LI; I_++)
                {
                    //tranform to slice index coords
                    sX = (I_ - halfI) * VolToSliceX;
                    sdotI = halfIs + (sX * FastScanDirection.X + sY * FastScanDirection.Y);
                    //make sure that we are still in the recon volumn
                    if (sdotI > 0 && sdotI < LsI_1 - 2)
                    {
                        lower_sI = (int)Math.Floor(sdotI);
                        u = (float)(sdotI - lower_sI);
                        weightData[J_, I_, 0] = u;
                        coordData[J_, I_, 0] = lower_sI;

                        counts[lower_sI] += (1 - u);
                        counts[lower_sI + 1] += (u);
                    }
                    else
                        weightData[J_, I_, 0] = -1;
                }
            }


            Weights[imageNumber] = weights;
            Coords[imageNumber] = coords;
            WeightSums[imageNumber] = counts;
        }

        protected void Initialize(bool IntializeWeights)
        {
            #region Setup Bounds and Constants

            int GridSize = Library[0].Width;
            float SizeFactor = mPassData.DataScaling;

            int firstWidth = (int)(Library[0].Width / Math.Sqrt(2));

            CutWidth = (int)(Library[0].Width - 5);
            CutStart = (int)((Library[0].Width - CutWidth) / 2);
            int reducedSize = (int)(CutWidth / Math.Sqrt(2));

            if (DensityGrid == null || ReconType == ReconTypes.FBP)
                DensityGrid = new float[(int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor), (int)(reducedSize / SizeFactor)];

            if (ReconType == ReconTypes.FBP)
                SetConstants(new Image<Gray, float>((int)(CutWidth), (int)(CutWidth)), 1);
            else
            {
                SetConstants(Library[1], 1);
                CutWidth = Library[1].Width;
            }

            #endregion

            #region Precalculate all the weighting and projection

            if (Weights == null || Weights[0].Width != DensityGrid.GetLength(2) || lastReconType != ReconType)
            {
                if (IntializeWeights)
                {

                    lastReconType = ReconType;

                    Weights = new Image<Gray, float>[Library.Count];
                    Coords = new Image<Gray, int>[Library.Count];
                    WeightSums = new float[Library.Count][];

                    //  ParallelOptions po = new ParallelOptions();
                    //  //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

                    // BatchWeightCalc(125)

                    Parallel.For(0, Library.Count, Program.threadingParallelOptions, x => BatchWeightCalc(x));
                   // for (int i = 0; i <= Library.Count-1; i++)
                   // { BatchWeightCalc(i); }
                }
            }

            #endregion


            //Since it is an oddbad, it gets it's own spot

            if (ReconType == ReconTypes.FBP)
            {
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
            }
            //else
            //{
            //    OnDemandImageLibrary lib2 = new OnDemandImageLibrary(Library.Count, true);
            //    for (int i = 0; i < Library.Count; i++)
            //    {
            //        var image = Library[i];
            //        image.ROI = new Rectangle(CutStart, CutStart, CutWidth, CutWidth);
            //        image = image.Copy();
            //        lib2[i] = image;

            //    }
            //    Library = lib2;
            //}

            ForwardProjections = new OnDemandImageLibrary(Library.Count, true);
            DiffProjections = new OnDemandImageLibrary(Library.Count, true);

            for (int i = 0; i < Library.Count; i++)
            {
                ForwardProjections[i] = new Image<Gray, float>(CutWidth, CutWidth);
                DiffProjections[i] = new Image<Gray, float>(CutWidth, CutWidth);
            }
        }

        #endregion

        protected Image<Gray, float> Convolve(Image<Gray, float> image)
        {
            image = image.Rotate(90, new Gray(0));
            var image2 = new Image<Gray, float>(ImageProcessing._2D.Convolution.Convolve(image.Data, Kernal));



            return image2.Rotate(-90, new Gray(0));
        }


        protected void BatchProject(int imageNumber)
        {
            imageNumber = (batchStartIndex + imageNumber * batchSkip) % Library.Count;

            Image<Gray, float> Slice2 = Library[imageNumber];
            Image<Gray, float> forwardProjection = ForwardProjections[imageNumber];
            Image<Gray, float> diffImage = DiffProjections[imageNumber];

            float[, ,] forwardData = forwardProjection.Data;
            float[, ,] differenceData = diffImage.Data;

            if (ReconType == ReconTypes.FBP)
            {
                // ImageProcessing.ImageFileLoader.Save_Bitmap(@"C:\temp\fbp1\image" + string.Format("{0:000}.png", imageNumber), Slice2);
                Image<Gray, float> convolved = Convolve(Slice2);

                //  ImageProcessing.ImageFileLoader.Save_Bitmap(@"C:\temp\fbp\image" + string.Format("{0:000}.png",imageNumber), convolved);

                convolved.ROI = new Rectangle(CutStart, CutStart, CutWidth, CutWidth);
                Slice2 = convolved.Copy().Mul(mPassData.Weights[imageNumber]);
                differenceData = Slice2.Data;
            }


            float[, ,] PaintingArray = Slice2.Data;

            int K_ = 0;
            float val;
            float u;
            int lower_sI;

            int volSliceLength = DensityGrid.GetLength(1) * DensityGrid.GetLength(2);

            unsafe
            {
                fixed (float* pf_Weight = Weights[imageNumber].Data, pf_Diff = differenceData, pf_forward = forwardData, pf_Painting = PaintingArray, pf_DensityGrid = DensityGrid, pf_ErrorGrid = ErrorGrid)
                {
                    fixed (int* pf_Coords = Coords[imageNumber].Data)
                    {
                        //we make the math unchecked to make it go faster.  there seem to be enough checks beforehand
                        //unchecked
                        {
                            float* pForward = pf_forward, pDiff = pf_Diff;
                            int s_K;

                            float* pWeights = pf_Weight;
                            int* pCoords = pf_Coords;

                            if (ReconType != ReconTypes.FBP)
                            {
                                for (int i = 0; i < differenceData.LongLength; i++)
                                {
                                    *pForward = 0;
                                    *pDiff = 0;
                                    pForward++;
                                    pDiff++;
                                }

                                #region UnSmear slice

                                for (K_ = 0; K_ < LK; K_++)
                                {
                                    float* pDensity = pf_DensityGrid + volSliceLength * K_;
                                    pWeights = pf_Weight;
                                    pCoords = pf_Coords;
                                    s_K = (int)(halfIs + (K_ - halfK));
                                    for (int sliceI = 0; sliceI < volSliceLength; sliceI++)
                                    {
                                        if (*pWeights != -1)
                                        {
                                            lower_sI = *pCoords;
                                            val = *pDensity;
                                            u = *pWeights;

                                            forwardData[lower_sI, s_K, 0] += val * (1 - u);
                                            forwardData[lower_sI + 1, s_K, 0] += val * u;
                                        }
                                        pCoords++;
                                        pDensity++;
                                        pWeights++;
                                    }
                                }

                                #endregion

                                #region DoDifference

                                if (ReconType != ReconTypes.ForwardOnly)
                                {
                                    pDiff = pf_Diff;
                                    pForward = pf_forward;
                                    float* pPaint = pf_Painting;

                                    //apply
                                    for (int pixelI = 0; pixelI < PaintingArray.LongLength; pixelI++)
                                    {
                                        *pDiff = lambda * (*pPaint - *pForward);

                                        pDiff++;
                                        pForward++;
                                        pPaint++;
                                    }
                                }
                                #endregion
                            }


                            #region Resmear

                            if (ReconType != ReconTypes.ForwardOnly)
                            {
                                float df1, df0;
                                int StartI = rnd.Next(LK);

                                for (int i = 0; i < LK; i++)
                                {
                                    //start on some plane and then work around the loop.  This spaces out the various slices a little at the beginning
                                    K_ = (i + StartI) % LK;
                                    LockIndicator[K_] = true;

                                    //indicate that the thread is locked
                                    lock (LockArray[K_])
                                    {
                                        s_K = (int)(K_ - halfK + halfIs);
                                        float* pError = pf_ErrorGrid + volSliceLength * K_;
                                        pWeights = pf_Weight;
                                        pCoords = pf_Coords;

                                        for (int sliceI = 0; sliceI < volSliceLength; sliceI++)
                                        {
                                            if (*pWeights != -1)
                                            {
                                                lower_sI = *pCoords;
                                                u = *pWeights;

                                                df0 = differenceData[lower_sI, s_K, 0];
                                                df1 = differenceData[lower_sI + 1, s_K, 0];

                                                *pError += (df0 * (1 - u) + df1 * (u));
                                            }
                                            pCoords++;
                                            pError++;
                                            pWeights++;
                                        }
                                    }
                                    LockIndicator[K_] = false;
                                }
                            }
                        }
                            #endregion
                    }
                }
            }

        }


        protected virtual float[, ,] DoProjections()
        {
            float alpha0 = .4f;
            alpha = alpha0;// *NoiseGuess() / 500;

            Initialize(true);

            if (mCleanProjections)
            {
                //CleanProjections();

                for (int j = 0; j < DensityGrid.GetLength(1); j++)
                    for (int k = 0; k < DensityGrid.GetLength(2); k++)
                        for (int n = 0; n < DensityGrid.GetLength(0); n++)
                            if (DensityGrid[n, j, k] < 0)
                                DensityGrid[n, j, k] = 0.001f;

                double m = 0;
                int hx = DensityGrid.GetLength(1) / 2;
                for (int i = 0; i < DensityGrid.GetLength(0); i++)
                    m += DensityGrid[i, hx, hx];

                double mP = Library[1].Data[Library[1].Height / 2, Library[1].Width / 2, 0];

                DensityGrid.DivideInPlace((float)(.9 * m / mP));
            }

            if (ReconType == ReconTypes.FBP)
            {
                ErrorGrid = DensityGrid;
            }
            else
            {
                ErrorGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
                SecondGrid = new float[DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2)];
            }

            // ParallelOptions po = new ParallelOptions();
            // //po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            int numberOfImages = Library.Count;

            for (int j = 0; j < m_nIterations; j++)
            {
                batchStartIndex = j;

                lambda = (float)(1.3f / Math.Sqrt(j + 1));// 3f / DensityGrid.GetLength(0);
                lambda = lambda / numberOfImages / DensityGrid.GetLength(0) * 2;


                Parallel.For(0, numberOfImages / batchSkip, Program.threadingParallelOptions, x => BatchProject(x));

                double m = ErrorGrid.MaxArray();
                double m2 = ErrorGrid.MinArray();
                double m3 = m2 + m;
                var CrossSections = ErrorGrid.ShowCross();
                Program.ShowBitmaps(CrossSections);

                if (ReconType != ReconTypes.FBP && ReconType != ReconTypes.ForwardOnly)
                {
                    if (ReconType == ReconTypes.Old)
                        Parallel.For(0, (int)DensityGrid.GetLength(0), Program.threadingParallelOptions, x => BatchAddAndZeroOLD(x));

                    if (ReconType == ReconTypes.SIRT)
                        Parallel.For(0, (int)DensityGrid.GetLength(0), Program.threadingParallelOptions, x => BatchAddAndZeroSIRT(x));

                    if (ReconType == ReconTypes.TIKH)
                    {
                        //alpha = (float)Math.Pow(alpha0, j / 4.0 + 1);
                        
                        if (j == m_nIterations - 1)
                            alpha = .8f;
                        else
                            alpha = .5f;
                        Parallel.For(0, (int)DensityGrid.GetLength(0), Program.threadingParallelOptions, x => BatchAddAndZeroTIKH(x));
                    }

                    CrossSections = SecondGrid.ShowCross();
                    Program.ShowBitmaps(CrossSections);

                    float[, ,] tempGrid = DensityGrid;
                    DensityGrid = SecondGrid;
                    SecondGrid = tempGrid;
                }
            }

            return DensityGrid;
        }
        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            DensityGrid = mPassData.DensityGrid;
            mPassData.DensityGrid = DoProjections();
        }


        #region Diagnostics

        protected float NoiseGuess()
        {
            float minVariation = float.MaxValue;
            for (int i = 0; i < Library.Count; i += 100)
            {
                var image = Library[i];
                float sideTop = 0;
                float sideBottom = 0;
                int Top = image.Height - 1;
                for (int x = 0; x < image.Width; x++)
                {
                    sideTop += Math.Abs(image.Data[0, x, 0] - image.Data[1, x, 0]);
                    sideBottom += Math.Abs(image.Data[Top, x, 0] - image.Data[Top - 1, x, 0]);
                }
                sideTop /= image.Width;
                sideBottom /= image.Width;

                if (sideTop > 0 && sideTop < minVariation)
                    minVariation = sideTop;

                if (sideBottom > 0 && sideBottom < minVariation)
                    minVariation = sideTop;
            }
            return minVariation;
        }

        #endregion

    }
}


