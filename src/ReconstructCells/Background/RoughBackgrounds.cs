﻿#if DEBUG
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
using System.IO;
using System.Threading;


namespace ReconstructCells.Background
{
    class RoughBackgrounds : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;
        OnDemandImageLibrary ThresLibrary;
        Image<Gray, float> pixelMap;
        static float RoughVariance = 1000;
        bool HasHoles = false;
        int nProjections = 25;
        private bool noIteration = true;


        Image<Gray, float> background = null;

        #region Properties
        #region Set
        public void SetNumberProjections(int numberProjections)
        {
            nProjections = numberProjections;
        }
        public void SetDoIterations(bool doIterations)
        {
            noIteration = !doIterations;
        }

        public void SetBackground(Image<Gray,float> background)
        {
            this.background = background;
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

        #region Brightest
        private static float GetVariation2(Image<Gray, float> images)
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
            return (float)(sumVar2 * 20);
        }


        private Image<Gray, float> GetRoughBackground()
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

            return CombineByBrightestLine(SelectedImages.ToArray(), variation);
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

        private unsafe Image<Gray, float> MaxProfile(Image<Gray, float>[] images, float variation)
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

                List<float> row = new List<float>(new float[images.Length]);
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
                        MaxRow.Data[y, x, 0] = row[row.Count - 5] - variation;
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
        #endregion


        private Image<Gray, float>[] Weights;
        private Image<Gray, float>[] SmoothAverages;
        private Image<Gray, float>[] RoughAverages;
        private Image<Gray, float>[] HoleAverages;
        private Image<Gray, float>[] HolesSmooths;
        private Image<Gray, float>[] FullAverages;
        private Image<Gray, float>[] Counts;
        private Image<Gray, float> RoughBack;
        long NBad = 0;
        #region Averages
        object lockObj = new object();
        object lockObj2 = new object();
        SemaphoreSlim m_semaphore = new SemaphoreSlim(5);
        private void BatchAverageSubtract(int Processor)
        {
            try
            {
                int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

                int startI = step * Processor;

                SmoothAverages[Processor] = RoughBack.CopyBlank();
                Weights[Processor] = RoughBack.CopyBlank();
                Counts[Processor] = RoughBack.CopyBlank();
                HoleAverages[Processor] = RoughBack.CopyBlank();
                HolesSmooths[Processor] = RoughBack.CopyBlank();

                var averaged = Weights[Processor];
                var smoothAveraged = SmoothAverages[Processor];
                var holes = HoleAverages[Processor];
                var holesSmooth = HolesSmooths[Processor];
                var count = Counts[Processor];
                //var data = Averages[Processor].Data;

                var sharped = RoughBack.CopyBlank();
                // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
                for (int i = startI; i < startI + step && i < Library.Count; i++)
                {
                    var map = Library[i].DivideImage2(RoughBack);
                    Image<Bgr, byte> imageThresh;
                    //lock (lockObj)
                    {
                        imageThresh = map.Convert<Bgr, byte>();

                        var ave2 = imageThresh.GetAverage();
                        var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                        m2 = m2.SmoothGaussian(121);

                        // float[, ,] mask = ilFilters.SmoothGuassianT(map.Data, 61);
                        //    Image<Gray, float> tImage = new Image<Gray, float>(mask);

                        //    System.Diagnostics.Debug.Print(m2.Width.ToString() + tImage.Width.ToString());

                        // map = ThresLibrary[i];

                        for (int x = 0; x < m2.Width; x++)
                            for (int y = 0; y < m2.Height; y++)
                            {
                                if (m2.Data[y, x, 0] > 238)
                                    map.Data[y, x, 0] = 1;
                                else
                                    map.Data[y, x, 0] = 0;
                            }
                        //    Image<Bgr, byte> m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));
                        //    map = m3.Convert<Gray, float>();

                        ThresLibrary[i] = map;
                        count.Data.AddToArray(map.Data);
                        var image = Library[i];
                        var smoothed = image.SmoothGaussian(11);

                        image.Data.SubtractFrom_To(smoothed.Data, sharped.Data);
                        holes.Data.AddToArray(sharped.Data);
                        holesSmooth.Data.AddToArray(smoothed.Data);
                        smoothed.Data.MultiplyInPlace(map.Data);
                        sharped.Data.MultiplyInPlace(map.Data);
                        averaged.Data.AddToArray(sharped.Data);
                        smoothAveraged.Data.AddToArray(smoothed.Data);
                    }
                }

                HolesSmooths[Processor] = holesSmooth;
                HoleAverages[Processor] = holes;
                SmoothAverages[Processor] = smoothAveraged;
                Weights[Processor] = averaged;
                Counts[Processor] = count;
                // averaged = averaged.DivideImage2(count);
            }
            catch (Exception ex)
            {
                Program.WriteLine(ex.Message + "\n\n");
                Program.WriteLine(ex.StackTrace);

                Environment.Exit(0);
                throw new Exception("did not kill gracefully\n" + ex.Message);
            }
        }

