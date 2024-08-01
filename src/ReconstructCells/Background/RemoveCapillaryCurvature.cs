#if DEBUG
  //#define TESTING
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
using System.Windows.Forms;


namespace ReconstructCells.Background
{
    class RemoveCapillaryCurvature : ReconstructNodeTemplate
    {
        private OnDemandImageLibrary Library;
        private Image<Gray, float> PixelMap;


        #region Code

        private float GetVariation2(Image<Gray, float> images)
        {
            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(PixelMap);

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

            variation = (float)(sumVar2 * 20);
            return variation;
        }

        private float GetVariation(Image<Gray, float> images)
        {
            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(PixelMap);

            //  float AverageValue = (float)(.85 * images.GetAverage().Intensity);

            int Width = images.Width;
            int Height = images.Height;


            //figure out the variation of the pixels
            float sumTop = 0, sumBottom = 0;
            for (int x = 0; x < Width; x += 5)
            {
                sumTop += images.Data[0, x, 0];
                sumBottom += images.Data[Height - 1, x, 0];
            }
            sumTop /= Width;
            sumBottom /= Width;

            //we use the standard deviation to handle the pixel changes
            float sdTop = 0, sdBottom = 0;
            for (int x = 0; x < Width; x += 5)
            {
                sdTop += (sumTop - images.Data[0, x, 0]) * (sumTop - images.Data[0, x, 0]);
                sdBottom += (sumBottom - images.Data[Height - 1, x, 0]) * (sumBottom - images.Data[Height - 1, x, 0]);
            }
            sdTop = (float)Math.Sqrt(sdTop / Width);
            sdBottom = (float)Math.Sqrt(sdBottom / Width);

            float variation = sdTop;
            if (sdBottom < variation) variation = sdBottom;

            return variation / 5;
        }

        int FirstIndex = 0;

