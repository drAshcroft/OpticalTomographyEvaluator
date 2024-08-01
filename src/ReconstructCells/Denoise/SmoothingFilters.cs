using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using MathLibrary;
using ImageProcessing;
using MathLibrary.FFT;
using ImageProcessing._3D;

namespace ReconstructCells.Imaging
{
    public class SmoothingFilters
    {
        private OnDemandImageLibrary Library = null;

        int Radius = 3;

        public SmoothingFilters(OnDemandImageLibrary library)
        {
            Library = library;

        }

        private void BatchMedian(int imageNumber)
        {
            Library[imageNumber] = Library[imageNumber].SmoothMedian(Radius);
        }

        private void BatchMedian2(int imageNumber)
        {
            Library[imageNumber] = Library[imageNumber].SmoothBilatral(Radius, 1000, Radius - 3);
            // var image =  Library[imageNumber] ;//= Library[imageNumber].SmoothMedian(Radius);
            /*  var imageOut = image.CopyBlank();
              int halfX=(int)Math.Floor(Radius/2d);
              List<float> SortList = new List<float>(new float[Radius * Radius]);
              int cc = 0;
              for (int x=Radius ;x<image.Width -Radius ;x++)
                  for (int y = Radius; y < image.Height - Radius; y++)
                  {
                      cc = 0;
                      for (int xx=x-halfX ;xx<x+halfX ;xx++)
                          for (int yy = y - halfX; yy < y + halfX; yy++)
                          {
                              SortList[cc] = image.Data[xx, yy, 0];
                              cc = cc + 1;
                          }
                      SortList.Sort();
                      imageOut.Data[x, y, 0] = SortList[SortList.Count / 2];
                  }
              Library[imageNumber] = imageOut;*/
        }

        public void MedianFilter(int Radius)
        {
            this.Radius = Radius;

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchMedian(x));
        }