        private void BatchAverageSubtractKnownLocation(int Processor)
        {
            //   try
            //  {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;

            SmoothAverages[Processor] = RoughBack.CopyBlank();
            Weights[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();
            HoleAverages[Processor] = RoughBack.CopyBlank();
            HolesSmooths[Processor] = RoughBack.CopyBlank();

            var averaged = Weights[Processor];
            var smoothAveraged = SmoothAverages[Processor];
            var holes = HoleAverages[Processor];
            var holesSmooth = HolesSmooths[Processor];
            var count = Counts[Processor];
            //var data = Averages[Processor].Data;

            var sharped = RoughBack.CopyBlank();
            // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            for (int i = startI; i < startI + step && i < Library.Count; i++)
            {
                var map = ThresLibrary[i];

                CellLocation cl = mPassData.Locations[i];
                Rectangle r = cl.ToRectangle();
                for (int x = r.Left; x < r.Right; x++)
                    for (int y = r.Top; y < r.Bottom; y++)
                    {
                        if (x > 0 && x < map.Width)
                            if (y > 0 && y < map.Height)
                            {
                                map.Data[y, x, 0] = 0;
                            }
                    }
                //    Image<Bgr, byte> m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));
                //    map = m3.Convert<Gray, float>();

                ThresLibrary[i] = map;
                count.Data.AddToArray(map.Data);
                var image = Library[i];

                m_semaphore.Wait();
                var smoothed = image.SmoothGaussian(11);
                m_semaphore.Release();


                image.Data.SubtractFrom_To(smoothed.Data, sharped.Data);
                holes.Data.AddToArray(sharped.Data);
                holesSmooth.Data.AddToArray(smoothed.Data);
                smoothed.Data.MultiplyInPlace(map.Data);
                sharped.Data.MultiplyInPlace(map.Data);
                averaged.Data.AddToArray(sharped.Data);
                smoothAveraged.Data.AddToArray(smoothed.Data);

            }

            HolesSmooths[Processor] = holesSmooth;
            HoleAverages[Processor] = holes;
            SmoothAverages[Processor] = smoothAveraged;
            Weights[Processor] = averaged;
            Counts[Processor] = count;

            //}
            //catch (Exception ex)
            //{
            //    Program.WriteLine(ex.Message + "\n\n");
            //    Program.WriteLine(ex.StackTrace);

            //    Environment.Exit(0);
            //    throw new Exception("did not kill gracefully\n" + ex.Message);
            //}
        }


        private void BatchAverage(int Processor)
        {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;


            Weights[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();

            var averaged = Weights[Processor];
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
                var image = Library[i].Mul(map);
                averaged = averaged.Add(image);
            }

            Weights[Processor] = averaged;
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

            RoughBack = CreateShapeTemplate(SelectedImages, variation);
            //   return RoughBack;
            ThresLibrary = new OnDemandImageLibrary(Library.Count, true, @"c:\temp", false);
            for (int i = 0; i < Library.Count; i++)
                ThresLibrary[i] = RoughBack.CopyBlank().Add(new Gray(255));

            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Program.threadingParallelOptions.MaxDegreeOfParallelism;

            SmoothAverages = new Image<Gray, float>[numberOfImages];
            Weights = new Image<Gray, float>[numberOfImages];
            Counts = new Image<Gray, float>[numberOfImages];
            HoleAverages = new Image<Gray, float>[numberOfImages];
            HolesSmooths = new Image<Gray, float>[numberOfImages];


            //BatchAverageSubtract(5);
            if (mPassData.Locations != null)
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchAverageSubtractKnownLocation(x));
            else
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchAverageSubtract(x));

            var smoothed = RoughBack.CopyBlank();
            var averaged = RoughBack.CopyBlank();
            var counts = RoughBack.CopyBlank();
            var smoothHoles = RoughBack.CopyBlank();
            var averageHoles = RoughBack.CopyBlank();

            for (int i = 0; i < numberOfImages; i++)
            {
                averaged = averaged.Add(Weights[i]);
                smoothed = smoothed.Add(SmoothAverages[i]);
                counts = counts.Add(Counts[i]);
                averageHoles = averageHoles.Add(HoleAverages[i]);
                smoothHoles = smoothHoles.Add(HolesSmooths[i]);
            }

            averageHoles = averageHoles.Add(smoothHoles);
            averageHoles = averageHoles.DivideImage2(Library.Count);
            averaged = averaged.Add(smoothed);
            averaged = averaged.DivideImage2(counts);

            double[] min, max;
            Point[] mP, MP;
            counts.MinMax(out min, out max, out mP, out MP);
            //long NBad = 0;
            if (min[0] == 0)
            {



                //var blank = averaged.SmoothGaussian(61);
                var blank = averaged.Copy();
                for (int y = 0; y < averaged.Height; y++)
                    for (int x = 0; x < averaged.Width; x++)
                    {

                        if (counts.Data[y, x, 0] == 0 || float.IsNaN(blank.Data[y, x, 0]))
                        {
                            //    blank.Data[y, x, 0] = lValue;// fullAveraged.Data[y, x, 0];
                            NBad++;
                            //}
                            //else
                            //    lValue = blank.Data[y, x, 0];
                            blank.Data[y, x, 0] = averageHoles.Data[y, x, 0];
                        }
                    }

                //for (int i = 0; i < 3; i++)
                //{
                //    blank = blank.SmoothGaussian(9);
                //    for (int y = 0; y < averaged.Height; y++)
                //        for (int x = 0; x < averaged.Width; x++)
                //        {

                //            if (counts.Data[y, x, 0] != 0)
                //                blank.Data[y, x, 0] = averaged.Data[y, x, 0];
                //        }
                //}

                averaged = blank;
                //double average = 1.0 / averaged.GetAverage().Intensity;
                //averaged = averaged.Mul(average);
                //return averaged;

                Program.WriteTagsToLog("Background", "background is Not complete, bad pixels=" + NBad);
            }

            double average = 1.0 / averaged.GetAverage().Intensity;
            averaged = averaged.Mul(average);