        int SecondIndex = 0;
        private void FindCapEdges(Image<Gray, float> image, out float firstIndexVal, out float secondIndexVal, out int firstIndex, out int secondIndex)
        {
#if TESTING

            Bitmap test;
#endif
            int Width = image.Width;
            int Height = image.Height;
           
            image = (Image<Gray, float>)image.Clone();
            image.DivideImage(PixelMap);

            //get the max values 
            float[] MaxRow = new float[Height];
            unchecked
            {
                int stepSize = 5;
                List<float> row = new List<float>(new float[(int)Math.Ceiling((double)Width / stepSize)]);
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    int i = 0;
                    for (int x = 0; x < Width; x += stepSize)
                    {
                        row[i] = image.Data[y, x, 0];
                        i++;
                        //value = images.Data[y, x, 0];

                        //if (value > MaxRow[y])
                        //{
                        //  MaxRow[y] = value;
                        //}
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

            float[] Diff = new float[MaxRow.Length];
            for (int i = 0; i < MaxRow.Length - 5; i++)
            {
                Diff[i] = MaxRow[i] - MaxRow[i + 3];
            }
            //float[][] diff2 = new float[2][];
            //diff2[0] = Diff;

            Diff = Diff.SmoothArrayBoxCar(15);

            double maxVal = double.MinValue;

            firstIndex = 10;
            secondIndex = image.Height - 10;
            for (int i = 0; i < 80; i++)
            {
                if (Diff[i] > maxVal)
                {
                    maxVal = Diff[i];
                    firstIndex = i;

                }
            }
            firstIndexVal = (float)maxVal;

            if (firstIndex < 31) firstIndex = 31;
            int fIndex = firstIndex;

            maxVal = double.MaxValue;
            for (int i = firstIndex - 30; i < firstIndex + 60; i++)
            {
                if (MaxRow[i] < maxVal)
                {
                    fIndex = i;
                    maxVal = MaxRow[i];
                }
            }
            firstIndex = fIndex;

            maxVal = double.MaxValue;

            for (int i = Diff.Length - 80; i < Diff.Length - 1; i++)
            {
                if (Diff[i] < maxVal)
                {
                    maxVal = Diff[i];
                    secondIndex = i;
                }
            }

            secondIndexVal = (float)maxVal;

            if (secondIndex + 32 > MaxRow.Length) secondIndex = MaxRow.Length - 31;
            int sIndex = secondIndex;
            maxVal = double.MaxValue;
            for (int i = secondIndex - 60; i < secondIndex + 30; i++)
            {
                if (MaxRow[i] < maxVal)
                {
                    sIndex = i;
                    maxVal = MaxRow[i];
                }
            }
            secondIndex = sIndex;
        }

        float variation;
        #region Linear
        /// <summary>
        /// The goal here is to get a rough removal of the curvature, and also to define the capilary boundries
        /// </summary>
        /// <param name="images"></param>
        private Image<Gray, float> RemoveCurvature1(Image<Gray, float> images, out  float[] FinalBack)
        {

            int Width = images.Width;
            int Height = images.Height;
            float value = 0;

            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(roughBack);

            //get the max values 
            float[] MaxRow = new float[Height];
            unchecked
            {
                int stepSize = 5;
                List<float> row = new List<float>(new float[(int)Math.Ceiling((double)Width / stepSize)]);
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    int i = 0;
                    for (int x = 0; x < Width; x += stepSize)
                    {
                        row[i] = images.Data[y, x, 0];
                        i++;
                        //value = images.Data[y, x, 0];

                        //if (value > MaxRow[y])
                        //{
                        //  MaxRow[y] = value;
                        //}
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }


            float[] shortened = new float[SecondIndex - FirstIndex];
            for (int i = 0; i < shortened.Length; i++)
                shortened[i] = MaxRow[i + FirstIndex];


            shortened = shortened.SmoothArrayMedianFilter(100);



            float[] AverageRow = new float[shortened.Length];
            float[] AverageCount = new float[shortened.Length];

            unchecked
            {
                //first get the upper average for each row
                for (int y = FirstIndex; y < SecondIndex; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = images.Data[y, x, 0];
                        if (value > shortened[y - FirstIndex] && float.IsNaN(value) == false)
                        {
                            AverageRow[y - FirstIndex] += value;
                            AverageCount[y - FirstIndex]++;
                        }
                    }
                }
            }


            //now normalize the lines
            float wholeAverage = 0;
            float lastValue = AverageRow[0] / AverageCount[0];
            for (int y = 0; y < AverageRow.Length; y++)
            {
                if (AverageCount[y] != 0)
                {
                    AverageRow[y] /= AverageCount[y];
                    lastValue = AverageRow[y];
                }
                else
                    AverageRow[y] = lastValue;
                // if (! float.IsNaN( AverageRow[y] ))
                wholeAverage += AverageRow[y];

            }
            wholeAverage /= AverageRow.Length;
            //test = AverageRow.DrawGraph();


            AverageRow = AverageRow.SmoothArrayBoxCar(51);



            FinalBack = new float[Height];
            for (int i = 0; i < FirstIndex; i++)
            {
                for (int x = 0; x < images.Width; x++)
                {
                    FinalBack[i] += images.Data[i, x, 0];
                }
                FinalBack[i] /= images.Width;
            }


            for (int i = SecondIndex; i < images.Height; i++)
            {
                for (int x = 0; x < images.Width; x++)
                {
                    FinalBack[i] += images.Data[i, x, 0];
                }
                FinalBack[i] /= images.Width;
            }

            for (int i = 0; i < AverageRow.Length; i++)
                FinalBack[i + FirstIndex] = AverageRow[i];

            for (int i = 0; i < MaxRow.Length; i++)
                FinalBack[i] /= wholeAverage;



            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        images.Data[y, x, 0] /= (FinalBack[y]);
                    }
                }
            }


            return images;

        }

