using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILNumerics;
using Emgu;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Threading;

namespace ImageProcessing._2D
{
    public class Deconvolution : ILMath
    {
        static SemaphoreSlim slimmer = new SemaphoreSlim(10, 12);

        public Image<Gray, float> obDeconvolve(Image<Gray, float>[] images, int PSFSize, float superResolutionFactor)
        {
            Image<Gray, float> im1;
            using ( ILNumerics.ILScope.Enter())
            {

                ILArray<double> sf = new double[] { PSFSize, PSFSize };// [150, 150];       % size of the PSF
                ILArray<double> maxiter = new double[] { 41, 2 };//   % number of iterations for f and x

                float srf = superResolutionFactor;     //      % superresolution factor

                //% intially there is no x
                //% iterate over all images
                ILArray<double>[] y = new ILArray<double>[images.Length];

                double[,] xD = new double[images[0].Height, images[0].Width];
                double v;
                for (int i = 0; i < images.Length; i++)
                {
                    //% load the next observed image
                    double[,] yD = new double[images[i].Height, images[i].Width];
                    for (int yy = 0; yy < images[i].Height; yy++)
                        for (int xx = 0; xx < images[i].Width; xx++)
                        {
                            v = images[i].Data[yy, xx, 0];
                            yD[yy, xx] = v;
                            xD[yy, xx] += v / 2;
                        }

                    y[i] = yD;
                    y[i] = y[i] + 2500;
                }

                ILArray<double> sy = size(y[0]);
                ILArray<double> f = zeros(new ILSize(sf));
                ILArray<double> sf2 = ceil(sf / 2);
                f[sf2[0], sf2[1]] = 1;   // a delta peak
                f = f / sumall(f);
                ILArray<double> sx = sy + sf - 1;
                ILArray<double> x = pos(cnv2tp(f, xD, srf));
                f = empty<double>();
                if (srf != 1)
                    x = null;

                for (int i = 0; i < images.Length; i++)
                {
                    ILRetArray<double>[] xO = obd(x, y[i], sf, maxiter, float.PositiveInfinity, srf, false);
                    x.a = xO[0];
                    f.a = xO[1];
                }

                //ILArray<double> x2 = pos(cnv2tp(f, xD, srf));
                //for (int i = images.Length - 1; i >= 0; i--)
                //{
                //    x2 = obd(x2, y[i], sf, maxiter, float.PositiveInfinity, srf);
                //}
                sy = size(y[0]) * srf;
                sx = (size(x) - sy) / 2;
                x = x[r(sx[0], sx[0] + sy[0] - 1), r(sx[1], sx[1] + sy[1] - 1)];


                ILArray<double> sF = size(x);
                ILArray<double> ia = ILMath.empty<double>(), ib = ILMath.empty<double>(), ja = ILMath.empty<double>(), jb = ILMath.empty<double>();

                ia = meshgrid(linspace<double>(sF[0] / -2, sF[0] / 2, sF[0]), linspace<double>(sF[1] / -2, sF[1] / 2, sF[0]), ib);

                ILArray<double> R = abs(pow(multiplyElem(ia, ia) + multiplyElem(ib, ib), .5));
                ILArray<double> R2 = (sF[0] / 2 * .75 - R)/25;

                ILArray<double> xLimits = (R2 / pow(1 + multiplyElem(R2, R2), .8) + 1) / 2;


                x = multiplyElem(x, xLimits);

                im1 = ConvertToImage(x);//.PyrDown();
            }
            // var im2 = ConvertToImage(x2);
            // var im3 = im1.Add(im2);
            return im1;

        }

        internal static ILRetArray<double> ConvertToArray(Image<Gray, float> y)
        {
            double v;

            double[,] yD = new double[y.Height, y.Width];
            for (int yy = 0; yy < y.Height; yy++)
                for (int xx = 0; xx < y.Width; xx++)
                {
                    v = y.Data[yy, xx, 0];
                    yD[yy, xx] = v;
                }

            ILArray<double> x = empty<double> ();
            x.a = yD;
            return x;

        }
        internal static Image<Gray, float> ConvertToImage(ILInArray<double> x)
        {
            using ( ILNumerics.ILScope.Enter(x))
            {
                ILArray<double> s = size(x);


                Image<Gray, float> xO = new Image<Gray, float>((int)s[0], (int)s[1]);

                //double[,] data = new double[xO.Height, xO.Width];
                double[] data = new double[xO.Height * xO.Width];
                x.ExportValues(ref data);
                //Buffer.BlockCopy(x.GetArrayForRead(), 0, data, 0, Buffer.ByteLength(data));

                int cc = 0;
                for (int yy = 0; yy < xO.Height; yy++)
                    for (int xx = 0; xx < xO.Width; xx++)
                    {
                        xO.Data[yy, xx, 0] = (float)data[cc];
                        cc++;
                    }

                return xO;
            }
        }

