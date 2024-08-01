using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILNumerics;
using Emgu;
using Emgu.CV.Structure;
using Emgu.CV;
using MathLibrary;

namespace ImageProcessing._2D
{
    public class TV : ILMath
    {
        public ILArray<double> TVDenoise(ILArray<double> I0, int maxOutIter,  int innerIter)
        {
            var I = Deconvolution.ConvertToImage(I0);

            var J_cv = I.SmoothMedian(3);
            var smooth = J_cv.Sub(I);
            float std = smooth.Data.StandardDeviation();
            float var_n = std * std;

            ILArray<double> J = Deconvolution.ConvertToArray(J_cv);// I0[full, full];// 

            //% params
            float ep_J = 0.01f;// % minimum mean change in image J
            ILArray<double> lam = 0;
            ILArray<double> J_old = J[full, full];
            float dt = .1f;
            float eps = 1;

            ILArray<double> converge = 1000;
            ILArray<double> converge_old = 99;
            int i = 0;
            while (converge > ep_J && converge[0] != converge_old[0] && i < maxOutIter)//,  % iterate until convergence
            {
                converge_old = converge[0];
                J_old = J[full, full];
                J = tv(J, innerIter, dt, eps, lam, I0, 0);   //  % scalar lam
                if (i%5==0)
                   lam = calc_lam(J, I0, var_n, eps);// % update lambda (fidelity term) 

                converge = sumall(abs(J - J_old)) / I.Data.LongLength;

                if (isnan(converge))
                {
                  //  System.Diagnostics.Debug.Print("");
                    J = J_old;
                    break;
                }
                i++;
            }


            //var smoothed = Deconvolution.ConvertToImage(J);

            //ILArray<double> diff = J - I0;
            //var smoothedif = Deconvolution.ConvertToImage(diff);

            //i = smoothedif.Width;

            return J;
        }

        public Image<Gray, float> TVDenoise(Image<Gray, float> I, int iter)
        {
            var smooth = I.SmoothGaussian(7).Sub(I);
            float std = smooth.Data.StandardDeviation();
            float var_n = std *std;

            ILArray<double> I0 = Deconvolution.ConvertToArray(I);

          //  ILMatFile mat = new ILMatFile(I0);
           // mat.Write(@"c:\temp\testImage.mat");

            ILArray<double> J = I0[full,full];
            //% params
            float ep_J = 0.01f;// % minimum mean change in image J
            ILArray<double> lam = 0;
            ILArray<double> J_old= J[full, full];
            float dt = .1f;
            float eps = 1;
           
            ILArray<double> converge = 1000;
            ILArray<double> converge_old=99;
            int i=0;
            while (converge > ep_J && converge[0] != converge_old[0] && i<140)//,  % iterate until convergence
            {
                converge_old = converge[0];
                J_old = J[full, full];
                J = tv(J, iter, dt, eps, lam, I0, 0);   //  % scalar lam
                //if (i%5==0)
                    lam = calc_lam(J, I0, var_n, eps)*5;// % update lambda (fidelity term) 

                converge = sumall(abs(J - J_old)) / I.Data.LongLength;
                i++;
            }


            var smoothed = Deconvolution.ConvertToImage(J);

            ILArray<double> diff = J - I0;
            var smoothedif = Deconvolution.ConvertToImage(diff);

            i = smoothedif.Width;

            return smoothed;
        }

