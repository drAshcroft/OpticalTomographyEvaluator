using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace ImageProcessing.BlobFinding
{
    public class CenterOfGravity
    {
        private static Point CenterOfGravityThresholded(Image<Bgr, byte> threshImage)
        {
            double X = 0, Y = 0;
            int CC = 0;
            for (int x = 0; x < threshImage.Width; x++)
                for (int y = 0; y < threshImage.Height; y++)
                {
                    if (threshImage.Data[y, x, 0] == 0)
                    {
                        X += x;
                        Y += y;
                        CC++;
                    }
                }

            return new Point((int)((double)X / CC), (int)((double)Y / CC));

        }

        public static void FindOTSUThreshold(Image<Gray, float> image, out int  Threshold  )
        {
            Bgr Average;
          
            var imageThresh = image.Convert<Bgr, byte>();
            Average = imageThresh.GetAverage();
            var thresholded = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));

            int Height = image.Height /2;
            for (int x=0;x<image.Width ;x++)
            {
                if (thresholded.Data[Height,x,0]==0)
                {
                    Threshold = imageThresh.Data [Height,x,0];
                    return ;
                }
            }

            Height = image.Height /3;
            for (int x=0;x<image.Width ;x++)
            {
                if (thresholded.Data[Height,x,0]==0)
                {
                    Threshold = imageThresh.Data [Height,x,0];
                    return ;
                }
            }

             Height = (image.Height*2) /3;
            for (int x=0;x<image.Width ;x++)
            {
                if (thresholded.Data[Height,x,0]==0)
                {
                    Threshold = imageThresh.Data [Height,x,0];
                    return ;
                }
            }

            Threshold =0;
        }
      
        public static Image<Gray, float> InvertAndApplyOTSUMask(Image<Gray, float> image, bool removeMaskedArea)
        {
            Bgr Average;
            
            var imageThresh = image.Convert<Bgr, byte>();
            Average = imageThresh.GetAverage();
            var thresholded = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            CvInvoke.cvFloodFill(thresholded,new Point(image.Width/2,image.Height /2), new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);
            
            var thresholded2=image.CopyBlank();
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (thresholded.Data[y, x, 0] == 100)
                        thresholded2.Data[y, x, 0] = 0;
                    else
                        thresholded2.Data[y, x, 0] = 255;
                }

            var filtered = thresholded2.SmoothGaussian(121);


            double sum = 0, count = 0;
            float val;

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    val = image.Data[y, x, 0];
                    if (val != 1000000 && filtered.Data[y, x, 0] > .9)
                    {
                        sum += val;
                        count++;
                    }
                }

            float background = (float)(sum / count);

            if (removeMaskedArea)
            {
                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        val = (background - image.Data[y, x, 0]);// *filtered.Data[y, x, 0];
                        if (val < 0) val = 0;
                        image.Data[y, x, 0] = val;
                    }
            }
            else
            {
                   for (int x = 0; x < image.Width; x++)
                       for (int y = 0; y < image.Height; y++)
                       {
                           val = (background - image.Data[y, x, 0]);// *filtered.Data[y, x, 0];
                           image.Data[y, x, 0] = val;
                       }
            }


            return image;
        }

        public static void CenterOfGravityThreshold_Centered(Image<Gray, float> image, Point lastCenter, int Threshold, out Point CellCenter, out int XLength, out bool touchesBorder)
        {
            Bgr Average;
           

            var imageThresh = image.Convert<Bgr, byte>();
            Average = imageThresh.GetAverage();
           imageThresh= imageThresh.ThresholdBinary(new Bgr(Threshold,Threshold,Threshold ), new Bgr(Color.White ));
            //imageThresh = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            if (imageThresh.Data[lastCenter.Y, lastCenter.X, 0] != 0)
            {
                lastCenter = CenterOfGravityThresholded(imageThresh);

            }

            CvInvoke.cvFloodFill(imageThresh, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);


            double X = 0, Y = 0;
            int minX = int.MaxValue, maxX = int.MinValue;
            int CC = 0;
            touchesBorder = false;
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    //fill is at 100, so only accept these values
                    if (imageThresh.Data[y, x, 0] == 100)
                    {
                        if (x == 0 || y == 0 || x == imageThresh.Width - 1 || y == imageThresh.Height - 1)
                        {
                            touchesBorder = true;
                        }
                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        X += x;
                        Y += y;
                        CC++;
                    }
                }

            XLength = maxX - minX;

            CellCenter = new Point((int)(X / CC), (int)(Y / CC));
        }

        public static void BlobSize(Image<Gray, float> image,  out Size blobSize)
        {
            Bgr Average;
           

            //var imageThresh = image.Convert<Bgr, byte>();
            var imageThresh = new Image<Bgr, byte>(image.ScaledBitmap);
            Average = imageThresh.GetAverage();
            var   imageThresh2 = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            Point lastCenter = new Point(image.Width / 2, image.Height / 2);

            CvInvoke.cvFloodFill(imageThresh2, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);

            float maxThres = 0;
            float averageOut = 0,cc=0;

            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    if (imageThresh2.Data[y, x, 0] == 100)
                    {
                        if (imageThresh.Data[y, x, 0] > maxThres)
                            maxThres = imageThresh.Data[x, y, 0];
                    }
                    else
                    {
                        averageOut += imageThresh.Data[x, y, 0];
                        cc++;
                    }
                }

            averageOut =(( averageOut / cc) + 2*maxThres )/3f;
            //now take this improved threshold and try again

            imageThresh2 = imageThresh.ThresholdBinaryInv(new Bgr(averageOut, averageOut, averageOut), new Bgr(Color.White));
            mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            CvInvoke.cvFloodFill(imageThresh2, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
             cc = 0;
           
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    //fill is at 100, so only accept these values
                    if (imageThresh2.Data[y, x, 0] == 100)
                    {
                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        if (y > maxY) maxY = y;
                        if (y < minY) minY = y;
                    }
                }

            blobSize = new Size(maxX - minX, maxY - minY);
           
        }

        public static void BlobSizeCloseClip(Image<Gray, float> image, out Size blobSize)
        {
            Bgr Average;
           

            //var imageThresh = image.Convert<Bgr, byte>();
            var imageThresh = new Image<Bgr, byte>(image.ScaledBitmap);
            Average = imageThresh.GetAverage();
            var imageThresh2 = imageThresh.ThresholdBinary(new Bgr(220, 220, 220), new Bgr(Color.White));   //.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            Point lastCenter = new Point(image.Width / 2, image.Height / 2);

            CvInvoke.cvFloodFill(imageThresh2, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);

            float maxThres = 0;
            float averageOut = 0, cc = 0;

            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    if (imageThresh2.Data[y, x, 0] == 100)
                    {
                        if (imageThresh.Data[y, x, 0] > maxThres)
                            maxThres = imageThresh.Data[x, y, 0];
                    }
                    else
                    {
                        averageOut += imageThresh.Data[x, y, 0];
                        cc++;
                    }
                }

            averageOut = ((averageOut / cc) + 2 * maxThres) / 3f;
            //now take this improved threshold and try again

            imageThresh2 = imageThresh.ThresholdBinaryInv(new Bgr(averageOut, averageOut, averageOut), new Bgr(Color.White));
            mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            CvInvoke.cvFloodFill(imageThresh2, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            cc = 0;

            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    //fill is at 100, so only accept these values
                    if (imageThresh2.Data[y, x, 0] == 100)
                    {
                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        if (y > maxY) maxY = y;
                        if (y < minY) minY = y;
                    }
                }

            blobSize = new Size(maxX - minX, maxY - minY);

        }

        public static void CenterOfGravity_Centered(Image<Gray, float> image, Point lastCenter, out Point CellCenter, out int XLength,out bool touchesBorder )
        {
            Bgr Average;
            
            //var imageThresh = image.Convert<Bgr, byte>();
            var imageThresh = new Image<Bgr, byte>(image.ScaledBitmap);
            Average = imageThresh.GetAverage();
            imageThresh = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            if (imageThresh.Data[lastCenter.Y, lastCenter.X, 0] != 0)
            {
                lastCenter = CenterOfGravityThresholded(imageThresh);
            }

            CvInvoke.cvFloodFill(imageThresh, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);


            double X = 0, Y = 0;
            int minX = int.MaxValue, maxX = int.MinValue;
            int CC = 0;
            touchesBorder = false;
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    //fill is at 100, so only accept these values
                    if (imageThresh.Data[y, x, 0] == 100)
                    {
                        if (x == 0 || y == 0 || x == imageThresh.Width - 1 || y == imageThresh.Height - 1)
                        {
                            touchesBorder = true;
                        }
                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        X += x;
                        Y += y;
                        CC++;
                    }
                }

            XLength = maxX - minX;

            CellCenter = new Point((int)(X / CC), (int)(Y / CC));
        }

        public static void CenterOfGravityIntensity_Centered(Image<Gray, float> image, Point lastCenter, out Point CellCenter, out int XLength, out bool touchesBorder)
        {
            Bgr Average;
          

            var imageThresh = image.Convert<Bgr, byte>();
            Average = imageThresh.GetAverage();
            imageThresh = imageThresh.ThresholdOTSU(Average, new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            if (imageThresh.Data[lastCenter.Y, lastCenter.X, 0] != 0)
            {
                lastCenter = CenterOfGravityThresholded(imageThresh);

            }

            CvInvoke.cvFloodFill(imageThresh, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);


            var blurredImage = imageThresh.SmoothGaussian(41);

            double X = 0, Y = 0;
            int minX = int.MaxValue, maxX = int.MinValue;
            int CC = 0;
            double  weights=0,val,valBlur;
            touchesBorder = false;
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                   // if (blurredImage.Data[y, x, 0] == 50)
                    valBlur = blurredImage.Data[y, x, 0];
                    if (valBlur  > 100 && valBlur<200)
                    {
                        if (imageThresh.Data [y,x,0]==100 && ( x == 0 || y == 0 || x == imageThresh.Width - 1 || y == imageThresh.Height - 1))
                        {
                            touchesBorder = true;
                        }

                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        val = image.Data[y,x,0]/1000;
                        weights += val;
                        X += x * val;
                        Y += y*val;
                        CC++;
                    }
                }

            XLength = maxX - minX;

            CellCenter = new Point((int)(X / weights ), (int)(Y / weights ));
        }

        public static void CenterOfGravity_Threshold(Image<Gray, float> image, Point lastCenter, out Point CellCenter, out int XLength, out bool touchesBorder, double ThresholdPercent)
        {
            

            var imageThresh = image.Convert<Bgr, byte>();

            double maxValue = 0;
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    if (imageThresh.Data[y, x, 0] > maxValue) maxValue = imageThresh.Data[y, x, 0];
                }

            maxValue *= ThresholdPercent;

            imageThresh = imageThresh.ThresholdBinary(new Bgr(maxValue, maxValue, maxValue), new Bgr(Color.White));
            var mask = new Image<Gray, byte>(image.Width + 2, image.Height + 2);

            MCvConnectedComp comp = new MCvConnectedComp();
            //Point center = new Point((int)(image.Width / 2.0f), (int)(image.Height / 2.0f));

            CvInvoke.cvFloodFill(imageThresh, lastCenter, new MCvScalar(100, 100, 100, 255),
                    new MCvScalar(6, 6, 6),
                    new MCvScalar(6, 6, 6), out comp,
                    Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED,
                    Emgu.CV.CvEnum.FLOODFILL_FLAG.DEFAULT, mask);


            int SearchValue =100;
          //  if (imageThresh.Data[lastCenter.Y, lastCenter.X, 0] == 100)
          //      SearchValue = 0;

            double X = 0, Y = 0;
            int minX = int.MaxValue, maxX = int.MinValue;
            touchesBorder = false;
            int CC = 0;
            for (int x = 0; x < imageThresh.Width; x++)
                for (int y = 0; y < imageThresh.Height; y++)
                {
                    if (imageThresh.Data[y, x, 0] == SearchValue)
                    {
                        if (x == 0 || y == 0 || x == imageThresh.Width - 1 || y == imageThresh.Height - 1)
                        {
                            touchesBorder = true;
                        }

                        if (x > maxX) maxX = x;
                        if (x < minX) minX = x;
                        X += x;
                        Y += y;
                        CC++;
                    }
                }



            XLength = maxX - minX;
            CellCenter = new Point((int)(X / CC), (int)(Y / CC));
        }


        public static PointF SimpleCenterOfGravity(Image<Gray, float> image)
        {
            var moments =  image.GetMoments(false);

            return  new PointF((float)moments.GravityCenter.x, (float)moments.GravityCenter.y);
        }
    }
}
