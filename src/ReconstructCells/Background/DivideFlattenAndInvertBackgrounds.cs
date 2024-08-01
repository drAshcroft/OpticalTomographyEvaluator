

#if DEBUG
// #define TESTING
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageProcessing._3D;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using MathLibrary;
using ImageProcessing;
using System.Diagnostics;

namespace ReconstructCells.Background
{
    class DivideFlattenAndInvertBackgrounds : ReconstructNodeTemplate
    {
        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        int suggestedCellSize;
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
        OnDemandImageLibrary Library2;
        int TrimSize = 10;

        float[] ImageMinValues;

        float[] EdgeValues;
        float[] EdgeRange;

        int Buffer = 30;

        AlignMethods fineAlignMethod = 0;
#if TESTING
         float[] beforeRoughness;
         float[] afterRoughness;
#endif
        public enum AlignMethods
        {
            COG,mCOG,nCOG,CC,tCC
        }

        #region Properties

        #region Set

        public void setSuggestedCellSize(int cellSize)
        {
            suggestedCellSize = cellSize;
        }
        public void setBackground(Image<Gray, float> backGround)
        {
            BackGround = backGround;
        }

        public void setMethod(AlignMethods method)
        {
            fineAlignMethod = method;
        }
        #endregion

        #endregion

        #region Code

        private double[] GuessBackgroundValue(int cellSize, int imageNumber, int buffer)
        {
            var cellLocation = Locations[imageNumber];
            int minCellX = (int)(cellLocation.CellCenter.X - cellSize / 2 - buffer);
            int maxCellX = (int)(cellLocation.CellCenter.X + cellSize / 2 + buffer);
            int minCellY = (int)(cellLocation.CellCenter.Y - cellSize / 2 - buffer);
            int maxCellY = (int)(cellLocation.CellCenter.Y + cellSize / 2 + buffer);


            var imageData = Library[imageNumber].Data;
            int width = Library[imageNumber].Width;
            int height = Library[imageNumber].Height;

            double sum = 0;
            long cc = 0;

            List<double> averages = new List<double>();

            Rectangle ROI = new Rectangle(minCellX - buffer, minCellY - buffer, buffer, buffer);
            //top left
            for (int x = ROI.X; x < ROI.Right; x++)
                for (int y = ROI.Y; y < ROI.Bottom; y++)
                {
                    if (x > 0 && y > 0)
                    {
                        if (x < width && y < height)
                        {
                            sum += imageData[y, x, 0];
                            cc++;
                        }
                    }
                }

            if (cc > 10)
                averages.Add(sum / cc);


            //top right
            ROI = new Rectangle(maxCellX, minCellY - buffer, buffer, buffer);
            sum = 0;
            cc = 0;
            for (int x = ROI.X; x < ROI.Right; x++)
                for (int y = ROI.Y; y < ROI.Bottom; y++)
                {
                    if (x > 0 && y > 0)
                    {
                        if (x < width && y < height)
                        {
                            sum += imageData[y, x, 0];
                            cc++;
                        }
                    }
                }
            if (cc > 10)
                averages.Add(sum / cc);

            ROI = new Rectangle(minCellX - buffer, maxCellY, buffer, buffer);
            //bottom left
            sum = 0;
            cc = 0;
            for (int x = ROI.X; x < ROI.Right; x++)
                for (int y = ROI.Y; y < ROI.Bottom; y++)
                {
                    if (x > 0 && y > 0)
                    {
                        if (x < width && y < height)
                        {
                            sum += imageData[y, x, 0];
                            cc++;
                        }
                    }
                }

            if (cc > 10)
                averages.Add(sum / cc);

            //bottom right
            ROI = new Rectangle(maxCellX, maxCellY, buffer, buffer);
            sum = 0;
            cc = 0;
            for (int x = ROI.X; x < ROI.Right; x++)
                for (int y = ROI.Y; y < ROI.Bottom; y++)
                {
                    if (x > 0 && y > 0)
                    {
                        if (x < width && y < height)
                        {
                            sum += imageData[y, x, 0];
                            cc++;
                        }
                    }
                }
            if (cc > 10)
                averages.Add(sum / cc);

            if (averages.Count == 0)
                return null;
            else
                return averages.ToArray();
        }


