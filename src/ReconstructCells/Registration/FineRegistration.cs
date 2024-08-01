using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using MathLibrary;
using MathLibrary.FFT;

namespace ReconstructCells.Registration
{
    public class FineRegistration : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;

        OnDemandImageLibrary TargetImages;
        double framesPerTarget;
        int Scale = 1;
        bool Halfsies = false;
        bool CutRegistered = false;

        #region Properties

        #region Set

        public void setTargets(OnDemandImageLibrary targetImages)
        {
            TargetImages = targetImages;
        }

        public void setDoHalf(bool doHalf)
        {
            Halfsies = doHalf;
        }

        /// <summary>
        /// Must be a factor of 2
        /// </summary>
        /// <param name="scale"></param>
        public void setAlignmentScale(int scale)
        {
            Scale = scale;
        }

        public void setCutRegistered(bool cutRealigned)
        {

            CutRegistered = cutRealigned;
        }
        #endregion

        #region Get


        #endregion

        #endregion

        #region Code
        private void BatchFindCellsUnscaled(int imageNumber)
        {
            double[] min, max;
            
            double U = (double)imageNumber / framesPerTarget;
            int index = (int)Math.Floor(U);
            double u = U - Math.Floor(U);

            Image<Gray,float> combined;
            
            if (framesPerTarget ==1)
                combined = TargetImages[imageNumber];
            else
                combined = TargetImages[index].AddWeighted(TargetImages[index + 1], u, 1 - u, 0);

            combined = combined.Mul(1d / combined.GetAverage().Intensity);

            var image2 = Library[imageNumber].Mul(Math.Abs(1d / Library[imageNumber].GetAverage().Intensity));


            const int padding = 15;
            var image5 = ImageProcessing._2D.ImageManipulation.CopyROI(combined, new Rectangle(-padding, -padding, combined.Width + 2 * padding, combined.Height + 2 * padding));
            Image<Gray, float> image3 = image5.MatchTemplate(image2, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCORR_NORMED );

            Point[] point1, point2;
            image3.MinMax(out min, out max, out point1, out point2);
            Point point3 = new Point();
            point3.X = point2[0].X - image3.Width / 2;
            point3.Y = point2[0].Y - image3.Height / 2;
            int sX = point3.X * -1-1;
            int sY = point3.Y * -1-1;


            //image3 = image2.CopyBlank();
            //MathFFTHelps.FFT_cnv2(combined.Data, image2.Data, image3.Data, 1);

            //image3 = image3.SmoothGaussian(5);

            //image3.MinMax(out min, out max, out pm, out pM);

            //int sX = -1 * (pM[0].X - image3.Width / 2);
            //int sY = -1 * (pM[0].Y - image3.Height / 2);

            mPassData.Locations[imageNumber].CellCenter.X += sX;
            mPassData.Locations[imageNumber].CellCenter.Y += sY;

            if (CutRegistered)
            {
                const int trim = 0;
                image3 = Library[imageNumber];
                Rectangle roi = new Rectangle(sX + trim, sY + trim, image3.Width - trim * 2, image3.Height - trim * 2);
                image3 = ImageProcessing._2D.ImageManipulation.CopyROI(Library[imageNumber], roi);
                Library[imageNumber] = image3;
            }
        }

