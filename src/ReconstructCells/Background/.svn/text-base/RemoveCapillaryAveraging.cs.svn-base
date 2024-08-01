#if DEBUG
#define TESTING
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
    class RemoveCapillaryCurvatureByAverage : ReconstructNodeTemplate
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


        float variation;
      
        private Image<Gray, float> GuessCurvature(Image<Gray, float>[] images, out  float[] FinalBack)
        {
            var aveImage = images[0].Clone();
            for (int i = 1; i < images.Length; i++)
                aveImage = aveImage.Add(images[i]);

            aveImage = aveImage.Mul(1d / images.Length);

            int Width = aveImage.Width;
            int Height = aveImage.Height;
            float value = 0;

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
                        row[i] = aveImage.Data[y, x, 0];
                        i++;
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

         //   Bitmap test = MaxRow.DrawGraph();
        //    int w = test.Width;

            float[] AverageRow = new float[MaxRow.Length];
            float[] AverageCount = new float[MaxRow.Length];

            unchecked
            {
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = aveImage.Data[y, x, 0];
                        if (value > MaxRow[y ] && float.IsNaN(value) == false)
                        {
                            AverageRow[y ] += value;
                            AverageCount[y ]++;
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

        //    test = AverageRow.DrawGraph();
        //    w = test.Width;

            AverageRow = AverageRow.SmoothArrayBoxCar(51);


            FinalBack = new float[Height];
          

            for (int i = 0; i < MaxRow.Length; i++)
                FinalBack[i] = AverageRow[i]/ wholeAverage;

            var fImage = images[0];
            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        fImage.Data[y, x, 0] /= (FinalBack[y]);
                    }
                }
            }


            return fImage;

        }

        private Image<Gray, float> RemoveCurvature(int imageNumber)
        {

            var aveImage = Library[imageNumber ].Clone();
            for (int i=imageNumber +1;i<imageNumber +4;i++)
                aveImage = aveImage.Add(Library[i%Library.Count ]);

            aveImage = aveImage.Mul(1d /4);

            aveImage.DivideImage(roughBack);

            int Width = aveImage.Width;
            int Height = aveImage.Height;
            float value = 0;

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
                        row[i] = aveImage.Data[y, x, 0];
                        i++;
                    }
                    row.Sort();
                    MaxRow[y] = row[row.Count - 5] - variation;
                }
            }

            Bitmap test = MaxRow.DrawGraph();
            int w = test.Width;

            float[] AverageRow = new float[MaxRow.Length];
            float[] AverageCount = new float[MaxRow.Length];

            unchecked
            {
                //first get the upper average for each row
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x += 5)
                    {
                        value = aveImage.Data[y, x, 0];
                        if (value > MaxRow[y] && float.IsNaN(value) == false)
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
            //test = AverageRow.DrawGraph();

           float[] AverageRowT = AverageRow.SmoothArrayBoxCar(21);

           int minL = AverageRowT.Length / 2;
           float minV = AverageRowT[minL];
           for (int i = minL; i > 0; i--)
           {
               if (minV > AverageRowT[i])
               {
                   minV = AverageRowT[i];
                   minL = i;
               }
           }

           int minH = AverageRowT.Length / 2;
            minV = AverageRowT[minH];
           for (int i = minL; i <AverageRowT.Length ; i++)
           {
               if (minV > AverageRowT[i])
               {
                   minV = AverageRowT[i];
                   minH = i;
               }
           }

           AverageRow = AverageRow.SmoothArrayHighPoints(21, 71);
        //    test = AverageRow.DrawGraph();
          //  w = test.Width;

          float[]  FinalBack = new float[Height];


            for (int i = 0; i < MaxRow.Length; i++)
                FinalBack[i] = AverageRow[i] / wholeAverage;

            var fImage = Library[imageNumber];
            //and finally flatten out the data
            unchecked
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        fImage.Data[y, x, 0] /= (FinalBack[y]);
                    }
                }
            }


            return fImage;

        }



        private void BatchRemoveCurves(int imageNumber)
        {
            var image = Library[imageNumber];
            image = RemoveCurvature( imageNumber);

           // Library[imageNumber] = image;


        }
       

        #endregion
        Image<Gray, float> roughBack;

        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;

            PixelMap = mPassData.PixelMap;
            roughBack = PixelMap.CopyBlank().Add(new Gray(1));
            int numberOfImages = 20;
            List<Image<Gray, float>> testImages = new System.Collections.Generic.List<Image<Gray, float>>();
            List<float> midList = new List<float>(new float[Library[0].Width]);
           
            for (int i = 0; i < numberOfImages; i++)
            {
                int index = (int)((float)i / numberOfImages * Library.Count);
                var image = Library[index].Copy();

              // image = GuessCurvature(new Image<Gray, float>[] { Library[(index) % Library.Count], Library[(index + 1) % Library.Count], Library[(index + 2) % Library.Count], Library[(index + 3) % Library.Count] }, out back);
               
                int y = image.Height / 2;
                for (int x = 0; x < image.Width; x++)
                    midList[x] = image.Data[y, x, 0];

                midList.Sort();

                image = image.Mul(60000 / midList[(int)(midList.Count * 4d / 5d)]);

                testImages.Add(image);
            }
            roughBack = RoughBackgrounds.EstimateBackground(testImages, true,true  );


            //BatchRemoveCurves(10);
         //   ParallelOptions po = new ParallelOptions();
         //   po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveCurves(x));


        }

    }
}
