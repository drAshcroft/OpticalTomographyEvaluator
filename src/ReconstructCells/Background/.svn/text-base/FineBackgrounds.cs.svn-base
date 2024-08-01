#if DEBUG
 //  #define TESTING
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using MathLibrary.Signal_Processing;
using System.Drawing;
using ImageProcessing._2D;
using MathLibrary;

namespace ReconstructCells.Background
{
    class FineBackgrounds : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;
        Image<Gray, float> pixelMap;
        static float RoughVariance = 1000;
        bool HasHoles = false;
        int nProjections=25;

        Image<Gray, float> background=null;

        #region Properties
        #region Set
        public void SetNumberProjections(int numberProjections)
        {
            nProjections = numberProjections;
        }

        #endregion

        #region Get
        public Image<Gray, float> getBackground()
        {
            return background;
        }

        public bool GetHasHoles()
        {
            return HasHoles;

        }
        #endregion
        #endregion

        #region Code 

        private float GetVariation2(Image<Gray, float> images)
        {
            int Width = images.Width;
            int Height = images.Height;


            //figure out the variation of the pixels
            double sumVar2 = 0;
            float d = 0;
            for (int x = 0; x < Width; x += 5)
            {
                d = images.Data[5, x, 0] - images.Data[6, x, 0];
                sumVar2 += d * d;
            }

            sumVar2 = Math.Sqrt(sumVar2 / (Width - 1));
            return  (float)(sumVar2 * 20);
        }


        private  Image<Gray, float> GetRoughBackground()
        {
            List<Image<Gray, float>> SelectedImages = new List<Image<Gray, float>>();
            for (int i = 0; i < nProjections; i++)
            {
                int index = (int)((float)i / nProjections * Library.Count);
                var image = Library[index];
                // image = RemoveCurvature(image);
                SelectedImages.Add(image);
            }

           float   variation = (GetVariation2(Library[1]) + GetVariation2(Library[Library.Count / 2]) + GetVariation2(Library[Library.Count - 5])) / 3;

            return CombineByBrightestLine(SelectedImages.ToArray(),variation );
        }

