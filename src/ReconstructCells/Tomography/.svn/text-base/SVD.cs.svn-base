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
using System.IO;

namespace ReconstructCells.Tomography
{
    public class SVDRecon : ReconTemplate
    {
        #region Properties

        #region Set

        #endregion

        #region Get


        #endregion

        #endregion

        #region Code



        const double P = 1.1;
     
        const double sigma = .1;
        double factor = 1 / P / Math.Pow(sigma, P) / 26;
        const double alpha = 100;
        int numberImages = 250;
        int GridSize = 250;
        float[][][, ,] Rows;

        //  int[,] Mask;
        Random rnd = new Random();

        #region Statistical


        private void BatchStatProject(int imageNumber)
        {
         
            double Angle = 2 * Math.PI * (double)imageNumber / numberImages;
            DoSIRTProjection_OneSlice(imageNumber, new Image<Gray, float>(GridSize, GridSize), Angle, Axis2D.YAxis);
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
        private void DoSIRTProjection_OneSlice(int imageNumber, Image<Gray, float> Slice, double AngleRadians, Axis2D ConvolutionAxis)
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

            SmearArray2DWhole(imageNumber, AngleRadians, Slice, vec, Point3D.CrossProduct(vec, vRotationAxis));
        }

        private void SmearArray2DWhole(int imageNumber, double AngleRadians, Image<Gray, float> Slice, Point3D Direction, Point3D FastScanDirection)
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

            
            double sX, sY, sdotI;
           
            float u;
            int lower_sI;


            int Width = DensityGrid.GetLength(1);
            float[][, ,] rWeights = Rows[imageNumber];
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
                        rWeights[lower_sI][I_, J_, 0] = 1 - u;
                        rWeights[lower_sI + 1][I_, J_, 0] = u;
                    }
                }
            }
        }
        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            DoProjections(mPassData.DensityGrid);
        }

       
        public float[, ,] DoProjections(float[, ,] densityGrid)
        {


            DensityGrid = new float[GridSize, GridSize, GridSize];
            SetConstants(new Image<Gray, float>(GridSize, GridSize));


            long size = GridSize * GridSize;
            Rows = new float[numberImages][][,,];
            for (int i = 0; i < numberImages; i++)
            {
                Rows[i] = new float[GridSize][, ,];
                for (int j = 0; j < GridSize; j++)
                {
                    Rows[i][j] = new float[GridSize, GridSize, 1];
                }
            }

            // ParallelOptions Program.threadingParallelOptions = new ParallelOptions();
            //Program.threadingParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

            Parallel.For(0, numberImages, Program.threadingParallelOptions, x => BatchStatProject(x));

            Library = new OnDemandImageLibrary(250, true, @"c:\temp", false);
            for (int i = 0; i < 250; i++)
                Library[i] = new Image<Gray, float>(Rows[25][i]);

            var test = Library[125];
            GridSize = test.Width;

            long rows = 0;
            byte[] bBuffer = new byte[Buffer.ByteLength(test.Data)];
            using (BinaryWriter b = new BinaryWriter(File.Create(@"c:\temp\A.bin")))
            {
                for (int i = 0; i < Rows.Length; i++)
                    for (int j = 0; j < Rows[i].Length; j++)
                    {
                        Buffer.BlockCopy(Rows[i][j], 0, bBuffer, 0, Buffer.ByteLength(test.Data));
                        b.Write(bBuffer);
                        rows++;
                    }
            }



            return DensityGrid;
        }
        #endregion

        private float[, ,] samp2(float[, ,] x, int sy)
        {

            float[, ,] arrayOutf = new float[sy, sy, 1];
            int startI = (int)(Math.Floor((x.GetLength(0) - sy) / 2d));

            if (startI >= 0)
            {
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[i, j, 0] = (float)(x[i + startI, j + startI, 0]);
                    }
                }

                return arrayOutf;
            }
            else
            {
                for (int i = 0; i < x.GetLength(0); i++)
                {
                    for (int j = 0; j < x.GetLength(1); j++)
                    {
                        arrayOutf[i - startI, j - startI, 0] = (float)(x[i, j, 0]);
                    }
                }

                return arrayOutf;

            }

        }

    }
}