        private ILRetArray<double>[] /* [x, f] =*/ obd(ILInArray<double> x_, ILInArray<double> y_, ILInArray<double> sf_, ILInArray<double> maxiter_, float clipping, float srf, bool iterate)
        {
            // Copyright (C) 2010 by Michael Hirsch & Stefan Harmeling.
           //  using (ILScope.Enter(x_, y_,  sf_, maxiter_))
            {
                ILArray<double> x = check(x_), y = check(y_), sf = check(sf_), maxiter = check(maxiter_);
                ILArray<double> sumf;
                ILArray<double> sy = size(y);            // size of blurred image   
                if (srf > 1)
                {
                    sy = floor(srf * sy);
                    sf = floor(srf * sf);
                }
                else
                {
                    if (srf < 1)
                    {
                        throw new Exception("superresolution factor must be one or larger");
                    }
                }

                ILArray<double> f;
                if (x != null)
                {
                    // check sizes
                    ILArray<double> sx = size(x);
                    if (any(sf != sx - sy + 1))
                        throw new Exception("size missmatch");

                    //  if (isempty(f1))
                    {
                        // intialize PSF
                        f = norm(y) / norm(x);
                        f = f * ones(new ILSize(sf)) / sqrt(prod(sf));
                    }
                    //  else
                    {
                        //     f = f1;
                    }
                    // estimate PSF with multiplicative updates
                    f = obd_update(f, x, y, maxiter[0], clipping, srf, RegularizationType.Tikh, 5);


                     sumf = sumall(f);
                    f = divide(f, sumf);                         // normalize f
                    x = sumf * x;                         // adjust x as well
                    // sx = size(x);
                }
                else
                {
                    f = zeros(new ILSize(sf));
                    ILArray<double> sf2 = ceil(sf / 2);
                    f[sf2[0], sf2[1]] = 1;   // a delta peak
                    f = f / sumall(f);
                    ILArray<double> sx = sy + sf - 1;
                    x.a = pos(cnv2tp(f, y, srf));
                    //ConvertToImage(x);

                    return new ILRetArray<double>[] { x, empty<double>() };
                }

               
                // improve true image x with multiplicative updates
                if (iterate == false)
                {
                    x.a = obd_update(x, f, y, maxiter[1], clipping, srf, RegularizationType.TV, 1);
                   // x.a = obd_update(x, f, y, 1, clipping, srf, RegularizationType.None, 1);
                }
                else
                {
                    for (int i = 0; i < maxiter[0] / 2; i++)
                    {
                       // using (ILScope.Enter())
                        {
                            f.a = obd_update(f, x, y, 2, clipping, srf, RegularizationType.None, 1);
                            sumf.a = sumall(f);
                            f.a = divide(f, sumf);                         // normalize f
                            x.a = sumf * x;                         // adjust x as well
                            x.a = obd_update(x, f, y, 2, clipping, srf, RegularizationType.None, 1);
                        }
                    }
                }
                return new ILRetArray<double>[] {  x, f };
            }
        }


        private enum RegularizationType
        {
            None, Tikh, Median, TV
        }

        TV tv = new TV();
        ILArray<double> PSFLimits = null;