        private unsafe Image<Gray, float> CombineByBrightest(Image<Gray, float>[] images)
        {
            Image<Gray, float> averageImage = new Image<Gray, float>(images[0].Width, images[0].Height);
            Image<Gray, float> ih2;
            unchecked
            {

                float nImages = images.Length;
                long length = averageImage.Data.LongLength;
                fixed (float* pAve = averageImage.Data)
                {
                    float* pAveI = pAve;
                    //first get the baseline
                    for (int i = 0; i < images.Length; i++)
                    {
                        ih2 = images[i].SmoothGaussian(19);
                        fixed (float* pImage = ih2.Data)
                        {
                            pAveI = pAve;
                            float* pImageI = pImage;
                            for (long j = 0; j < length; j++)
                            {
                                *pAveI += *pImageI;
                                pAveI++;
                                pImageI++;
                            }
                        }
                    }

                    pAveI = pAve;

                    for (long j = 0; j < length; j++)
                    {
                        *pAveI /= nImages;
                        pAveI++;

                    }

                }

#if TESTING
                Bitmap b = averageImage.ToBitmap();
                //averageImage = averageImage.Mul(1.0d / (double)images.Length);

                var t = averageImage.ScaledBitmap;
#endif
                //now find the brightest point, which is likely to be the point that does not 
                //have a cell
                float maxValue = 0;
                int maxX = 0, maxY = 0;
                for (int x = 0; x < images[0].Width; x++)
                    for (int y = 0; y < images[0].Height; y++)
                    {
                        if (averageImage.Data[y, x, 0] > maxValue)
                        {
                            maxValue = averageImage.Data[y, x, 0];
                            maxX = x;
                            maxY = y;
                        }
                    }

                //now find the variance at that point
                float minValue = float.MaxValue;
                maxValue = float.MinValue;
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].Data[maxY, maxX, 0] > maxValue) maxValue = images[i].Data[maxY, maxX, 0];
                    if (images[i].Data[maxY, maxX, 0] < minValue) minValue = images[i].Data[maxY, maxX, 0];
                }
                float variance = (maxValue - minValue);

                RoughVariance = variance;

                //remove this value from the threshold image
                bool Holes = false;
                float[,] Divisor = null;
                double sum = 0;
                long count = 0;
                for (int tryI = 0; tryI < 2; tryI++)
                {

                    averageImage = averageImage.Sub(new Gray(variance / 2));
                    //  b = averageImage.ToBitmap();
                    //now loop through and add together all the bright pixels
                    ih2 = new Image<Gray, float>(images[0].Width, images[0].Height);
                    Divisor = new float[images[0].Height, images[0].Width];
#if TESTING
                   var  ih3 = new Image<Gray, float>(images[0].Width, images[0].Height);
#endif

                    for (int i = 0; i < images.Length; i++)
                    {
                        for (int x = 0; x < images[0].Width; x++)
                            for (int y = 0; y < images[0].Height; y++)
                            {
                                if (images[i].Data[y, x, 0] > averageImage.Data[y, x, 0])
                                {
                                    ih2.Data[y, x, 0] += images[i].Data[y, x, 0];
                                    Divisor[y, x]++;
#if TESTING
                                    ih3.Data[y,x,0]=1;
                                   
#endif
                                }
                            }
                    }
#if TESTING
                    b = Divisor.MakeBitmap();
#endif
                    //now get the average and divide out the image
                    sum = 0;
                    count = 0;
                    Holes = false;
                    for (int x = 0; x < images[0].Width; x++)
                        for (int y = 0; y < images[0].Height; y++)
                        {
                            if (Divisor[y, x] > 0)
                            {
                                averageImage.Data[y, x, 0] = ih2.Data[y, x, 0] / Divisor[y, x];
                                sum += averageImage.Data[y, x, 0];
                                count++;
                            }
                            else
                                Holes = true;
                        }

                    //  b = averageImage.ToBitmap();
                }

                ih2 = averageImage;
                // b = ih2.ToBitmap();

                float average = (float)(sum / count);
                if (Holes)
                {
                    for (int x = 0; x < images[0].Width; x++)
                        for (int y = 0; y < images[0].Height; y++)
                        {
                            if (Divisor[y, x] == 0)
                            {
                                ih2.Data[y, x, 0] = average;
                            }
                        }

                    HasHoles = true;
                }

                ih2 = ih2.Mul(1.0 / average);

                ih2 = ih2.SmoothGaussian(61);
#if TESTING
                b = ih2.ToBitmap();
                int w = b.Width;