        private ILArray<double> calc_lam(ILArray<double> I, ILArray<double> I0, double var_n, double ep)
        {
            //%% private function: calc_lam (by Guy Gilboa).
            //%% calculate scalar fidelity term of the Total-Variation
            //%% input: current image, original noisy image, noise variance, epsilon
            //%% output: lambda
            //%% example: lam=calc_lam(I,I0,var_n,ep)

            ILArray<double> n = size(I);
            ILArray<double> ny = n[0];
            ILArray<double> nx = n[1];
            double ep2 = ep * ep;

            ILArray<double> I_x, I_y, I_xx, I_yy, Dp, Dm, I_xy, Num, Den, I_t;

            ILArray<double> r_x = counter(1, 1, nx);
            r_x[end] = nx - 1;

            ILArray<double> r_y = counter(1, 1, ny);
            r_y[end] = ny - 1;

            ILArray<double> ir_x = counter(-1, 1, nx);
            ir_x[0] = 0;

            ILArray<double> ir_y = counter(-1, 1, ny);
            ir_y[0] = 0;


            //%% do iterations
            //% estimate derivatives
            I_x = (I[full, r_x] - I[full, ir_x]) / 2;
            I_y = (I[r_y, full] - I[ir_y, full]) / 2;
            I_xx = I[full, r_x] + I[full, ir_x] - 2 * I;
            I_yy = I[r_y, full] + I[ir_y, full] - 2 * I;
            Dp = I[r_y, r_x] + I[ir_y, ir_x];//Dp = I([2:ny ny],[2:nx nx])+I([1 1:ny-1],[1 1:nx-1]);
            Dm = I[ir_y, r_x] + I[r_y, ir_x];//Dm = I([1 1:ny-1],[2:nx nx])+I([2:ny ny],[1 1:nx-1]);
            I_xy = (Dp - Dm) / 4;
            //% compute flow
            Num = multiplyElem(I_xx, (ep2 + pow(I_y, 2))) -
             2 * multiplyElem(multiplyElem(I_x, I_y), I_xy) +
             multiplyElem(I_yy, (ep2 + pow(I_x, 2)));
            Den = pow((ep2 + pow(I_x, 2) + pow(I_y, 2)), (3d / 2));
            I_t = multiplyElem(Num, Den);


            ILArray<double> Div = divide(Num, Den);
            //  %%% fidelity term
            return mean(mean(multiplyElem(Div, (I - I0)))) / var_n;//  %% 
        }

        public ILArray<double> tv(ILArray<double> I, int iter, float dt, double ep, ILArray<double> lam, ILArray<double> I0, float C)
        {


            //if ~exist('ep')
            //   ep=1;
            //end
            //if ~exist('dt')
            //   dt=ep/5;  % dt below the CFL bound
            //end
            //if ~exist('lam')
            //   lam=0;
            //end
            //if ~exist('I0')
            //    I0=I;
            //end
            //if ~exist('C')
            //    C=0;
            //end
            ILArray<double> n = size(I);
            ILArray<double> ny = n[0];
            ILArray<double> nx = n[1];
            double ep2 = ep * ep;

            ILArray<double> I_x, I_y, I_xx, I_yy, Dp, Dm, I_xy, Num, Den, I_t;

            ILArray<double> r_x = counter(1, 1, nx);
            r_x[end] = nx - 1;

            ILArray<double> r_y = counter(1, 1, ny);
            r_y[end] = ny - 1;

            ILArray<double> ir_x = counter(-1, 1, nx);
            ir_x[0] = 0;

            ILArray<double> ir_y = counter(-1, 1, ny);
            ir_y[0] = 0;

            ILArray<double> I2 = I[full, full];
            for (int i = 0; i < iter; i++)
            {
                //%% do iterations
                //% estimate derivatives
                I_x = (I[full, r_x] - I[full, ir_x]) / 2;
                I_y = (I[r_y,full] - I[ir_y, full]) / 2;
                I_xx = I[full, r_x] + I[full, ir_x] - 2 * I;
                I_yy = I[r_y, full] + I[ir_y, full] - 2 * I;
                Dp = I[r_y, r_x] + I[ir_y, ir_x];//Dp = I([2:ny ny],[2:nx nx])+I([1 1:ny-1],[1 1:nx-1]);
                Dm = I[ir_y, r_x] + I[r_y, ir_x];//Dm = I([1 1:ny-1],[2:nx nx])+I([2:ny ny],[1 1:nx-1]);
                I_xy = (Dp - Dm) / 4;
                //% compute flow
                Num = multiplyElem(I_xx, (ep2 + pow(I_y, 2))) -
                 2 * multiplyElem(multiplyElem(I_x, I_y), I_xy) +
                 multiplyElem(I_yy, (ep2 + pow(I_x, 2)));
                Den = pow((ep2 + pow(I_x, 2) + pow(I_y, 2)), (3d / 2));
                I_t = divide(Num, Den) + multiplyElem(lam, (I0 - I + C));
                I2 = I2 + dt * I_t;//  %% evolve image by dt
            }

            return I2;
        }
    }
}
