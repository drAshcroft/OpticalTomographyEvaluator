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
    class RemoveCapillaryCurvatureBig : ReconstructNodeTemplate
    {
        private OnDemandImageLibrary Library;

        private float maxLower = 1500;

        private Image<Gray, float> CurveImage;


        public Image<Gray, float> getCurveImage()
        {
            return CurveImage;
        }

        #region Code
        private void AverageCurve(int imageNumber)
        {
            Image<Gray, float> aveImage = Library[imageNumber].Copy();

            aveImage.DivideImage(roughBack);
            int Width = aveImage.Width;
            int Height = aveImage.Height;
            float value = 0;

            //get the max values 
            float[] MaxRow = new float[Height];
            float[] UpperRow = new float[Height];
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
                    value = row[row.Count - 35];
                    UpperRow[y] = value;
                    MaxRow[y] = value - maxLower;
                }
            }

            //Bitmap test = MaxRow.DrawGraph();
            //int w = test.Width;

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
                        if (value > MaxRow[y] && float.IsNaN(value) == false)// && value <UpperRow[y])
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

                CurveImage.Data[y, imageNumber, 0] = AverageRow[y];
            }
            Maxs[imageNumber] = AverageRow.Max();
            wholeAverage /= AverageRow.Length;
            aves[imageNumber] = wholeAverage;
        }
        private void RemoveCurve(int imageNumber)
        {
            Image<Gray, float> aveImage = Library[imageNumber].Copy();

            float d;
           // double sum = 0;
            for (int y = 0; y < aveImage.Height; y++)
            {
                d = CurveImage.Data[y,imageNumber,0];
                for (int x = 0; x < aveImage.Width; x++)
                {
                    aveImage.Data[y, x, 0] /= d;
                   // sum += aveImage.Data[y, x, 0];
                }
            }

            //double ave = sum / aveImage.Data.LongLength;

            //aveImage = aveImage.Mul(targetIntensity /ave  );

            Library[imageNumber] = aveImage;
        }

        private float  GetVariance()
        {

            Rectangle[] rois = new Rectangle[] { new Rectangle(100, 400, 100, 100), new Rectangle(300, 400, 100, 100), new Rectangle(1400, 400, 100, 100) };
            Rectangle ROI = Rectangle.Empty;
            Random rnd = new Random();

            float sdRoughness = 0,cc=0;
            float[] roughness = new float[Library.Count];
            for (int i = 0; i < Library.Count; i+=150)
            {
                var image = Library[i].Copy();
                roughness[i] = float.MaxValue;
                for (int j = 0; j < rois.Length; j++)
                {
                    ROI = rois[j];
                    var image2 = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, roughBack, ROI);
                    //image.ROI=ROI;

                  //  var image2 = image.Copy();
                    double ave = image2.GetAverage().Intensity;
                    double sd = 0;
                    for (int y = 0; y < image2.Height; y++)
                        for (int x = 0; x < image2.Width; x++)
                        {
                            sd = sd + Math.Pow(image2.Data[y, x, 0] - ave, 2);
                        }
                    sd = Math.Sqrt(sd / image2.Data.LongLength);
                   // if (sd < roughness[i]) roughness[i] = (float)sd;
                   // image.ROI = Rectangle.Empty;
                    sdRoughness += (float)sd;
                    cc++;
                }
            }
            return (float)(sdRoughness /cc);
        }

        #endregion
        Image<Gray, float> roughBack;
        float targetIntensity;

        float[] Maxs;
        float[] aves;
        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;

            roughBack = Library[0].CopyBlank().Add(new Gray(1));
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

            roughBack = RoughBackgrounds.EstimateBackground(testImages, true, true);
            // roughBack = testImages[0].CopyBlank().Add(new Gray(1));
            //  return;

            maxLower = GetVariance()*2.7f;

            CurveImage = new Image<Gray, float>(Library.Count, Library[0].Height);
            Maxs = new float[Library.Count];
            aves = new float[Library.Count];


            numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => AverageCurve(x));


            var g = Maxs.DrawGraph();

            var smoothed = smoothArray(  Maxs,21);

            CurveImage = CurveImage.SmoothBilatral(21, 500, 9);// CorrectCurves(Maxs, smoothed);

            targetIntensity  = CurveImage.Data.AverageArray();
            CurveImage = CurveImage.Mul(1d / targetIntensity);

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => RemoveCurve(x));


            
        }

        //private Image<Gray,float> CorrectCurves(float[] maxs, float[] smoothed)
        //{
        //    Image<Gray,float> CurveImage2=CurveImage.Copy();
        //    float[] divArray = new float[smoothed.Length];
        //    for (int i = 0; i < maxs.Length; i++)
        //    {
        //        divArray[i] = smoothed[i] / maxs[i];
        //    }

        //    for (int y=0;y<CurveImage.Height;y++)
        //        for (int x = 0; x < Library.Count; x++)
        //        {
        //            CurveImage2.Data[y, x, 0] *= divArray[x];
        //        }

        //    return CurveImage2;
        //}
      

        private float[] smoothArray(float[] array, int Period)
        {

            float[] outArray = new float[array.Length];
            int cc = 0;
            float sum = 0;
            int startI, endI;
            for (int i = 0; i < array.Length; i++)
            {
                startI = i - Period;
                endI = i + Period;
                if (startI < 0) startI = 0;
                if (endI > array.Length) endI = array.Length;
                sum = 0;
                cc = 0;
                for (int j = startI; j < endI; j++)
                {
                    sum += array[j]; cc++;
                }
                outArray[i] = sum / cc;
            }
            return outArray;
        }
    }
}