#endif

                //  System.Diagnostics.Debug.Print(b.ToString());
            }
            return ih2;
        }

        private unsafe Image<Gray,float> MaxProfile(Image<Gray, float>[] images, float variation)
        {

#if TESTING

            Bitmap test;
#endif
            int Width = images[0].Width;
            int Height = images[0].Height;
            
            //get the max values 
            Image<Gray, float> MaxRow = new Image<Gray, float>(Width, Height);
            unchecked
            {
              
                List<float> row = new List<float>(new float[ images.Length]  );
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    
                    for (int x = 0; x < Width; x++)
                    {
                        for (int i = 0; i < images.Length; i++)
                        {
                            row[i] = images[i].Data[y, x, 0];
                        }
                        row.Sort();
                        MaxRow.Data[y,x,0] = row[row.Count - 5] - variation;
                    }
                }
            }

            return MaxRow.SmoothGaussian(9);
        }

        private unsafe Image<Gray, float> CombineByBrightestLine(Image<Gray, float>[] images, float variance)
        {
            Image<Gray, float> averageImage = new Image<Gray, float>(images[0].Width, images[0].Height);
            Image<Gray, float> ih2;
            unchecked
            {
                averageImage = MaxProfile(images, variance);
#if TESTING
                Bitmap b = averageImage.ToBitmap();
                //averageImage = averageImage.Mul(1.0d / (double)images.Length);

              
#endif
                RoughVariance = variance;

                //remove this value from the threshold image
                bool Holes = false;
                float[,] Divisor = null;
                double sum = 0;
                long count = 0;
                for (int tryI = 0; tryI < 2; tryI++)
                {
                    //  b = averageImage.ToBitmap();
                    //now loop through and add together all the bright pixels
                    ih2 = new Image<Gray, float>(images[0].Width, images[0].Height);
                    Divisor = new float[images[0].Height, images[0].Width];
#if TESTING
                   
#endif

                    for (int i = 0; i < images.Length; i++)
                    {
                        for (int x = 0; x < images[0].Width; x++)
                            for (int y = 0; y < images[0].Height; y++)
                            {
                                if (images[i].Data[y, x, 0] > averageImage.Data[y, x, 0])
                                {
                                    ih2.Data[y, x, 0] += images[i].Data[y, x, 0];
                                    Divisor[y, x]++;
#if TESTING
                                   

#endif
                                }
                            }
                    }
#if TESTING
                    b = Divisor.MakeBitmap();
#endif
                    //now get the average and divide out the image
                    sum = 0;
                    count = 0;
                    Holes = false;
                    for (int x = 0; x < images[0].Width; x++)
                        for (int y = 0; y < images[0].Height; y++)
                        {
                            if (Divisor[y, x] > 0)
                            {
                                averageImage.Data[y, x, 0] = ih2.Data[y, x, 0] / Divisor[y, x];
                                sum += averageImage.Data[y, x, 0];
                                count++;
                            }
                            else
                                Holes = true;
                        }

                    //  b = averageImage.ToBitmap();
                }

                ih2 = averageImage;
                // b = ih2.ToBitmap();

                float average = (float)(sum / count);
                if (Holes)
                {
                    for (int x = 0; x < images[0].Width; x++)
                        for (int y = 0; y < images[0].Height; y++)
                        {
                            if (Divisor[y, x] == 0)
                            {
                                ih2.Data[y, x, 0] = average;
                            }
                        }

                    HasHoles = true;
                }

                ih2 = ih2.Mul(1.0 / average);

              //  ih2 = ih2.SmoothGaussian(61);
#if TESTING
                b = ih2.ToBitmap();
                int w = b.Width;
#endif

                //  System.Diagnostics.Debug.Print(b.ToString());
            }
            return ih2;
        }



        private Image<Gray, float>[] Averages;
        private Image<Gray, float>[] SmoothAverages;
        private Image<Gray, float>[] Counts;
        private Image<Gray, float> RoughBack;
        #region Averages 
      
        private void BatchAverageSubtract(int Processor)
        {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;

            SmoothAverages[Processor] = RoughBack.CopyBlank();
            Averages[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();

            var averaged = Averages[Processor];
            var smoothAveraged = SmoothAverages[Processor]; 
            var count = Counts[Processor];
            //var data = Averages[Processor].Data;

            // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            for (int i = startI; i < startI + step && i < Library.Count; i++)
            {
                var map = Library[i].DivideImage2(RoughBack);

                var imageThresh = map.Convert<Bgr, byte>();
                var ave2 = imageThresh.GetAverage();
                // var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(1, 1, 1));
                var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                m2 = m2.SmoothGaussian(121);
                //var  m3 =  m2.MorphologyEx(se,Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN,2);
                var m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));

                map = m3.Convert<Gray, float>();

                count = count.Add(map);
                var image = Library[i];
                
                var smoothed = image.SmoothGaussian (11);                
                var sharped = image.Sub(smoothed);
                
                smoothed =smoothed.Mul(map);
                sharped = sharped.Mul(map);

                averaged = averaged.Add(sharped);
                smoothAveraged = smoothAveraged.Add(smoothed);
            }

            SmoothAverages[Processor] = smoothAveraged;
            Averages[Processor] = averaged;
            Counts[Processor] = count;
            // averaged = averaged.DivideImage2(count);
        }


        private void BatchAverage(int Processor)
        {
            int step =(int)Math.Round ( (double)Library.Count /(  Environment.ProcessorCount - 1) );

            int startI= step  *Processor ;


            Averages[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();

            var averaged = Averages[Processor];
            var count = Counts[Processor];
            //var data = Averages[Processor].Data;

           // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            for (int i = startI; i < startI + step && i<Library.Count ; i++)
            {
                var map = Library[i].DivideImage2(RoughBack);

                var imageThresh = map.Convert<Bgr, byte>();
                var ave2 = imageThresh.GetAverage();
               // var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(1, 1, 1));
                var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                m2 = m2.SmoothGaussian(121);
               //var  m3 =  m2.MorphologyEx(se,Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN,2);
                var m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));

                map =m3.Convert<Gray, float>();

                count = count.Add(map);
                var image = Library[i].Mul(map);
                averaged = averaged.Add(image);
            }

            Averages[Processor] = averaged;
            Counts[Processor] = count;
           // averaged = averaged.DivideImage2(count);
        }

        private unsafe Image<Gray, float> AverageThreshold()
        {
            List<Image<Gray, float>> SelectedImages = new List<Image<Gray, float>>();
            for (int i = 0; i < nProjections; i++)
            {
                int index = (int)((float)i / nProjections * Library.Count);
                var image = Library[index];
                // image = RemoveCurvature(image);
                SelectedImages.Add(image);
            }

            float variation = (GetVariation2(Library[1]) + GetVariation2(Library[Library.Count / 2]) + GetVariation2(Library[Library.Count - 5])) / 3;

            RoughBack =   CreateShapeTemplate(SelectedImages, variation);


         //   ParallelOptions po = new ParallelOptions();
          //  po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Program.threadingParallelOptions.MaxDegreeOfParallelism;

            SmoothAverages = new Image<Gray, float>[numberOfImages];
            Averages = new Image<Gray, float>[numberOfImages];
            Counts = new Image<Gray, float>[numberOfImages];


            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchAverageSubtract(x));

            var smoothed = RoughBack.CopyBlank();
            var averaged = RoughBack.CopyBlank();
            var counts = RoughBack.CopyBlank();

            for (int i = 0; i < numberOfImages; i++)
            {
                averaged = averaged.Add(Averages[i]);
                smoothed = smoothed.Add(SmoothAverages[i]);
                counts = counts.Add(Counts[i]);
            }


            averaged = averaged.Add(smoothed);
            averaged = averaged.DivideImage2(counts);

            //var map = SelectedImages[1].DivideImage2(RoughBack);

            //double [] max,min ;
            //Point[] pM,pm;
            //map.MinMax(out min, out max , out pM, out pm);
            //var ave = map.GetAverage();

            //Gray thresh = new Gray((max [0]* 3 + ave.Intensity) / 4);

            
            //for (int i = 0; i < Library.Count ; i++)
            //{
             
            //}

           // count = count.Sub(new Gray (nProjections)).Mul(-1);

            double  average = 1.0/ averaged.GetAverage().Intensity ;

            return  averaged.Mul(average);
        }
        #endregion

        private  Image<Gray, float> CreateShapeTemplate( List<Image<Gray, float>> images, float variation)
        {
            Image<Gray, float> shape=images[0].Copy ();
            
            for (int i = 1; i < images.Count; i++)
            {
                shape = images[i].Max(shape);
            }

            shape =  shape.SmoothGaussian(25);
            shape = shape.Sub(new Gray(variation));
            shape =  shape.Mul (1.0/ 256);// shape.GetAverage().Intensity );

            return shape ;
        }

        #endregion

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            pixelMap = mPassData.PixelMap;


            background = AverageThreshold();
            //background = GetRoughBackground();
        }

        #region Extra
        public static Image<Gray, float> RemoveBackground(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI)
        {
            if (ROI.X > 0 && ROI.Y > 0 && ROI.Right < image.Width && ROI.Bottom < image.Height)
            {
                return ImageManipulation.RemoveClearBackground(image, Background, ROI);
            }
            else
                return ImageManipulation.RemoveClippedBackground(image, Background, ROI, RoughVariance);

        }


        public static Image<Gray, float> RemoveBackgroundMasked(Image<Gray, float> image, Image<Gray, float> Background, Rectangle ROI)
        {
            if (ROI.X > 0 && ROI.Y > 0 && ROI.Right < image.Width && ROI.Bottom < image.Height)
            {
                return ImageManipulation.RemoveClearBackground(image, Background, ROI);
            }
            else
                return ImageManipulation.RemoveClippedBackgroundMasked(image, Background, ROI, RoughVariance);

        }

        #endregion
    }
}