        private ILArray<double> /*f =*/ obd_update(ILArray<double> f, ILArray<double> x, ILArray<double> y, ILArray<double> maxiter, float clipping, ILArray<double> srf, RegularizationType reg, int regFreq)
        {
            // dep}ing on the value of sf the roles of f and x can be swapped
            ILArray<double> sf = size(f);
            ILArray<double> sy = size(y);    // for srf > 1, the low resolution y
            //  ILLogical m = (y < clipping);     // where do we have clipping
            //ILArray<double>  y = multiplyElem( y , m);                      // deal with clipping

            for (int i = 0; i < maxiter; i++)
            {
                ILArray<double> ytmp = pos(cnv2(x, f, sy));
                //   ytmp = multiplyElem( ytmp , m);                 // deal with clipping
                ILArray<double> nom = pos(cnv2tp(x, y, srf));
                ILArray<double> denom = pos(cnv2tp(x, ytmp, srf));
                float tol = 1e-10f;
                ILArray<double> factor = divide((nom + tol), (denom + tol));
                factor = reshape(factor, new ILSize(sf));
                f = multiplyElem(f, factor);

                if (i % regFreq == 0 && i > maxiter / 2 - 1)
                {
                    switch (reg)
                    {
                        case RegularizationType.None:
                            break;
                        case RegularizationType.Tikh:
                         //   f = ConvertToArray(ConvertToImage(f).SmoothMedian(3));
                            f = ILMath.pow(f, 1.1);
                            if (PSFLimits == null)
                            {
                                ILArray<double> sF = size(f);
                                ILArray<double> ia = ILMath.empty<double>(), ib = ILMath.empty<double>(), ja = ILMath.empty<double>(), jb = ILMath.empty<double>();

                                ia = meshgrid(linspace<double>(sF[0] / -2, sF[0] / 2, sF[0]), linspace<double>(sF[1] / -2, sF[1] / 2, sF[0]), ib);

                                ILArray<double> R = abs(pow(multiplyElem(ia, ia) + multiplyElem(ib, ib), .5));
                                ILArray<double> R2 = (sF[0] / 2 * .75 - R) / 25;

                                PSFLimits = (R2 / pow(1 + multiplyElem(R2, R2), .5) + 1) / 2;
                            }

                            f = multiplyElem(f, PSFLimits);

                            ILArray<double> sumf = sumall(f);
                            f = divide(f, sumf);
                            break;
                        case RegularizationType.Median:
                            f = ConvertToArray(ConvertToImage(f).SmoothMedian(3));
                            break;
                        case RegularizationType.TV:
                            f = tv.TVDenoise(f, 2, 1);
                            break;
                    }
                }
            }
            return f;
        }

        //////////////////////////////////
        private ILArray<double> pos(ILArray<double> x, float epsilon)
        {
            ILArray<double> idx = find(x < 0);
            x[idx] = 0;
            return x;
        }

        private ILArray<double> pos(ILArray<double> x)
        {
            ILArray<double> idx = find(x < 0);
            x[idx] = 0;
            return x;
        }

        //////////////////////////////////
        private ILArray<double> cnv2slice(ILArray<double> A, ILBaseArray i, ILBaseArray j)
        {
            A = A[i, j];
            return A;
        }

        private ILArray<double> cnv2slice(ILArray<double> A, string i, string j)
        {
            A = A[i, j];
            return A;
        }

        //////////////////////////////////
        private ILArray<double> cnv2(ILArray<double> x, ILArray<double> f, ILArray<double> sy)
        {
            ILArray<double> sx = size(x);
            ILArray<double> sf = size(f);
            ILArray<double> y;
            if (all(sx >= sf))
            {   // x is larger or equal to f
                // perform convolution in Fourier space

                // ILArray<double> padded = cnv2pad(f, sy);
                slimmer.Wait();


                ILArray<complex> step1 = fft2(f, (int)sx[0], (int)sx[1]);
                ILArray<complex> step2 = fft2(x);
                ILArray<complex> step3 = multiplyElem(step2, step1);
                ILArray<complex> step4 = ifft2(step3);

                //ILNumerics.Native.ILMKLFFT mkl = new ILNumerics.Native.ILMKLFFT();
                //ILArray<complex> step1 = mkl.FFTForward(padded, 2);// fft2(f, (int)sx[0], (int)sx[1]);
                //ILArray<complex> step2 = mkl.FFTForward(x, 2);// fft2(x);
                //ILArray<complex> step3 = multiplyElem(step2, step1);
                //ILArray<complex> step4 = mkl.FFTBackward(step3, 2);// ifft2(step3);

                //mkl.FreePlans();

                slimmer.Release();
                y = real(step4);
                y = cnv2slice(y, (int)(sf[0] - 1) + ":" + (int)(sx[0] - 1), (int)(sf[1] - 1) + ":" + (int)(sx[1] - 1));
            }
            else
            {
                if (all(sx <= sf))
                {
                    // x is smaller or equal than f
                    y = cnv2(f, x, sy);
                }
                else
                {
                    // x and f are incomparable
                    throw new Exception("[cnv2.m] x must be at least as large as f or vice versa.");
                }
            }



            if (any(sy < size(y)))
            {
                // y = samp2(y, sy);   // downsample
                var t = ConvertToImage(y);
                t = t.PyrDown();
                y = ConvertToArray(t);
            }

            return y;
        }