        private void BatchFindCells(int imageNumber)
        {
            double[] min, max;
            Point[] pm, pM;

            double U = (double)imageNumber / framesPerTarget;
            int index = (int)Math.Floor(U);
            double u = U - Math.Floor(U);

            var combined = TargetImages[index].AddWeighted(TargetImages[index + 1], u, 1 - u, 0).SmoothGaussian(5);

            combined = combined.Mul(1d / combined.GetAverage().Intensity);

            var image2 = Library[imageNumber].Mul(1d / Library[imageNumber].GetAverage().Intensity);

            Image<Gray, float> image3 = new Image<Gray, float>(image2.Width * Scale, image2.Height * Scale);

            int padding = Scale;
            MathFFTHelps.FFT_cnv2(combined.Data, image2.Data, image3.Data, padding);

            var image4 = image3.SmoothGaussian(5);
            image4.MinMax(out min, out max, out pm, out pM);

            float sX = (-1 * (pM[0].X / (float)padding - image2.Width / 2f));
            float sY = (-1 * (pM[0].Y / (float)padding - image2.Height / 2f));

            mPassData.Locations[imageNumber].CellCenter.X += sX;
            mPassData.Locations[imageNumber].CellCenter.Y += sY;

            Rectangle roi = new Rectangle((int)Math.Floor(sX + 10), (int)Math.Floor(sY + 10), image2.Width - 20, image2.Height - 20);

            image2 = ImageProcessing._2D.ImageManipulation.CopyROI(Library[imageNumber], roi);

            float uX = (float)(sX - Math.Floor(sX));
            float uY = (float)(sY - Math.Floor(sY));

            if (uX != 0 || uY != 0)
            {
                float uX1 = (1 - uX), uY1 = (1 - uY);
                float vX, vX1;
                image3 = image2.CopyBlank();
                for (int x = 1; x < image2.Width; x++)
                {
                    int xP = x - 1;
                    for (int y = 1; y < image2.Height; y++)
                    {
                        vX = image2.Data[y - 1, xP, 0] * uY + image2.Data[y, xP, 0] * uY1;
                        vX1 = image2.Data[y - 1, x, 0] * uY + image2.Data[y, x, 0] * uY1;
                        image3.Data[y - 1, xP, 0] = vX * uX + vX1 * uX1;
                    }
                }
                image2 = image3;
            }

            Library[imageNumber] = image2;
        }

        private void BatchFindCellsO(int imageNumber)
        {
            double U = (double)imageNumber / framesPerTarget;
            int index = (int)Math.Floor(U);
            double u = U - Math.Floor(U);

            var combined = TargetImages[index].AddWeighted(TargetImages[index + 1], u, 1 - u, 0);

            PointF shift = new PointF();
            var corrected = ImageProcessing._2D.MotionRegistration.PhaseCorrelation(combined, Library[imageNumber], out shift);

            mPassData.Locations[imageNumber].CellCenter.X += (int)shift.X;
            mPassData.Locations[imageNumber].CellCenter.Y += (int)shift.Y;

            Rectangle ROI = new Rectangle(0, 0, corrected.Width, corrected.Height);
            Library[imageNumber] = ImageProcessing._2D.ImageManipulation.CopyROI(corrected, ROI); // corrected.Copy();
        }

        //public void RegisterCellHalf(Image<Gray, float>[] TargetImages)
        //{
        //    this.TargetImages = TargetImages;

        //    framesPerTarget = ((double)Library.Count / 2 / (TargetImages.Length - 1));

        //    var t = TargetImages[0].SmoothGaussian(11);

        //    for (int i = 0; i < TargetImages.Length; i++)
        //    {
        //        TargetImages[i] = TargetImages[i].SmoothGaussian(11);
        //    }

        //    //now that the cell positions are estimated, fill in all the other cells
        //    ParallelOptions po = new ParallelOptions();
        //    po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

        //    int numberOfImages = Library.Count / 2;

        //    Parallel.For(0, numberOfImages, po, x => BatchFindCells(x));
        //}

        private void BatchFindCellsOtherHalf(int imageNumber)
        {
            imageNumber += (Library.Count / 2);
            double U = (double)imageNumber / framesPerTarget;
            int index = (int)Math.Floor(U);
            double u = U - Math.Floor(U);

            var combined = TargetImages[(index) % TargetImages.Count].AddWeighted(TargetImages[(index + 1) % TargetImages.Count], u, 1 - u, 0);
            PointF shift = new PointF();
            var corrected = ImageProcessing._2D.MotionRegistration.PhaseCorrelation(combined, Library[imageNumber], out shift);

            mPassData.Locations[imageNumber].CellCenter.X += (int)shift.X;
            mPassData.Locations[imageNumber].CellCenter.Y += (int)shift.Y;
            Library[imageNumber] = corrected;//
            // var other = Library[imageNumber+250].Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
            // var corrected2 = ImageProcessing._2D.MotionRegistration.PhaseCorrelation(combined, other);

            //Library[imageNumber] = corrected.Add(corrected2);
        }

        private void RegisterCell()
        {
            if (Halfsies)
                framesPerTarget = (((double)Library.Count / 2) / (TargetImages.Count ));
            else
                framesPerTarget = ((double)Library.Count / (TargetImages.Count ));


            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            if (Halfsies)
                numberOfImages /= 2;

            if (Scale == 1)
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindCellsUnscaled(x));
            else
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindCells(x));
        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            RegisterCell();
        }

    }
}