        private double amplitude;
        private void BatchAddNoise(int imageNumber)
        {
            var image = Library[imageNumber];
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    image.Data[y, x, 0] += (float)(amplitude * Utilities.SimpleRNG.GetNormal());
                }
            Library[imageNumber] = image;
        }

        public void AddGaussianNoiseFilter(double amplitude)
        {
            this.amplitude = amplitude;
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchAddNoise(x));
        }


        private void BatchGuassSmooth(int imageNumber)
        {
            Library[imageNumber] = Library[imageNumber].SmoothMedian(Radius);
        }

        public void GuassSmoothFilter(int Radius)
        {
            this.Radius = Radius;

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchGuassSmooth(x));
        }


        private void BatchBilateral(int imageNumber)
        {
            Library[imageNumber] = Library[imageNumber].SmoothBilatral(Radius, 500, 2);
        }

        public void BilateralSmoothFilter(int Radius)
        {
            this.Radius = Radius;

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchBilateral(x));
        }


        #region TV
        private int Iterations;
        static double ep = 1;
        static double dt = 1 / 5; // % dt below the CFL bound
        static double lam = 0;
        static double C = 0;
        OnDemandImageLibrary IOs;
        float[] Differences;
        /// <summary>
        /// performs the total variation filter on the image.  this will alter the input image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="iter"></param>
        /// <returns></returns>
        public static Image<Gray, float> TV(Image<Gray, float> image, Image<Gray, float> I0, int iter)
        {

            int ny = image.Height;
            int nx = image.Width;
            double ep2 = ep * ep;

            Image<Gray, float> I_x = image.CopyBlank(), I_y = image.CopyBlank(), I_xx = image.CopyBlank(),
                I_yy = image.CopyBlank(), Dp = image.CopyBlank(), Dm = image.CopyBlank(), I_xy = image.CopyBlank(), num = image.CopyBlank(), denom = image.CopyBlank(),
                I_t = image.CopyBlank();
            for (int i = 0; i < iter; i++)
            {
                //% estimate derivatives
                //I_x = (I(:,[2:nx nx])-I(:,[1 1:nx-1]))/2;
                for (int x = 1; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        I_x.Data[y, x - 1, 0] = (image.Data[y, x, 0] - image.Data[y, x - 1, 0]) / 2;
                    }
                //	I_y = (I([2:ny ny],:)-I([1 1:ny-1],:))/2;
                for (int x = 0; x < image.Width; x++)
                    for (int y = 1; y < image.Height; y++)
                    {
                        I_y.Data[y - 1, x, 0] = (image.Data[y, x, 0] - image.Data[y - 1, x, 0]) / 2;
                    }


                //I_xx = I(:,[2:nx nx])+I(:,[1 1:nx-1])-2*I;
                for (int x = 1; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        I_xx.Data[y, x - 1, 0] = (image.Data[y, x, 0] + image.Data[y, x - 1, 0]) - 2 * image.Data[y, x - 1, 0];
                    }

                //I_yy = I([2:ny ny],:)+I([1 1:ny-1],:)-2*I;
                for (int x = 0; x < image.Width; x++)
                    for (int y = 1; y < image.Height; y++)
                    {
                        I_yy.Data[y - 1, x, 0] = (image.Data[y, x, 0] + image.Data[y - 1, x, 0]) - 2 * image.Data[y - 1, x, 0];
                    }

                //              	Dp = I([2:ny ny],[2:nx nx])+I([1 1:ny-1],[1 1:nx-1]);
                for (int x = 1; x < nx; x++)
                    for (int y = 1; y < ny; y++)
                    {
                        Dp.Data[y - 1, x - 1, 0] = (image.Data[y, x, 0] + image.Data[y - 1, x - 1, 0]);
                    }
                //	Dm = I([1 1:ny-1],[2:nx nx])+I([2:ny ny],[1 1:nx-1]);
                for (int x = 0; x < nx - 1; x++)
                    for (int y = 0; y < ny - 1; y++)
                    {
                        Dm.Data[y, x, 0] = (image.Data[y, x + 1, 0] + image.Data[y + 1, x, 0]);
                    }

                for (int x = 0; x < nx - 1; x++)
                    for (int y = 0; y < ny - 1; y++)
                    {
                        I_xy.Data[y, x, 0] = (Dp.Data[y, x + 1, 0] + Dm.Data[y + 1, x, 0]) / 4;
                    }


                // % compute flow
                //I_t = Num./Den + lam.*(I0-I+C);
                double d;
                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        //Num = I_xx.*(ep2+I_y.^2)-2*I_x.*I_y.*I_xy+I_yy.*(ep2+I_x.^2);
                        num.Data[y, x, 0] = (float)(I_xx.Data[y, x, 0] * (ep2 + I_y.Data[y, x, 0] * I_y.Data[y, x, 0])
                            - 2 * I_x.Data[y, x, 0] * I_y.Data[y, x, 0] * I_xy.Data[y, x, 0]
                            + I_yy.Data[y, x, 0] * (ep2 + I_x.Data[y, x, 0] * I_x.Data[y, x, 0]));

                        // Den = (ep2+I_x.^2+I_y.^2).^(3/2);
                        d = (ep2 + I_x.Data[y, x, 0] * I_x.Data[y, x, 0] + I_y.Data[y, x, 0] * I_y.Data[y, x, 0]);
                        denom.Data[y, x, 0] = (float)Math.Pow(d, 3d / 2d);

                        I_t.Data[y, x, 0] = (float)(num.Data[y, x, 0] / denom.Data[y, x, 0] + lam * (I0.Data[y, x, 0] - image.Data[y, x, 0] + C));
                    }

                //I=I+dt*I_t; // %% evolve image by dt
                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        image.Data[y, x, 0] = (float)(image.Data[y, x, 0] - dt * I_t.Data[y, x, 0]);
                    }
            }
            return image;
        }

        private void BatchTV(int imageNumber)
        {
            var imageIn = Library[imageNumber].Copy();

            var imageOut = TV(imageIn, IOs[imageNumber], Iterations);//.SmoothBilatral(Radius, 500, 1);
            imageIn = Library[imageNumber];
            double dif = 0;
            for (int y = 0; y < imageIn.Height; y++)
                for (int x = 0; x < imageIn.Width; x++)
                    dif += Math.Abs(imageIn.Data[y, x, 0] - imageOut.Data[y, x, 0]);
            Differences[imageNumber] = (float)(dif / imageIn.Data.LongLength);
            Library[imageNumber] = imageOut;
        }

        public void TotalVariation(int iterations)
        {
            Iterations = iterations;
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            Differences = new float[Library.Count];

           

            var mean = Library[0].SmoothGaussian(11);
            mean = mean.Sub(Library[0]);
            double  std_n=0;
            for (int i=0;i<mean.Width;i++)
                std_n+= Math.Pow( mean.Data[mean.Height/2,i,0],2);
            std_n = Math.Sqrt( std_n / mean.Width);
        
            float var_n =(float)( std_n * std_n); // % Gaussian noise standard deviation
            float reduced_pw = 1.5f * var_n;  // % power to reduce in first phase
            float sig_w = 5;
            float ws = 4 * sig_w + 1;//  % window size
            //%%%%%%%%%%%%%%

            //%%% Add noise
            IOs = new OnDemandImageLibrary(Library);
            //I0 = I(:,:); // % noisy input image
            //% show original and noisy images
            //% run normal tv - strong denoising

            float ep_J = 0.1f;
            lam = 0;
          //  Iterations = 10; 
            dt = 0.2f;
            ep = 1;// (float)Math.Pow(std_n, .5);
            float aveDiff = 10000;
            while (aveDiff > ep_J)//,  % iterate until convergence
            {
                //Parallel.For(0, numberOfImages, po, x => BatchTV(x));
                BatchTV(0);
                lam = calc_lam(Library[0], IOs[0], reduced_pw, 1); //% update lambda (fidelity term)
                aveDiff = Differences[0];// Differences.Average();
            }
        }

        public float calc_lam(Image<Gray, float> image, Image<Gray, float> I0, float var_n, float ep)
        {
            //%% private function: calc_lam (by Guy Gilboa).
            //%% calculate scalar fidelity term of the Total-Variation
            //%% input: current image, original noisy image, noise variance, epsilon
            //%% output: lambda
            //%% example: lam=calc_lam(I,I0,var_n,ep)


            int ny = image.Width, nx = image.Height;
            float ep2 = ep * ep;

            Image<Gray, float> I_x = image.CopyBlank(), I_y = image.CopyBlank(), I_xx = image.CopyBlank(),
              I_yy = image.CopyBlank(), Dp = image.CopyBlank(), Dm = image.CopyBlank(), I_xy = image.CopyBlank(), num = image.CopyBlank(), denom = image.CopyBlank(),
              I_t = image.CopyBlank();

            //% estimate derivatives
            //I_x = (I(:,[2:nx nx])-I(:,[1 1:nx-1]))/2;
            for (int x = 1; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    I_x.Data[y, x - 1, 0] = (image.Data[y, x, 0] - image.Data[y, x - 1, 0]) / 2;
                }
            //	I_y = (I([2:ny ny],:)-I([1 1:ny-1],:))/2;
            for (int x = 0; x < image.Width; x++)
                for (int y = 1; y < image.Height; y++)
                {
                    I_y.Data[y - 1, x, 0] = (image.Data[y, x, 0] - image.Data[y - 1, x, 0]) / 2;
                }


            //I_xx = I(:,[2:nx nx])+I(:,[1 1:nx-1])-2*I;
            for (int x = 1; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    I_xx.Data[y, x - 1, 0] = (image.Data[y, x, 0] + image.Data[y, x - 1, 0]) - 2 * image.Data[y, x - 1, 0];
                }

            //I_yy = I([2:ny ny],:)+I([1 1:ny-1],:)-2*I;
            for (int x = 0; x < image.Width; x++)
                for (int y = 1; y < image.Height; y++)
                {
                    I_yy.Data[y - 1, x, 0] = (image.Data[y, x, 0] + image.Data[y - 1, x, 0]) - 2 * image.Data[y - 1, x, 0];
                }

            //              	Dp = I([2:ny ny],[2:nx nx])+I([1 1:ny-1],[1 1:nx-1]);
            for (int x = 1; x < nx; x++)
                for (int y = 1; y < ny; y++)
                {
                    Dp.Data[y - 1, x - 1, 0] = (image.Data[y, x, 0] + image.Data[y - 1, x - 1, 0]);
                }
            //	Dm = I([1 1:ny-1],[2:nx nx])+I([2:ny ny],[1 1:nx-1]);
            for (int x = 0; x < nx - 1; x++)
                for (int y = 0; y < ny - 1; y++)
                {
                    Dm.Data[y, x, 0] = (image.Data[y, x + 1, 0] + image.Data[y + 1, x, 0]);
                }

            for (int x = 0; x < nx - 1; x++)
                for (int y = 0; y < ny - 1; y++)
                {
                    I_xy.Data[y, x, 0] = (Dp.Data[y, x + 1, 0] + Dm.Data[y + 1, x, 0]) / 4;
                }


            // % compute flow
            //I_t = Num./Den + lam.*(I0-I+C);
            double d;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    //Num = I_xx.*(ep2+I_y.^2)-2*I_x.*I_y.*I_xy+I_yy.*(ep2+I_x.^2);
                    num.Data[y, x, 0] = (float)(I_xx.Data[y, x, 0] * (ep2 + I_y.Data[y, x, 0] * I_y.Data[y, x, 0])
                        - 2 * I_x.Data[y, x, 0] * I_y.Data[y, x, 0] * I_xy.Data[y, x, 0]
                        + I_yy.Data[y, x, 0] * (ep2 + I_x.Data[y, x, 0] * I_x.Data[y, x, 0]));

                    // Den = (ep2+I_x.^2+I_y.^2).^(3/2);
                    d = (ep2 + I_x.Data[y, x, 0] * I_x.Data[y, x, 0] + I_y.Data[y, x, 0] * I_y.Data[y, x, 0]);
                    denom.Data[y, x, 0] = (float)Math.Pow(d, 3d / 2d);

                    I_t.Data[y, x, 0] = (float)(num.Data[y, x, 0] / denom.Data[y, x, 0]);
                }

            double sum = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    sum +=Math.Abs (I_t.Data[y, x, 0] * (image.Data[y, x, 0] - I0.Data[y, x, 0]));
                }
            sum = sum / image.Data.LongLength / var_n;
            return (float)sum;
            //             Num = I_xx.*(ep2+I_y.^2)-2*I_x.*I_y.*I_xy+I_yy.*(ep2+I_x.^2);
            //   Den = (ep2+I_x.^2+I_y.^2).^(3/2);
            //   Div = Num./Den;
            //  // %%% fidelity term
            //lam = mean(mean(Div.*(I-I0)))./var_n;  %% 
        }


        #endregion

        private float[, ,] vol;
        private void BatchVolumeBilateral(int sliceNumber)
        {
            if (sliceNumber > 0)
            {
                Image<Gray, float> image = new Image<Gray, float>(vol.GetLength(1), vol.GetLength(2));

                Buffer.BlockCopy(vol, sliceNumber * image.Width * image.Height * sizeof(float), image.Data, 0, Buffer.ByteLength(image.Data));

                var image2 = image.SmoothGaussian(7);// .SmoothBilatral(Radius, 500, 2);
                //var image3 = new Image<Gray, float>(image2.Width * 2, image2.Height);
                //image3 = Combine(image, image2, image3);

                Buffer.BlockCopy(image2.Data, 0, vol, sliceNumber * image2.Width * image2.Height * sizeof(float), Buffer.ByteLength(image2.Data));
            }
        }

        private Image<Gray, float> Combine(Image<Gray, float> image, Image<Gray, float> image2, Image<Gray, float> image3)
        {
            image3.ROI = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
            image.CopyTo(image3);
            image3.ROI = new System.Drawing.Rectangle(image.Width, 0, image2.Width, image2.Height);
            image2.CopyTo(image3);
            image3.ROI = System.Drawing.Rectangle.Empty;
            return image3;
        }

        public float[, ,] SmoothRecon(float[, ,] volume, int radius)
        {
            Radius = radius;
            vol = volume;

           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = volume.GetLength(0);

            // Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchVolumeBilateral(x));
            BatchVolumeBilateral(100);
            return vol;
        }

        public void filterKalman(OnDemandImageLibrary library, float percentvar, float gain)
        {

            int width = library[0].Width;
            int height = library[0].Height;
            int dimension = width * height;
            int stacksize = library.Count;

            float[] stackslice = new float[dimension];
            float[] filteredslice = new float[dimension];
            float[] noisevar = new float[dimension];
            float[] average = new float[dimension];
            float[] predicted = new float[dimension];
            float[] predictedvar = new float[dimension];

            float Kalman;

            for (int i = 0; i < dimension; ++i)
            {
                noisevar[i] = percentvar;
            }

            unsafe
            {

                Buffer.BlockCopy(library[0].Data, 0, predicted, 0, Buffer.ByteLength(predicted));
                //for (int i = 0; i < dimension; i++)
                //    predicted[i] = pStack[i];// = toDouble(stack.getPixels(1), bitDepth);

                for (int i = 0; i < dimension; ++i)
                {
                    predictedvar[i] = percentvar;
                }


                for (int i = 1; i < stacksize; ++i)
                {
                    fixed (float* pStack = library[i].Data)
                    {
                        for (int k = 0; k < library[i].Data.LongLength; ++k)
                        {
                            Kalman = predictedvar[k] / (predictedvar[k] + noisevar[k]);
                            predicted[k] = (float)(gain * predicted[k] + (1.0 - gain) * pStack[k] + Kalman * (pStack[k] - predicted[k]));
                            predictedvar[k] = (float)(predictedvar[k] * (1.0 - Kalman));

                            pStack[k] = predicted[k];
                        }
                    }
                }
            }
        }


        public void filterKalman(ref float[, ,] stack, float percentvar, float gain)
        {

            int width = stack.GetLength(1);
            int height = stack.GetLength(2);
            int dimension = width * height;
            int stacksize = stack.GetLength(0);
            float[] stackslice = new float[dimension];
            float[] filteredslice = new float[dimension];
            float[] noisevar = new float[dimension];
            float[] average = new float[dimension];
            float[] predicted = new float[dimension];
            float[] predictedvar = new float[dimension];
            float[] observed = new float[dimension];
            float[] Kalman = new float[dimension];
            float[] corrected = new float[dimension];
            float[] correctedvar = new float[dimension];

            for (int i = 0; i < dimension; ++i)
            {
                noisevar[i] = percentvar;
            }

            unsafe
            {
                fixed (float* pStack = stack)
                {

                    for (int i = 0; i < dimension; i++)
                        predicted[i] = pStack[i];// = toDouble(stack.getPixels(1), bitDepth);

                    predictedvar = noisevar;

                    for (int i = 1; i < stacksize; ++i)
                    {
                        // IJ.showProgress(i, stacksize);
                        int sliceStart = dimension * (i);
                        float d;
                        for (int j = 0; j < dimension; j++)
                        {
                            d = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);
                            stackslice[j] = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);;
                            observed[j] = d;
                        }

                        //= toDouble(stackslice, 64);
                        for (int k = 0; k < Kalman.Length; ++k)
                            Kalman[k] = predictedvar[k] / (predictedvar[k] + noisevar[k]);
                        for (int k = 0; k < corrected.Length; ++k)
                            corrected[k] = (float)(gain * predicted[k] + (1.0 - gain) * observed[k] + Kalman[k] * (observed[k] - predicted[k]));
                        for (int k = 0; k < correctedvar.Length; ++k)
                            correctedvar[k] = (float)(predictedvar[k] * (1.0 - Kalman[k]));
                        predictedvar = correctedvar;
                        predicted = corrected;
                        for (int j = 0; j < dimension; j++)
                        {
                            pStack[sliceStart + j] = corrected[j];
                        }
                    }
                }
            }
        }

        private float[, ,] mVariance = null;
        private float[, ,] mPredicted = null;

        public float[, ,] filterKalmanX(float[, ,] stack, float percentvar, float gain, bool SendToPool)
        {
            float[, ,] outArray = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];

            int width = stack.GetLength(1);
            int height = stack.GetLength(2);
            int dimension = stack.GetLength(0) * stack.GetLength(1);
            int stacksize = stack.GetLength(2);
            float[] stackslice = new float[dimension];
            float[] filteredslice = new float[dimension];
            float[] noisevar = new float[dimension];
            float[] average = new float[dimension];
            float[] predicted = new float[dimension];
            float[] predictedvar = new float[dimension];
            float[] observed = new float[dimension];
            float[] Kalman = new float[dimension];
            float[] corrected = new float[dimension];
            float[] correctedvar = new float[dimension];

            for (int i = 0; i < dimension; ++i)
            {
                noisevar[i] = percentvar;
            }

            unsafe
            {
                fixed (float* pStack = stack)
                {

                    // for (int i = 0; i < dimension; i++)
                    int cc = 0;
                    for (int z = 0; z < stack.GetLength(0); z++)
                        for (int y = 0; y < stack.GetLength(1); y++)
                        {
                            predicted[cc] = stack[z, y, 0];// = toDouble(stack.getPixels(1), bitDepth);
                            cc++;
                        }

                    for (int i = 0; i < dimension; ++i)
                    {
                        predictedvar[i] = noisevar[i];
                    }

                    for (int i = 1; i < stack.GetLength(2); ++i)
                    {
                        // IJ.showProgress(i, stacksize);
                        //int sliceStart = dimension * (i);
                        // float d;
                        //for (int j = 0; j < dimension; j++)
                        //{
                        //    d = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);
                        //  //  stackslice[j] = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);;
                        //    observed[j] = d;
                        //}

                        cc = 0;
                        for (int z = 0; z < stack.GetLength(0); z++)
                            for (int y = 0; y < stack.GetLength(1); y++)
                            {
                                observed[cc] = stack[z, y, i];// = toDouble(stack.getPixels(1), bitDepth);
                                cc++;
                            }

                        //= toDouble(stackslice, 64);
                        for (int k = 0; k < Kalman.Length; ++k)
                            Kalman[k] = predictedvar[k] / (predictedvar[k] + noisevar[k]);
                        for (int k = 0; k < predicted.Length; ++k)
                            predicted[k] = (float)(gain * predicted[k] + (1.0 - gain) * observed[k] + Kalman[k] * (observed[k] - predicted[k]));
                        for (int k = 0; k < predictedvar.Length; ++k)
                            predictedvar[k] = (float)(predictedvar[k] * (1.0 - Kalman[k]));
                        // predictedvar = correctedvar;
                        // predicted = corrected;

                        if (SendToPool == false)
                        {
                            cc = 0;
                            for (int z = 0; z < stack.GetLength(0); z++)
                                for (int y = 0; y < stack.GetLength(1); y++)
                                {
                                    outArray[z, y, i] = predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    cc++;
                                }
                        }
                        else
                        {

                            cc = 0;
                            for (int z = 0; z < stack.GetLength(0); z++)
                                for (int y = 0; y < stack.GetLength(1); y++)
                                {
                                    mPredicted[z, y, i] += predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    mVariance[z, y, i] += predictedvar[cc];
                                    cc++;
                                }

                        }
                        //for (int j = 0; j < dimension; j++)
                        //{
                        //    pStack[sliceStart + j] = predicted[j];
                        //}
                    }
                }
            }

            return outArray;
        }

        public float[, ,] filterKalmanY(float[, ,] stack, float percentvar, float gain, bool SendToPool)
        {
            float[, ,] outArray = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];

            int width = stack.GetLength(1);
            int height = stack.GetLength(2);
            int dimension = stack.GetLength(0) * stack.GetLength(2);
            int stacksize = stack.GetLength(1);
            float[] stackslice = new float[dimension];
            float[] filteredslice = new float[dimension];
            float[] noisevar = new float[dimension];
            float[] average = new float[dimension];
            float[] predicted = new float[dimension];
            float[] predictedvar = new float[dimension];
            float[] observed = new float[dimension];
            float[] Kalman = new float[dimension];
            float[] corrected = new float[dimension];
            float[] correctedvar = new float[dimension];

            for (int i = 0; i < dimension; ++i)
            {
                noisevar[i] = percentvar;
            }

            unsafe
            {
                fixed (float* pStack = stack)
                {

                    // for (int i = 0; i < dimension; i++)
                    int cc = 0;
                    for (int z = 0; z < stack.GetLength(0); z++)
                        for (int x = 0; x < stack.GetLength(2); x++)
                        {
                            predicted[cc] = stack[z, 0, x];// = toDouble(stack.getPixels(1), bitDepth);
                            cc++;
                        }

                    for (int i = 0; i < dimension; ++i)
                    {
                        predictedvar[i] = noisevar[i];
                    }

                    for (int i = 1; i < stack.GetLength(1); ++i)
                    {
                        // IJ.showProgress(i, stacksize);
                        //int sliceStart = dimension * (i);
                        // float d;
                        //for (int j = 0; j < dimension; j++)
                        //{
                        //    d = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);
                        //  //  stackslice[j] = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);;
                        //    observed[j] = d;
                        //}

                        cc = 0;
                        for (int z = 0; z < stack.GetLength(0); z++)
                            for (int x = 0; x < stack.GetLength(2); x++)
                            {
                                observed[cc] = stack[z, i, x];// = toDouble(stack.getPixels(1), bitDepth);
                                cc++;
                            }

                        //= toDouble(stackslice, 64);
                        for (int k = 0; k < Kalman.Length; ++k)
                            Kalman[k] = predictedvar[k] / (predictedvar[k] + noisevar[k]);
                        for (int k = 0; k < predicted.Length; ++k)
                            predicted[k] = (float)(gain * predicted[k] + (1.0 - gain) * observed[k] + Kalman[k] * (observed[k] - predicted[k]));
                        for (int k = 0; k < predictedvar.Length; ++k)
                            predictedvar[k] = (float)(predictedvar[k] * (1.0 - Kalman[k]));
                        // predictedvar = correctedvar;
                        // predicted = corrected;
                        if (SendToPool == false)
                        {
                            cc = 0;
                            for (int z = 0; z < stack.GetLength(0); z++)
                                for (int x = 0; x < stack.GetLength(2); x++)
                                {
                                    outArray[z, i, x] = predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    cc++;
                                }
                        }
                        else
                        {

                            cc = 0;
                            for (int z = 0; z < stack.GetLength(0); z++)
                                for (int x = 0; x < stack.GetLength(2); x++)
                                {
                                    mPredicted[z, i, x] += predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    mVariance[z, i, x] += predictedvar[cc];
                                    cc++;
                                }

                        }
                    }
                }
            }

            return outArray;
        }

        public float[, ,] filterKalmanZ(float[, ,] stack, float percentvar, float gain, bool SendToPool)
        {
            float[, ,] outArray = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];

            int width = stack.GetLength(1);
            int height = stack.GetLength(2);
            int dimension = stack.GetLength(1) * stack.GetLength(2);
            int stacksize = stack.GetLength(0);
            float[] stackslice = new float[dimension];
            float[] filteredslice = new float[dimension];
            float[] noisevar = new float[dimension];
            float[] average = new float[dimension];
            float[] predicted = new float[dimension];
            float[] predictedvar = new float[dimension];
            float[] observed = new float[dimension];
            float[] Kalman = new float[dimension];
            float[] corrected = new float[dimension];
            float[] correctedvar = new float[dimension];

            for (int i = 0; i < dimension; ++i)
            {
                noisevar[i] = percentvar;
            }

            unsafe
            {
                fixed (float* pStack = stack)
                {

                    // for (int i = 0; i < dimension; i++)
                    int cc = 0;
                    for (int x = 0; x < stack.GetLength(2); x++)
                        for (int y = 0; y < stack.GetLength(1); y++)
                        {
                            predicted[cc] = stack[0, y, x];// = toDouble(stack.getPixels(1), bitDepth);
                            cc++;
                        }

                    for (int i = 0; i < dimension; ++i)
                    {
                        predictedvar[i] = noisevar[i];
                    }

                    for (int i = 1; i < stack.GetLength(0); ++i)
                    {
                        // IJ.showProgress(i, stacksize);
                        //int sliceStart = dimension * (i);
                        // float d;
                        //for (int j = 0; j < dimension; j++)
                        //{
                        //    d = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);
                        //  //  stackslice[j] = pStack[sliceStart + j];// toDouble(stack.getPixels(i + 1), bitDepth);;
                        //    observed[j] = d;
                        //}

                        cc = 0;
                        for (int x = 0; x < stack.GetLength(2); x++)
                            for (int y = 0; y < stack.GetLength(1); y++)
                            {
                                observed[cc] = stack[i, y, x];// = toDouble(stack.getPixels(1), bitDepth);
                                cc++;
                            }

                        //= toDouble(stackslice, 64);
                        for (int k = 0; k < Kalman.Length; ++k)
                            Kalman[k] = predictedvar[k] / (predictedvar[k] + noisevar[k]);
                        for (int k = 0; k < predicted.Length; ++k)
                            predicted[k] = (float)(gain * predicted[k] + (1.0 - gain) * observed[k] + Kalman[k] * (observed[k] - predicted[k]));
                        for (int k = 0; k < predictedvar.Length; ++k)
                            predictedvar[k] = (float)(predictedvar[k] * (1.0 - Kalman[k]));
                        // predictedvar = correctedvar;
                        // predicted = corrected;
                        if (SendToPool == false)
                        {
                            cc = 0;
                            for (int x = 0; x < stack.GetLength(2); x++)
                                for (int y = 0; y < stack.GetLength(1); y++)
                                {
                                    outArray[i, y, x] = predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    cc++;
                                }
                        }
                        else
                        {

                            cc = 0;
                            for (int x = 0; x < stack.GetLength(2); x++)
                                for (int y = 0; y < stack.GetLength(1); y++)
                                {
                                    mPredicted[i, y, x] += predicted[cc];// = toDouble(stack.getPixels(1), bitDepth);
                                    mVariance[i, y, x] += predictedvar[cc];
                                    cc++;
                                }

                        }
                    }
                }
            }

            return outArray;
        }


        public float[, ,] filterKalman3D(float[, ,] stack, float percentvar, float gain)
        {
            mVariance = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];
            mPredicted = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];
            filterKalmanX(stack, percentvar, gain, true);
            filterKalmanY(stack, percentvar, gain, true);
            filterKalmanZ(stack, percentvar, gain, true);

            return mPredicted;
            //float[, ,] outArray = new float[stack.GetLength(0), stack.GetLength(1), stack.GetLength(2)];
            //unsafe
            //{
            //    fixed (float* pStack = stack)
            //    {

            //        float Kalman;
            //        float val;
            //        for (int z = 0; z < stack.GetLength(0); ++z)
            //            for (int x = 0; x < stack.GetLength(2); x++)
            //                for (int y = 0; y < stack.GetLength(1); y++)
            //                {
            //                    val = mPredicted[z, y, x] / 3;
            //                    Kalman = (mVariance[z, y, x] / (mVariance[z, y, x] + 3 * percentvar)) / 3;
            //                    outArray[z, y, x] = (float)(gain * val + (1.0 - gain) * stack[z, y, x] + Kalman * (stack[z, y, x] - val));
            //                }
            //    }
            //}

            //return outArray;
        }






        #region SR
        OnDemandImageLibrary PSF_Projection;
        Image<Gray, float> PSF_Bounds;
        int MaxIterations=4, NPSF=2, NImage=2;
        public void DeconvolveSmooth(int maxIterations, int nPSF, int nImage)
        {
            //the forward projections have a totally different set of problems than the normals
            Image<Gray, float> PSF = new Image<Gray, float>(100, 100);
            PSF = PSF.Add(new Gray(1d / Math.Sqrt(PSF.Width)));

            //bounds for the PPs
            PSF_Bounds = new Image<Gray, float>(100, 100);
            int hW = PSF_Bounds.Width / 2;
            double R;
            double sigma = 3;
            sigma = 2 / sigma / sigma;
            for (int xx = 0; xx < PSF_Bounds.Width; xx++)
                for (int yy = 0; yy < PSF_Bounds.Height; yy++)
                {
                    R = (Math.Pow(xx - hW, 2) + Math.Pow(yy - hW, 2));
                    PSF_Bounds.Data[yy, xx, 0] = (float)(1 * Math.Exp(-1 * (R * sigma)));
                }

            PSF_Projection = new OnDemandImageLibrary(Library.Count, true, @"c:\temp", false);

            for (int i = 0; i < Library.Count; i++)
            {
                PSF_Projection[i] = PSF_Bounds.Copy();
            }

            //now put the bounds further out
            sigma = 15;
            sigma = 2 / sigma / sigma;
            for (int xx = 0; xx < PSF_Bounds.Width; xx++)
                for (int yy = 0; yy < PSF_Bounds.Height; yy++)
                {
                    R = (Math.Pow(xx - hW, 2) + Math.Pow(yy - hW, 2));
                    PSF_Bounds.Data[yy, xx, 0] = (float)(1 * Math.Exp(-1 * (R * sigma)));
                }


            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count/2;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchDeconv(x));

        }

        private void BatchDeconv(int imageNumber)
        {
            try
            {
                var temp = DeconvolveImage(imageNumber, MaxIterations, NPSF, NImage);
                Library[imageNumber] = temp.Item1;
                Library[imageNumber + Library.Count/2] = temp.Item1.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                PSF_Projection[imageNumber] = temp.Item2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);

            }
        }

        private Tuple<Image<Gray, float>, Image<Gray, float>> DeconvolveImage(int imageNumber, int nMacroIter, int nPSFIter, int nImageIter)
        {
            var image = Library[imageNumber];
            var image2 = Library[(imageNumber + Library.Count / 2) % Library.Count].Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
            Image<Gray, float> PSF1 = PSF_Projection[imageNumber];
            Image<Gray, float> PSF2 = PSF_Projection[imageNumber];
            int sx = image.Width + PSF_Bounds.Width - 1;

            Image<Gray, float> X = new Image<Gray, float>(pos(samp2(image.Data, (int)sx)));
            //   float[, ,] xData = ; image.Copy();
            float[, ,] temp = null;

            for (int i = 0; i < nMacroIter; i++)
            {
                temp = PSF1.Copy().Data;
                temp.NormalizeArray();
                var t = obd2(X, image.Data, temp, new int[] { nPSFIter, nImageIter }, 1, false, PSF_Bounds.Data);
                X = t.Item1;
                PSF1 = t.Item2;

                temp = PSF2.Copy().Data;
                temp.NormalizeArray();
                t = obd2(X, image2.Data, temp, new int[] { nPSFIter, nImageIter }, 1, false, PSF_Bounds.Data);
                X = t.Item1;
                PSF2 = t.Item2;
            }
            X = new Image<Gray, float>(samp2(X.Data, image.Width)).Mul(image.Width);
            PSF_Projection[imageNumber] = PSF1;
            //  PSF = new Image<Gray, float>(temp);
            return new Tuple<Image<Gray, float>, Image<Gray, float>>(X, PSF1);
        }

      

        #region matlab fakes
        private int[] size(float[, ,] x)
        {
            return new int[] { x.GetLength(0), x.GetLength(1) };
        }
        private int floor(float x)
        {
            return (int)Math.Floor(x);
        }

        private int ceil(float x)
        {
            return (int)Math.Ceiling(x);
        }

        private int floor(double x)
        {
            return (int)Math.Floor(x);
        }
        private void error(object o)
        {
            Program.WriteLine(o.ToString());

        }
        private int isempty(object o)
        {
            if (o == null)
                return 1;
            else
                return 0;
        }
        private float[, ,] zeros(int[] size)
        {
            return new float[size[0], size[1], 1];
        }
        private float[, ,] zeros(int size)
        {
            return new float[size, size, 1];
        }
        private float[, ,] zeros(double size)
        {
            return new float[(int)size, (int)size, 1];
        }
        #endregion
        private Tuple<Image<Gray, float>, Image<Gray, float>> obd2(Image<Gray, float> x, float[, ,] y, float[, ,] f, int[] maxiter, double srf, bool regularize, float[, ,] PSFBounds)
        {

            double sf = f.GetLength(0);
            var sy = size(y);            //size of blurred image
            if (srf >= 1)
            {
                sy[0] = floor(srf * sy[0]);
                sy[1] = floor(srf * sy[1]);
                sf = floor(srf * sf);
            }
            else
            {
                if (srf < 1)
                    error("superresolution factor must be one or larger");
            }

            if (x != null)
            {
                var sx = size(x.Data);

                //estimate PSF with multiplicative updates
                obd_updatePSF(ref f, x.Data, y, maxiter[1], srf, regularize, PSFBounds);
                double fSum = f.SumArray();
                f.DivideInPlace((float)fSum);
                //Image<Gray, float> t = new Image<Gray, float>(f);
                //int w2 = t.Width;
                //f.NormalizeArray();
                x.Data.MultiplyInPlace((float)fSum);
            }
            else
            {
                //if ((isempty(f) == 1))
                {
                    f = zeros(sf);
                    int sf2 = (int)Math.Ceiling(sf / 2);
                    f[sf2, sf2, 0] = 1;   //a delta peak

                }

                var sx = sy[0] + sf - 1;

                //  var xData = pos(cnv2tp(f, y, srf));
                float[, ,] xData = pos(samp2(y, (int)sx));

                x = new Image<Gray, float>(xData);
                // f = origPSF.CopyThis();
                // f.NormalizeArray();
                return new Tuple<Image<Gray, float>, Image<Gray, float>>(x, new Image<Gray, float>(f));
            }

            float[, ,] data = x.Data;

            //improve true image x with multiplicative updates
            obd_update(ref data, f, y, maxiter[1], srf);

            var fY = y.MaxArray();
            var fX = x.Data.MaxArray();

            var w = fY - fX;

            return new Tuple<Image<Gray, float>, Image<Gray, float>>(x, new Image<Gray, float>(f));
        }


        //%%%%%%%%%%%%%%%%
        private void obd_updatePSF(ref float[, ,] f, float[, ,] x, float[, ,] y, int iters, double srf, bool regularize, float[, ,] psfBounds)
        {
            //depending on the value of sf the roles of f and x can be swapped
            var sf = size(f);
            var sy = size(y);    //for srf > 1, the low resolution y



            float[, ,] factor = zeros(f.GetLength(0));

            float tol = (float)(1e-10);

            if (regularize)
            {
                for (int i = 0; i < iters / 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var ytmp = pos(cnv2(x, f, sy[0]));
                        // ytmp = ytmp .* m;                 //deal with clipping
                        var nom = pos(cnv2tp(x, y, srf));
                        var denom = pos(cnv2tp(x, ytmp, srf));
                        nom.AddToArray(tol);
                        denom.AddToArray(tol);
                        factor.DivideToArray(nom, denom);
                        //  factor = reshape(factor, sf);
                        //  imagesc(factor);drawnow;
                        f.MultiplyInPlace(factor);
                    }

                    for (int xx = 0; xx < psfBounds.GetLength(0); xx++)
                        for (int yy = 0; yy < psfBounds.GetLength(1); yy++)
                        {
                            if (f[xx, yy, 0] > psfBounds[xx, yy, 0])
                                f[xx, yy, 0] = psfBounds[xx, yy, 0];

                        }
                }

            }
            else
            {
                for (int i = 0; i < iters; i++)
                {
                    var ytmp = pos(cnv2(x, f, sy[0]));
                    // ytmp = ytmp .* m;                 //deal with clipping
                    var nom = pos(cnv2tp(x, y, srf));
                    var denom = pos(cnv2tp(x, ytmp, srf));
                    nom.AddToArray(tol);
                    denom.AddToArray(tol);
                    factor.DivideToArray(nom, denom);
                    //  factor = reshape(factor, sf);
                    //  imagesc(factor);drawnow;
                    f.MultiplyInPlace(factor);
                }
            }

        }

        //%%%%%%%%%%%%%%%%
        private void obd_update(ref float[, ,] x1, float[, ,] f, float[, ,] y, int iters, double srf)
        {
            //depending on the value of sf the roles of f and x can be swapped
            var sf = size(x1);
            var sy = size(y);    //for srf > 1, the low resolution y



            float[, ,] factor = zeros(x1.GetLength(0));

            float tol = (float)(1e-10);
            for (int i = 0; i < iters; i++)
            {
                var ytmp = pos(cnv2(f, x1, sy[0]));
                // ytmp = ytmp .* m;                 //deal with clipping
                var nom = pos(cnv2tp(f, y, srf));
                var denom = pos(cnv2tp(f, ytmp, srf));
                nom.AddToArray(tol);
                denom.AddToArray(tol);
                factor.DivideToArray(nom, denom);
                //  factor = reshape(factor, sf);
                //  imagesc(factor);drawnow;
                x1.MultiplyInPlace(factor);
            }

        }

        //%%%%%%%%%%%%%%%%%
        private float[, ,] pos(float[, ,] x)
        {
            x.EnforcePositive();
            return x;
        }

        //%%%%%%%%%%%%%%%%%
        private float[, ,] cnv2slice(float[, ,] A, int[] i, int[] j)
        {
            float[, ,] aN = new float[i[1] - i[0], j[1], j[0]];
            int x = 0, y = 0;
            for (int I = i[0]; I < i[1]; I++)
            {
                for (int J = j[0]; J < j[1]; J++)
                {
                    aN[x, y, 0] = A[I, J, 0];
                    y++;
                }
                x++;
            }
            return aN;
        }



        //%%%%%%%%%%%%%%%%%
        private /*y*/ float[, ,] cnv2(float[, ,] x, float[, ,] f, int sy)
        {
            var sx = size(x);
            var sf = size(f);
            float[, ,] y = null;
            if (sx[0] >= sf[0]) //x is larger or equal to f
            {
                //perform convolution in Fourier space
                // y = ifft2(fft2(x) .* fft2(f, sx(1), sx(2)));
                y = MathFFTHelps.FFT_cnv2(x, f);
                //Image<Gray, float> t = new Image<Gray, float>(y);
                //  int w = t.Width;
                //y = cnv2slice(y, sf(1):sx(1), sf(2):sx(2));
            }
            else
            {
                if (sx[0] <= sf[0])  //x is smaller or equal than f
                {
                    y = cnv2(f, x, sy);
                }
                else
                    error("[cnv2.m] x must be at least as large as f or vice versa.");
            }

            if (sy > size(y)[0])
                error("[cnv2.m] size missmatch");


            if ((sy < size(y)[0]))
            {
                y = samp2(y, sy);   //downsample
                //  Image<Gray, float> t = new Image<Gray, float>(y);
                //  int w = t.Width;
            }
            return y;
        }

        //%%%%%%%%%%%%%%%%%
        private /* f*/ float[, ,] cnv2tp(float[, ,] x, float[, ,] y, double srf)
        {
            var sx = size(x);
            float[, ,] y2 = y;
            if (srf > 1)
            {
                y2 = samp2(y, floor(srf * size(y)[0]));    //upsample
            }
            var sy = size(y)[0];

            int sf;
            float[, ,] f = null;
            //perform the linear convolution in Fourier space
            if (sx[0] >= sy)
            {
                sf = sx[0] - sy + 1;
                f = MathFFTHelps.conv2(x, y, sf); //ifft2(conj(fft2(x)).*fft2(cnv2pad(y, sf)));
                //  Image<Gray, float> t = new Image<Gray, float>(f);
                //  int w = t.Width;
                // f = cnv2slice(tmp, 1:sf(1), 1:sf(2));
            }
            else
            {
                if (sx[0] <= sy)
                {
                    sf = sy + sx[0] - 1;
                    f = MathFFTHelps.CrossCorrelationFFT(y, x, sf);
                    //f = ifft2(conj(fft2(x, sf(1), sf(2))).*fft2(cnv2pad(y, sx), sf(1), sf(2)));}
                }
                else  //x and y are incomparable
                    error("[cnv2.m] x must be at least as large as y or vice versa.");
            }
            //f = real(f);
            return f;
        }



        //        //%%%%%%%%%%%%%
        //        private /* B =*/float[, ,] cnv2pad(float[, ,] A, int sf){
        ////PAD with zeros from the top-left
        //i = sf(1);  j = sf(2);
        //[rA, cA] = size(A);
        //float[,,] B = zeros(rA+i-1, cA+j-1);
        //B(i:end, j:end) = A;
        //return    B;}

        //%%%%%%%%%%%%
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
            //sx = size(x);
            // downsample by factor srf
            //y = sampmat(sy(1), sx(1)) * x * sampmat(sy(2), sx(2))';
            // return null;
        }


        private float[, ,] samp2Flip(float[, ,] x, int sy)
        {

            float[, ,] arrayOutf = new float[sy, sy, 1];
            int startI = (int)(Math.Floor((x.GetLength(0) - sy) / 2d));

            if (startI >= 0)
            {
                int top = arrayOutf.GetLength(0) - 1;
                for (int i = 0; i < arrayOutf.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayOutf.GetLength(1); j++)
                    {
                        arrayOutf[top - i, j, 0] = (float)(x[i + startI, j + startI, 0]);
                    }
                }

                return arrayOutf;
            }
            else
            {
                int top = x.GetLength(0) - 1;
                for (int i = 0; i < x.GetLength(0); i++)
                {
                    for (int j = 0; j < x.GetLength(1); j++)
                    {
                        arrayOutf[i - startI, j - startI, 0] = (float)(x[top - i, j, 0]);
                    }
                }

                return arrayOutf;

            }
            //sx = size(x);
            // downsample by factor srf
            //y = sampmat(sy(1), sx(1)) * x * sampmat(sy(2), sx(2))';
            // return null;
        }

        //%%%%%%%%%%%%%
        private double[,] sampmat(int m, int n)
        {
            return null;
            //D = kron(speye(m), ones(n, 1)') * kron(speye(n), ones(m, 1))/m;
        }
        //%%%%% } OF obd.m %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%



        #endregion

    }
}
