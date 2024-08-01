#if DEBUG
//#define TESTING
#endif


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
using Cloo;
using System.Runtime.InteropServices;

namespace ReconstructCells.Tomography
{
    public class GPURecon : OptRecon
    {
        #region Type Defines



        [StructLayout(LayoutKind.Explicit)]
        private struct uint2
        {
            [FieldOffset(0)]
            public uint x;
            [FieldOffset(1)]
            public uint y;
            public uint2(int x, int y)
            {
                this.x = (uint)x;
                this.y = (uint)y;
            }
            public uint2(uint[] x)
            {
                this.x = (uint)x[0];
                this.y = (uint)x[1];
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct uint4
        {
            [FieldOffset(0)]
            public uint x;
            [FieldOffset(1)]
            public uint y;
            [FieldOffset(2)]
            public uint z;
            [FieldOffset(3)]
            public uint w;
            public uint4(int x, int y, int z, int w)
            {
                this.x = (uint)x;
                this.y = (uint)y;
                this.z = (uint)z;
                this.w = (uint)w;
            }
            public uint4(uint[] x)
            {
                this.x = (uint)x[0];
                this.y = (uint)x[1];
                this.z = (uint)x[2];
                this.w = (uint)x[3];
            }
        }
        #endregion

        #region GPU Code

        //int Pos_rc_ImageOut = 0;
        //int Pos_rc_pImpulse = 1;
        //int Pos_rc_impulseWidth = 2;
        //int Pos_rc_inputDimensions = 3;
        //int Pos_rc_imageIn = 4;
        public static string rotateConvolutionKernal = @"
 __kernel void rotateConvolution(  
		__global float  * imageOut,
		__constant float  * pImpulse,
		const uint  impulseWidth,
		const uint2  inputDimensions,
		__global float  * imageIn
)
{
            int tid   = get_global_id(0);
            int width = inputDimensions.x;
            int height = inputDimensions.y;

            int x = tid % width;
            int y = tid / width;

            int ImpulseIND = 0;
            int StartJ = y - impulseWidth / 2;
            int EndJ = y + impulseWidth / 2;

            if (StartJ < 0)
            {
                StartJ = 0;
                ImpulseIND = impulseWidth / 2 - y;
            }

            if (EndJ > height) EndJ = height - 1;

            int memIND = x + StartJ * width;

            float sum = 0;
            for (int j = StartJ; j < EndJ; j++)
            {
                sum = sum + imageIn[memIND] * pImpulse[ImpulseIND];
                ImpulseIND++;
                memIND += width;
            }

            int outid = x * width + y;
            imageOut[outid] = sum;
}
";



        public static string simpleConvolution = @"
__kernel void simpleConvolution(  
		__global float  * imageOut,
		__constant float  * pImpulse,
		const uint  impulseWidth,
		const uint2  inputDimensions,
		__global float  * imageIn
)
{
    uint tid   = get_global_id(0);
    
    uint width  = inputDimensions.x;
    //uint height = inputDimensions.y;
    
    uint x = tid%width;
    uint y = tid/width;
    
    uint ImpulseIND=0;
    int StartI = x -impulseWidth/2 ;
    int EndI = x +impulseWidth/2 ;

    if (StartI <0)
    {
      StartI=0;
      ImpulseIND=impulseWidth/2- x;
    }
    if (EndI >width) EndI=width-1;

    StartI=y*width+ StartI;
    EndI=y*width+ EndI+1;
 
    float sum =0;
    for (int j = StartI; j <EndI; j++)
    {
         sum = sum+ imageIn[j]* pImpulse[ImpulseIND];
		 ImpulseIND++; 
    }
    uint outid = x*width+y;
    imageOut[outid]=sum;
}
";

        //int Pos_FBP_cube = 0;
        //int Pos_FBP_cubeDimensions = 1;
        //int Pos_FBP_imageDimensions = 2;
        //int Pos_FBP_imageIn = 3;
        //int Pos_FBP_FastScanDirection = 4;

        public static string FBProjectionKernal = @"        
__kernel void FBProjection(  __global float  * cube,
                             const    uint4  cubeDimensions,
						     const    uint2  imageDimensions,
							__global float  * imageIn,
                             const    float4 FastScanDirection )
{

			int tid   = get_global_id(0);

            int LI = cubeDimensions.x;
            int LJ = cubeDimensions.y;
            int LK = cubeDimensions.z;

            int LsI = imageDimensions.x;
            int sliceSize = LI * LJ;
           
            int remander = tid % sliceSize;

            float  sdotI, u;
            int lower_sI, lower_sJ;


            float halfI;
            halfI = (float)LI / 2;
            sdotI = ( (remander % LI) - halfI) * FastScanDirection.x +  ( (int)(remander / LI) - halfI) * FastScanDirection.y + LsI / 2;
            //make sure that we are still in the recon volumn
            if (sdotI > 0 && sdotI < LsI-1)
            {
                lower_sI = (int)floor(sdotI);
                u = sdotI - lower_sI;
                lower_sJ = (int)((tid / sliceSize - LK / 2) + imageDimensions.y / 2);

                uint paintIndex = (uint)(lower_sJ * LsI + lower_sI);
                cube[tid] += imageIn[paintIndex] * u + imageIn[paintIndex + 1] * (1 - u);
            }
}";


        public static string ForwardProjectKernal= @"

  float unplot(__global  float* pCubeSlice, int sliceWidth, int sliceHeight, int x, int y, float c)
        {
            if (x > 0 && y > 0 && x < sliceWidth && y < sliceHeight)
            {
                return (*(pCubeSlice + y * sliceWidth + x)) * c;
            }
            return 0;
        }

        int ipart(float x)
        {
            return floor(x);

        }


        float fpart(float x)
        {
            return x - (floor(x));
        }

        float rfpart(float x)
        {
            return 1 - fpart(x);
        }


        private void swap(float* A, float* B)
        {
            float t = *A;
            *A = *B;
            *B = t;
        }
        
        float readLine(__global float* pCubeSlice, int sliceWidth, int sliceHeight, float x0, float y0, float x1, float y1)
        {
            bool steep = abs((int)(y1 - y0)) > abs((int)(x1 - x0));

            float sumLine = 0;

            if (steep)
            {
                swap( &x0, &y0);
                swap( &x1, &y1);
            }
            if (x0 > x1)
            {
                swap( &x0,  &x1);
                swap( &y0,  &y1);
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dy / dx;

            // handle first endpoint
            float xend = round(x0);
            float yend = y0 + gradient * (xend - x0);
            float xgap = rfpart(x0 + 0.5f);
            int xpxl1 = (int)xend;  //this will be used in the main loop
            int ypxl1 = ipart(yend);
            if (steep)
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl1, xpxl1, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl1 + 1, xpxl1, fpart(yend) * xgap);
            }
            else
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1 + 1, fpart(yend) * xgap);
            }
            float intery = yend + gradient; // first y-intersection for the main loop

