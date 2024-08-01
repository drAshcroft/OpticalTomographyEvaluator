using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Emgu.CV.Structure;
using Emgu.CV;

namespace ImageProcessing._3D
{
    public static class ArrayToBitmap3D
    {
        public static Bitmap[] ShowCross(this float[, ,] DensityGrid)
        {
            #region Save as Images

            int ZSlices = DensityGrid.GetLength(0);
            int XSlices = DensityGrid.GetLength(1);
            int YSlices = DensityGrid.GetLength(2);

            Bitmap[] b = new Bitmap[3];

            double[,] Slice = new double[YSlices, XSlices];
            double d;
            int Hl = XSlices - 1, Wl = YSlices - 1;
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    d = DensityGrid[ZSlices / 2, Wl - x, Hl - y];
                    if (d > 0)
                        d =Math.Pow( d,1.5);
                    else
                        d = 0;
                    Slice[y, x] = d;

                }
            }
            b[0] = Slice.MakeBitmap();

            double[,] SliceX = new double[XSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    d=DensityGrid[z, x, YSlices / 2];
                    if (d > 0)
                        d = Math.Pow(d, 1.5);
                    else
                        d = 0;
                    SliceX[x, z] = d;// 
                }
            }
            b[1] = SliceX.MakeBitmap();
            double[,] SliceY = new double[YSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int y = 0; y < YSlices; y++)
                {
                    d = DensityGrid[z, XSlices / 2, y];
                    if (d > 0)
                        d = Math.Pow(d, 1.5);
                    else
                        d = 0;
                    SliceY[y, z] = d;// 
                }
            }
            b[2] = SliceY.MakeBitmap();
            #endregion
            return b;
        }

        public static Image<Gray, float>[] ShowCrossHigh(this float[, ,] DensityGrid)
        {
            #region Save as Images

            int ZSlices = DensityGrid.GetLength(0);
            int XSlices = DensityGrid.GetLength(1);
            int YSlices = DensityGrid.GetLength(2);

            Image<Gray, float>[] b = new Image<Gray, float>[3];

            b[0] = new Image<Gray, float>(XSlices, YSlices);
            float[, ,] Slice = b[0].Data;
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    Slice[y, x, 0] = DensityGrid[ZSlices / 2, x, y];
                }
            }


            b[1] = new Image<Gray, float>(XSlices, ZSlices);
            Slice = b[1].Data;
            for (int z = 0; z < ZSlices; z++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    Slice[z, x, 0] = DensityGrid[z, x, YSlices / 2];
                }
            }

            b[2] = new Image<Gray, float>(ZSlices, YSlices);
            Slice = b[2].Data;
            for (int z = 0; z < ZSlices; z++)
            {
                for (int y = 0; y < YSlices; y++)
                {
                    Slice[y, z, 0] = DensityGrid[z, XSlices / 2, y];
                }
            }

            #endregion
            return b;
        }

        public static Image<Gray, float> ShowXCrossHigh(this float[, ,] DensityGrid, int sliceNumber)
        {
            #region Save as Images

            int ZSlices = DensityGrid.GetLength(0);
            int XSlices = DensityGrid.GetLength(1);
            int YSlices = DensityGrid.GetLength(2);

            var b = new Image<Gray, float>(XSlices, YSlices);
            float[, ,] Slice = b.Data;
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    Slice[y, x, 0] = DensityGrid[sliceNumber, x, y];
                }
            }

            #endregion
            return b;
        }


        /// <summary>
        /// Filepattern  =  "C:\temp\test"  output will be c:\temp\test1.tif
        /// </summary>
        /// <param name="DensityGrid"></param>
        /// <param name="FilePattern"></param>
        public static void SaveCross(this float[, ,] DensityGrid, string FilePattern)
        {
            string filename = Path.GetDirectoryName(FilePattern) + "\\" + Path.GetFileNameWithoutExtension(FilePattern);
            string extension = Path.GetExtension(FilePattern).ToLower();

            if (extension.Trim() == "")
            {
                filename = Path.GetDirectoryName(FilePattern) + "\\" + Path.GetFileNameWithoutExtension(FilePattern);
                extension = ".tif";
            }
            if (extension.Trim() == ".tiff")
                extension = ".tif";
            #region Save as Images

            int ZSlices = DensityGrid.GetLength(0);
            int XSlices = DensityGrid.GetLength(1);
            int YSlices = DensityGrid.GetLength(2);

            double[,] Slice = new double[XSlices, YSlices];
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    if (DensityGrid[ZSlices / 2, x, y] > -500)
                        Slice[x, y] = DensityGrid[ZSlices / 2, x, y];
                }
            }

            if (extension == ".tif")
                ImageProcessing.ImageFileLoader.Save_TIFF(filename + "_1.tiff", Slice);
            else
                Slice.MakeBitmap().Save(filename + "_1" + extension);




            double[,] SliceX = new double[XSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    if (DensityGrid[z, x, YSlices / 2] > -500)
                        SliceX[x, z] = DensityGrid[z, x, YSlices / 2];
                }
            }
            if (extension == ".tif")
                ImageProcessing.ImageFileLoader.Save_TIFF(filename + "_2.tiff", SliceX);
            else
                SliceX.MakeBitmap().Save(filename + "_2" + extension);



            double[,] SliceY = new double[YSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int y = 0; y < YSlices; y++)
                {
                    if (DensityGrid[z, XSlices / 2, y] > -500)
                        SliceY[y, z] = DensityGrid[z, XSlices / 2, y];
                }
            }
            if (extension == ".tif")
                ImageProcessing.ImageFileLoader.Save_TIFF(filename + "_3.tiff", SliceY);
            else
                SliceY.MakeBitmap().Save(filename + "_3" + extension);
            #endregion

        }
    }
}