            if (NBad > 5000)
            {
                return StealBackground(averaged, NBad);
            }
            else
                return averaged;
        }

       
        private Image<Gray, float> StealBackground(Image<Gray, float> existingBackground, long nBad)
        {


            string[] backgrounds = Directory.GetFiles(@"z:\ASU_Recon\Backgrounds", "*backnew_" + Program.DateStamp + "_*.*");

            long leastbad = (long)(nBad * .4);
            int leastIndex = -1;
            for (int i = 0; i < backgrounds.Length; i++)
            {
                string[] parts = Path.GetFileNameWithoutExtension(backgrounds[i]).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                long fileBad;
                if (parts.Length == 2)
                    fileBad = long.Parse(parts[1]);
                else
                    fileBad = long.Parse(parts[4]);
                if (fileBad < leastbad)
                {
                    leastbad = fileBad;
                    leastIndex = i;
                }
            }

            if (leastIndex > -1)
            {
                Program.WriteTagsToLog("Background Stolen", "True");
                noIteration = true;
                return ImageProcessing.ImageFileLoader.Load_Tiff(backgrounds[leastIndex]);
            }
            else
            {
                Program.WriteTagsToLog("Background Stolen", "False");
                return existingBackground;
            }
        }
        #endregion

        private static Image<Gray, float> CreateShapeTemplate(List<Image<Gray, float>> images, float variation)
        {
            Image<Gray, float> shape = images[0].Copy();

            for (int i = 1; i < images.Count; i++)
            {
                shape = images[i].Max(shape);
            }

            shape = shape.SmoothGaussian(45);
            shape = shape.Sub(new Gray(variation));
            shape = shape.Mul(1.0 / 256);// shape.GetAverage().Intensity );

            return shape;
        }

        private static Image<Gray, float> CreateAverageTemplate(List<Image<Gray, float>> images, float variation, bool ScaleData, bool guassian)
        {
            Image<Gray, float> shape = images[0].Copy();

            for (int i = 1; i < images.Count; i++)
            {
                shape = images[i].Max(shape);
            }

            //shape = shape.SmoothGaussian(25);
            shape = shape.Sub(new Gray(variation));

            Image<Gray, float> Sums = shape.CopyBlank();

            List<float> values = new List<float>(new float[images.Count]);
            int index = images.Count - 1;
            float val;
            for (int x = 0; x < images[0].Width; x++)
                for (int y = 0; y < images[0].Height; y++)
                {

                    for (int i = 0; i < images.Count; i++)
                    {
                        values[i] = images[i].Data[y, x, 0];

                    }
                    values.Sort();
                    val = values[index];
                    if (val < shape.Data[y, x, 0])
                        val = shape.Data[y, x, 0] + variation;
                    Sums.Data[y, x, 0] = val;
                }

            if (guassian)
                Sums = Sums.SmoothGaussian(71);
            else
                Sums = Sums.SmoothBilatral(71, 200, 31);

            if (ScaleData)
                Sums = Sums.Mul(1d / Sums.GetAverage().Intensity);
            return Sums;
        }

        #region EasyAverage


        private void BatchEasyAverage(int Processor)
        {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;

            var temp = Library[10];

            Weights[Processor] = temp.CopyBlank();
            Counts[Processor] = temp.CopyBlank();

            var weights = Weights[Processor];
            var count = Counts[Processor];
            var fullAverages = temp.CopyBlank();
            var smoothAve = temp.CopyBlank();
            var roughAve = temp.CopyBlank();


            int half = Library[10].Height / 2;
            List<float> midLine = new List<float>(new float[Library[10].Width]);
            for (int i = startI; i < startI + step && i < Library.Count; i++)
            {
                var image = Library[i];

                for (int j = 0; j < image.Width; j++)
                    midLine[j] = image.Data[half, j, 0];

                midLine.Sort();

                float RoughBack = midLine[midLine.Count - 20];

                var map = image.DivideImage2(RoughBack);
                map = map.Mul(250);
                var imageThresh = map.Convert<Bgr, byte>();
                var ave2 = imageThresh.GetAverage();
                // var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(1, 1, 1));
                var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                m2 = m2.SmoothGaussian(121);
                //var  m3 =  m2.MorphologyEx(se,Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN,2);
                var m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));

                map = m3.Convert<Gray, float>();

                ThresLibrary[i] = map;

                count = count.Add(map);

                image = Library[i].Mul(map);

                float C = 60000 / RoughBack / 255;
                float val, val2;
                unsafe
                {
                    fixed (float* pWeights = weights.Data, pImage = image.Data, pAverages = fullAverages.Data)
                    {
                        float* ppWeights = pWeights;
                        float* ppImage = pImage;
                        float* ppAverages = pAverages;
                        for (long k = 0; k < image.Data.LongLength; k++)
                        {
                            val = *(ppImage) * C;
                            val2 = (float)Math.Sqrt(val);
                            *ppWeights += val2;
                            *ppAverages += val * val2;
                            ppImage++;
                            ppWeights++;
                            ppAverages++;
                            //weights = weights.Add(image);

                            //image = image.Mul(image);
                            //fullAverages = fullAverages.Add(image);
                        }
                    }
                }
            }

            Weights[Processor] = weights;
            Counts[Processor] = count;
            FullAverages[Processor] = fullAverages;

        }


        private unsafe Image<Gray, float> EasyAverageThreshold()
        {
            double[] min, max;
            Point[] mP, MP;

            ThresLibrary = new OnDemandImageLibrary(Library.Count, true, @"c:\temp", false);

            var temp = Library[10];

            // ParallelOptions Program.threadingParallelOptions = new ParallelOptions();
            //Program.threadingParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Program.threadingParallelOptions.MaxDegreeOfParallelism;

            // SmoothAverages = new Image<Gray, float>[numberOfImages];
            Weights = new Image<Gray, float>[numberOfImages];
            Counts = new Image<Gray, float>[numberOfImages];
            FullAverages = new Image<Gray, float>[numberOfImages];
            RoughAverages = new Image<Gray, float>[numberOfImages];
            SmoothAverages = new Image<Gray, float>[numberOfImages];

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchEasyAverage(x));

            var smoothed = temp.CopyBlank();
            var roughed = temp.CopyBlank();
            var weights = temp.CopyBlank();
            var counts = temp.CopyBlank();
            var fullAveraged = temp.CopyBlank();

            for (int i = 0; i < numberOfImages; i++)
            {
                weights = weights.Add(Weights[i]);
                fullAveraged = fullAveraged.Add(FullAverages[i]);
                counts = counts.Add(Counts[i]);
            }

            weights = fullAveraged.DivideImage2(weights);
            counts.MinMax(out min, out max, out mP, out MP);
            if (min[0] == 0)
            {
                Program.WriteTagsToLog("Background", "background is NotFinite");

                string[] backs = Directory.GetFiles(@"z:\ASU_Recon\Backgrounds", @"background_" + Program.DateStamp + "_*.tif");
                if (backs.Length > 0)
                {
                    Program.WriteTagsToLog("Background_fix", "loaded background");
                    weights = ImageProcessing.ImageFileLoader.Load_Tiff(backs[0]);
                }
                else
                {
                    Program.WriteTagsToLog("Background_fix", "fudged it");
                    double intens = 0, countI = 0;
                    for (int y = 0; y < fullAveraged.Height; y++)
                        for (int x = 0; x < fullAveraged.Width; x++)
                        {

                            if (counts.Data[y, x, 0] != 0)
                            {
                                intens += weights.Data[y, x, 0];
                                countI++;
                            }
                        }

                    weights = weights.Mul(1 / (intens / countI));

                    for (int y = 0; y < fullAveraged.Height; y++)
                        for (int x = 0; x < fullAveraged.Width; x++)
                        {

                            if (counts.Data[y, x, 0] == 0)
                                weights.Data[y, x, 0] = 1;// fullAveraged.Data[y, x, 0];

                        }
                }

            }
            else
            {
                double average = weights.GetAverage().Intensity;
                average = 1.0 / average;
                weights = weights.Mul(average);
                ImageProcessing.ImageFileLoader.Save_TIFF(@"z:\ASU_Recon\Backgrounds\background_" + Program.DateStamp + "_" + Program.TimeStamp + ".tif", weights);

            }


            //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\average.tif", averaged);

            //averaged = roughed.Add(smoothed);
            //average = 1.0 / averaged.GetAverage().Intensity;
            //averaged=averaged.Mul(average);
            // return averaged.Mul(average);
            return weights;

        }


        #endregion


        #region WeightedAverage

        private void BatchWeightedAverageO(int Processor)
        {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;


            Weights[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();

            var averaged = Weights[Processor];
            var count = Counts[Processor];
            var fullAverages = RoughBack.CopyBlank();
            var smoothAve = RoughBack.CopyBlank();
            var roughAve = RoughBack.CopyBlank();

            //var data = Averages[Processor].Data;

            // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            for (int i = startI; i < startI + step && i < Library.Count; i++)
            {
                var image = Library[i];


                //if (mPassData.FluorImage == false)
                //{
                //    int h = image.Height / 2;
                //    float[] Line = new float[image.Width];
                //    List<float> lLine = new List<float>(Line);
                //    for (int j = 0; j < image.Width; j++)
                //        lLine[j] = image.Data[h, j, 0];

                //    lLine.Sort();

                //    float ave = 0;
                //    int cc = 0;
                //    for (int j = (int)(Math.Round(image.Width / 4.0) * 3 - 5); j < image.Width; j++)
                //    {
                //        ave += lLine[j];
                //        cc++;
                //    }

                //    ave = (float)(ave / cc);

                //    image.MinMax(out min, out max, out mP, out MP);

                //    image = image.Sub(new Gray(ave - 60000));
                //}

                //Library[i] = image;

                var map = image.DivideImage2(RoughBack);
                map = map.Mul(250);
                var imageThresh = map.Convert<Bgr, byte>();
                var ave2 = imageThresh.GetAverage();
                // var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(1, 1, 1));
                var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                m2 = m2.SmoothGaussian(121);
                //var  m3 =  m2.MorphologyEx(se,Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN,2);
                var m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));

                map = m3.Convert<Gray, float>();

                ThresLibrary[i] = map;

                count = count.Add(map);
                fullAverages = fullAverages.Add(image);
                image = Library[i].Mul(map);
                averaged = averaged.Add(image);

                var smoothed = Library[i].SmoothGaussian(61);
                smoothAve = smoothAve.Add(smoothed.Mul(map));

                var roughed = Library[i].Sub(smoothed.Mul(.9));

                roughed = roughed.Mul(map);
                roughAve = roughAve.Add(roughed);
            }

            Weights[Processor] = averaged;
            Counts[Processor] = count;
            FullAverages[Processor] = fullAverages;
            SmoothAverages[Processor] = smoothAve;
            RoughAverages[Processor] = roughAve;
            // averaged = averaged.DivideImage2(count);
        }

        private void BatchWeightedAverage(int Processor)
        {
            int step = (int)Math.Round((double)Library.Count / (Environment.ProcessorCount - 1));

            int startI = step * Processor;


            Weights[Processor] = RoughBack.CopyBlank();
            Counts[Processor] = RoughBack.CopyBlank();

            var weights = Weights[Processor];
            var count = Counts[Processor];
            var fullAverages = RoughBack.CopyBlank();
            var smoothAve = RoughBack.CopyBlank();
            var roughAve = RoughBack.CopyBlank();

            //var data = Averages[Processor].Data;

            // var se = new StructuringElementEx(71,71, 35, 35, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            for (int i = startI; i < startI + step && i < Library.Count; i++)
            {
                var image = Library[i];


                //if (mPassData.FluorImage == false)
                //{
                //    int h = image.Height / 2;
                //    float[] Line = new float[image.Width];
                //    List<float> lLine = new List<float>(Line);
                //    for (int j = 0; j < image.Width; j++)
                //        lLine[j] = image.Data[h, j, 0];

                //    lLine.Sort();

                //    float ave = 0;
                //    int cc = 0;
                //    for (int j = (int)(Math.Round(image.Width / 4.0) * 3 - 5); j < image.Width; j++)
                //    {
                //        ave += lLine[j];
                //        cc++;
                //    }

                //    ave = (float)(ave / cc);

                //    image.MinMax(out min, out max, out mP, out MP);

                //    image = image.Sub(new Gray(ave - 60000));
                //}

                //Library[i] = image;

                var map = image.DivideImage2(RoughBack);
                map = map.Mul(250);
                var imageThresh = map.Convert<Bgr, byte>();
                var ave2 = imageThresh.GetAverage();
                // var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(1, 1, 1));
                var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

                m2 = m2.SmoothGaussian(121);
                //var  m3 =  m2.MorphologyEx(se,Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN,2);
                var m3 = m2.ThresholdBinary(new Bgr(238, 238, 238), new Bgr(Color.White));

                map = m3.Convert<Gray, float>();

                ThresLibrary[i] = map;

                count = count.Add(map);

                image = Library[i].Mul(map);
                float val, val2;
                unsafe
                {
                    fixed (float* pWeights = weights.Data, pImage = image.Data, pAverages = fullAverages.Data)
                    {
                        float* ppWeights = pWeights;
                        float* ppImage = pImage;
                        float* ppAverages = pAverages;
                        for (long k = 0; k < image.Data.LongLength; k++)
                        {
                            val = *(ppImage);
                            val2 = (float)Math.Sqrt(val);
                            *ppWeights += val2;
                            *ppAverages += val * val2;
                            ppImage++;
                            ppWeights++;
                            ppAverages++;
                            //weights = weights.Add(image);

                            //image = image.Mul(image);
                            //fullAverages = fullAverages.Add(image);
                        }
                    }
                }
            }

            Weights[Processor] = weights;
            Counts[Processor] = count;
            FullAverages[Processor] = fullAverages;

        }


        public static Image<Gray, float> EstimateBackground(List<Image<Gray, float>> library, bool scale, bool GaussianAverage)
        {

            List<Image<Gray, float>> SelectedImages = new List<Image<Gray, float>>();

            List<float> midList = new List<float>(new float[library[0].Width]);
            int numberOfImages = library.Count;
            for (int i = 0; i < numberOfImages; i++)
            {
                int index = i;
                var image = library[index];
                SelectedImages.Add(image);
            }

            float variation = 2000;// (GetVariation2(library[1]) + GetVariation2(library[library.Count / 2]) + GetVariation2(library[library.Count - 5])) / 3;

            return CreateAverageTemplate(SelectedImages, variation, scale, GaussianAverage);
        }




        private unsafe Image<Gray, float> WeightedAverageThreshold()
        {
            List<Image<Gray, float>> SelectedImages = new List<Image<Gray, float>>();
            double[] min, max;
            Point[] mP, MP;
            for (int i = 0; i < nProjections; i++)
            {
                int index = (int)((float)i / nProjections * Library.Count);
                var image = Library[index];
                SelectedImages.Add(image);
            }

            RoughBack = EstimateBackground(SelectedImages, false, true);

            float variation = (GetVariation2(Library[1]) + GetVariation2(Library[Library.Count / 2]) + GetVariation2(Library[Library.Count - 5])) / 3;

            RoughBack = RoughBack.Sub(new Gray(variation));

            ThresLibrary = new OnDemandImageLibrary(Library);

            //    RoughBack = CreateShapeTemplate(SelectedImages, variation);

            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Program.threadingParallelOptions.MaxDegreeOfParallelism;

            // SmoothAverages = new Image<Gray, float>[numberOfImages];
            Weights = new Image<Gray, float>[numberOfImages];
            Counts = new Image<Gray, float>[numberOfImages];
            FullAverages = new Image<Gray, float>[numberOfImages];
            RoughAverages = new Image<Gray, float>[numberOfImages];
            SmoothAverages = new Image<Gray, float>[numberOfImages];

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchWeightedAverage(x));

            var smoothed = RoughBack.CopyBlank();
            var roughed = RoughBack.CopyBlank();
            var weights = RoughBack.CopyBlank();
            var counts = RoughBack.CopyBlank();
            var fullAveraged = RoughBack.CopyBlank();

            for (int i = 0; i < numberOfImages; i++)
            {
                weights = weights.Add(Weights[i]);
                fullAveraged = fullAveraged.Add(FullAverages[i]);
                counts = counts.Add(Counts[i]);
            }

            weights = fullAveraged.DivideImage2(weights);
            counts.MinMax(out min, out max, out mP, out MP);
            if (min[0] == 0)
            {
                Program.WriteTagsToLog("Background", "background is NotFinite");

                string[] backs = Directory.GetFiles(@"z:\ASU_Recon\Backgrounds", @"background_" + Program.DateStamp + "_*.tif");
                if (backs.Length > 0)
                {
                    Program.WriteTagsToLog("Background_fix", "loaded background");
                    weights = ImageProcessing.ImageFileLoader.Load_Tiff(backs[0]);
                }
                else
                {
                    Program.WriteTagsToLog("Background_fix", "fudged it");
                    double intens = 0, countI = 0;
                    for (int y = 0; y < fullAveraged.Height; y++)
                        for (int x = 0; x < fullAveraged.Width; x++)
                        {

                            if (counts.Data[y, x, 0] != 0)
                            {
                                intens += weights.Data[y, x, 0];
                                countI++;
                            }
                        }

                    weights = weights.Mul(1 / (intens / countI));

                    for (int y = 0; y < fullAveraged.Height; y++)
                        for (int x = 0; x < fullAveraged.Width; x++)
                        {

                            if (counts.Data[y, x, 0] == 0)
                                weights.Data[y, x, 0] = 1;// fullAveraged.Data[y, x, 0];

                        }
                }

            }
            else
            {
                double average = weights.GetAverage().Intensity;
                average = 1.0 / average;
                weights = weights.Mul(average);
                ImageProcessing.ImageFileLoader.Save_TIFF(@"z:\ASU_Recon\Backgrounds\background_" + Program.DateStamp + "_" + Program.TimeStamp + ".tif", weights);

            }


            //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\average.tif", averaged);

            //averaged = roughed.Add(smoothed);
            //average = 1.0 / averaged.GetAverage().Intensity;
            //averaged=averaged.Mul(average);
            // return averaged.Mul(average);
            return weights;

        }


        #endregion


        #region Iterative

        int[, ,] ExampleImages;
        OnDemandImageLibrary AdjustedImages;
        float alpha;
        Image<Gray, float> OriginalBack;
        int Iteration = 1;
        int MaxIteration = 10;
        private void BatchIterativeWierd(int Processor)
        {
            int step = (int)Math.Round(RoughBack.Width / (Environment.ProcessorCount - 1d));

            int startI = step * Processor + 1;
            float diff;
            double error;
            float ORatio = (float)((1 - Math.Pow((Iteration / MaxIteration), 2)) * .5);
            for (int x = startI; x < startI + step && x < RoughBack.Width - 1; x++)
            {
                for (int y = 1; y < RoughBack.Height - 1; y++)
                {
                    error = 0;
                    for (int k = 0; k < ExampleImages.GetLength(2); k++)
                    {
                        var image = AdjustedImages[ExampleImages[y, x, k]];

                        diff = image.Data[y, x, 0];// -(image.Data[y, x + 1, 0] + image.Data[y, x - 1, 0] + image.Data[y + 1, x, 0] + image.Data[y - 1, x, 0]) / 4;
                        error += diff;
                    }

                    error = error * alpha;//Math.Sign(error) * Math.Pow(Math.Abs(error * alpha / 5), .5);
                    diff = (float)(OriginalBack.Data[y, x, 0] * ORatio + (1 - ORatio) * (RoughBack.Data[y, x, 0] + (error)));
                    if (diff < 0) diff = .1f;
                    RoughBack.Data[y, x, 0] = diff;
                }

            }
        }
        Image<Gray, float> newBack;
        private void BatchIterative(int Processor)
        {
            int step = (int)Math.Round(RoughBack.Width / (Program.threadingParallelOptions.MaxDegreeOfParallelism - 1d));

            int startI = step * Processor + 1;
            float diff;
            double error;
            double error2;
            float d;
            float ORatio = (float)((1 - Math.Pow((Iteration / MaxIteration), 2)) * .5);
            for (int x = startI; x < startI + step && x < RoughBack.Width - 1; x++)
            {
                for (int y = 1; y < RoughBack.Height - 1; y++)
                {

                    error = 0;
                    for (int k = 0; k < ExampleImages.GetLength(2); k++)
                    {
                        var image = AdjustedImages[ExampleImages[y, x, k]];
                        diff = image.Data[y, x, 0];
                        diff = Math.Abs(diff - image.Data[y, x + 1, 0])
                            + Math.Abs(diff - image.Data[y, x - 1, 0])
                            + Math.Abs(diff - image.Data[y + 1, x, 0])
                            + Math.Abs(diff - image.Data[y - 1, x, 0]);
                        error += diff / 4;
                    }
                    error /= ExampleImages.GetLength(2);
                    error2 = 0;
                    d = (float)(error / 4);
                    for (int k = 0; k < ExampleImages.GetLength(2); k++)
                    {
                        var image = AdjustedImages[ExampleImages[y, x, k]];
                        diff = image.Data[y, x, 0] - d;
                        diff = Math.Abs(diff - image.Data[y, x + 1, 0])
                            + Math.Abs(diff - image.Data[y, x - 1, 0])
                            + Math.Abs(diff - image.Data[y + 1, x, 0])
                            + Math.Abs(diff - image.Data[y - 1, x, 0]);
                        if (diff > 300)
                            diff = 300;
                        else
                        {
                            if (diff < -300) diff = -300;
                        }
                        error2 += diff / 4;
                    }

                    diff = RoughBack.Data[y, x, 0];
                    error = (error - error2) * alpha;
                    if (error > 500 || error < -500)
                    {
                        error = Math.Sign(error) * 500;
                    }
                    d = (float)(RoughBack.Data[y, x, 0] - error);
                    newBack.Data[y, x, 0] = d;// OriginalBack.Data[y, x, 0] * .3f + .7f * d;
                }

            }
        }

        private unsafe Image<Gray, float> IterativeBackgroundO(bool doItWeird)
        {
            mPassData.AddInformation ("RoughBackgrounds.IterativeBackgroundO", doItWeird.ToString());

            int[] AvailableImages = new int[30];
            //give the first best images

            for (int i = 0; i < AvailableImages.Length; i++)
                AvailableImages[i] = (int)((Library.Count - 1) / AvailableImages.Length * i);

            //now select the images that each pixel can compare to without falling within a threshold problem
            int maxImages = (int)Math.Floor(AvailableImages.Length / 2d);
            ExampleImages = new int[RoughBack.Height, RoughBack.Width, maxImages];

            maxImages--;
            for (int i = 0; i < RoughBack.Width; i++)
                for (int j = 0; j < RoughBack.Height; j++)
                {
                    int cc = 0;
                    for (int k = 0; k < AvailableImages.Length && cc < maxImages; k++)
                    {
                        var thres = ThresLibrary[AvailableImages[k]];
                        if (thres.Data[j, i, 0] != 0)
                        {
                            ExampleImages[j, i, cc] = k;
                            cc = cc + 1;
                            //  if (k > maxImages) maxImages = k;
                        }
                    }

                    //just hope that the value can be assigned despite the obvious problem of being inside the cell
                    if (cc == 0)
                    {
                        for (int k = 0; k < maxImages; k++)
                        {
                            ExampleImages[j, i, cc] = k;
                        }
                    }
                }




            AdjustedImages = new OnDemandImageLibrary(AvailableImages.Length, true, @"c:\temp", false);

            float ave = (float)Library[AvailableImages[1]].GetAverage().Intensity;

            RoughBack = RoughBack.Mul(ave);

            MaxIteration = 4;
            for (int k = 0; k < MaxIteration; k++)
            {
                newBack = RoughBack.CopyBlank();

                Iteration = k + 1;
                for (int i = 0; i < AvailableImages.Length; i++)
                {
                    AdjustedImages[i] = Library[AvailableImages[i]].Copy().DivideImage2(RoughBack);
                    AdjustedImages[i] = AdjustedImages[i].Mul(ave);
                    var back = AdjustedImages[i].SmoothGaussian(9);

                    AdjustedImages[i] = AdjustedImages[i].Sub(back);
                }

                alpha = (float)(.1f / (k + 1));
                //now divide out the images to make processing faster

                // ParallelOptions po = new ParallelOptions();
                //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;
                int numberOfImages = Program.threadingParallelOptions.MaxDegreeOfParallelism;

                if (doItWeird)
                    Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchIterativeWierd(x));
                else
                    Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchIterative(x));

                RoughBack = newBack.Copy();
                for (int x = 0; x < 3; x++)
                    for (int y = 0; y < RoughBack.Height; y++)
                    {

                        RoughBack.Data[y, x, 0] = OriginalBack.Data[y, x, 0] * ave;
                        RoughBack.Data[y, RoughBack.Width - 1 - x, 0] = OriginalBack.Data[y, RoughBack.Width - 1 - x, 0] * ave;
                    }

                for (int x = 0; x < RoughBack.Width; x++)
                    for (int y = 0; y < 3; y++)
                    {

                        RoughBack.Data[y, x, 0] = OriginalBack.Data[y, x, 0] * ave;
                        RoughBack.Data[RoughBack.Height - 1 - y, x, 0] = OriginalBack.Data[RoughBack.Height - 1 - y, x, 0] * ave;
                    }

                System.Diagnostics.Debug.Print(RoughBack.Width.ToString());
            }
            RoughBack = RoughBack.Mul(1d / ave);
            return RoughBack;
        }


        private unsafe Image<Gray, float> IterativeBackground()
        {
            mPassData.AddInformation ("RoughBackgrounds.IterativeBackground", "");
            double Average = Library[10].GetAverage().Intensity;
            var correction = RoughBack.CopyBlank();
            MaxIteration = 1;
            unsafe
            {
                unchecked
                {

                    fixed (float* pCorrData = correction.Data)

                        for (int k = 0; k < MaxIteration; k++)
                        {

                            float* pCorr = pCorrData;
                            float cc = 0;
                            for (int i = (k % 3); i < Library.Count; i += 3)
                            {
                                pCorr = pCorrData;
                                var image = Library[i];
                                image = image.DivideImage2(RoughBack);
                                var back = image.SmoothGaussian(9);
                                image = image.Sub(back);

                                fixed (float* pData = image.Data)
                                {
                                    float* ppData = pData;
                                    for (long j = 0; j < image.Data.LongLength; j++)
                                    {
                                        *pCorr += *ppData;
                                        pCorr++;
                                        ppData++;
                                    }
                                }
                                cc++;
                            }


                            pCorr = pCorrData;


                            for (long j = 0; j < correction.Data.LongLength; j++)
                            {

                                *pCorr /= cc;

                                pCorr++;
                            }


                        }
                }



            }

            return correction;
        }


        #endregion

        #endregion

        public static void GetStandardDeviation(OnDemandImageLibrary library, Image<Gray, float> back, Rectangle ROI)
        {
            float[] roughness = new float[library.Count];
            for (int i = 0; i < library.Count; i++)
            {
                var image = library[i];
                //image.ROI = ROI;
                var image2 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, back, ROI);
                // image.ROI = Rectangle.Empty;
                double ave = image2.GetAverage().Intensity;
                double sd = 0;
                for (int y = 0; y < image2.Height; y++)
                    for (int x = 0; x < image2.Width; x++)
                    {
                        sd = sd + Math.Pow(image2.Data[y, x, 0] - ave, 2);
                    }
                sd = Math.Sqrt(sd / image2.Data.LongLength);
                roughness[i] = (float)sd;
            }

            roughness.CopyData();

        }

        public static void GetStandardDeviation(OnDemandImageLibrary library, Image<Gray, float> back, Image<Gray, float> correction, Rectangle ROI)
        {
            float[] roughness = new float[library.Count];
            for (int i = 0; i < library.Count; i++)
            {
                var image = library[i];
                //image.ROI = ROI;
                var image2 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, back, ROI);
                correction.ROI = ROI;
                var corr = correction.Copy();
                image2 = image2.Sub(corr);
                // image.ROI = Rectangle.Empty;
                double ave = image2.GetAverage().Intensity;
                double sd = 0;
                for (int y = 0; y < image2.Height; y++)
                    for (int x = 0; x < image2.Width; x++)
                    {
                        sd = sd + Math.Pow(image2.Data[y, x, 0] - ave, 2);
                    }
                sd = Math.Sqrt(sd / image2.Data.LongLength);
                roughness[i] = (float)sd;
            }

            roughness.CopyData();

        }