            // handle second endpoint

            xend = round(x1);
            yend = y1 + gradient * (xend - x1);
            xgap = fpart(x1 + 0.5f);
            int xpxl2 = (int)xend; //this will be used in the main loop
            int ypxl2 = ipart(yend);
            if (steep)
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl2, xpxl2, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl2 + 1, xpxl2, fpart(yend) * xgap);
            }
            else
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2 + 1, fpart(yend) * xgap);
            }

            // main loop

            for (int x = xpxl1 + 1; x < xpxl2 - 1; x++)
            {
                if (steep)
                {
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery), x, rfpart(intery));
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery) + 1, x, fpart(intery));
                }
                else
                {
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery), rfpart(intery));
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery) + 1, fpart(intery));
                }
                intery = intery + gradient;
            }

            return sumLine;
        }


__kernel void sForwardProject(
            __global float* cube,    
            const uint4   cubeDimensions,
            const uint2   imageDimensions,
            __global float* imageIn,
            const float4 FastScanDirection  )
        {
            int tid   = get_global_id(0);
            int width = imageDimensions.x;

            int x = tid / width;
            int y = tid % width;

            int LI = cubeDimensions.x;
            int LJ = cubeDimensions.y;
            int LK = cubeDimensions.z;

            int sliceSize = LI * LJ;

            float xVect = x - width / 2;

            float x0 = xVect * FastScanDirection.x + width * FastScanDirection.y / 2 + LJ / 2;
            float y0 = xVect * FastScanDirection.y + width *  (FastScanDirection.x * -1) / 2 + LJ / 2;

            float x1 = xVect * FastScanDirection.x - width * FastScanDirection.y / 2 + LJ / 2;
            float y1 = xVect * FastScanDirection.y - width *  (FastScanDirection.x * -1) / 2 + LJ / 2;

            float z0 = y - imageDimensions.y / 2 + LK / 2;

            if (z0 > 0 && z0 < LK)//&& 0 == y - imageDimensions[1] / 2f)
                imageIn[tid] = readLine(cube + sliceSize * (int)z0, LJ, LK, x0, y0, x1, y1);
        }