        private double[] EstimateBackgroundValue(int imageNumber)
        {
            double[] averages = null;
            int cellSize = CellSize;
            int buffer = Buffer;
            do
            {
                averages = GuessBackgroundValue(cellSize, imageNumber, buffer);

                cellSize = (int)(cellSize * .75);
                buffer = 10;

            } while (averages == null);

            return averages;
        }

        private int ImprovedCellSize(int suggestedCellSize)
        {
            const int Buffer = 30;

            int halfCellSize = (int)(suggestedCellSize / 2.0d);
            Image<Gray, float> AverageImage = new Image<Gray, float>(halfCellSize * 2 + 2 * Buffer, halfCellSize * 2 + 2 * Buffer);
            for (int i = 0; i < 100; i += 10)
            {
                var cellLocation = Locations[i];
                int minCellX = (int)(cellLocation.CellCenter.X - halfCellSize - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + halfCellSize + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - halfCellSize - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + halfCellSize + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);


                var image = ReconstructCells.Background.RoughBackgrounds.RemoveBackgroundMasked(Library[i], BackGround, ROI);

                AverageImage = AverageImage.Add(image);
            }

            AverageImage = AverageImage.Mul(1.0 / 10.0);

            Point center;
            int cellWidth;
            bool touchesBorder = false;
            //if (!mPassData.FluorImage)
            ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Threshold(AverageImage, new Point(AverageImage.Width / 2, AverageImage.Height / 2), out center, out cellWidth, out touchesBorder, .85);
            //  else
            //    ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Threshold(AverageImage, new Point(AverageImage.Width / 2, AverageImage.Height / 2), out center, out cellWidth, out touchesBorder, .85);

            return cellWidth * 2;
        }

        private void BatchInvertBackground(int imageNumber)
        {
            var image = Library2[imageNumber];

            if (mPassData.FluorImage)
            {
                //   image = image.Rotate(90, new Gray(0));
            }
            else
            {
                // image = image.Add(new Gray(45000));
                // ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, true);
            }
            PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image);

            int nCellSize = (int)(Math.Floor(((double)image.Width - TrimSize) / 2));

            var ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);



            if (ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height)
                Library2[imageNumber] = image.Copy(ROI);
            else
            {
                Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
                var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                if (ROI.X < 0)
                {
                    ROIdest.X = -1 * ROI.X;
                    int right = ROI.Right;
                    ROIsrc.X = 0; ROIsrc.Width = right;
                    ROIdest.Width = right;
                }

                if (ROI.Y < 0)
                {
                    ROIdest.Y = -1 * ROI.Y;
                    int bottom = ROI.Bottom;
                    ROIsrc.Y = 0; ROIsrc.Height = bottom;
                    ROIdest.Height = bottom;
                }

                if (ROI.Right > image.Width)
                {
                    ROIsrc.Width = image.Width - ROI.X;
                    ROIdest.Width = ROIsrc.Width;
                }

                if (ROI.Bottom > image.Height)
                {
                    ROIsrc.Height = image.Height - ROI.Y;
                    ROIdest.Height = ROIsrc.Height;
                }

                // nImage.ROI = ROIdest;
                // image.ROI = ROIsrc;

                //nImage = image.Copy();
                int xx = ROIdest.X;
                for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                {
                    int yy = ROIdest.Y;
                    for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                    {
                        nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                        yy++;
                    }
                    xx++;
                }

                //image.CopyTo(nImage);
                Library2[imageNumber] = nImage;
            }

        }