        ////////////////////////////////////
        private ILArray<double> cnv2tp(ILArray<double> x, ILArray<double> y, ILArray<double> srf)
        {
            ILArray<double> sx = size(x);
            if (srf > 1)
            {
                var t = ConvertToImage(y);
                t = t.PyrUp();
                y = ConvertToArray(t);
                // y = samp2(y, floor(srf * size(y)));    // upsample
            }
            ILArray<double> sy = size(y);
            ILArray<double> f = null;
            // perform the linear convolution in Fourier space
            if (all(sx >= sy))
            {
                ILArray<double> sf = sx - sy + 1;

                slimmer.Wait();

                ILNumerics.Native.ILMKLFFT mkl = new ILNumerics.Native.ILMKLFFT();
                ILArray<complex> step1 = mkl.FFTForward(cnv2pad(y, sf), 2);// fft2(f, (int)sx[0], (int)sx[1]);
                ILArray<complex> step2 = conj(mkl.FFTForward(x, 2));// fft2(x);
                ILArray<complex> step3 = multiplyElem(step2, step1);
                ILArray<complex> step4 = mkl.FFTBackward(step3, 2);// ifft2(step3);

                mkl.FreePlans();

                //ILArray<complex> t1 = fft2(cnv2pad(y, sf));
                //ILArray<complex> t2 = conj(fft2(x));
                //ILArray<double> t3 = real(ifft2(multiplyElem(t2, t1)));
                slimmer.Release();
                ILArray<double> t3 = real(step4);

                f = cnv2slice(t3, "0:" + (int)(sf[0] - 1), "0:" + (int)(sf[1] - 1));

                // f = cnv2slice(ifft2(conj(fft2(x)).*fft2(cnv2pad(y, sf))), 1:sf(1), 1:sf(2));
            }
            else
            {
                if (all(sx <= sy))
                {
                    ILArray<double> sf = sy + sx - 1;

                    slimmer.Wait();
                    f = real(ifft2(multiplyElem(conj(fft2(x, (int)sf[0], (int)sf[1])), fft2(cnv2pad(y, sx), (int)sf[0], (int)sf[1]))));
                    slimmer.Release();
                }
                else
                {  // x and y are incomparable
                    throw new Exception("[cnv2.m] x must be at least as large as y or vice versa.");
                }
            }
            // ILArray<double> f2 = real(f);
            return f;
        }

        ////////////////////////////
        private ILArray<double> cnv2pad(ILArray<double> A, ILArray<double> sf)
        {
            // PAD with zeros from the top-left
            ILArray<double> rc = size(A);
            ILArray<double> B = zeros(new ILSize(rc[0] + sf[0] - 1, rc[1] + sf[1] - 1));


            // ILArray<double> C = B[r((int)sf[0]-1, end), r((int)sf[1]-1, end)];
            B[r((int)sf[0] - 1, end), r((int)sf[1] - 1, end)] = A;
            return B;
        }

        ////////////////////////////
        private ILArray<double> samp2(ILArray<double> x, ILArray<double> sy)
        {
            ILArray<double> sx = size(x);
            // downsample by factor srf
            //y = sampmat(sy(1), sx(1)) * x * sampmat(sy(2), sx(2))';

            ILArray<double> y = sampmat((int)sy[0], (int)sx[0]) * x * sampmat((int)sy[1], (int)sx[1]).T;
            return y;
        }

        ////////////////////////////
        private ILArray<double> sampmat(int m, int n)
        {
            ILArray<double> D = kron(eye(m, m), ones(n, 1).T) * kron(eye(n, n), ones(m, 1)) / m;
            return D;
        }

        private ILArray<double> kron(ILArray<double> A, ILArray<double> B)
        {
            ILArray<double> ma = size(A);
            ILArray<double> mb = size(B);


            //[ma,na] = size(A);
            //[mb,nb] = size(B);

            // [ia,ib] = meshgrid(1:ma,1:mb);
            //[ja,jb] = meshgrid(1:na,1:nb);
            ILArray<double> ia = ILMath.empty<double>(), ib = ILMath.empty<double>(), ja = ILMath.empty<double>(), jb = ILMath.empty<double>();

            ia = meshgrid(linspace<double>(0, ma[0] - 1, ma[0]), linspace<double>(0, mb[0] - 1, mb[0]), ib);
            ja = meshgrid(linspace<double>(0, ma[1] - 1, ma[1]), linspace<double>(0, mb[1] - 1, mb[1]), jb);
            ILArray<double> K = multiplyElem(A[ia, ja], B[ib, jb]);
            return K;
        }
    }
}