";


        #endregion

        #region GPU Prep
        static IList<ComputeDevice> devices;
        static ComputePlatform platform;
        static ComputeContext context;
        static ComputeKernel kernelBackProject = null;
        static ComputeKernel kernelConvolve;
        static ComputeKernel kernelForward = null;
        ComputeBuffer<float> ComputeDataVolume;
        ComputeBuffer<float> ComputeImpulse;
        float[] Impulse;
        static ComputeCommandQueue commands;
        static ComputeEventList eventList;
        static ComputeProgram program;
        ComputeBuffer<float> ImageOutB;
       // ComputeBuffer<float> ImageWeights;
        ComputeBuffer<float> inputImageBuffer;


        private static void PrepareGPU()
        {
            string[] Platforms = AvailablePlatforms();

            if (rnd.NextDouble()>.5)
                SetupGPU(Platforms[0], 0);
            else
                SetupGPU(Platforms[0], 1);
        }

        private static string[] AvailablePlatforms()
        {
            // Populate OpenCL Platform ComboBox
            string[] availablePlatforms = new string[ComputePlatform.Platforms.Count];
            for (int i = 0; i < availablePlatforms.Length; i++)
                availablePlatforms[i] = ComputePlatform.Platforms[i].Name;

          

            return availablePlatforms;
        }

        private static bool BuildProgram(string Platform, string ProgramSource, int Device, out  ComputeContext context, out ComputeProgram program)
        {
            System.Diagnostics.Debug.Print(ProgramSource);
            context = null;
            program = null;
            devices = new List<ComputeDevice>();

            if (ComputePlatform.Platforms.Count == 0)
                return false;

            foreach (ComputePlatform cp in ComputePlatform.Platforms)
            {
                if (cp.Name == Platform)
                    platform = cp;
            }

            devices.Add(platform.Devices[Device]);

            if (devices.Count == 0)
                return false;

            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
            context = new ComputeContext(devices, properties, null, IntPtr.Zero);


            // Create and build the opencl program.
            program = new ComputeProgram(context, ProgramSource);
            program.Build(null, null, null, IntPtr.Zero);
            return true;
        }

        private static bool SetupGPU(string Platform, int device)
        {
            bool ret = BuildProgram(Platform, rotateConvolutionKernal + "\n" + FBProjectionKernal + "\n" + ForwardProjectKernal, device, out context, out program);

            if (ret == true)
            {
                // Create the kernel function and set its arguments.
                kernelConvolve = program.CreateKernel("rotateConvolution");
                kernelBackProject = program.CreateKernel("FBProjection");
                kernelForward = program.CreateKernel("sForwardProject");

                // Create the event wait list. An event list is not really needed for this example but it is important to see how it works.
                // Note that events (like everything else) consume OpenCL resources and creating a lot of them may slow down execution.
                // For this reason their use should be avoided if possible.
                eventList = new ComputeEventList();

                // Create the command queue. This is used to control kernel execution and manage read/write/copy operations.
                commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

                return true;
            }
            return ret;
        }

        #endregion


        private bool m_getForwardProjections=false ;
        public void setGetForwardProjections(bool doForwardProjections)
        {
            m_getForwardProjections = doForwardProjections;
        }


        private object lockObject = new object();
        public GPURecon()
        {

        }

        /*
      __global float  * cube,
      const    uint4  cubeDimensions,
      const    uint2  imageDimensions,
      __global float  * imageIn,
      const    float4 FastScanDirection
       */
        private void ConvolveAndProject()
        {
            unsafe
            {
                eventList = new ComputeEventList();


                float[] ImageData = new float[Library[1].Data.LongLength];// (float[])Library[0].ManagedArray;

                Buffer.BlockCopy(Library[0].Data, 0, ImageData, 0, Buffer.ByteLength(ImageData));

                commands.WriteToBuffer(ImageData, inputImageBuffer, true, null);

                float[] fastScanDirection;
                for (int imageNumber = 0; imageNumber < Library.Count - 1; imageNumber++)
                {
                    //ImageData = (float[])Library[imageNumber].ManagedArray;
                    Buffer.BlockCopy(Library[imageNumber + 1].Data, 0, ImageData, 0, Buffer.ByteLength(ImageData));
                    fastScanDirection = getFastScan(imageNumber);
                    fixed (float* pFS_Dir = fastScanDirection)
                    {
                        kernelBackProject.SetArgument(4, (IntPtr)(sizeof(uint) * 4), (IntPtr)pFS_Dir);

                        //now that all the memory is finally set up, run the kernals
                        commands.Execute(kernelConvolve, null, new long[] { ImageData.LongLength }, null, eventList);

                        commands.WriteToBuffer(ImageData, inputImageBuffer, true, null);

                        //now that all the memory is finally set up, run the kernals
                        commands.Execute(kernelBackProject, null, new long[] { DensityGrid.LongLength }, null, eventList);
                    }
                }


                fastScanDirection = getFastScan(Library.Count - 1);
                fixed (float* pFS_Dir = fastScanDirection)
                {
                    kernelBackProject.SetArgument(4, (IntPtr)(sizeof(uint) * 4), (IntPtr)pFS_Dir);
                    commands.Execute(kernelConvolve, null, new long[] { ImageData.LongLength }, null, eventList);
                    commands.Execute(kernelBackProject, null, new long[] { DensityGrid.LongLength }, null, eventList);
                }


                eventList.Wait();


                if (m_getForwardProjections || ReconType == ReconTypes.ForwardOnly)
                {
                    for (int imageNumber = 0; imageNumber < Library.Count; imageNumber++)
                    {
                        Image<Gray, float> image = Library[0].CopyBlank();
                      
                        fastScanDirection = getFastScan(imageNumber);
                        fixed (float* pFS_Dir = fastScanDirection, pf_image = image.Data)
                        {
                            for (int j = 0; j < ImageData.Length; j++)
                                ImageData[j] = 0;
                            /***********************************************************************************************************************************
                            __kernel void ForwardProjection(   __global float* cube,    
              const uint4   cubeDimensions,
              const uint2   imageDimensions,
              __global float* imageIn,
              const float4 FastScanDirection  )
                           */

                            kernelForward.SetArgument(4, (IntPtr)(sizeof(uint) * 4), (IntPtr)pFS_Dir);
                            commands.WriteToBuffer(ImageData, ImageOutB, true, null);

                            //now that all the memory is finally set up, run the kernals
                            commands.Execute(kernelForward, null, new long[] { image.Data.LongLength }, null, eventList);

                            eventList.Wait();

                            commands.ReadFromBuffer(ImageOutB, ref ImageData, true, 0, 0, ImageData.Length, null);
                            Buffer.BlockCopy(ImageData, 0, image.Data, 0, Buffer.ByteLength(ImageData));
                          
                           
                            ForwardProjections[imageNumber] = image;
                        }
                    }
                }


                //  eventList = null;
                fixed (float* pCube = DensityGrid)
                {
                    var region = new SysIntX3(DensityGrid.GetLength(0), DensityGrid.GetLength(1), DensityGrid.GetLength(2));
                    commands.ReadFromBuffer(ComputeDataVolume, ref DensityGrid, true, new SysIntX3(0, 0, 0), new SysIntX3(0, 0, 0), region, null);
                }
            }
        }

        //public OnDemandImageLibrary GetForwardProjections()
        //{
        //    OnDemandImageLibrary forwardProjections = new OnDemandImageLibrary(Library.Count, true);
        //    unsafe
        //    {
        //        eventList = new ComputeEventList();

        //        //fixed (float* pCube)
        //        //{
        //        //ComputeDataVolume = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, DensityGrid.LongLength, (IntPtr)pCube);
        //        //       kernelForward.SetMemoryArgument(0, ComputeDataVolume);


        //        float[] ImageData = new float[Library[1].Data.LongLength];
        //        float[] ImageDataO = new float[Library[1].Data.LongLength];
        //        float[] fastScanDirection;
        //        for (int imageNumber = 0; imageNumber < Library.Count; imageNumber++)
        //        {
        //            Image<Gray, float> image = Library[0].CopyBlank();

        //            fastScanDirection = getFastScan(imageNumber);
        //            fixed (float* pFS_Dir = fastScanDirection, pf_image = image.Data)
        //            {
        //                /***********************************************************************************************************************************
        //                 __global float  * cube,
        //                    const    uint4  cubeDimensions,
        //                    const    uint2  imageDimensions,
        //                   __global float  * imageOut,
        //                    const    float4 FastScanDirection
        //                */
        //                kernelForward.SetArgument(5, (IntPtr)(sizeof(uint) * 4), (IntPtr)pFS_Dir);
        //                commands.WriteToBuffer(ImageData, ImageOutB, true, null);
        //                //now that all the memory is finally set up, run the kernals
        //                commands.Execute(kernelForward, null, new long[] { DensityGrid.LongLength }, null, eventList);
        //                eventList.Wait();

        //                commands.ReadFromBuffer(ImageOutB, ref ImageDataO, true, 0, 0, ImageDataO.Length, null);
        //                Buffer.BlockCopy(ImageDataO, 0, image.Data, 0, Buffer.ByteLength(ImageDataO));
        //            }

        //            forwardProjections[imageNumber] = image;
        //        }


        //    }
        //    return forwardProjections;
        //}

        /*
         *Convolve
       __global float  * imageOut,
       __constant float  * pImpulse,
       const uint  impulseWidth,
       const uint2  inputDimensions,
       __global float  * imageIn
        */

        /*
         * FBP
       __global float  * cube,
       const    uint4  cubeDimensions,
       const    uint2  imageDimensions,
       __global float  * imageIn,
       const    float4 FastScanDirection
        */
        private Image<Gray, float> BetweenImage;

        private void InitializeGPU()
        {
            //locing takes time, so check first
            if (kernelBackProject == null)
            {
                lock (lockObject)
                {
                    if (kernelBackProject == null)
                    {
                        PrepareGPU();
                    }
                }
            }

            unsafe
            {

                BetweenImage = Library[1].Copy();
                var WeightsImage = Library[1].CopyBlank();
                float[, ,] ImageData = Library[1].Data;
                float[,] imageDataOut = new float[ImageData.GetLength(0), ImageData.GetLength(1)];
                fixed (float* pImage = ImageData, pImageOut = imageDataOut, pCube = DensityGrid, pBetween = BetweenImage.Data, pWeights = WeightsImage.Data)
                {
                    uint[] imageDims = new uint[] { (uint)ImageData.GetLength(0), (uint)ImageData.GetLength(1) };
                    uint[] cubeDims = (new uint[] { (uint)DensityGrid.GetLength(0), (uint)DensityGrid.GetLength(1), (uint)DensityGrid.GetLength(2), 0 });
                    fixed (uint* pImageDims = imageDims, pVolDims = cubeDims)
                    {

                        if (ReconType == ReconTypes.FBP)
                        {
                            /************************************************************************************************************************************/
                            ImageOutB = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, BetweenImage.Data.LongLength, (IntPtr)pBetween);
                            kernelConvolve.SetMemoryArgument(0, ImageOutB);

                            if (Impulse == null || Kernal.Length != Impulse.Length)
                            {

                                Impulse = Kernal;

                                ComputeImpulse = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, Impulse);
                                kernelConvolve.SetMemoryArgument(1, ComputeImpulse);
                                kernelConvolve.SetValueArgument<uint>(2, (uint)Impulse.Length);
                            }

                            kernelConvolve.SetArgument(3, (IntPtr)(sizeof(uint) * 2), (IntPtr)pImageDims);

                            //input the image into the convolution kernal
                            inputImageBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, ImageData.LongLength, (IntPtr)pImageOut);
                            kernelConvolve.SetMemoryArgument(4, inputImageBuffer);

                            /************************************************************************************************************************************/
                        }


                        ComputeDataVolume = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, DensityGrid.LongLength, (IntPtr)pCube);
                        kernelBackProject.SetMemoryArgument(0, ComputeDataVolume);
                        kernelBackProject.SetArgument(1, (IntPtr)(sizeof(uint) * 4), (IntPtr)pVolDims);

                        kernelBackProject.SetArgument(2, (IntPtr)(sizeof(uint) * 2), (IntPtr)pImageDims);
                        kernelBackProject.SetMemoryArgument(3, ImageOutB);

                        /***********************************************************************************************************************************
                          __kernel void ForwardProjection(   __global float* cube,    
            const uint4   cubeDimensions,
            const uint2   imageDimensions,
            __global float* imageIn,
            const float4 FastScanDirection  )
                         */

                        kernelForward.SetMemoryArgument(0, ComputeDataVolume);
                        kernelForward.SetArgument(1, (IntPtr)(sizeof(uint) * 4), (IntPtr)pVolDims);

                        kernelForward.SetArgument(2, (IntPtr)(sizeof(uint) * 2), (IntPtr)pImageDims);
                        kernelForward.SetMemoryArgument(3, ImageOutB);

                       
                    }
                }
            }



        }

        protected override float[, ,] DoProjections()
        {
            alpha = .4f;// *NoiseGuess() / 500;

            Initialize(false);
            InitializeGPU();

            if (mCleanProjections)
            {
                //CleanProjections();

                for (int j = 0; j < DensityGrid.GetLength(1); j++)
                    for (int k = 0; k < DensityGrid.GetLength(2); k++)
                        for (int n = 0; n < DensityGrid.GetLength(0); n++)
                            if (DensityGrid[n, j, k] < 0)
                                DensityGrid[n, j, k] = 0;

                double m = 0;
                int hx = DensityGrid.GetLength(1) / 2;
                for (int i = 0; i < DensityGrid.GetLength(0); i++)
                    m += DensityGrid[i, hx, hx];

                double mP = Library[1].Data[Library[1].Height / 2, Library[1].Width / 2, 0];

                DensityGrid.DivideInPlace((float)(m / mP));
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

                //Parallel.For(0, numberOfImages / batchSkip, Program.threadingParallelOptions, x => BatchProject(x));



#if TESTING
                TestProgram();
                return DensityGrid;
#else

                ConvolveAndProject();
#endif

                var CrossSections = DensityGrid.ShowCross();
                Program.ShowBitmaps(CrossSections);


                //double m = ErrorGrid.MaxArray();
                //double m2 = ErrorGrid.MinArray();
                //double m3 = m2 + m;
                CrossSections = ErrorGrid.ShowCross();
                Program.ShowBitmaps(CrossSections);

                if (ReconType != ReconTypes.FBP && ReconType != ReconTypes.ForwardOnly)
                {
                    Parallel.For(0, (int)DensityGrid.GetLength(0), Program.threadingParallelOptions, x => BatchAddAndZeroSIRT(x));

                    CrossSections = SecondGrid.ShowCross();
                    Program.ShowBitmaps(CrossSections);

                    float[, ,] tempGrid = DensityGrid;
                    DensityGrid = SecondGrid;
                    SecondGrid = tempGrid;
                }
            }

            return DensityGrid;
        }


        private float[] getFastScan(int imageNumber)
        {
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

            Point3D vec = Point3D.RotateAroundAxis(axis, vRotationAxis, AngleRadians);

            Point3D Direction = vec;
            Point3D FastScanDirection = Point3D.CrossProduct(vec, vRotationAxis);
            //make sure all the vectors are the right size
            Direction.Normalize();
            FastScanDirection.Normalize();
            return new float[] { (float)FastScanDirection.X, (float)FastScanDirection.Y, (float)FastScanDirection.Z };
        }


        #region FakeGPU

