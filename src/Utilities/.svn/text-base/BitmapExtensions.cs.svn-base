using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities
{
    public static class BitmapExtensions
    {
        public static double[,] ConvertToDoubleArray(this Bitmap Image, bool Rotate90)
        {
            if (Rotate90)
                Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            int iWidth = Image.Width;
            int iHeight = Image.Height;

            double[,] ImageArray = new double[iWidth, iHeight];

            BitmapData bmd = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, Image.PixelFormat);

            double g1, g2, g3;
            unsafe
            {

                if (bmd.Stride / (double)bmd.Width == 4)
                {
                    for (int y = 0; y < iHeight; y++)
                    {
                        Int32* scanline = (Int32*)((byte*)bmd.Scan0 + y * bmd.Stride);

                        for (int x = 0; x < iWidth; x++)
                        {
                            byte* bits = (byte*)scanline;
                            g1 = bits[0];
                            g2 = bits[1];
                            g3 = bits[2];

                            ImageArray[x, y] = (g1 + g2 + g3) / 3d;
                            scanline++;
                        }
                    }
                }
                else if (bmd.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    for (int y = 0; y < iHeight; y++)
                    {
                        byte* scanline = ((byte*)bmd.Scan0 + y * bmd.Stride);

                        for (int x = 0; x < iWidth; x++)
                        {
                            byte* bits = (byte*)scanline;
                            g1 = bits[0];
                            g2 = bits[1];
                            g3 = bits[2];

                            ImageArray[x, y] = (g1 + g2 + g3) / 3d;
                            scanline += 3;
                        }
                    }

                }
            }
            Image.UnlockBits(bmd);
            return ImageArray;
        }

    }
}