#if PAPER_TESTS

        Rectangle firstBox;
        public void setBox(Rectangle box)
        {
            firstBox = box;
        }
#endif

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            pixelMap = mPassData.PixelMap;


            if (background == null)
            {

                if (Program.VGPPReader != null && mPassData.Locations == null)
                {
                    mPassData = Registration.RoughRegister.GetVGSuggestion(mPassData, Program.VGPPReader);
                }


                mPassData.AddInformation("RoughBackgrounds.AverageThreshold", "");
                background = AverageThreshold();

                try
                {
                    RoughBack = background;
                    OriginalBack = background.Copy();

                    if (noIteration == false)
                        background = IterativeBackgroundO(false);

                    ImageProcessing.ImageFileLoader.Save_TIFF(@"z:\ASU_Recon\Backgrounds\backnew_" + Program.DateStamp + "_" + NBad.ToString() + ".tif", background);

                    RoughBack = background;
                    OriginalBack = background.Copy();

                    mPassData.AddInformation("RoughBackgrounds.NumberProjections", nProjections.ToString());

                    mPassData.Library.divideLibrary(background);
                    mPassData.theBackground = background;
                }
                catch
                {
                    if (mPassData.theBackground != null)
                        mPassData.Library.divideLibrary(background);
                }
            }
            else
            {
                mPassData.Library.divideLibrary(background);
            }
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


        #region Tests