#if TESTING

        unsafe void plot(float* pCubeSlice, int sliceWidth, int sliceHeight, int x, int y, float c)
        {
            if (x > 0 && y > 0 && x < sliceWidth && y < sliceHeight)
            {
                *(pCubeSlice + y * sliceWidth + x) += c;
            }
        }

        unsafe float unplot(float* pCubeSlice, int sliceWidth, int sliceHeight, int x, int y, float c)
        {
            if (x > 0 && y > 0 && x < sliceWidth && y < sliceHeight)
            {
                return (*(pCubeSlice + y * sliceWidth + x)) * c;
            }
            return 0;
        }
        //     plot the pixel at (x, y) with brightness c (where 0 ≤ c ≤ 1)

        int ipart(float x)
        {
            return (int)(Math.Floor(x));

        }
        //     return  'integer part of x'

        float round(float x)
        {
            return (float)Math.Round(x);
        }
        //     return ipart(x + 0.5)

        float fpart(float x)
        {
            return x - (float)(Math.Floor(x));
        }

        float rfpart(float x)
        {
            return 1 - fpart(x);
        }

        float abs(float x)
        {
            return (float)Math.Abs(x);
        }

        private void swap(ref float A, ref float B)
        {
            float t = A;
            A = B;
            B = t;
        }
        private unsafe void drawLine(float* pCubeSlice, int sliceWidth, int sliceHeight, float pixelColor, float x0, float y0, float x1, float y1)
        {
            bool steep = abs(y1 - y0) > abs(x1 - x0);

            if (steep)
            {
                swap(ref x0, ref  y0);
                swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                swap(ref x0, ref  x1);
                swap(ref y0, ref y1);
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dy / dx;

            // handle first endpoint
            float xend = round(x0);
            float yend = y0 + gradient * (xend - x0);
            float xgap = rfpart(x0 + 0.5f);
            int xpxl1 = (int)xend;  //this will be used in the main loop
            int ypxl1 = ipart(yend);
            if (steep)
            {
                plot(pCubeSlice, sliceWidth, sliceHeight, ypxl1, xpxl1, rfpart(yend) * xgap * pixelColor);
                plot(pCubeSlice, sliceWidth, sliceHeight, ypxl1 + 1, xpxl1, fpart(yend) * xgap * pixelColor);
            }
            else
            {
                plot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1, rfpart(yend) * xgap * pixelColor);
                plot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1 + 1, fpart(yend) * xgap * pixelColor);
            }
            float intery = yend + gradient; // first y-intersection for the main loop

            // handle second endpoint

            xend = round(x1);
            yend = y1 + gradient * (xend - x1);
            xgap = fpart(x1 + 0.5f);
            int xpxl2 = (int)xend; //this will be used in the main loop
            int ypxl2 = ipart(yend);
            if (steep)
            {
                plot(pCubeSlice, sliceWidth, sliceHeight, ypxl2, xpxl2, rfpart(yend) * xgap * pixelColor);
                plot(pCubeSlice, sliceWidth, sliceHeight, ypxl2 + 1, xpxl2, fpart(yend) * xgap * pixelColor);
            }
            else
            {
                plot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2, rfpart(yend) * xgap * pixelColor);
                plot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2 + 1, fpart(yend) * xgap * pixelColor);
            }

            // main loop

            for (int x = xpxl1 + 1; x < xpxl2 - 1; x++)
            {
                if (steep)
                {
                    plot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery), x, rfpart(intery) * pixelColor);
                    plot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery) + 1, x, fpart(intery) * pixelColor);
                }
                else
                {
                    plot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery), rfpart(intery) * pixelColor);
                    plot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery) + 1, fpart(intery) * pixelColor);
                }
                intery = intery + gradient;
            }
        }


        private unsafe float readLine(float* pCubeSlice, int sliceWidth, int sliceHeight, float x0, float y0, float x1, float y1)
        {
            bool steep = abs(y1 - y0) > abs(x1 - x0);

            float sumLine = 0;

            if (steep)
            {
                swap(ref x0, ref  y0);
                swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                swap(ref x0, ref  x1);
                swap(ref y0, ref y1);
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dy / dx;

            // handle first endpoint
            float xend = round(x0);
            float yend = y0 + gradient * (xend - x0);
            float xgap = rfpart(x0 + 0.5f);
            int xpxl1 = (int)xend;  //this will be used in the main loop
            int ypxl1 = ipart(yend);
            if (steep)
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl1, xpxl1, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl1 + 1, xpxl1, fpart(yend) * xgap);
            }
            else
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl1, ypxl1 + 1, fpart(yend) * xgap);
            }
            float intery = yend + gradient; // first y-intersection for the main loop

            // handle second endpoint

            xend = round(x1);
            yend = y1 + gradient * (xend - x1);
            xgap = fpart(x1 + 0.5f);
            int xpxl2 = (int)xend; //this will be used in the main loop
            int ypxl2 = ipart(yend);
            if (steep)
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl2, xpxl2, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ypxl2 + 1, xpxl2, fpart(yend) * xgap);
            }
            else
            {
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2, rfpart(yend) * xgap);
                sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, xpxl2, ypxl2 + 1, fpart(yend) * xgap);
            }

            // main loop

            for (int x = xpxl1 + 1; x < xpxl2 - 1; x++)
            {
                if (steep)
                {
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery), x, rfpart(intery));
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, ipart(intery) + 1, x, fpart(intery));
                }
                else
                {
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery), rfpart(intery));
                    sumLine += unplot(pCubeSlice, sliceWidth, sliceHeight, x, ipart(intery) + 1, fpart(intery));
                }
                intery = intery + gradient;
            }

            return sumLine;
        }


        private unsafe void sBackProject(float* cube,
                                         int[] cubeDimensions,
                                         int[] imageDimensions,
                                         float* imageIn,
                                         float[] FastScanDirection,
            int tid)
        {
            int width = imageDimensions[0];
            int x = tid % width;
            int y = tid / width;

            int LI = cubeDimensions[0];
            int LJ = cubeDimensions[1];
            int LK = cubeDimensions[2];

            int LsI = imageDimensions[0];
            int LsJ = imageDimensions[1];

            int sliceSize = LI * LJ;

            float[] SlowScan = new float[4];

            SlowScan[0] = FastScanDirection[1];
            SlowScan[1] = FastScanDirection[0] * -1;

            float xVect = x - width / 2f;

            float x0 = xVect * FastScanDirection[0] + width * SlowScan[0] / 2 + LJ / 2f;
            float y0 = xVect * FastScanDirection[1] + width * SlowScan[1] / 2 + LJ / 2f;

            float x1 = xVect * FastScanDirection[0] - width * SlowScan[0] / 2 + LJ / 2f;
            float y1 = xVect * FastScanDirection[1] - width * SlowScan[1] / 2 + LJ / 2f;

            float z0 = y - imageDimensions[1] / 2f + LK / 2f;

            if (z0 > 0 && z0 < LK)//&& 0 == y - imageDimensions[1] / 2f)
                drawLine(cube + sliceSize * (int)z0, LJ, LK, imageIn[tid], x0, y0, x1, y1);
        }

        private unsafe void sForwardProject(float* cube,
                                        int[] cubeDimensions,
                                        int[] imageDimensions,
                                        float* imageIn,
                                        float[] FastScanDirection,
           int tid)
        {
            int width = imageDimensions[0];
            int x = tid % width;
            int y = tid / width;

            int LI = cubeDimensions[0];
            int LJ = cubeDimensions[1];
            int LK = cubeDimensions[2];

            int LsI = imageDimensions[0];
            int LsJ = imageDimensions[1];

            int sliceSize = LI * LJ;

            float[] SlowScan = new float[4];

            SlowScan[0] = FastScanDirection[1];
            SlowScan[1] = FastScanDirection[0] * -1;

            float xVect = x - width / 2f;

            float x0 = xVect * FastScanDirection[0] + width * SlowScan[0] / 2 + LJ / 2f;
            float y0 = xVect * FastScanDirection[1] + width * SlowScan[1] / 2 + LJ / 2f;

            float x1 = xVect * FastScanDirection[0] - width * SlowScan[0] / 2 + LJ / 2f;
            float y1 = xVect * FastScanDirection[1] - width * SlowScan[1] / 2 + LJ / 2f;

            float z0 = y - imageDimensions[1] / 2f + LK / 2f;

            if (z0 > 0 && z0 < LK)//&& 0 == y - imageDimensions[1] / 2f)
                imageIn[tid] = readLine(cube + sliceSize * (int)z0, LJ, LK, x0, y0, x1, y1);
        }

        private unsafe void SiddonProject(int imageNumber)
        {

            Image<Gray, float> cImage = cLibrary[imageNumber];
            int[] imageDims = new int[] { (int)cImage.Data.GetLength(0), (int)cImage.Data.GetLength(1) };
            int[] cubeDims = new int[] { (int)DensityGrid.GetLength(0), (int)DensityGrid.GetLength(1), (int)DensityGrid.GetLength(2) };
            int tid;
            fixed (float* pf_cube = DensityGrid, pf_ImageIn = cImage.Data)
            {
                float[] fastScanDirection = getFastScan(imageNumber);
                for (tid = 0; tid < cImage.Data.LongLength; tid++)
                    sBackProject(pf_cube, cubeDims, imageDims, pf_ImageIn, fastScanDirection, tid);
            }


        }

        private unsafe void SiddonForwardProject(int imageNumber)
        {

            Image<Gray, float> cImage = cLibrary[imageNumber];
            int[] imageDims = new int[] { (int)cImage.Data.GetLength(0), (int)cImage.Data.GetLength(1) };
            int[] cubeDims = new int[] { (int)DensityGrid.GetLength(0), (int)DensityGrid.GetLength(1), (int)DensityGrid.GetLength(2) };
            int tid;
            fixed (float* pf_cube = DensityGrid, pf_ImageIn = cImage.Data)
            {
                float[] fastScanDirection = getFastScan(imageNumber);
                for (tid = 0; tid < cImage.Data.LongLength; tid++)
                    sForwardProject(pf_cube, cubeDims, imageDims, pf_ImageIn, fastScanDirection, tid);
            }
            cLibrary[imageNumber] = cImage;

        }

        Image<Gray, float> tImage;
        OnDemandImageLibrary cLibrary;
        private void TestProgram()
        {

            cLibrary = new OnDemandImageLibrary(Library.Count, true);

            ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount;


            // BatchConvolveImage(0);
            //Parallel.For(0, Library.Count, Program.threadingParallelOptions, x => BatchConvolveImage(x));


            //cLibrary.SaveImages(@"c:\temp\convolved\image.tif");
            cLibrary = new OnDemandImageLibrary(@"c:\temp\convolved", true, "", false);

            //cLibrary.RotateLibrary(90);

            Parallel.For(0, Library.Count, Program.threadingParallelOptions, x => BatchProject(x));

            tImage = new Image<Gray, float>(DensityGrid.GetLength(0), DensityGrid.GetLength(1));

            Parallel.For(0, Library.Count, Program.threadingParallelOptions, x => SiddonForwardProject(x));
            // for (int i=0;i<Library.Count ;i++)
            //   ForwardProject(i);

            var b = DensityGrid.ShowCross();
            int w = b.Length;
        }

        #region Convolve
        private unsafe void BatchConvolveImage(int imageNumber)
        {
            Image<Gray, float> cImage = Library[imageNumber].CopyBlank();
            int[] imageDims = new int[] { (int)cImage.Data.GetLength(0), (int)cImage.Data.GetLength(1) };
            int tid;
            fixed (float* pf_image = Library[imageNumber].Data, pf_kernal = Kernal, pf_ImageOut = cImage.Data)
            {
                for (tid = 0; tid < cImage.Data.LongLength; tid++)
                    rotateConvolution(pf_ImageOut, pf_kernal, (int)Kernal.Length, imageDims, pf_image, tid);
            }

            int w = cImage.Width;

            cLibrary[imageNumber] = cImage;
        }

        private unsafe void rotateConvolution(
                float* imageOut,
                float* pImpulse,
                int impulseWidth,
                int[] inputDimensions,
                float* imageIn,
             int tid
       )
        {
            //= get_global_id(0);

            int width = inputDimensions[0];
            int height = inputDimensions[1];

            int x = tid % width;
            int y = tid / width;

            int ImpulseIND = 0;
            int StartJ = y - impulseWidth / 2;
            int EndJ = y + impulseWidth / 2;

            if (StartJ < 0)
            {
                StartJ = 0;
                ImpulseIND = impulseWidth / 2 - y;
            }

            if (EndJ > height) EndJ = height - 1;

            int memIND = x + StartJ * width;

            float sum = 0;
            for (int j = StartJ; j < EndJ; j++)
            {
                sum = sum + imageIn[memIND] * pImpulse[ImpulseIND];
                ImpulseIND++;
                memIND += width;
            }

            int outid = x * width + y;
            imageOut[outid] = sum;
        }




        private unsafe void simpleConvolutionT(
         float* imageOut,
         float* pImpulse,
         uint impulseWidth,
         uint2 inputDimensions,
         float* imageIn,
            uint tid
)
        {

            uint width = inputDimensions.x;
            //uint height = inputDimensions.y;

            uint x = tid % width;
            uint y = tid / width;

            uint ImpulseIND = 0;
            uint StartI = x - impulseWidth / 2;
            uint EndI = x + impulseWidth / 2;

            if (StartI < 0)
            {
                StartI = 0;
                ImpulseIND = impulseWidth / 2 - x;
            }
            if (EndI > width) EndI = width - 1;

            StartI = y * width + StartI;
            EndI = y * width + EndI + 1;

            float sum = 0;
            for (uint j = StartI; j < EndI; j++)
            {
                sum = sum + imageIn[j] * pImpulse[ImpulseIND];
                ImpulseIND++;
            }
            uint outid = x * width + y;
            imageOut[outid] = sum;
        }

        #endregion


        private unsafe void BatchProject(int imageNumber)
        {

            Image<Gray, float> cImage = cLibrary[imageNumber];
            int[] imageDims = new int[] { (int)cImage.Data.GetLength(0), (int)cImage.Data.GetLength(1) };
            int[] cubeDims = new int[] { (int)DensityGrid.GetLength(0), (int)DensityGrid.GetLength(1), (int)DensityGrid.GetLength(2) };
            int tid;
            fixed (float* pf_cube = DensityGrid, pf_ImageIn = cImage.Data)
            {
                float[] fastScanDirection = getFastScan(imageNumber);
                for (tid = 0; tid < DensityGrid.LongLength; tid++)
                    FBProjection(pf_cube, cubeDims, imageDims, pf_ImageIn, fastScanDirection, tid);
            }
        }

        private unsafe void FBProjection(float* cube,
                                         int[] cubeDimensions,
                                         int[] imageDimensions,
                                         float* imageIn,
                                         float[] FastScanDirection,
            int tid)
        {
            //	uint tid   = get_global_id(0);

            int LI = cubeDimensions[0];
            int LJ = cubeDimensions[1];
            int LK = cubeDimensions[2];

            int LsI = imageDimensions[0];
            int sliceSize = LI * LJ;

            int remander = tid % sliceSize;

            float sdotI, u;
            int lower_sI, lower_sJ;


            float halfI;
            halfI = (float)LI / 2;
            sdotI = ((remander % LI) - halfI) * FastScanDirection[0] + ((int)(remander / LI) - halfI) * FastScanDirection[1] + LsI / 2f;
            //make sure that we are still in the recon volumn
            if (sdotI > 0 && sdotI < LsI - 1)
            {
                lower_sI = (int)Math.Floor(sdotI);
                u = sdotI - lower_sI;
                lower_sJ = (int)((tid / sliceSize - LK / 2f) + imageDimensions[1] / 2f);

                uint paintIndex = (uint)(lower_sJ * LsI + lower_sI);
                cube[tid] += imageIn[paintIndex] * u + imageIn[paintIndex + 1] * (1 - u);
            }

        }

#endif

        #endregion




    }
}