#if TESTING
        Image<Gray, float> badBack;

        private void BatchRemoveBackgroundTest(int imageNumber)
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;
                Buffer = 100;
                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
            
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];

                try
                {
                    EdgeValues[imageNumber] = 0;
                }
                catch { }

                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);
                image.MinMax(out min, out max, out pm, out pM);

                EdgeRange[imageNumber] = (float)(max[0] - min[0]);

                int sizeIncrease = 1;

                //  ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image);
                //   ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                Library[imageNumber] = image;

              //  return;
        #region refine cutting
                PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image);
               
                int nCellSize =  (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

              
                cellLocation.CellCenter.X += (int)(p.X - cellHalf);
                cellLocation.CellCenter.Y += (int)(p.Y - cellHalf);

                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    image = image.Copy(ROI);
                    Library[imageNumber] = image;
                }
                else
                {
        #region boundry Problems ___ lazy problem
                    Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                    var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
                    var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                    if (ROI.X < 0)
                    {
                        ROIdest.X = -1 * ROI.X;
                        int right = ROI.Right;
                        ROIsrc.X = 0; ROIsrc.Width = right;
                        ROIdest.Width = right;
                    }

                    if (ROI.Y < 0)
                    {
                        ROIdest.Y = -1 * ROI.Y;
                        int bottom = ROI.Bottom;
                        ROIsrc.Y = 0; ROIsrc.Height = bottom;
                        ROIdest.Height = bottom;
                    }

                    if (ROI.Right > image.Width)
                    {
                        ROIsrc.Width = image.Width - ROI.X;
                        ROIdest.Width = ROIsrc.Width;
                    }

                    if (ROI.Bottom > image.Height)
                    {
                        ROIsrc.Height = image.Height - ROI.Y;
                        ROIdest.Height = ROIsrc.Height;
                    }

                    // nImage.ROI = ROIdest;
                    // image.ROI = ROIsrc;

                    //nImage = image.Copy();
                    int xx = ROIdest.X;
                    for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                    {
                        int yy = ROIdest.Y;
                        for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                            yy++;
                        }
                        xx++;
                    }

                    //image.CopyTo(nImage);
                    Library[imageNumber] = nImage;
        #endregion
                }
        #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }

            //  Library[imageNumber] = image;
        }