        private Image<Gray, float> RemoveCurvature2(Image<Gray, float> images, out  float[] FinalBack, int imageNumber)
        {

            int Width = images.Width;
            int Height = images.Height;
            float value = 0;

            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(roughBack);


            //get the max values 
            float[] MaxRow = new float[Height];
            unchecked
            {
                int stepSize = 5;
                List<float> row = new List<float>(new float[(int)Math.Ceiling((double)Width / stepSize)]);
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    int i = 0;
                    for (int x = 0; x < Width; x += stepSize)
                    {
                        row[i] = images.Data[y, x, 0];
                        i++;
                        //value = images.Data[y, x, 0];

                        //if (value > MaxRow[y])
                        //{
                        //  MaxRow[y] = value;
                        //}
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

            Bitmap test = MaxRow.DrawGraph();

            float[] shortened = new float[SecondIndex - FirstIndex];
            for (int i = 0; i < shortened.Length; i++)
                shortened[i] = MaxRow[i + FirstIndex];


            shortened = shortened.SmoothArrayMedianFilter(100);

            test = shortened.DrawGraph();

            float[] AverageRow = new float[shortened.Length];
            float[] AverageCount = new float[shortened.Length];

            unchecked
            {
                //first get the upper average for each row
                for (int y = FirstIndex; y < SecondIndex; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = images.Data[y, x, 0];
                        if (value > shortened[y - FirstIndex] && float.IsNaN(value) == false)
                        {
                            AverageRow[y - FirstIndex] += value;
                            AverageCount[y - FirstIndex]++;
                        }
                    }
                }
            }


            //now normalize the lines
            float wholeAverage = 0;
            float lastValue = AverageRow[0] / AverageCount[0];
            for (int y = 0; y < AverageRow.Length; y++)
            {
                if (AverageCount[y] != 0)
                {
                    AverageRow[y] /= AverageCount[y];
                    lastValue = AverageRow[y];
                }
                else
                    AverageRow[y] = lastValue;
                // if (! float.IsNaN( AverageRow[y] ))
                wholeAverage += AverageRow[y];

            }
            wholeAverage /= AverageRow.Length;

            test = AverageRow.DrawGraph();


            AverageRow = AverageRow.SmoothArrayBoxCar(51);



            FinalBack = new float[Height];
            for (int i = 0; i < FirstIndex; i++)
            {
                for (int x = 0; x < images.Width; x++)
                {
                    FinalBack[i] += images.Data[i, x, 0];
                }
                FinalBack[i] /= images.Width;
            }


            for (int i = SecondIndex; i < images.Height; i++)
            {
                for (int x = 0; x < images.Width; x++)
                {
                    FinalBack[i] += images.Data[i, x, 0];
                }
                FinalBack[i] /= images.Width;
            }

            for (int i = 0; i < AverageRow.Length; i++)
                FinalBack[i + FirstIndex] = AverageRow[i];

            for (int i = 0; i < MaxRow.Length; i++)
                FinalBack[i] /= wholeAverage;

            test = FinalBack.DrawGraph();

            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        images.Data[y, x, 0] /= (FinalBack[y]);
                    }
                }
            }

            List<float> midList = new List<float>(new float[images.Width]);

            int yy = images.Height / 2;
            for (int x = 0; x < images.Width; x++)
                midList[x] = images.Data[yy, x, 0];

            midList.Sort();

            images = images.Mul(60000 / midList[(int)(midList.Count * 4d / 5d)]);//.Add(new Gray(30000));

            int w = test.Width;
            return images;

        }

        private Image<Gray, float> RemoveCurvature3(Image<Gray, float> images, out  float[] FinalBack, int imageNumber)
        {

            int Width = images.Width;
            int Height = images.Height;
            float value = 0;

            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(roughBack);


            //get the max values 
            float[] MaxRow = new float[Height];
            unchecked
            {
                int stepSize = 5;
                List<float> row = new List<float>(new float[(int)Math.Ceiling((double)Width / stepSize)]);
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    int i = 0;
                    for (int x = 0; x < Width; x += stepSize)
                    {
                        row[i] = images.Data[y, x, 0];
                        i++;
                        //value = images.Data[y, x, 0];

                        //if (value > MaxRow[y])
                        //{
                        //  MaxRow[y] = value;
                        //}
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

#if TESTING
            Bitmap test = MaxRow.DrawGraph();
#endif
            var shortened = MaxRow.SmoothArrayMedianFilter(30);
#if TESTING
            Bitmap test2 = shortened.DrawGraph();

            int test2w = test2.Width;
            test = shortened.DrawGraph();
#endif
            float[] AverageRow = new float[MaxRow.Length];
            float[] AverageCount = new float[MaxRow.Length];

            unchecked
            {
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = images.Data[y, x, 0];
                        if (value > shortened[y] && float.IsNaN(value) == false)
                        {
                            AverageRow[y] += value;
                            AverageCount[y]++;
                        }
                    }
                }
            }


            //now normalize the lines
            float wholeAverage = 0;
            float lastValue = AverageRow[0] / AverageCount[0];
            for (int y = 0; y < AverageRow.Length; y++)
            {
                if (AverageCount[y] != 0)
                {
                    AverageRow[y] /= AverageCount[y];
                    lastValue = AverageRow[y];
                }
                else
                    AverageRow[y] = lastValue;
                // if (! float.IsNaN( AverageRow[y] ))
                wholeAverage += AverageRow[y];

            }
            wholeAverage /= AverageRow.Length;
#if TESTING
            test = AverageRow.DrawGraph();
#endif

            AverageRow = AverageRow.SmoothArrayBoxCar(101);
#if TESTING
            test = AverageRow.DrawGraph();
#endif



            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        images.Data[y, x, 0] /= (AverageRow[y] / wholeAverage);
                    }
                }
            }



            //      images = images.Mul(60000 / midList[(int)(midList.Count * 4d / 5d)]);//.Add(new Gray(30000));
