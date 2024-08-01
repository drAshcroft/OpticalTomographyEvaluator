using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageProcessing
{
    public static class ArraysToBitmaps
    {
        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static int[,] unmakeBitmapInt(this Bitmap ImageArray)
        {
            int iWidth = ImageArray.Width;
            int iHeight = ImageArray.Height;

            BitmapData bmd = ImageArray.LockBits(new Rectangle(0, 0, ImageArray.Width, ImageArray.Height), ImageLockMode.ReadOnly, ImageArray.PixelFormat);

            int[,] outArray = new int[iWidth, iHeight];
            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = ((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        outArray[x, y] = bits[0] + bits[1] + bits[2];
                        scanline += 3;
                    }
                }
            }
            ImageArray.UnlockBits(bmd);
            return outArray;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static double [,] unmakeBitmap(this Bitmap ImageArray)
        {
            int iWidth = ImageArray.Width;
            int iHeight = ImageArray.Height;

            BitmapData bmd = ImageArray.LockBits(new Rectangle(0, 0, ImageArray.Width, ImageArray.Height), ImageLockMode.ReadOnly, ImageArray.PixelFormat);

            double[,] outArray = new double[iWidth, iHeight];
            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = ((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        outArray[x, y] = bits[0] + bits[1] + bits[2];
                        scanline+=3;
                    }
                }
            }
            ImageArray.UnlockBits(bmd);
            return outArray;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j] && ImageArray[i, j] != 0 && ImageArray[i, j] > -90000) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);

            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        if (g < 0) g = 0;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }


        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap24FlipHorizonal(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            int fWidth = iWidth - 1;
            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = (byte*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[fWidth - x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline += 3;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap24(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = (byte*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline += 3;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        /// <summary>
        /// Converts offsetX 2D array to offsetX intensity bitmap
        /// Uses the corners to set the min of the bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmapCorner(this double[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }

            iMin = ImageArray[5, 5];

            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this double[,] ImageArray, double MinContrast, double MaxContrast)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = MaxContrast;
            double iMin = MinContrast;
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this float[,] ImageArray, float MinContrast, float MaxContrast)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = MaxContrast;
            double iMin = MinContrast;
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

        public static Bitmap MakeBitmap(this float[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }


        public static Bitmap MakeScaledBitmap(this Image<Gray,float> ImageArray, float min,float max)
        {
            int iWidth = ImageArray.Width;
            int iHeight = ImageArray.Height;
            double iMax = max;
            double iMin = min;

           
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);

            float[, ,] imageArray = ImageArray.Data;

            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (imageArray[y, x,0] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }
        /*public static Bitmap MakeBitmap(this float[,] ImageArray, double  MinContrast, float MaxContrast)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = MaxContrast;
            double iMin = MinContrast;
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = 255;// g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }*/

        /// <summary>
        /// Converts to intensity bitmap
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns></returns>
        public static Bitmap MakeBitmap(this Int32[,] ImageArray)
        {
            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);
            double iMax = -10000;
            double iMin = 10000;

            for (int i = 0; i < iWidth; i++)
                for (int j = 0; j < iHeight; j++)
                {
                    if (iMax < ImageArray[i, j]) iMax = ImageArray[i, j];
                    if (iMin > ImageArray[i, j]) iMin = ImageArray[i, j];
                }
            double iLength = iMax - iMin;


            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);


            unsafe
            {
                for (int y = 0; y < iHeight; y++)
                {
                    Int32* scanline = (Int32*)((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        byte* bits = (byte*)scanline;
                        int g = (int)(255d * (ImageArray[x, y] - iMin) / iLength);
                        if (g > 255) g = 255;
                        byte g2 = (byte)g;
                        bits[0] = g2;
                        bits[1] = g2;
                        bits[2] = g2;
                        scanline++;
                    }
                }
            }
            b.UnlockBits(bmd);
            return b;
        }

      
    }
}