#endif
#if PAPER_TESTS
        #region Correlation
        private Image<Gray, float> CutImage(Image<Gray, float> image, Rectangle ROI)
        {
            if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
            {
                image = image.Copy(ROI);
            }
            else
            {
        #region boundry Problems ___ lazy problem
                Image<Gray, float> nImage = image.Copy();

                var ROIdest = new Rectangle(0, 0, ROI.Width, ROI.Width);
                var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                if (ROI.X < 0)
                {
                    ROIdest.X = -1 * ROI.X;
                    int right = ROI.Right;
                    ROIsrc.X = 0; ROIsrc.Width = right;
                    ROIdest.Width = right;
                }

                if (ROI.Y < 0)
                {
                    ROIdest.Y = -1 * ROI.Y;
                    int bottom = ROI.Bottom;
                    ROIsrc.Y = 0; ROIsrc.Height = bottom;
                    ROIdest.Height = bottom;
                }

                if (ROI.Right > image.Width)
                {
                    ROIsrc.Width = image.Width - ROI.X;
                    ROIdest.Width = ROIsrc.Width;
                }

                if (ROI.Bottom > image.Height)
                {
                    ROIsrc.Height = image.Height - ROI.Y;
                    ROIdest.Height = ROIsrc.Height;
                }


                int xx = ROIdest.X;
                for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                {
                    int yy = ROIdest.Y;
                    for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                    {
                        if (yy > 0 && xx > 0 && yy < image.Height && xx < image.Width)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                        }
                        yy++;
                    }
                    xx++;
                }


                image = nImage;
                #endregion
            }
            return image;
        }
        private void CrossCorrelateLib(ref OnDemandImageLibrary library)
        {
            TrimSize = 6;
            int nCellSize = library[10].Width;// (int)(Math.Floor(((double)library[10].Width - TrimSize) / 2));
            Rectangle ROI;
            for (int j = 0; j < library.Count; j += 11)
            {
                var compImage = library[j];
                for (int i = j + 1; i < 12; i++)
                {
                    var image = library[i];
                    PointF p = MathLibrary.FFT.MathFFTHelps.FindShift(compImage.Data, image.Data);
                    ROI = new Rectangle((int)(Math.Floor(p.X)), (int)(Math.Floor(p.Y)), 2 * nCellSize, 2 * nCellSize);
                    library[i] = CutImage(image, ROI);
                }
                ROI = new Rectangle((int)Math.Floor(TrimSize / 2d), (int)Math.Floor(TrimSize / 2d), 2 * nCellSize, 2 * nCellSize);
                library[j] = CutImage(library[j], ROI);
            }
        }
        #endregion
        private PointF COGNoised(Image<Gray, float> image)
        {
            float xx = 0, yy = 0;
            for (int i = 0; i < 4; i++)
            {
                var imageT = image.Clone();
                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        imageT.Data[y, x, 0] += (float)(Utilities.SimpleRNG.GetNormal() * 1000);
                    }
                PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(imageT);
                xx += p.X;
                yy += p.Y;
            }
            return new PointF(xx / 4f, yy / 4f);
        }

        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;
                if (!mPassData.FluorImage)
                    Buffer = 20;
                else
                    Buffer = 5;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);


                if (Program.TestMode == "0")
                {
                    ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\brightImages\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);

                    string index = string.Format("  {0}", imageNumber);
                    index = index.Substring(index.Length - 3);
                    ImageProcessing.ImageFileLoader.Save_16bit_TIFF(Program.dataFolder + "\\brightImages16\\imageTIFs" + index + ".tif", image, 1, 0);
                }
                else if (Program.TestMode == "5")
                {
                    image = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\testbad\data\bmFiltered\image" + imageNumber + ".tif");
                }
                else if (Program.TestMode == "6")
                {
                    string index = string.Format("  {0}", imageNumber);
                    index = index.Substring(index.Length - 3);
                    image = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\testbad\data\safirFiltered\image" + index + ".tif");
                }
                //image = image.SmoothMedian(3).PyrUp();
                //  image= image.PyrUp();
                //  ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);

                //   ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                if (Program.TestMode == "0")
                {
                    Library[imageNumber] = image;
                    return;
                }

                int sizeIncrease = 1;
               // if (Program.TestMode != "5" && Program.TestMode != "6")
                {
                    mPassData.DataScaling = sizeIncrease;
                    //sizeIncrease = mPassData.DataScaling;

                    //image = image.PyrUp();
                }
        #region refine cutting
                PointF p;

                if (Program.TestMode == "1")
                    p = COGNoised(image);
                else
                {
                    var image2 = image.Copy();
                    ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                    p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image2);
                }

                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);



                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    image = image.Copy(ROI);
                }
                else
                {
        #region boundry Problems ___ lazy problem
                    Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                    var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
                    var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                    if (ROI.X < 0)
                    {
                        ROIdest.X = -1 * ROI.X;
                        int right = ROI.Right;
                        ROIsrc.X = 0; ROIsrc.Width = right;
                        ROIdest.Width = right;
                    }

                    if (ROI.Y < 0)
                    {
                        ROIdest.Y = -1 * ROI.Y;
                        int bottom = ROI.Bottom;
                        ROIsrc.Y = 0; ROIsrc.Height = bottom;
                        ROIdest.Height = bottom;
                    }

                    if (ROI.Right > image.Width)
                    {
                        ROIsrc.Width = image.Width - ROI.X;
                        ROIdest.Width = ROIsrc.Width;
                    }

                    if (ROI.Bottom > image.Height)
                    {
                        ROIsrc.Height = image.Height - ROI.Y;
                        ROIdest.Height = ROIsrc.Height;
                    }

                    // nImage.ROI = ROIdest;
                    // image.ROI = ROIsrc;

                    //nImage = image.Copy();
                    int xx = ROIdest.X;
                    for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                    {
                        int yy = ROIdest.Y;
                        for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                            yy++;
                        }
                        xx++;
                    }

                    //image.CopyTo(nImage);
                    image = nImage;
                    #endregion
                }
                //#if PAPER_TESTS
                //                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\back_TIF\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);
                //                try
                //                {
                //                    ImageProcessing.ImageFileLoader.SaveFits(Program.dataFolder + "\\back_FIT\\imageFits" + string.Format("{0:000}.fit", imageNumber), image);
                //                }
                //                catch { }
                //#endif
                Library[imageNumber] = image;
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }

            //  Library[imageNumber] = image;
        }