#if PAPER_TESTS
        /*    public static    void TestSmoothness(OnDemandImageLibrary Library, Image<Gray,float>back, string classification)
        {
            List<Image<Gray, float>> testImages = new List<Image<Gray, float>>();
            List<Image<Gray, float>> threshImages = new List<Image<Gray, float>>();
            for (int i = 0; i < Library.Count; i += 50)
            {
                testImages.Add(Library[i]);
                threshImages.Add(ThresLibrary[i]);
            }

            Rectangle ROI=Rectangle.Empty;
            Random rnd = new Random();

            bool badRegion = false;
            for (int tryI = 0; tryI < 15 && badRegion == false; tryI++)
            {
                ROI = new Rectangle((int)(rnd.NextDouble() * 300 + 400), (int)(rnd.NextDouble() * (300) + 400), 100, 100);
               
                for (int i = 0; i < testImages.Count && badRegion ==false ; i++)
                {
                    threshImages[i].ROI=ROI;
                    var image = threshImages[i].Copy();

                    for (int x = 0; x < image.Width && badRegion == false; x++)
                        for (int y = 0; y < image.Height && badRegion == false; y++)
                        {
                            if (image.Data[y, x, 0] == 0)
                            {
                                badRegion = true;
                            }
                        }
                }
            }

            float[] roughness = new float[Library.Count];
            for (int i = 0; i < Library.Count; i++)
            {
                var image = Library[i];
                //image.ROI = ROI;
                var image2 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, back, ROI);
                // image.ROI = Rectangle.Empty;
                double ave = image2.GetAverage().Intensity;
                double sd = 0;
                for (int y = 0; y < image2.Height; y++)
                    for (int x = 0; x < image2.Width; x++)
                    {
                        sd = sd + Math.Pow(image2.Data[y, x, 0] - ave, 2);
                    }
                sd = Math.Sqrt(sd / image2.Data.LongLength);
                roughness[i] = (float)sd;
            }

            string junk = "";
            for (int i = 0; i < roughness.Length; i++)
            {
               
                junk += roughness[i] + "\n";
            }

            System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_backgroundSmoothnessSTD_feature_" +  Program.WorkTags  + ".csv", junk);


        }*/
        public static void TestSmoothness(OnDemandImageLibrary Library, Image<Gray, float> back, string classification,Rectangle cellBox)
        {
         

            Rectangle[] rois = new Rectangle[] { new Rectangle(100, 400, 100, 100), new Rectangle(300, 400, 100, 100), new Rectangle(1400, 400, 100, 100) };
            Rectangle ROI = Rectangle.Empty;
            Random rnd = new Random();

            float[] roughness = new float[Library.Count];
            for (int i = 0; i < Library.Count; i++)
            {
                var image = Library[i].Copy();
                roughness[i] = float.MaxValue;
                for (int j = 0; j < rois.Length; j++)
                {
                    ROI = rois[j];
                    var image2 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, back, ROI);
                    double ave = image2.GetAverage().Intensity;
                    double sd = 0;
                    for (int y = 0; y < image2.Height; y++)
                        for (int x = 0; x < image2.Width; x++)
                        {
                            sd = sd + Math.Pow(image2.Data[y, x, 0] - ave, 2);
                        }
                    sd = Math.Sqrt(sd / image2.Data.LongLength);
                    if (sd < roughness[i]) roughness[i] = (float)sd;
                }
            }

            Program.WriteLine("Background Score" + classification);
            string junk = "";
            for (int i = 0; i < roughness.Length; i++)
            {
                junk += roughness[i] + "\n";
                Program.WriteLine(roughness[i].ToString());
            }
            Program.WriteLine(" ");

            var image3 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[1].Copy(), back, cellBox);
            //ImageProcessing.ImageFileLoader.Save_Bitmap(Program.dataFolder + "\\backExample_" + classification + ".bmp",image3);
            ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\backExample_" + classification + ".tif", image3);
            System.IO.File.WriteAllText(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_backgroundSmoothnessSTD_feature_" + Program.WorkTags + ".csv", junk);
        }

        public static void TestReadnoise(OnDemandImageLibrary Library)
        {
            Point[] p = new Point[] { new Point(10, 400), new Point(100, 400), new Point(1000, 400), new Point(1500, 450) };
            float[] aveRoughness = new float[p.Length];
            float[] sdRoughness = new float[p.Length];
            for (int i = 0; i < Library.Count; i++)
            {
                var image = Library[i];
            
                for (int j = 0; j < p.Length; j++)
                {
                    aveRoughness[j] += image.Data[p[j].Y, p[j].X, 0];
                }

            }


        

                for (int j = 0; j < p.Length; j++)
                {
                    aveRoughness[j] /= Library.Count;
                }

           


            for (int i = 0; i < Library.Count; i++)
            {
                var image = Library[i];

                for (int j = 0; j < p.Length; j++)
                {
                    sdRoughness[j] += (float)Math.Pow((aveRoughness[j]- image.Data[p[j].Y, p[j].X, 0]),2);
                }
            }

            float minSD = float.MaxValue;
            for (int j = 0; j < p.Length; j++)
            {
                sdRoughness[j] =(float) Math.Sqrt(sdRoughness[j] / Library.Count);
                if (sdRoughness[j] < minSD) minSD = sdRoughness[j];
            }

            Program.WriteLine("Read noise ," + minSD);
        }

#endif

        #endregion
    }
}