#if TESTING
            int w = test.Width;
#endif

            FinalBack = AverageRow;
            return images;

        }

#if TESTING
        private Image<Gray, float> ExampleImage;
        public static Image<Gray, float>[] backgrounds;
        float[] readNoise = new float[500];
#endif
        private void BatchRemoveCurves(int imageNumber)
        {
            var image = Library[imageNumber];



            float[] background;
            image = RemoveCurvature3(image, out background, imageNumber);

            Library[imageNumber] = image;


        }
        #endregion

        #region nonlinear
        private void BatchRemoveCurvesNonlinear(int imageNumber)
        {
            var image = Library[imageNumber];
            image = RemoveCurvatureNonlinear(image);
            Library[imageNumber] = image;
        }

        private Image<Gray, float> RemoveCurvatureNonlinear(Image<Gray, float> images)
        {
#if TESTING

            Bitmap test;
#endif
            int Width = images.Width;
            int Height = images.Height;
            float value = 0;

            images = (Image<Gray, float>)images.Clone();
            images.DivideImage(PixelMap);

            //get the max values 
            float[] MaxRow = new float[Height];
            unchecked
            {
                int stepSize = 5;
                List<float> row = new List<float>(new float[(int)Math.Ceiling((double)Width / stepSize)]);
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    int i = 0;
                    for (int x = 0; x < Width; x += stepSize)
                    {
                        row[i] = images.Data[y, x, 0];
                        i++;
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

            MaxRow = MaxRow.SmoothArrayMedianFilter(100);

#if TESTING
            test = MaxRow.DrawGraph();
#endif

            float[] AverageRow = new float[Height];
            float[] AverageCount = new float[Height];

            unchecked
            {
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = images.Data[y, x, 0];
                        if (value > MaxRow[y] && float.IsNaN(value) == false)
                        {
                            AverageRow[y] += value;
                            AverageCount[y]++;
                        }
                    }
                }
            }
#if TESTING
            test = AverageRow.DrawGraph();
#endif

            //now normalize the lines
            float wholeAverage = 0;
            float lastValue = AverageRow[0] / AverageCount[0];
            for (int y = 0; y < Height; y++)
            {
                if (AverageCount[y] != 0)
                {
                    AverageRow[y] /= AverageCount[y];
                    lastValue = AverageRow[y];
                }
                else
                    AverageRow[y] = lastValue;
                // if (! float.IsNaN( AverageRow[y] ))
                wholeAverage += AverageRow[y];

            }
            wholeAverage /= Height;
            //test = AverageRow.DrawGraph();
            AverageRow = AverageRow.SmoothArrayBoxCar(100);



#if TESTING
            test = AverageRow.DrawGraph();
#endif

            for (int i = 0; i < AverageRow.Length; i++)
                AverageRow[i] /= wholeAverage;
            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        images.Data[y, x, 0] /= (AverageRow[y]);
                    }
                }
            }
            // System.Diagnostics.Debug.Print("\n\n\n");
            // Bitmap b = images.ToBitmap();
            // int w = b.Width;

#if TESTING

            int w = test.Width;