#else
        private void BatchRemoveBackground(int imageNumber)
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;
                if (!mPassData.FluorImage)
                    Buffer = 30;
                else
                    Buffer = 5;

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

                //var image = Library[imageNumber];
                //ROI.Inflate(20, 20);
                //image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);


               // ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\brightImages\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);

                //string index = string.Format("  {0}", imageNumber);
                //index = index.Substring(index.Length - 3);
                //ImageProcessing.ImageFileLoader.Save_16bit_TIFF(Program.dataFolder + "\\brightImages16\\imageTIFs" + index + ".tif", image, 1, 0);


                //ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);
                //  ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\bigSino\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);



                ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                var image = Library[imageNumber];
                image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);

                image.MinMax(out min, out max, out pm, out pM);

                EdgeRange[imageNumber] = (float)(max[0] - min[0]);
                // image.MinMax(out min, out  max, out pm, out pM);

                mPassData.DataScaling = 1;
                int sizeIncrease =  mPassData.DataScaling;

               
            //    image = image.PyrUp();
                var image2 = image.Copy();
                //image = image.SmoothMedian(3).PyrUp();
                //  image= image.PyrUp();
                //  ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                //   ImageProcessing._2D.ImageManipulation.InvertImageAndClip(image);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);


                if (fineAlignMethod == AlignMethods.COG)
                {
                    Library[imageNumber] = image;
                    return;
                }

                //  return ;
                #region refine cutting
                PointF p = ImageProcessing.BlobFinding.CenterOfGravity.SimpleCenterOfGravity(image2);
                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

                ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

                cellLocation.CellCenter.X += (int)(p.X - cellHalf);
                cellLocation.CellCenter.Y += (int)(p.Y - cellHalf);

                // beforeRoughness[imageNumber] = cellLocation.CellCenter.X;
                // afterRoughness[imageNumber] = cellLocation.CellCenter.Y;
                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    image = image.Copy(ROI);
                }
                else
                {
                    #region boundry Problems ___ lazy problem
                    Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                    var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
                    var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                    if (ROI.X < 0)
                    {
                        ROIdest.X = -1 * ROI.X;
                        int right = ROI.Right;
                        ROIsrc.X = 0; ROIsrc.Width = right;
                        ROIdest.Width = right;
                    }

                    if (ROI.Y < 0)
                    {
                        ROIdest.Y = -1 * ROI.Y;
                        int bottom = ROI.Bottom;
                        ROIsrc.Y = 0; ROIsrc.Height = bottom;
                        ROIdest.Height = bottom;
                    }

                    if (ROI.Right > image.Width)
                    {
                        ROIsrc.Width = image.Width - ROI.X;
                        ROIdest.Width = ROIsrc.Width;
                    }

                    if (ROI.Bottom > image.Height)
                    {
                        ROIsrc.Height = image.Height - ROI.Y;
                        ROIdest.Height = ROIsrc.Height;
                    }

                    // nImage.ROI = ROIdest;
                    // image.ROI = ROIsrc;

                    //nImage = image.Copy();
                    int xx = ROIdest.X;
                    for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                    {
                        int yy = ROIdest.Y;
                        for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                            yy++;
                        }
                        xx++;
                    }

                    //image.CopyTo(nImage);
                    image = nImage;
                    #endregion
                }
                //#if PAPER_TESTS
                //                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\back_TIF\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), image);
                //                try
                //                {
                //                    ImageProcessing.ImageFileLoader.SaveFits(Program.dataFolder + "\\back_FIT\\imageFits" + string.Format("{0:000}.fit", imageNumber), image);
                //                }
                //                catch { }
                //#endif
                Library[imageNumber] = image;
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();
                MessageBox.Show(ex.Message);
            }

            //  Library[imageNumber] = image;
        }
#endif

        private void RemoveBackgrounds()
        {
            if (mPassData.FluorImage)
                CellSize = (int)(suggestedCellSize * 1);
            else
                CellSize = (int)(suggestedCellSize * 1.5);// (int)(ImprovedCellSize(suggestedCellSize) * 1.1);


            //  BatchRemoveBackground(357);

            //List<double> averages = new List<double>();
            //averages.AddRange(EstimateBackgroundValue(1));
            //averages.AddRange(EstimateBackgroundValue(125));
            //averages.AddRange(EstimateBackgroundValue(250));
            //averages.AddRange(EstimateBackgroundValue(375));
            //averages.AddRange(EstimateBackgroundValue(490));

            //averages.Sort();
            //BackgroundAverage = (float)averages[averages.Count / 2];
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
           // po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;
            //  BatchRemoveBackground(336);
#if TESTING
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackgroundTest(x));
#else
            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchRemoveBackground(x));
#endif
        }

        private void AddNoise()
        {

            string DataFolder = Program.DataFolder;

            for (int iter = 0; iter < 7; iter += 1)
            {
                Library2 = new OnDemandImageLibrary(Library.Count, true, "c:\\temp", false);

               // ParallelOptions po = new ParallelOptions();
               // po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

                int numberOfImages = Library2.Count;

                // Parallel.For(0, numberOfImages, po, x => BatchInvertBackground(x));

                double ave = 30;// Library2[1].GetAverage().Intensity;
                //double[] minV,maxV;
                //Point[] mP,MP;
                //Library2[1].MinMax(out minV, out maxV, out mP, out MP);

                //minV[0] = minV[0] + maxV[0];
                ave = 25 * Math.Pow(2, iter);
                for (int i = 0; i < Library2.Count; i++)
                {
                    var image = Library[i].Copy();
                    for (int x = 0; x < image.Width; x++)
                        for (int y = 0; y < image.Height; y++)
                        {
                            image.Data[y, x, 0] += (float)(Utilities.SimpleRNG.GetNormal() * ave);
                        }
                    Library2[i] = image;
                }

                PassData pass = new PassData();
                pass.Library = Library2;

                var ps = new Tomography.PseudoSiddon();
                ps.setFilter("Han", 1024);
                ps.SetInput(pass);
                ps.RunNode();

                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_noise_" + iter + "\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);
                }
                catch { }
            }
        }

        private void DoFilteredWork()
        {
            try
            {
                double[] min, max;
                Point[] pm, pM;

                string DataFolder = Program.DataFolder;

                for (int i = 0; i < Library2.Count; i++)
                {
                    Library2[i].MinMax(out min, out max, out pm, out pM);
                    var image = Library2[i].Add(new Gray(UInt16.MaxValue - max[0]));
                    string index = string.Format("  {0}", i);
                    index = index.Substring(index.Length - 3);
                    ImageProcessing.ImageFileLoader.Save_16bit_TIFF(Program.DataFolder + "\\preFiltered\\image" + index + ".tif", image, 1, 0);
                }


                string[] Files = Directory.GetFiles(Program.DataFolder + "\\prefiltered");
                //   return;
                //{

                //    Process p = new Process();
                //    p.StartInfo.FileName = (@"C:\Program Files (x86)\ndsafir\ndsafir.exe");
                //    p.StartInfo.Arguments = "-sampling 2 - iter 1 -i " + DataFolder + "\\prefiltered\\image%3d.tif -first 0 -last " + (Files.Length - 1) + " -o " + DataFolder + "\\filtered\\image%3d.tif  ";

                //    p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                //    p.Start();

                //    p.WaitForExit();

                //}


                Library2 = new OnDemandImageLibrary(DataFolder + "\\filtered", true, "c:\\temp", false);

              //  ParallelOptions po = new ParallelOptions();
              //  po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

                int numberOfImages = Library2.Count;

                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertBackground(x));

                Library2.SaveImages(Program.DataFolder + "\\filtered2\\image.tif");

                PassData pass = new PassData();
                pass.Library = Library2;

                var ps = new Tomography.PseudoSiddon();
                ps.setFilter("RamLak", 1024);
                ps.SetInput(pass);
                ps.RunNode();

                // ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_FBP_P.tif", ps.GetOutput().DensityGrid);
                ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_FBP_P.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

                //try
                //{
                //    Directory.CreateDirectory(Program.dataFolder + "\\projectionobject_P_FBP");
                //}
                //catch { }
                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_P_FBP_16\\image.tif", ps.GetOutput().DensityGrid, 0, 16);
                }
                catch { }
                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_P_FBP_8\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);
                }
                catch { }

                var CrossSections = ps.GetOutput().DensityGrid.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_FBP_P.jpg");
                CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_FBP_P.jpg");
                CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_FBP_P.jpg");

                var mip = new Visualizations.MIP();
                mip.setNumberProjections(40);
                mip.setFileName(Program.DataFolder + "\\mip_FBP_P.avi");
                mip.SetInput(ps.GetOutput());

                mip.RunNode();

                var ps2 = new Tomography.SIRTRecon();
                ps2.SetInput(pass);
                ps2.RunNode();

                //ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_Tik_P.tif", ps.GetOutput().DensityGrid);
                ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_SIRT_P.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

                //try
                //{
                //    Directory.CreateDirectory(Program.dataFolder + "\\projectionobject_P_sirt");
                //}
                //catch { }
                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_P_sirt_16\\image.tif", ps.GetOutput().DensityGrid, 0, 16);
                }
                catch { }
                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_P_sirt_8\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);
                }
                catch { }

                CrossSections = ps.GetOutput().DensityGrid.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_sirt_P.jpg");
                CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_sirt_P.jpg");
                CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_sirt_P.jpg");

                mip = new Visualizations.MIP();
                mip.setNumberProjections(40);
                mip.setFileName(Program.DataFolder + "\\mip_sirt_P.avi");
                mip.SetInput(ps.GetOutput());

                mip.RunNode();
            }
            catch { }
        }

        #endregion


        protected override void RunNodeImpl()
        {
            Library = mPassData.Library;
            Locations = mPassData.Locations;

#if TESTING
            string DataFolder = Program.dataFolder;
            defaultWidth = Library[10].Width;


            //try
            //{
            //    //if (Directory.Exists(Program.dataFolder + "\\prefiltered"))
            //    //    Directory.Delete(Program.dataFolder + "\\prefiltered", true);

            //    //if (Directory.Exists(Program.dataFolder + "\\filtered"))
            //    //    Directory.Delete(Program.dataFolder + "\\filtered", true);

            //    Directory.CreateDirectory(Program.dataFolder + "\\prefiltered2");
            //    Directory.CreateDirectory(Program.dataFolder + "\\prefiltered");
            //    Directory.CreateDirectory(Program.dataFolder + "\\filtered");
            //}
            //catch { }


         //   Library2 = new OnDemandImageLibrary(Library.Count, true, "c:\\temp", false);

         //   badBack = BackGround.SmoothGaussian(21);// Library[10].CopyBlank().Add(new Gray(1));
         ////   badBack =  Library[10].CopyBlank().Add(new Gray(1));
         //   ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\badBack.tif", badBack);

            beforeRoughness = new float[Library.Count];
            afterRoughness = new float[Library.Count];

#endif
#if PAPER_TESTS
            // Library2 = new OnDemandImageLibrary(Library.Count, true, "c:\\temp", false);
            //Directory.CreateDirectory(Program.dataFolder + "\\back_TIF");
            //Directory.CreateDirectory(Program.dataFolder + "\\back_FIT");
            if (Program.TestMode == "0")
            {
                Directory.CreateDirectory(Program.dataFolder + "\\brightImages");
                Directory.CreateDirectory(Program.dataFolder + "\\brightImages16");
            }

#endif
            string fold = Program.DataFolder + "\\brightImages";
            Directory.CreateDirectory(fold);
            //  Directory.CreateDirectory(Program.dataFolder + "\\brightImages16");
            // Directory.CreateDirectory(Program.dataFolder + "\\OBD");
            // Directory.CreateDirectory(Program.dataFolder + "\\bigSino");
            EdgeValues = new float[Library.Count];
            EdgeRange = new float[Library.Count];


            ImageMinValues = new float[Library.Count];

            RemoveBackgrounds();



#if PAPER_TESTS

            //Library2.SaveImages(@"C:\temp\registered\image.tif");

            string label = "";
            if (Program.TestMode == "0")
                label = "Center of Gravity";
            else if (Program.TestMode == "1")
                label = "Noised COG";
            else if (Program.TestMode == "2")
            {
                label = "Cross Correlate";
                CrossCorrelateLib(ref Library);
                // Library2.SaveImages(@"C:\temp\registered\image.tif");
            }
            else if (Program.TestMode == "4")
                label = "Refined COG";
            else if (Program.TestMode == "3" || Program.TestMode == "5" || Program.TestMode == "6")
            {
                label = "Align by Recon";
                Registration.AlignByRecon ar = new Registration.AlignByRecon();

                ar.SetInput(mPassData);
                ar.setNumberOfProjections(125);
                ar.setScale(1);
                ar.RunNode();

                Library = ar.GetOutput().Library;
            }

            //if (Program.TestMode != "0")// && Program.TestMode != "5" && Program.TestMode != "6")
            //{
            //    for (int i = 0; i < Library.Count; i++)
            //        Library[i] = Library[i].PyrDown();
            //}
            mPassData.DataScaling = 1;
            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(Library, label);

#else
            switch (fineAlignMethod)
            {
                case AlignMethods.tCC:
                    Registration.AlignByRecon ar = new Registration.AlignByRecon();
                    ar.SetInput(mPassData);
                    ar.setNumberOfProjections(125);
                    ar.setScale(1);
                    ar.RunNode();
                    break;
               
            }

#endif

        }


        #region Extras

        public Image<Gray,float> MakeSingoGram(int slice)
        {
            float[,,] sinoGram = new float[Library.Count, Library[0].Height,1];
            for (int i = 0; i < Library.Count; i++)
            {
                for (int j = 0; j < Library[i].Height; j++)
                {
                    sinoGram[i, j,0] = Library[i].Data[j, slice, 0];
                }
            }


            return new Image<Gray,float>( sinoGram);
        }


        public static Image<Gray, float> MakeSinoGram(int slice, OnDemandImageLibrary lib, Image<Gray,float> background, CellLocation[] locations)
        {
            var image = ClipImage(lib[0], background, locations[0], 250);
            float[, ,] sinoGram = new float[lib.Count, image.Height, 1];
            for (int i = 0; i < lib.Count; i++)
            {
                 image = ClipImage(lib[i], background, locations[i], 250);
                for (int j = 0; j < image.Height; j++)
                {
                    sinoGram[i, j, 0] = image.Data[j, image.Width/2, 0];
                }
            }


            return new Image<Gray, float>(sinoGram);

        }


        private static  Image<Gray,float> ClipImage(Image<Gray,float> image, Image<Gray,float> BackGround, CellLocation cellLocation,int CellSize)
        {
            try
            {
               
              
              int      Buffer = 30;
              

             
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                int maxCellX = (int)(cellLocation.CellCenter.X + CellSize / 2 + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                int maxCellY = (int)(cellLocation.CellCenter.Y + CellSize / 2 + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, maxCellX - minCellX, maxCellY - minCellY);
                int cellHalf = (int)((ROI.Width) / 2d);

              
                ROI.Inflate(20, 20);
                return  ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);

            }
            catch 
            {
                return null;
            }

            //  Library[imageNumber] = image;
        }
        #endregion
    }
}