#endif
            return images;

        }

        #endregion
        #endregion

        Image<Gray, float> roughBack;

        /*        #if TESTING
                  //  ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\testbad\data\prefiltered\image010.tif", Library[10]);
                    ExampleImage = new Image<Gray, float>(Library.Count, Library[10].Height);
                   // var t = ImageProcessing.ImageFileLoader.Load_Tiff(@"c:\temp\brightfield.tif");
                    var t = ImageProcessing.ImageFileLoader.LoadImage(@"v:\cct001\201203\16\cct001_20120316_154119\500PP\noise.png");
                  t=  t.Add(new Gray(60000 - 3500));
                    var e = Library[10].Copy();
                    double eA = e.GetAverage().Intensity;
                    // t = e.CopyBlank().Add(new Gray(60000));
                    e = e.DivideImage2(t).Mul(60000);
                    ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\testbad\data\prefiltered\image010.tif", e);
                    Rectangle ROI = new Rectangle(1200, 400, 100, 100);
                    e.ROI = ROI;
                    var ts = e.Copy();
                    double ave = ts.GetAverage().Intensity;
                    double s = 0;
                    for (int y = 0; y < ts.Width; y++)
                        for (int x = 0; x < ts.Height; x++)
                            s += Math.Pow(ts.Data[y, x, 0] - ave, 2);
                    s = Math.Sqrt(s / ts.Data.LongLength);

                    ////var e = Library[10].CopyBlank();
                    ////for (int i = 0; i < Library.Count; i++)
                    ////    e = e.Add(Library[i]);

                    ////e = e.Mul(1d / Library.Count);

                    ////ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\brightfield.tif", e);

        #endif
 
         #if TESTING
                    //ExampleImage = new Image<Gray, float>(Library.Count, Library[10].Height);
                    //var t = roughBack;// ImageProcessing.ImageFileLoader.Load_Tiff(@"c:\temp\brightfield.tif");
                    //var e = Library[10].Copy();
                    //double eA = e.GetAverage().Intensity;
                    //// t = e.CopyBlank().Add(new Gray(60000));
                    //e = e.DivideImage2(t);//.Mul(60000);
                    //ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\testbad\data\prefiltered\image010.tif", e);
                    //Rectangle ROI = new Rectangle(1200, 400, 100, 100);
                    //e.ROI = ROI;
                    //var ts = e.Copy();
                    //double ave = ts.GetAverage().Intensity;
                    //double s = 0;
                    //for (int y = 0; y < ts.Width; y++)
                    //    for (int x = 0; x < ts.Height; x++)
                    //        s += Math.Pow(ts.Data[y, x, 0] - ave, 2);
                    //s = Math.Sqrt(s / ts.Data.LongLength);

                    ////var e = Library[10].CopyBlank();
                    ////for (int i = 0; i < Library.Count; i++)
                    ////    e = e.Add(Library[i]);

                    ////e = e.Mul(1d / Library.Count);

                    ////ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\brightfield.tif", e);

        #endif
         */

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;

            PixelMap = mPassData.PixelMap;
            roughBack = PixelMap.CopyBlank().Add(new Gray(1));
            int numberOfImages = 20;
            List<Image<Gray, float>> testImages = new System.Collections.Generic.List<Image<Gray, float>>();
            List<float> midList = new List<float>(new float[Library[0].Width]);
            float[] back;
            FirstIndex = 0;
            SecondIndex = Library[10].Height;



            for (int i = 0; i < numberOfImages; i++)
            {
                int index = (int)((float)i / numberOfImages * Library.Count);
                var image = Library[index].Copy();

                image = RemoveCurvature1(image, out back);

                int y = image.Height / 2;
                for (int x = 0; x < image.Width; x++)
                    midList[x] = image.Data[y, x, 0];

                midList.Sort();

                image = image.Mul(60000 / midList[(int)(midList.Count * 4d / 5d)]);

                testImages.Add(image);
            }
            roughBack = RoughBackgrounds.EstimateBackground(testImages, true,true );


            variation = (GetVariation2(Library[1]) + GetVariation2(Library[Library.Count / 2]) + GetVariation2(Library[Library.Count - 5])) / 3;

            float[] minVals = new float[5], maxVals = new float[5];
            int[] fIndex = new int[5], sIndex = new int[5];
            int step = (int)((Library.Count - 10) / 5f);
            int bestIndex = 0;
            float minIndex = float.MinValue;
            for (int i = 0; i < 5; i++)
            {
                FindCapEdges(Library[i * step], out minVals[i], out maxVals[i], out fIndex[i], out sIndex[i]);
                if (minIndex > minVals[i])
                {
                    bestIndex = i;
                    minIndex = minVals[i];
                }
            }

            FirstIndex = fIndex[bestIndex];
            SecondIndex = sIndex[bestIndex];
#if TESTING
            backgrounds = new Image<Gray, float>[Library.Count];
#endif
            // BatchRemoveCurves(10);
            //  BatchRemoveCurves(0);
          // // ParallelOptions po = new ParallelOptions();
           // po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveCurves(x));

#if TESTING
            readNoise.CopyData();

            Clipboard.SetImage(ExampleImage.ScaledBitmap);
            backgrounds = new Image<Gray, float>[Library.Count];
#endif
        }

    }
}
