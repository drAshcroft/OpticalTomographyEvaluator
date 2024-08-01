using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageProcessing._2D;
using System.Drawing.Imaging;
using Emgu.CV.Structure;
using System.IO;
using BitMiracle.LibTiff.Classic;
using System.Drawing;
using MathLibrary;
using Utilities;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using nom.tam.fits;
using nom.tam.util;
using System.Runtime.InteropServices;

namespace ImageProcessing
{
    public static class ImageFileLoader
    {
        #region Save Bitmaps
        public static void Save_Bitmap(string Filename, object Image)
        {
            string extenstion = Path.GetExtension(Filename).ToLower();
            if (extenstion == ".ivg")
            {
                if (Image.GetType() == typeof(Image<Gray, float>))
                {
                    SaveIVGFile(Filename, (Image<Gray, float>)Image);
                }
                else
                    throw new Exception("Not yet implemented");

            }
            else if (Path.GetExtension(Filename).ToLower() == ".cct")
            {
                if (Image.GetType() == typeof(Bitmap))
                {
                    Save_Raw(Filename, (Bitmap)Image);
                }
                else if (Image.GetType() == typeof(Image<Gray, float>))
                {
                    Save_Raw(Filename, (Image<Gray, float>)Image);
                }

            }
            else
            {
                if (Path.GetExtension(Filename).ToLower().Contains("tif"))
                {
                    Save_TIFF(Filename, (Image<Gray, float>)Image);
                }
                else
                {
                    if (Image.GetType() == typeof(Bitmap))
                    {
                        if (File.Exists(Filename) == true)
                            File.Delete(Filename);
                        ((Bitmap)Image).Save(Filename);
                    }
                    else if (Image.GetType() == typeof(Image<Gray, float>))
                    {
                        ((Image<Gray, float>)Image).ToBitmap().Save(Filename);
                    }

                    else
                    {
                        throw new Exception("Do not know how to save this image type");
                    }
                }
            }
        }

        #region Raw Data
        public static void Save_Raw(string Filename, double[,] imageData)
        {
            if (Path.GetExtension(Filename).ToLower().Contains("cct") == true)
                System.Diagnostics.Debug.Print("");
            Save_TIFF(Filename, imageData);
        }

        public static void Save_Raw(string Filename, float[,] imageData)
        {
            if (Path.GetExtension(Filename).ToLower().Contains("cct") == true)
                System.Diagnostics.Debug.Print("");
            Save_TIFF(Filename, imageData);
        }

        public static void Save_Raw(string Filename, Bitmap imageData)
        {
            if (Path.GetExtension(Filename).ToLower().Contains("cct") == true)
                System.Diagnostics.Debug.Print("");
            Save_TIFF(Filename, imageData);
        }

        public static void Save_Raw(string Filename, Image<Gray, float> imageData)
        {
            if (Path.GetExtension(Filename).ToLower().Contains("cct") == true)
                System.Diagnostics.Debug.Print("");
            Save_TIFF(Filename, imageData);
        }



        public static void Save_Raw(string Filename, float[, ,] data)
        {

            string extention = Path.GetExtension(Filename).ToLower();
            if (extention == ".cct")
            {
                System.Diagnostics.Debug.Print("");
            }
            else if (extention == ".bmp" || extention == ".gif" || extention == ".jpeg" || extention == ".png" || extention == ".tiff" || extention == ".tif" || extention == ".jpg")
            {
                if (extention == ".png" || extention == ".tiff" || extention == ".tif")
                {
                    #region savepng
                    float[, ,] Slice = data;

                    double Min = double.MaxValue;
                    double Max = double.MinValue;
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(2); x++)
                            {
                                if (Slice[z, y, x] > Max) Max = Slice[z, y, x];
                                if (Slice[z, y, x] < Min) Min = Slice[z, y, x];
                            }
                        }
                    }

                    double Scale = ushort.MaxValue / (Max - Min);

                    string ppath = Path.GetDirectoryName(Filename);
                    string pFilename = Path.GetFileNameWithoutExtension(Filename);
                    int Hl = Slice.GetLength(1) - 1;
                    int Wl = Slice.GetLength(0) - 1;
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        Emgu.CV.Image<Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(Slice.GetLength(2), Slice.GetLength(1));

                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(0); x++)
                            {
                                image.Data[y, x, 0] = (UInt16)(Scale * (Slice[z, Hl - y, Wl - x] - Min));
                            }
                        }
                        image.Save(ppath + "\\" + pFilename + string.Format("{0:000}", z) + extention);
                    }
                    #endregion
                }
                else if (extention == ".tiff" || extention == ".tif")
                {
                    Save_Tiff_Stack(Filename, data);
                }
                else
                {
                    #region AllOthers

                    float[, ,] Slice = data;

                    double Min = double.MaxValue;
                    double Max = double.MinValue;
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(2); x++)
                            {
                                if (Slice[z, y, x] > Max) Max = Slice[z, y, x];
                                if (Slice[z, y, x] < Min) Min = Slice[z, y, x];
                            }
                        }
                    }

                    double Scale = byte.MaxValue / (Max - Min);

                    string ppath = Path.GetDirectoryName(Filename);
                    string pFilename = Path.GetFileNameWithoutExtension(Filename);
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        Emgu.CV.Image<Gray, byte> image = new Emgu.CV.Image<Gray, byte>(Slice.GetLength(2), Slice.GetLength(1));

                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(2); x++)
                            {
                                image.Data[y, x, 0] = (byte)(Scale * (Slice[z, y, x] - Min));
                            }
                        }
                        image.Save(ppath + "\\" + pFilename + string.Format("{0:000}", z) + extention);
                    }
                    #endregion
                }
            }
        }

        public static void Save_Raw(string Filename, double[, ,] data)
        {
            string extention = Path.GetExtension(Filename).ToLower();
            if (extention == ".cct")
            {
                System.Diagnostics.Debug.Print("");
            }
            else if (extention == ".bmp" || extention == ".gif" ||
                extention == ".jpeg" || extention == ".png" || extention == ".tiff" || extention == ".tif" ||
                extention == ".jpg")
            {
                if (extention == ".png")
                {
                    #region savepng
                    double[, ,] Slice = data;

                    double Min = double.MaxValue;
                    double Max = double.MinValue;
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(2); x++)
                            {
                                if (Slice[z, y, x] > Max) Max = Slice[z, y, x];
                                if (Slice[z, y, x] < Min) Min = Slice[z, y, x];
                            }
                        }
                    }

                    double Scale = ushort.MaxValue / (Max - Min);

                    string ppath = Path.GetDirectoryName(Filename);
                    string pFilename = Path.GetFileNameWithoutExtension(Filename);
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        Emgu.CV.Image<Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(Slice.GetLength(0), Slice.GetLength(1));

                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(0); x++)
                            {
                                image.Data[x, y, 0] = (UInt16)(Scale * (Slice[z, y, x] - Min));
                            }
                        }
                        image.Save(ppath + "\\" + pFilename + z.ToString() + extention);
                    }
                    #endregion
                }
                else if (extention == ".tiff" || extention == ".tif")
                {
                    Save_Tiff_Stack(Filename, data);
                }
                else
                {
                    #region AllOthers

                    double[, ,] Slice = data;

                    double Min = double.MaxValue;
                    double Max = double.MinValue;
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(2); x++)
                            {
                                if (Slice[z, y, x] > Max) Max = Slice[z, y, x];
                                if (Slice[z, y, x] < Min) Min = Slice[z, y, x];
                            }
                        }
                    }

                    double Scale = byte.MaxValue / (Max - Min);

                    string ppath = Path.GetDirectoryName(Filename);
                    string pFilename = Path.GetFileNameWithoutExtension(Filename);
                    for (int z = 0; z < Slice.GetLength(0); z++)
                    {
                        Emgu.CV.Image<Gray, byte> image = new Emgu.CV.Image<Gray, byte>(Slice.GetLength(0), Slice.GetLength(1));

                        for (int y = 0; y < Slice.GetLength(1); y++)
                        {
                            for (int x = 0; x < Slice.GetLength(0); x++)
                            {
                                image.Data[x, y, 0] = (byte)(Scale * (Slice[z, y, x] - Min));
                            }
                        }
                        image.Save(ppath + "\\" + pFilename + z.ToString() + extention);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// saves the recon volume as either a monolithic file or 
        /// as a stack of images depending on the selected file type
        /// </summary>
        /// <param name="Filename"></param>
        public static void Save_Raw(string Filename, double[][,] Data)
        {
            //todo: make it possible to save stack as a tiff
            string Extension = Path.GetExtension(Filename).ToLower();
            if (Extension == "." || Extension == "")
            {
                Extension = ".raw";
            }
            if (Extension == ".raw")
            {
                #region SaveRawFile

                string HeaderFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) + ".dat";

                using (StreamWriter outfile = new StreamWriter(HeaderFile))
                {
                    /*ObjectFileName:	walnut.raw
                    Resolution:	128 96 114
                    SliceThickness:	1 1 1
                    Format:		USHORT
                    ObjectModel:	I
                     */

                    outfile.WriteLine("ObjectFileName:" + Filename);
                    outfile.WriteLine("Resolution: " + Data[0].GetLength(1) + " " + Data[0].GetLength(0) + " " + Data.GetLength(0));
                    outfile.WriteLine("SliceThickness:	1 1 1");
                    outfile.WriteLine("Format:		USHORT");
                    outfile.WriteLine(" ObjectModel:	I");
                }

                FileStream BinaryFile = new FileStream(Filename, FileMode.Create, FileAccess.Write);
                BinaryWriter Writer = new BinaryWriter(BinaryFile);

                for (int z = 0; z < Data.GetLength(0); z++)
                {
                    for (int y = 0; y < Data[0].GetLength(0); y++)
                    {
                        for (int x = 0; x < Data[0].GetLength(1); x++)
                        {
                            Writer.Write((double)Data[z][y, x]);
                        }
                    }
                }
                Writer.Close();
                BinaryFile.Close();
                #endregion
            }
            else if (Extension == ".cct")
            {
                #region SaveCCTFile
                FileStream BinaryFile = new FileStream(Filename, FileMode.Create, FileAccess.Write);

                #endregion
            }
            else if (Extension == ".bmp" || Extension == ".jpg")
            {
                #region Save as Images

                int ZSlices = Data.GetLength(0);
                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);

                double Max = double.MinValue;
                double Min = double.MaxValue;
                double max = double.MinValue;
                double min = double.MaxValue;

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    max = Slice.MaxArray();
                    min = Slice.MinArray();
                    if (max > Max) Max = max;
                    if (min < Min) Min = min;
                }

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    Bitmap b = Slice.MakeBitmap(Min, Max);
                    b.Save(outFile + string.Format("_{0:000}", z) + Extension);
                }

                #endregion
            }
            else if (Extension == ".png" || Extension == ".tif")
            {
                #region Save as Images

                int ZSlices = Data.GetLength(0);
                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);

                double Max = double.MinValue;
                double Min = double.MaxValue;
                double max = double.MinValue;
                double min = double.MaxValue;

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    max = Slice.MaxArray();
                    min = Slice.MinArray();
                    if (max > Max) Max = max;
                    if (min < Min) Min = min;
                }

                double A = UInt16.MaxValue / (Max - Min);

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    Save_Raw(outFile + string.Format("_{0:000}", z) + Extension, Slice);
                    /*                    FreeImageAPI.FreeImageBitmap fib = new FreeImageAPI.FreeImageBitmap(Slice.GetLength(0), Slice.GetLength(1), FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16);
                                        unsafe
                                        {
                                            for (int y = 0; y < Slice.GetLength(1); y++)
                                            {
                                                UInt16* pOut = (UInt16*)((byte*)fib.Scan0 + fib.Pitch * y);

                                                for (int x = 0; x < Slice.GetLength(0); x++)
                                                {
                                                    UInt16 Gray = (UInt16)(A * (Slice[x, y] - Min));
                                                }
                                            }
                                        }
                                        fib.Save();*/
                }

                #endregion
            }
            else if (Extension == ".tif" || Extension == ".tiff")
            {
                Save_Tiff_Stack(Filename, Data);
            }
        }
        /// <summary>
        /// saves the recon volume as either a monolithic file or 
        /// as a stack of images depending on the selected file type
        /// </summary>
        /// <param name="Filename"></param>
        public static void Save_Raw(string Filename, float[][,] Data)
        {
            //todo: make it possible to save stack as a tiff
            string Extension = Path.GetExtension(Filename).ToLower();
            if (Extension == "." || Extension == "")
            {
                Extension = ".raw";
            }
            if (Extension == ".raw")
            {
                #region SaveRawFile

                string HeaderFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) + ".dat";

                using (StreamWriter outfile = new StreamWriter(HeaderFile))
                {
                    /*ObjectFileName:	walnut.raw
                    Resolution:	128 96 114
                    SliceThickness:	1 1 1
                    Format:		USHORT
                    ObjectModel:	I
                     */

                    outfile.WriteLine("ObjectFileName:" + Filename);
                    outfile.WriteLine("Resolution: " + Data[0].GetLength(1) + " " + Data[0].GetLength(0) + " " + Data.GetLength(0));
                    outfile.WriteLine("SliceThickness:	1 1 1");
                    outfile.WriteLine("Format:		USHORT");
                    outfile.WriteLine(" ObjectModel:	I");
                }

                FileStream BinaryFile = new FileStream(Filename, FileMode.Create, FileAccess.Write);
                BinaryWriter Writer = new BinaryWriter(BinaryFile);

                for (int z = 0; z < Data.GetLength(0); z++)
                {
                    for (int y = 0; y < Data[0].GetLength(0); y++)
                    {
                        for (int x = 0; x < Data[0].GetLength(1); x++)
                        {
                            Writer.Write((double)Data[z][y, x]);
                        }
                    }
                }
                Writer.Close();
                BinaryFile.Close();
                #endregion
            }
            else if (Extension == ".cct")
            {
                #region SaveCCTFile
                FileStream BinaryFile = new FileStream(Filename, FileMode.Create, FileAccess.Write);

                #endregion
            }
            else if (Extension == ".bmp" || Extension == ".jpg")
            {
                #region Save as Images

                int ZSlices = Data.GetLength(0);
                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);

                float Max = float.MinValue;
                float Min = float.MaxValue;
                float max = float.MinValue;
                float min = float.MaxValue;

                for (int z = 0; z < ZSlices; z++)
                {
                    float[,] Slice = (float[,])Data[z];
                    max = Slice.MaxArray();
                    min = Slice.MinArray();
                    if (max > Max) Max = max;
                    if (min < Min) Min = min;
                }

                for (int z = 0; z < ZSlices; z++)
                {
                    float[,] Slice = (float[,])Data[z];
                    Bitmap b = Slice.MakeBitmap(Min, Max);
                    b.Save(outFile + string.Format("_{0:000}", z) + Extension);
                }

                #endregion
            }
            else if (Extension == ".png")
            {
                #region Save as Images

                int ZSlices = Data.GetLength(0);
                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);

                float Max = float.MinValue;
                float Min = float.MaxValue;
                float max = float.MinValue;
                float min = float.MaxValue;

                for (int z = 0; z < ZSlices; z++)
                {
                    float[,] Slice = (float[,])Data[z];
                    max = Slice.MaxArray();
                    min = Slice.MinArray();
                    if (max > Max) Max = max;
                    if (min < Min) Min = min;
                }

                double A = UInt16.MaxValue / (Max - Min);

                for (int z = 0; z < ZSlices; z++)
                {
                    float[,] Slice = (float[,])Data[z];
                    Save_Raw(outFile + string.Format("_{0:000}", z) + Extension, Slice);
                    /*                    FreeImageAPI.FreeImageBitmap fib = new FreeImageAPI.FreeImageBitmap(Slice.GetLength(0), Slice.GetLength(1), FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16);
                                        unsafe
                                        {
                                            for (int y = 0; y < Slice.GetLength(1); y++)
                                            {
                                                UInt16* pOut = (UInt16*)((byte*)fib.Scan0 + fib.Pitch * y);

                                                for (int x = 0; x < Slice.GetLength(0); x++)
                                                {
                                                    UInt16 Gray = (UInt16)(A * (Slice[x, y] - Min));
                                                }
                                            }
                                        }
                                        fib.Save();*/
                }

                #endregion
            }
            else if (Extension == ".tif" || Extension == ".tiff")
            {
                Save_Tiff_Stack(Filename, Data);
            }
        }

        public static void Save_Raw(string Filename, double[][,] Data, int BitDepth)
        {

            string Extension = Path.GetExtension(Filename).ToLower();
            if (Extension == ".png" || Extension == ".tif" && BitDepth == 16)
            {
                #region Save as Images

                int ZSlices = Data.GetLength(0);
                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);

                double Max = double.MinValue;
                double Min = double.MaxValue;
                double max = double.MinValue;
                double min = double.MaxValue;

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    max = Slice.MaxArray();
                    min = Slice.MinArray();
                    if (max > Max) Max = max;
                    if (min < Min) Min = min;
                }

                double A = (UInt16.MaxValue - 1) / (Max - Min);

                for (int z = 0; z < ZSlices; z++)
                {
                    double[,] Slice = (double[,])Data[z];
                    Save_Raw(outFile + string.Format("_{0:000}", z) + Extension, Slice);
                    /* FreeImageAPI.FreeImageBitmap fib = new FreeImageAPI.FreeImageBitmap(Slice.GetLength(0), Slice.GetLength(1), FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16);
                     unsafe
                     {
                         for (int y = 0; y < Slice.GetLength(1); y++)
                         {
                             UInt16* pOut = (UInt16*)((byte*)fib.Scan0 + fib.Stride * y);

                             for (int x = 0; x < Slice.GetLength(0); x++)
                             {
                                 UInt16 Gray = (UInt16)Math.Truncate(A * (Slice[x, y] - Min));
                                 *pOut = Gray;
                                 pOut++;
                             }
                         }
                     }
                     fib.Save(outFile + string.Format("_{0:000}", z) + Extension);*/
                }

                #endregion
            }
            else
            {
                Save_Raw(Filename, Data);
            }
        }
        #endregion

        #region Tiffs

        public static void Save_Tiff_Stack(string Filename, double[, ,] Data)
        {
            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                for (int z = 0; z < Data.GetLength(2); z++)
                {
                    // Emgu.CV.Image<Gray, float> image = new Emgu.CV.Image<Gray, float>(Data.GetLength(1), Data.GetLength(2));


                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                    output.SetField(TiffTag.BITSPERSAMPLE, 32);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                    //output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                    if (z % 2 == 0)
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);

                    //                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                    // specify that it's a page within the multipage file
                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number
                    output.SetField(TiffTag.PAGENUMBER, z, Data.GetLength(2));

                    float[] samples = new float[width];
                    byte[] buffer = new byte[samples.Length * sizeof(float)];
                    for (int i = 0; i < height; i++)
                    {

                        for (int j = 0; j < width; j++)
                            samples[j] = (float)(Data[j, i, z]);


                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                    output.WriteDirectory();
                }
            }

        }
        public static void Save_Tiff_Stack(string Filename, float[, ,] Data)
        {
            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                for (int z = 0; z < Data.GetLength(2); z++)
                {
                    // Emgu.CV.Image<Gray, float> image = new Emgu.CV.Image<Gray, float>(Data.GetLength(1), Data.GetLength(2));


                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                    output.SetField(TiffTag.BITSPERSAMPLE, 32);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                    //output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                    if (z % 2 == 0)
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);

                    //                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                    // specify that it's a page within the multipage file
                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number
                    output.SetField(TiffTag.PAGENUMBER, z, Data.GetLength(2));


                    for (int i = 0; i < height; i++)
                    {
                        float[] samples = new float[width];
                        for (int j = 0; j < width; j++)
                            samples[j] = (float)(Data[j, i, z]);

                        byte[] buffer = new byte[samples.Length * sizeof(float)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                    output.WriteDirectory();
                }
            }
        }
        public static void Save_Tiff_Stack(string Filename, double[][,] Data)
        {
            int width = Data[0].GetLength(0);
            int height = Data[0].GetLength(1);

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                for (int z = 0; z < Data.Length; z++)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                    output.SetField(TiffTag.BITSPERSAMPLE, 32);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                    if (z % 2 == 0)
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);

                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                    // specify that it's a page within the multipage file
                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number
                    output.SetField(TiffTag.PAGENUMBER, z, Data.GetLength(2));


                    for (int i = 0; i < height; i++)
                    {
                        float[] samples = new float[width];
                        for (int j = 0; j < width; j++)
                            samples[j] = (float)(Data[z][j, i]);

                        byte[] buffer = new byte[samples.Length * sizeof(float)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                    output.WriteDirectory();
                }
            }
        }
        public static void Save_Tiff_Stack(string Filename, float[][,] Data)
        {
            int width = Data[0].GetLength(0);
            int height = Data[0].GetLength(1);

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                for (int z = 0; z < Data.Length; z++)
                {
                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                    output.SetField(TiffTag.BITSPERSAMPLE, 32);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                    if (z % 2 == 0)
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);

                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                    // specify that it's a page within the multipage file
                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number
                    output.SetField(TiffTag.PAGENUMBER, z, Data.GetLength(2));


                    for (int i = 0; i < height; i++)
                    {
                        float[] samples = new float[width];
                        for (int j = 0; j < width; j++)
                            samples[j] = (float)(Data[z][j, i]);

                        byte[] buffer = new byte[samples.Length * sizeof(float)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                    output.WriteDirectory();
                }
            }
        }

        public static void Save_Tiff_VirtualStack(string Filename, double[, ,] Data, double MinValue, int BitDepth)
        {
            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            double MaxValue = Data.MaxArray();

            double Scale;

            if (BitDepth == 8)
            {
                Scale = byte.MaxValue / (MaxValue - MinValue);
            }
            else
            {
                Scale = Int16.MaxValue / (MaxValue - MinValue);
            }
            string pPath = Path.GetDirectoryName(Filename);
            string pFilename = Path.GetFileNameWithoutExtension(Filename);
            for (int z = 0; z < Data.GetLength(2); z++)
            {

                using (Tiff output = Tiff.Open(pPath + "\\" + pFilename + string.Format("{0:0000}.tif", z), "w"))
                {

                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, BitDepth);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                    if (BitDepth == 8)
                    {
                        byte[] samples = new byte[width];
                        byte val;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                val = (byte)(Scale * (Data[i, j, z] - MinValue)); ;
                                if (val < 0) val = 0;

                                samples[j] = val;
                            }

                            output.WriteScanline(samples, i);
                        }
                    }
                    else
                    {
                        byte[] buffer = new byte[width * sizeof(short)];
                        ushort[] samples = new ushort[width];
                        ushort val;
                        for (int i = 0; i < height; i++)
                        {

                            for (int j = 0; j < width; j++)
                            {
                                val = (ushort)(Scale * (Data[i, j, z] - MinValue)); ;
                                if (val < 0) val = 0;

                                samples[j] = val;
                            }

                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }
                }
            }

        }
        public static void Save_Tiff_VirtualStack(string Filename, float[, ,] Data, float MinValue, int BitDepth)
        {
            string pPath = Path.GetDirectoryName(Filename);

            string pFilename = Path.GetFileNameWithoutExtension(Filename);

            //  MessageBox.Show(pPath);
            if (Directory.Exists(pPath) == false)
                Directory.CreateDirectory(pPath);
            else
            {
                try
                {
                    Directory.Delete(pPath, true);
                    Directory.CreateDirectory(pPath);
                }
                catch { }
            }

            //if (BitDepth == 8)
            //{
            //    Save_Raw(Filename, Data);
            //    return;
            //}

            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            double MaxValue = Data.MaxArray();
            MinValue = Data.MinArray();
            double Scale;

            //  MinValue = 0;
            if (BitDepth == 8)
                Scale = (byte.MaxValue - 1) / (MaxValue - MinValue);
            else
                Scale = (Int16.MaxValue - 40) / (MaxValue - MinValue);


            for (int z = 0; z < Data.GetLength(2); z++)
            {

                using (Tiff output = Tiff.Open(pPath + "\\" + pFilename + string.Format("{0:0000}.tif", z), "w"))
                {
                    output.SetField(TiffTag.IMAGEWIDTH, height);
                    output.SetField(TiffTag.IMAGELENGTH, width);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, BitDepth);

                    if (BitDepth == 16)
                        output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.INT);

                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, 16);
                    output.SetField(TiffTag.XRESOLUTION, .0734);
                    output.SetField(TiffTag.YRESOLUTION, .0734);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                    output.SetField(TiffTag.ARTIST, "ASU Recon");
                    output.SetField(TiffTag.COPYRIGHT, "Biodesign Institute");
                    DateTime n = DateTime.Now;
                    string date = string.Format("{0:0000}:{1:00}:{2:00} {3:00}:{4:00}:{5:00}", n.Year, n.Month, n.Date, n.Hour, n.Minute, n.Second);
                    output.SetField(TiffTag.DATETIME, date);
                    output.SetField(TiffTag.XRESOLUTION, .0734);
                    output.SetField(TiffTag.YRESOLUTION, .0734);
                    //output.SetField(TiffTag.EXIF_PIXELXDIMENSION, .0734);
                    //output.SetField(TiffTag.EXIF_PIXELYDIMENSION, .0734);


                    if (BitDepth == 8)
                    {
                        byte[] buffer = new byte[width * sizeof(byte)];
                        byte[] samples = new byte[width];

                        double v;
                        int Hl = height - 1;
                        int Wl = width - 1;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                v = (Scale * (Data[z, Hl - i, Wl - j] - MinValue)); ;
                                if (v < 0) v = 0;
                                if (v > 255) v = 255;
                                samples[j] = (byte)v;
                            }

                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }

                    else
                    {
                        byte[] buffer = new byte[width * sizeof(short)];
                        short[] samples = new short[width];
                        short val;
                        int Hl = height - 1;
                        int Wl = width - 1;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                val = (short)(Scale * (Data[z, Hl - i, Wl - j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }

                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }
                }
            }
        }


        public static void Save_Tiff_VirtualStackSATIR(string Filename, float[, ,] Data, float MinValue, int BitDepth)
        {
            string pPath = Path.GetDirectoryName(Filename);
            string pFilename = Path.GetFileNameWithoutExtension(Filename);
            if (Directory.Exists(pPath) == false)
                Directory.CreateDirectory(pPath);


            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            double MaxValue = Data.MaxArray();
            MinValue = Data.MinArray() - 1000;
            double Scale;

            //  MinValue = 0;
            if (BitDepth == 8)
                Scale = (byte.MaxValue - 1) / (MaxValue - MinValue);
            else
                Scale = (Int16.MaxValue - 40) / (MaxValue - MinValue);


            for (int z = 0; z < Data.GetLength(2); z++)
            {
                string index = string.Format("  {0}", z);
                index = index.Substring(index.Length - 3);
                // ImageProcessing.ImageFileLoader.ConvertTo_16bit_TIFF(Files[i], DataFolder + "\\converted\\image" + index + ".tif");
                using (Tiff output = Tiff.Open(pPath + "\\" + pFilename + index + ".tif", "w"))
                {
                    output.SetField(TiffTag.IMAGEWIDTH, height);
                    output.SetField(TiffTag.IMAGELENGTH, width);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, BitDepth);

                    if (BitDepth == 16)
                        output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.INT);

                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, 16);
                    output.SetField(TiffTag.XRESOLUTION, .0734);
                    output.SetField(TiffTag.YRESOLUTION, .0734);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                    output.SetField(TiffTag.ARTIST, "ASU Recon");
                    output.SetField(TiffTag.COPYRIGHT, "Biodesign Institute");
                    DateTime n = DateTime.Now;
                    string date = string.Format("{0:0000}:{1:00}:{2:00} {3:00}:{4:00}:{5:00}", n.Year, n.Month, n.Date, n.Hour, n.Minute, n.Second);
                    output.SetField(TiffTag.DATETIME, date);
                    output.SetField(TiffTag.XRESOLUTION, .0734);
                    output.SetField(TiffTag.YRESOLUTION, .0734);
                    output.SetField(TiffTag.EXIF_PIXELXDIMENSION, .0734);
                    output.SetField(TiffTag.EXIF_PIXELYDIMENSION, .0734);


                    if (BitDepth == 8)
                    {
                        byte[] buffer = new byte[width * sizeof(byte)];
                        byte[] samples = new byte[width];

                        double v;
                        int Hl = height - 1;
                        int Wl = width - 1;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                v = (Scale * (Data[z, Hl - i, Wl - j] - MinValue)); ;
                                if (v < 0) v = 0;
                                if (v > 255) v = 255;
                                samples[j] = (byte)v;
                            }

                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }

                    else
                    {
                        byte[] buffer = new byte[width * sizeof(short)];
                        short[] samples = new short[width];
                        short val;
                        int Hl = height - 1;
                        int Wl = width - 1;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                val = (short)(Scale * (Data[z, Hl - i, Wl - j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }

                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }
                }
            }
        }


        public static void Save_Tiff_VirtualStack(string Filename, double[][,] Data, double MinValue, int BitDepth)
        {
            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            double MaxValue = 0;
            for (int i = 0; i < Data.GetLength(0); i++)
            {
                double mValue = Data[i].MaxArray();
                if (mValue > MaxValue) MaxValue = mValue;
            }
            double Scale;

            if (BitDepth == 8)
            {
                Scale = byte.MaxValue / (MaxValue - MinValue);
            }
            else
            {
                Scale = Int16.MaxValue / (MaxValue - MinValue);
            }
            string pPath = Path.GetDirectoryName(Filename);
            string pFilename = Path.GetFileNameWithoutExtension(Filename);
            for (int z = 0; z < Data.Length; z++)
            {
                using (Tiff output = Tiff.Open(pPath + "\\" + pFilename + string.Format("{0:0000}.tif", z), "w"))
                {

                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, BitDepth);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                    if (BitDepth == 8)
                    {
                        byte[] samples = new byte[width];
                        byte val;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                val = (byte)(Scale * (Data[z][i, j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }
                            //byte[] buffer = new byte[samples.Length * sizeof(short)];
                            //Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(samples, i);
                        }
                    }
                    else
                    {
                        byte[] buffer = new byte[width * sizeof(short)];
                        short[] samples = new short[width];
                        short val;
                        for (int i = 0; i < height; i++)
                        {

                            for (int j = 0; j < width; j++)
                            {
                                val = (short)(Scale * (Data[z][i, j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }
                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }
                }
            }
        }
        public static void Save_Tiff_VirtualStack(string Filename, float[][,] Data, float MinValue, int BitDepth)
        {
            int width = Data.GetLength(0);
            int height = Data.GetLength(1);

            double MaxValue = 0;
            for (int i = 0; i < Data.GetLength(0); i++)
            {
                double mValue = Data[i].MaxArray();
                if (mValue > MaxValue) MaxValue = mValue;
            }
            double Scale;

            if (BitDepth == 8)
            {
                Scale = byte.MaxValue / (MaxValue - MinValue);
            }
            else
            {
                Scale = Int16.MaxValue / (MaxValue - MinValue);
            }

            string pPath = Path.GetDirectoryName(Filename);
            string pFilename = Path.GetFileNameWithoutExtension(Filename);

            for (int z = 0; z < Data.Length; z++)
            {
                using (Tiff output = Tiff.Open(pPath + "\\" + pFilename + string.Format("{0:0000}.tif", z), "w"))
                {

                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, BitDepth);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                    if (BitDepth == 8)
                    {
                        byte[] samples = new byte[width];
                        byte val;
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                val = (byte)(Scale * (Data[z][i, j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }
                            //byte[] buffer = new byte[samples.Length * sizeof(short)];
                            //Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(samples, i);
                        }
                    }
                    else
                    {
                        byte[] buffer = new byte[width * sizeof(short)];
                        short[] samples = new short[width];
                        short val;
                        for (int i = 0; i < height; i++)
                        {

                            for (int j = 0; j < width; j++)
                            {
                                val = (short)(Scale * (Data[z][i, j] - MinValue)); ;
                                if (val < 0) val = 0;
                                samples[j] = val;
                            }
                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                            output.WriteScanline(buffer, i);
                        }
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Explicit)]
        private struct Int16Converter
        {
            [FieldOffset(0)]
            public ulong Value;
            [FieldOffset(0)]
            public byte Byte1;
            [FieldOffset(1)]
            public byte Byte2;
            [FieldOffset(2)]
            public byte Byte3;

            public Int16Converter(ulong value)
            {
                Byte1 = Byte2 = Byte3 = 0;
                Value = value;
            }

            public static implicit operator ulong(Int16Converter value)
            {
                return value.Value;
            }

            public static implicit operator Int16Converter(ulong value)
            {
                return new Int16Converter(value);
            }
        }

        public static void Save_Compressed(string filename, Image<Gray, float> imageData, int compression)
        {

            float[, ,] ImageArray = imageData.Data;

            int iWidth = ImageArray.GetLength(0);
            int iHeight = ImageArray.GetLength(1);

            Bitmap b = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);

            unsafe
            {

                for (int y = 0; y < iHeight; y++)
                {
                    byte* scanline = ((byte*)bmd.Scan0 + (y) * bmd.Stride);

                    for (int x = 0; x < iWidth; x++)
                    {
                        Int16Converter conv16 = (ulong)ImageArray[x, y, 0];
                        byte* bits = (byte*)scanline;
                        bits[0] = conv16.Byte1;
                        bits[1] = conv16.Byte2;
                        bits[2] = conv16.Byte3;
                        scanline += 3;
                    }
                }
            }
            b.UnlockBits(bmd);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }

            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)compression);
            b.Save(filename, ici, ep);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void Save_TIFF(string Filename, Image<Gray, float> imageData)
        {

            int width = imageData.Width;
            int height = imageData.Height;

            float[, ,] ImageArray = imageData.Data;

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                output.SetField(TiffTag.BITSPERSAMPLE, 32);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                for (int i = 0; i < height; i++)
                {
                    float[] samples = new float[width];
                    for (int j = 0; j < width; j++)
                        samples[j] = ImageArray[i, j, 0];

                    byte[] buffer = new byte[samples.Length * sizeof(float)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, i);
                }
            }
        }

        public static void ConvertTo_16bit_TIFF(string filename, string outFilename)
        {
            string exten = Path.GetExtension(filename).ToLower();

            if (exten == ".png")
            {
                Emgu.CV.Image<Emgu.CV.Structure.Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(filename);
                image.Save(outFilename);
            }
            else
            {

                UInt16[, ,] bmd = null;
                using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open)))
                {

                    int length = (int)b.BaseStream.Length;
                    {
                        int MagicNumber1 = b.ReadInt16();
                        // Image.Close();
                        int MagicNumber2 = b.ReadInt16();

                        if ((MagicNumber1 != 26454) || (MagicNumber2 != 25171))
                        {
                            //  throw new Exception("Wrong file format");
                        }

                        int width = b.ReadInt16();
                        int height = b.ReadInt16();
                        int bitsPerPixel = b.ReadInt16();

                        Emgu.CV.Image<Emgu.CV.Structure.Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(width, height);
                        b.ReadInt32();
                        b.ReadInt16();
                        b.ReadInt16();
                        b.ReadInt16();

                        double RealBBP = Math.Round((double)((length - 20) / width / height));

                        byte[] Buffer = b.ReadBytes(length - 20);


                        bmd = image.Data;
                        ushort Gray;

                        {
                            unsafe
                            {
                                fixed (byte* pBuffer = Buffer)
                                {
                                    ushort* pIn = (ushort*)pBuffer;
                                    ushort temp = 1;
                                    byte* ptemp = (byte*)&temp;
                                    for (int y = 0; y < width; y++)
                                    {
                                        for (int x = 0; x < height; x++)
                                        {

                                            try
                                            {
                                                Gray = *pIn;
                                                bmd[y, x, 0] = Gray;
                                                pIn++;
                                            }
                                            catch
                                            {
                                                System.Diagnostics.Debug.Print("");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        b.Close();

                        image.Save(outFilename);
                    }

                }

            }
        }

        public static void Save_16bit_TIFF(string Filename, Image<Gray, float> imageData, float Scale, float Min)
        {
            int width = imageData.Width;
            int height = imageData.Height;

            //float[, ,] ImageArray = imageData.Data;


            //Emgu.CV.Image<Emgu.CV.Structure.Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(imageData.Width, imageData.Height);
            //UInt16[, ,] samples = image.Data;
            //float val;
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        val = ImageArray[i, j, 0];
            //        if (val > Min) 
            //            samples[i, j, 0] = (UInt16)(Scale * (val - Min));
            //    }
            //}
            //image.Save(Filename);

            float[, ,] ImageArray = imageData.Data;

            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                try
                {
                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    output.SetField(TiffTag.BITSPERSAMPLE, 16);
                    output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.UINT);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 88.0);
                    output.SetField(TiffTag.YRESOLUTION, 88.0);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                    for (int i = 0; i < height; i++)
                    {

                        UInt16[] samples = new UInt16[width];
                        for (int j = 0; j < width; j++)
                            samples[j] = (UInt16)(Scale * (ImageArray[i, j, 0] - Min));

                        byte[] buffer = new byte[samples.Length * sizeof(UInt16)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                        output.WriteScanline(buffer, i);
                    }
                }
                catch
                {
                    width = width + 1;
                }
            }
        }





        public static void Save_16bit_TIFF(string Filename, Bitmap imageData)
        {
            Emgu.CV.Image<Emgu.CV.Structure.Bgr, UInt16> image = new Emgu.CV.Image<Bgr, ushort>(imageData);

            image.Save(Filename);
        }
        public static void Save_16bit_TIFF(string Filename, double[,] imageData, double Scale, double Min)
        {
            int width = imageData.GetLength(0);
            int height = imageData.GetLength(1);


            Emgu.CV.Image<Emgu.CV.Structure.Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(width, height);
            UInt16[, ,] samples = image.Data;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    samples[i, j, 0] = (UInt16)(Scale * (imageData[i, j] - Min));
            }
            image.Save(Filename);


            /* using (Tiff output = Tiff.Open(Filename, "w"))
             {
                 output.SetField(TiffTag.IMAGEWIDTH, width);
                 output.SetField(TiffTag.IMAGELENGTH, height);
                 output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                 output.SetField(TiffTag.BITSPERSAMPLE, 16);
                 output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                 output.SetField(TiffTag.ROWSPERSTRIP, height);
                 output.SetField(TiffTag.XRESOLUTION, 88.0);
                 output.SetField(TiffTag.YRESOLUTION, 88.0);
                 output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                 output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                 output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                 output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                 output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                 for (int i = 0; i < height; i++)
                 {
                     short[] samples = new short[width];
                     for (int j = 0; j < width; j++)
                         samples[j] = (short)(Scale * (imageData[j, i] - Min));

                     byte[] buffer = new byte[samples.Length * sizeof(short)];
                     Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                     output.WriteScanline(buffer, i);
                 }
             }*/


        }
        public static void Save_16bit_TIFF(string Filename, float[,] imageData, double Scale, double Min)
        {
            int width = imageData.GetLength(0);
            int height = imageData.GetLength(1);


            //Emgu.CV.Image<Emgu.CV.Structure.Gray, UInt16> image = new Emgu.CV.Image<Gray, ushort>(width, height);
            //UInt16[, ,] samples = image.Data;
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //        samples[i, j, 0] = (UInt16)(Scale * (imageData[i, j] - Min));
            //}
            //image.Save(Filename);


            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.BITSPERSAMPLE, 16);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                for (int i = 0; i < height; i++)
                {
                    short[] samples = new short[width];
                    for (int j = 0; j < width; j++)
                        samples[j] = (short)(Scale * (imageData[j, i] - Min));

                    byte[] buffer = new byte[samples.Length * sizeof(short)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, i);
                }
            }
        }

        public static void Save_TIFF(string Filename, double[,] imageData)
        {
            int width = imageData.GetLength(0);
            int height = imageData.GetLength(1);


            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                output.SetField(TiffTag.BITSPERSAMPLE, 32);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                for (int i = 0; i < height; i++)
                {
                    float[] samples = new float[width];
                    for (int j = 0; j < width; j++)
                        samples[j] = (float)(imageData[j, i]);

                    byte[] buffer = new byte[samples.Length * sizeof(float)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, i);
                }
            }
        }

        public static void Save_TIFF(string Filename, float[,] imageData)
        {
            int width = imageData.GetLength(0);
            int height = imageData.GetLength(1);


            using (Tiff output = Tiff.Open(Filename, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                output.SetField(TiffTag.BITSPERSAMPLE, 32);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                for (int i = 0; i < height; i++)
                {
                    float[] samples = new float[width];
                    for (int j = 0; j < width; j++)
                        samples[j] = imageData[j, i];

                    byte[] buffer = new byte[samples.Length * sizeof(float)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, i);
                }
            }
        }

        public static void Save_TIFF(string Filename, Bitmap imageData)
        {
            Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> image = new Emgu.CV.Image<Bgr, byte>(imageData);

            image.Save(Filename);
            /*
            using (Tiff tif = Tiff.Open(Filename, "w"))
            {
                byte[] raster = getImageRasterBytes(imageData, imageData.PixelFormat);
                tif.SetField(TiffTag.IMAGEWIDTH, imageData.Width);
                tif.SetField(TiffTag.IMAGELENGTH, imageData.Height);
                tif.SetField(TiffTag.COMPRESSION, Compression.LZW);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);

                tif.SetField(TiffTag.ROWSPERSTRIP, imageData.Height);

                tif.SetField(TiffTag.XRESOLUTION, imageData.HorizontalResolution);
                tif.SetField(TiffTag.YRESOLUTION, imageData.VerticalResolution);

                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);

                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                int stride = raster.Length / imageData.Height;
                convertSamples(raster, imageData.Width, imageData.Height);

                for (int i = 0, offset = 0; i < imageData.Height; i++)
                {
                    tif.WriteScanline(raster, offset, i, 0);
                    offset += stride;
                }
            }*/
        }
        #endregion

        private static byte[] getImageRasterBytes(Bitmap bmp, PixelFormat format)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            byte[] bits = null;

            try
            {
                // Lock the managed memory
                BitmapData bmpdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, format);

                // Declare an array to hold the bytes of the bitmap.
                bits = new byte[bmpdata.Stride * bmpdata.Height];

                // Copy the values into the array.
                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, bits, 0, bits.Length);

                // Release managed memory
                bmp.UnlockBits(bmpdata);
            }
            catch
            {
                return null;
            }

            return bits;
        }

        /// <summary>
        /// Converts BGR samples into RGB samples
        /// </summary>
        private static void convertSamples(byte[] data, int width, int height)
        {
            int stride = data.Length / height;
            const int samplesPerPixel = 3;

            for (int y = 0; y < height; y++)
            {
                int offset = stride * y;
                int strideEnd = offset + width * samplesPerPixel;

                for (int i = offset; i < strideEnd; i += samplesPerPixel)
                {
                    byte temp = data[i + 2];
                    data[i + 2] = data[i];
                    data[i] = temp;
                }
            }
        }


        /// <summary>
        /// takes a volume and saves the three axis cross
        /// </summary>
        /// <param name="Filename"></param>
        public static void SaveCross(string Filename, float[, ,] DataWhole)
        {
            string Extension = Path.GetExtension(Filename).ToLower();
            if (Extension == ".bmp" || Extension == ".jpg" || Extension == ".png" || Extension == ".tif")
            {
                #region Save as Images

                int ZSlices = DataWhole.GetLength(0);
                int XSlices = DataWhole.GetLength(1);
                int YSlices = DataWhole.GetLength(2);

                string outFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename);


                {
                    double[,] Slice = new double[XSlices, YSlices];
                    for (int y = 0; y < YSlices; y++)
                    {
                        for (int x = 0; x < XSlices; x++)
                        {
                            Slice[x, y] = DataWhole[ZSlices / 2, x, y];
                        }
                    }
                    Bitmap b = Slice.MakeBitmap();
                    b.Save(outFile + "_Z" + Extension);
                }

                {
                    double[,] SliceX = new double[XSlices, ZSlices];
                    for (int z = 0; z < ZSlices; z++)
                    {
                        for (int x = 0; x < XSlices; x++)
                        {
                            SliceX[x, z] = DataWhole[z, x, YSlices / 2];
                        }
                    }
                    Bitmap b = SliceX.MakeBitmap();
                    b.Save(outFile + "_X" + Extension);
                }

                {
                    double[,] SliceY = new double[YSlices, ZSlices];
                    for (int z = 0; z < ZSlices; z++)
                    {
                        for (int y = 0; y < YSlices; y++)
                        {
                            SliceY[y, z] = DataWhole[z, XSlices / 2, y];
                        }
                    }
                    Bitmap b = SliceY.MakeBitmap();
                    b.Save(outFile + "_Y" + Extension);
                }
                #endregion
            }

        }

        public static void SaveFits(string filename, Image<Gray, float> image)
        {

            double[][] x = new double[image.Height][];
            for (int y = 0; y < image.Height; y++)
            {
                double[] line = new double[image.Width];
                for (int xx = 0; xx < image.Width; xx++)
                {
                    line[xx] = image.Data[y, xx, 0];
                }
                x[y] = line;
            }

            nom.tam.fits.Fits f = new Fits();
            BasicHDU h = FitsFactory.HDUFactory(x);
            f.AddHDU(h);
            BufferedDataStream s = new BufferedDataStream(new FileStream(filename, FileMode.Create));
            f.Write(s);


        }

        public static void SaveIVGFile(string Filename, Image<Gray, float> image)
        {
            byte[] bBuffer;
            int length;
            using (BinaryReader b = new BinaryReader(File.Open(@"c:\temp\000.ivg", FileMode.Open)))
            {
                length = (int)b.BaseStream.Length;
                bBuffer = b.ReadBytes(length);
            }



            Int16[,] imageData = new Int16[image.Height, image.Width];
            int width = image.Width;
            int height = image.Height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        imageData[y, x] = (Int16)Math.Round(image.Data[y, x, 0]);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.Print("");
                    }
                }
            }

            Buffer.BlockCopy(imageData, 0, bBuffer, 20, Buffer.ByteLength(imageData));

            using (BinaryWriter b = new BinaryWriter(File.Create(Filename)))
            {
                b.Write(bBuffer);
            }
        }

        public static void SaveIVGFile(string Filename, float[, ,] image)
        {
            byte[] bBuffer;
            int length;
            using (BinaryReader b = new BinaryReader(File.Open(@"c:\temp\000.ivg", FileMode.Open)))
            {
                length = (int)b.BaseStream.Length;
                bBuffer = b.ReadBytes(length);
            }

            int width = image.GetLength(1);
            int height = image.GetLength(0);


            Int16[,] imageData = new Int16[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        imageData[y, x] = (Int16)Math.Round(image[y, x, 0]);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.Print("");
                    }
                }
            }

            Buffer.BlockCopy(imageData, 0, bBuffer, 20, Buffer.ByteLength(imageData));

            using (BinaryWriter b = new BinaryWriter(File.Create(Filename)))
            {
                b.Write(bBuffer);
            }
        }
        #endregion

        static object CriticalSectionFIBLoad = new object();

        #region Load Bitmaps
        public static Emgu.CV.Image<Gray, float> LoadImage(string Filename)
        {
            if (Path.GetExtension(Filename) == ".ivg")
            {
                float[, ,] ih = null;
                Emgu.CV.Image<Gray, float> image = null;
                //  try
                {
                    ih = ImageFileLoader.LoadIVGFile(Filename);
                }
                //   catch (Exception ex)
                {
                    //   System.Diagnostics.Debug.Print(ex.Message);
                    // throw ex;
                }

                // try
                {
                    image = new Emgu.CV.Image<Gray, float>(ih.GetLength(0), ih.GetLength(1));
                    Buffer.BlockCopy(ih, 0, image.Data, 0, Buffer.ByteLength(ih));
                }
                // catch (Exception ex)
                {
                    //   System.Diagnostics.Debug.Print(ex.Message);
                }

                return image;
            }
            else if (Path.GetExtension(Filename) == ".mbin")
            {
                float[, ,] ih = null;
                Emgu.CV.Image<Gray, float> image = null;
                try
                {
                    ih = ImageFileLoader.LoadMatlabBinFile(Filename);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                    throw ex;
                }

                try
                {
                    image = new Emgu.CV.Image<Gray, float>(ih.GetLength(0), ih.GetLength(1));
                    Buffer.BlockCopy(ih, 0, image.Data, 0, Buffer.ByteLength(ih));


                    image.ROI = new Rectangle((int)(image.Width * .11), (int)(image.Height * .11), (int)(image.Width * .78), (int)(image.Height * .78));
                    image = image.Copy();
                    //image = image.Log();

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }

                // image =   image.Log();

                return image;
            }
            else if (Path.GetExtension(Filename) == ".tif")
            {
                return Load_Tiff(Filename);
            }
            var imageO = new Emgu.CV.Image<Gray, float>(Filename);
            return imageO;
        }

        public static float[, ,] Load_TiffStack(string Filename)
        {
            int width = 100;
            int height = 100;
            int pageCount = 0;
            using (Tiff tif = Tiff.Open(Filename, "r"))
            {
                FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
                width = value[0].ToInt();

                value = tif.GetField(TiffTag.IMAGELENGTH);
                height = value[0].ToInt();

                do
                {
                    ++pageCount;
                } while (tif.ReadDirectory());
            }
            float[, ,] DataCube = new float[width, height, pageCount];

            using (Tiff tif = Tiff.Open(Filename, "r"))
            {
                FieldValue[] value = tif.GetField(TiffTag.SAMPLEFORMAT);

                if (value[0].Value.ToString() == "IEEEFP")
                {
                    int LineWidth = width * sizeof(float);
                    byte[] buffer = new byte[width * sizeof(float)];
                    int LineCount = Buffer.ByteLength(buffer);
                    int pageSize = LineCount * height;
                    for (int page = 0; page < pageCount; page++)
                    {
                        for (int i = 0; i < height; i++)
                        {
                            tif.ReadScanline(buffer, i);
                            Buffer.BlockCopy(buffer, 0, DataCube, i * LineWidth + page * pageSize, LineCount);
                        }
                        tif.ReadDirectory();
                    }

                }
                else
                {
                    // Read the image into the memory buffer
                    int[] raster = new int[height * width];

                    if (!tif.ReadRGBAImage(width, height, raster))
                    {
                        System.Windows.Forms.MessageBox.Show("Could not read image");
                        return null;
                    }

                }
            }

            return DataCube;
        }

        public static Emgu.CV.Image<Gray, float> Load_Tiff(string Filename)
        {
            // var imageO = new Emgu.CV.Image<Gray, float>(Filename);
            // return imageO;
            using (Tiff tif = Tiff.Open(Filename, "r"))
            {
                /*  output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.SAMPLEFORMAT, BitMiracle.LibTiff.Classic.SampleFormat.IEEEFP);
                output.SetField(TiffTag.BITSPERSAMPLE, 32);
                output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);*/

                // Find the width and height of the image
                FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
                int width = value[0].ToInt();

                value = tif.GetField(TiffTag.IMAGELENGTH);
                int height = value[0].ToInt();

                value = tif.GetField(TiffTag.SAMPLEFORMAT);


                FieldValue[] value2 = tif.GetField(TiffTag.BITSPERSAMPLE);

                if (value2 != null && value2[0].Value.ToString() == "16")
                {
                    Emgu.CV.Image<Gray, UInt16> ih = new Emgu.CV.Image<Gray, UInt16>(width, height);
                    UInt16[, ,] data = ih.Data;
                    int LineWidth = width * sizeof(UInt16);
                    byte[] buffer = new byte[width * sizeof(UInt16)];
                    int LineCount = Buffer.ByteLength(buffer);
                    for (int i = 0; i < height; i++)
                    {
                        tif.ReadScanline(buffer, i);
                        Buffer.BlockCopy(buffer, 0, data, i * LineWidth, LineCount);
                    }
                    // Bitmap b = ih.ToBitmap();

                    return ih.Convert<Gray, float>();


                }
                else if (value == null || value[0].Value.ToString() == "IEEEFP")
                {
                    Emgu.CV.Image<Gray, Byte> ih = new Emgu.CV.Image<Gray, Byte>(width, height);
                    byte[, ,] data = ih.Data;
                    int LineWidth = width;
                    byte[] buffer = new byte[width];
                    int LineCount = height;
                    //int LineCount = Buffer.ByteLength(buffer);
                    for (int i = 0; i < height; i++)
                    {
                        tif.ReadScanline(buffer, i);
                        Buffer.BlockCopy(buffer, 0, data, i * LineWidth, LineCount);
                    }
                    // Bitmap b = ih.ToBitmap();

                    return ih.Convert<Gray,float>();
                }
                else
                {
                    // Read the image into the memory buffer
                    int[] raster = new int[height * width];

                    if (!tif.ReadRGBAImage(width, height, raster))
                    {
                        System.Windows.Forms.MessageBox.Show("Could not read image", Filename);
                        return null;
                    }

                }
            }
            var imageO = new Emgu.CV.Image<Gray, float>(Filename);
            return imageO;
        }

        public static Emgu.CV.Image<Gray, float> Load_CCT(string Filename)
        {
            FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            BinaryReader Reader = new BinaryReader(BinaryFile);
            BinaryFile.Seek(0, SeekOrigin.Begin);
            int sizeX, sizeY, sizeZ;

            int ArrayRank = Reader.ReadInt32();

            if (ArrayRank != 2)
                MessageBox.Show("Can only display 2D arrays");

            sizeX = Reader.ReadInt32();
            sizeY = Reader.ReadInt32();
            sizeZ = Reader.ReadInt32();

            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();

            double[,] image = new double[sizeX, sizeY];


            byte[] buffer = new byte[Buffer.ByteLength(image)];
            Reader.Read(buffer, 0, buffer.Length);

            Buffer.BlockCopy(buffer, 0, image, 0, buffer.Length);

            Reader.Close();
            BinaryFile.Close();
            return ImageManipulation.ConvertToImage(image);
        }

        public static double[,] Load_RawToDouble(string Filename)
        {
            FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            BinaryReader Reader = new BinaryReader(BinaryFile);
            BinaryFile.Seek(0, SeekOrigin.Begin);
            int sizeX, sizeY, sizeZ;

            int ArrayRank = Reader.ReadInt32();

            if (ArrayRank != 2)
                MessageBox.Show("Can only display 2D arrays");

            sizeX = Reader.ReadInt32();
            sizeY = Reader.ReadInt32();
            sizeZ = Reader.ReadInt32();

            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();
            Reader.ReadDouble();

            double[,] image = new double[sizeX, sizeY];


            byte[] buffer = new byte[Buffer.ByteLength(image)];
            Reader.Read(buffer, 0, buffer.Length);

            Buffer.BlockCopy(buffer, 0, image, 0, buffer.Length);

            Reader.Close();
            BinaryFile.Close();
            return image;
        }

        public static float[, ,] LoadMatlabBinFile(string Filename)
        {
            float[, ,] bmd = null;
            using (BinaryReader b = new BinaryReader(File.Open(Filename, FileMode.Open)))
            {

                int length = (int)b.BaseStream.Length;
                {
                    int width = b.ReadInt32();
                    int height = b.ReadInt32();

                    byte[] Buffer = b.ReadBytes(length - 4);


                    bmd = new float[height, width, 1];
                    float Gray;


                    //  if (RealBBP == 2)
                    {
                        unsafe
                        {
                            fixed (byte* pBuffer = Buffer)
                            {
                                float* pIn = (float*)pBuffer;
                                float temp = 1;
                                byte* ptemp = (byte*)&temp;
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < width; x++)
                                    {
                                        Gray = *pIn;
                                        bmd[y, x, 0] = Gray;
                                        pIn++;
                                    }
                                }
                            }
                        }
                    }

                    b.Close();
                }
                return bmd;
            }
        }

        public static float[, ,] LoadIVGFile(string Filename)
        {
            float[, ,] bmd = null;
            using (BinaryReader b = new BinaryReader(File.Open(Filename, FileMode.Open)))
            {


                int length = (int)b.BaseStream.Length;
                {
                    int MagicNumber1 = b.ReadInt16();
                    // Image.Close();
                    int MagicNumber2 = b.ReadInt16();

                    if ((MagicNumber1 != 26454) || (MagicNumber2 != 25171))
                    {
                        //  throw new Exception("Wrong file format");
                    }

                    int width = b.ReadInt16();
                    int height = b.ReadInt16();
                    int bitsPerPixel = b.ReadInt16();


                    b.ReadInt32();
                    b.ReadInt16();
                    b.ReadInt16();
                    b.ReadInt16();

                    double RealBBP = Math.Round((double)((length - 20) / width / height));

                    byte[] Buffer = b.ReadBytes(length - 20);

                    bmd = new float[width, height, 1];
                    ushort Gray;


                    //  if (RealBBP == 2)
                    {
                        unsafe
                        {
                            fixed (byte* pBuffer = Buffer)
                            {
                                ushort* pIn = (ushort*)pBuffer;
                                ushort temp = 1;
                                byte* ptemp = (byte*)&temp;
                                for (int y = 0; y < width; y++)
                                {
                                    for (int x = 0; x < height; x++)
                                    {
                                        /* byte* t = (byte*)pIn;
                                         *ptemp = *(t + 1);
                                         *(ptemp + 1) = *(t);
                                         Gray = temp;*/
                                        try
                                        {
                                            Gray = *pIn;
                                            bmd[y, x, 0] = Gray;
                                            pIn++;
                                        }
                                        catch
                                        {
                                            System.Diagnostics.Debug.Print("");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    b.Close();
                }
                return bmd;
            }
        }
        #endregion


        public enum RawFileTypes
        {
            UInt16, Float32
        }

        public static void SaveDensityData(string filename, float[, ,] DataWhole, RawFileTypes fileType)
        {
            string Extension = Path.GetExtension(filename).ToLower();
            if (Extension == "." || Extension == "")
            {
                Extension = ".raw";
            }
            double max1 = DataWhole.MaxArray();
            double min1 = DataWhole.MinArray();

            if (Extension == ".raw")
            {
                #region SaveRawFile

                string HeaderFile = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".dat";

                using (StreamWriter outfile = new StreamWriter(HeaderFile))
                {
                    outfile.WriteLine("ObjectFileName:\t" + Path.GetFileName(filename));
                    outfile.WriteLine("Resolution:\t" + DataWhole.GetLength(0) + " " + DataWhole.GetLength(1) + " " + DataWhole.GetLength(2));
                    outfile.WriteLine("SliceThickness:\t1 1 1");
                    if (fileType == RawFileTypes.UInt16)
                        outfile.WriteLine("Format:\tUSHORT");
                    else
                        outfile.WriteLine("Format:\tFLOAT");
                    outfile.WriteLine("ObjectModel:\tI");
                }

                FileStream BinaryFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
                BinaryWriter Writer = new BinaryWriter(BinaryFile);

                if (fileType == RawFileTypes.Float32)
                {

                    double length = float.MaxValue / (max1 - min1);
                    for (int z = 0; z < DataWhole.GetLength(0); z++)
                    {
                        for (int y = 0; y < DataWhole.GetLength(1); y++)
                        {
                            for (int x = 0; x < DataWhole.GetLength(2); x++)
                            {
                                //Writer.Write((float)((DataWhole[z, y, x] - min1) * length));
                                Writer.Write((float)(DataWhole[z, y, x]));
                            }
                        }
                    }
                }
                else
                {

                    double length = ushort.MaxValue / (max1 - min1);
                    for (int z = 0; z < DataWhole.GetLength(0); z++)
                    {
                        for (int y = 0; y < DataWhole.GetLength(1); y++)
                        {
                            for (int x = 0; x < DataWhole.GetLength(2); x++)
                            {
                                Writer.Write((ushort)((DataWhole[z, y, x] - min1) * length));
                            }
                        }
                    }
                }

                Writer.Close();
                BinaryFile.Close();
                #endregion
            }
            else
                Save_Raw(filename, DataWhole);
        }

        public static double[, ,] OpenDensityData(string[] Filenames)
        {
            string Extension = Path.GetExtension(Filenames[0]).ToLower();
            if (Extension == ".bmp" || Extension == ".jpg" || Extension == ".png" || Extension == ".tif")
            {
                Filenames = Filenames.SortNumberedFiles();

                int sizeX, sizeY, sizeZ;

                Bitmap b = new Bitmap(Filenames[0]);
                sizeX = b.Width;
                sizeY = b.Height;
                sizeZ = Filenames.Length;

                double[, ,] mDensityGrid = new double[sizeX, sizeY, sizeZ];



                for (int z = 0; z < sizeZ; z++)
                {
                    b = new Bitmap(Filenames[z]);
                    double[,] Data = b.ConvertToDoubleArray(false);
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            mDensityGrid[x, y, z] = Data[x, y];
                        }
                    }
                }
                return mDensityGrid;
            }
            else if (Extension == ".ivg")
            {
                Filenames = Filenames.SortNumberedFiles();

                int sizeX, sizeY, sizeZ;

                float[, ,] b = LoadIVGFile(Filenames[0]);
                sizeX = b.GetLength(1);
                sizeY = b.GetLength(0);
                sizeZ = Filenames.Length;
                double[, ,] mDensityGrid = new double[sizeX, sizeY, sizeZ];




                for (int z = 0; z < sizeZ; z++)
                {
                    b = LoadIVGFile(Filenames[z]);
                    // double[,] Data = MathImageHelps. ConvertToDoubleArray(b, false);
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            mDensityGrid[z, y, x] = b[y, x, 0];// Data[y, x];
                        }
                    }
                }


                return mDensityGrid;
            }
            return null;
        }
        public static double[, ,] OpenDensityData(string Filename)
        {
            string Extension = Path.GetExtension(Filename).ToLower();

            if (Path.GetExtension(Filename).ToLower() == ".raw" && File.Exists(Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".dat") == true)
                Filename = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".dat";

            if (Path.GetExtension(Filename).ToLower() == ".cct")
            {
                #region Open Raw
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);
                int sizeX, sizeY, sizeZ;

                int ArrayRank = Reader.ReadInt32();

                sizeX = Reader.ReadInt32();
                sizeY = Reader.ReadInt32();
                sizeZ = Reader.ReadInt32();


                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();


                double[, ,] mDensityGrid;
                if ((sizeX > 200 || sizeY > 200 || sizeZ > 200) && (IntPtr.Size != 8))
                {
                    mDensityGrid = new double[200, 200, 200];
                    double cX = 199d / sizeX;
                    double cY = 199d / sizeY;
                    double cZ = 199d / sizeZ;

                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                mDensityGrid[(int)(z * cZ), (int)(y * cY), (int)(x * cX)] = Reader.ReadDouble();
                            }
                        }
                    }



                }
                else
                {
                    mDensityGrid = new double[sizeZ, sizeY, sizeX];

                    byte[] buffer = Reader.ReadBytes(mDensityGrid.Length * 8);
                    double[] BufD = new double[mDensityGrid.Length];
                    Buffer.BlockCopy(buffer, 0, BufD, 0, mDensityGrid.Length * 8);
                    buffer = null;
                    unsafe
                    {
                        fixed (double* pDouble = BufD)
                        fixed (double* pData = mDensityGrid)
                        {
                            double* pIn = pDouble;
                            double* pOut = pData;
                            for (int i = 0; i < BufD.Length; i++)
                            {
                                *pOut = *pIn;
                                pOut++;
                                pIn++;
                            }

                        }
                    }
                    /*for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[z, y, x] = (float)Reader.ReadDouble();
                            }
                        }
                    }*/

                }

                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;

                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".dat")
            {
                #region Open dat
                int DataType = 0;
                int sizeX, sizeY, sizeZ;

                sizeX = 0;
                sizeY = 0;
                sizeZ = 0;
                Dictionary<string, string> Tags = new Dictionary<string, string>();

                using (StreamReader sr = new StreamReader(Filename))
                {

                    String line;
                    string[] parts;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() != "" && line.Trim().StartsWith("#") == false)
                        {
                            parts = line.Replace("\t", "").Split(':');
                            Tags.Add(parts[0].Trim().ToLower(), parts[1]);
                        }
                    }
                    /*  ObjectFileName:	ProjectionObject.raw
                      Resolution:	220 220 220
                      SliceThickness:	1 1 1
                      Format:	FLOAT
                      ObjectModel:	I*/
                    parts = Tags["resolution"].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(parts[0], out sizeX);
                    int.TryParse(parts[1], out sizeY);
                    int.TryParse(parts[2], out sizeZ);

                    if (Tags["format"].ToLower() == "float")
                        DataType = 1;
                    if (Tags["format"].ToLower() == "double")
                        DataType = 2;
                    if (Tags["format"].ToLower() == "ushort")
                        DataType = 3;
                }

                Filename = Path.GetDirectoryName(Filename) + "\\" + Tags["objectfilename"];
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);


                double[, ,] mDensityGrid = new double[sizeX, sizeY, sizeZ];


                if (DataType == 2)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = Reader.ReadDouble();
                            }
                        }
                    }
                }
                else if (DataType == 1)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = Reader.ReadSingle();
                            }
                        }
                    }
                }
                else if (DataType == 3)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = (float)Reader.ReadUInt16();
                            }
                        }
                    }
                }


                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".raw")
            {
                #region Open Bin

                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);

                long nPoints = (int)(BinaryFile.Length / 8d);
                nPoints = (long)Math.Pow((double)nPoints, (1d / 3d));

                int sizeX, sizeY, sizeZ;

                sizeX = (int)nPoints;
                sizeY = (int)nPoints;
                sizeZ = (int)nPoints;

                double[, ,] mDensityGrid = new double[sizeX, sizeY, sizeZ];

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int z = 0; z < sizeZ; z++)
                        {
                            mDensityGrid[x, y, z] = Reader.ReadDouble();
                        }
                    }
                }
                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }

            return null;

        }

        public static float[, ,] OpenDensityDataFloat(string Filename)
        {
            string Extension = Path.GetExtension(Filename).ToLower();

            if (Path.GetExtension(Filename).ToLower() == ".raw" && File.Exists(Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".dat") == true)
                Filename = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".dat";

            if (Path.GetExtension(Filename).ToLower() == ".cct")
            {
                #region Open Raw
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);
                int sizeX, sizeY, sizeZ;

                int ArrayRank = Reader.ReadInt32();

                sizeX = Reader.ReadInt32();
                sizeY = Reader.ReadInt32();
                sizeZ = Reader.ReadInt32();


                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();


                float[, ,] mDensityGrid;
                if ((sizeX > 200 || sizeY > 200 || sizeZ > 200) && (IntPtr.Size != 8))
                {
                    mDensityGrid = new float[200, 200, 200];
                    double cX = 199d / sizeX;
                    double cY = 199d / sizeY;
                    double cZ = 199d / sizeZ;

                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                mDensityGrid[(int)(z * cZ), (int)(y * cY), (int)(x * cX)] = (float)Reader.ReadDouble();
                            }
                        }
                    }



                }
                else
                {
                    mDensityGrid = new float[sizeZ, sizeY, sizeX];

                    byte[] buffer = Reader.ReadBytes(mDensityGrid.Length * 8);
                    double[] BufD = new double[mDensityGrid.Length];
                    Buffer.BlockCopy(buffer, 0, BufD, 0, mDensityGrid.Length * 8);
                    buffer = null;
                    unsafe
                    {
                        fixed (double* pDouble = BufD)
                        fixed (float* pData = mDensityGrid)
                        {
                            double* pIn = pDouble;
                            float* pOut = pData;
                            for (int i = 0; i < BufD.Length; i++)
                            {
                                *pOut = (float)*pIn;
                                pOut++;
                                pIn++;
                            }

                        }
                    }
                    /*for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[z, y, x] = (float)Reader.ReadDouble();
                            }
                        }
                    }*/

                }

                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;

                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".dat")
            {
                #region Open dat
                int DataType = 0;
                int sizeX, sizeY, sizeZ;

                sizeX = 0;
                sizeY = 0;
                sizeZ = 0;
                Dictionary<string, string> Tags = new Dictionary<string, string>();

                using (StreamReader sr = new StreamReader(Filename))
                {

                    String line;
                    string[] parts;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() != "" && line.Trim().StartsWith("#") == false)
                        {
                            parts = line.Replace("\t", "").Split(':');
                            Tags.Add(parts[0].Trim().ToLower(), parts[1]);
                        }
                    }
                    /*  ObjectFileName:	ProjectionObject.raw
                      Resolution:	220 220 220
                      SliceThickness:	1 1 1
                      Format:	FLOAT
                      ObjectModel:	I*/
                    parts = Tags["resolution"].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(parts[0], out sizeX);
                    int.TryParse(parts[1], out sizeY);
                    int.TryParse(parts[2], out sizeZ);

                    if (Tags["format"].ToLower() == "float")
                        DataType = 1;
                    if (Tags["format"].ToLower() == "double")
                        DataType = 2;
                    if (Tags["format"].ToLower() == "ushort")
                        DataType = 3;
                }

                Filename = Path.GetDirectoryName(Filename) + "\\" + Tags["objectfilename"];
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);


                float[, ,] mDensityGrid = new float[sizeX, sizeY, sizeZ];


                if (DataType == 2)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = (float)Reader.ReadDouble();
                            }
                        }
                    }
                }
                else if (DataType == 1)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = Reader.ReadSingle();
                            }
                        }
                    }
                }
                else if (DataType == 3)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[x, y, z] = (float)Reader.ReadUInt16();
                            }
                        }
                    }
                }


                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".raw")
            {
                #region Open Bin

                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);

                long nPoints = (int)(BinaryFile.Length / 8d);
                nPoints = (long)Math.Pow((double)nPoints, (1d / 3d));

                int sizeX, sizeY, sizeZ;

                sizeX = (int)nPoints;
                sizeY = (int)nPoints;
                sizeZ = (int)nPoints;

                float[, ,] mDensityGrid = new float[sizeX, sizeY, sizeZ];

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int z = 0; z < sizeZ; z++)
                        {
                            mDensityGrid[x, y, z] = (float)Reader.ReadDouble();
                        }
                    }
                }
                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".tif")
            {
                return Load_TiffStack(Filename);
            }

            return null;

        }

       


        public static float[, ,] OpenDensityDataFloat(string[] Filenames)
        {
            string Extension = Path.GetExtension(Filenames[0]).ToLower();
            if (Extension == ".png" || Extension == ".tif" || Extension == ".tiff")
            {
                Filenames = Filenames.SortNumberedFiles();

                int sizeX, sizeY, sizeZ;

                Image<Gray, float> b = new Image<Gray, float>(Filenames[0]);
                sizeX = b.Width;
                sizeY = b.Height;
                sizeZ = Filenames.Length;

                float[, ,] mDensityGrid = new float[sizeZ, sizeY, sizeX];
                //mDensityGrid = new PhysicalArray(sizeX, sizeY, sizeZ, -1, 1, -1, 1, -1, 1);

                //Thread[] ts = new Thread[sizeZ];
                for (int z = 0; z < sizeZ; z++)
                {
                   // ts[z] = new Thread(delegate(object index)
                        {
                            int zz = z;//(int)index;
                            b = new Image<Gray, float>(Filenames[zz]);
                            float[, ,] Data = b.Data;
                            for (int y = 0; y < sizeY; y++)
                            {
                                for (int x = 0; x < sizeX; x++)
                                {
                                    mDensityGrid[zz, y, x] = (float)Data[y, x, 0];
                                }
                            }
                            b.Dispose();
                            b = null;
                        }//);
                    //ts[z].Start(z);
                    //GC.Collect();
                }

                //for (int i = 0; i < ts.Length; i++)
                //    ts[i].Join();

                return mDensityGrid;

            }
            if (Extension == ".bmp" || Extension == ".jpg" || Extension == ".png" || Extension == ".tif")
            {
                Filenames = Filenames.SortNumberedFiles();

                int sizeX, sizeY, sizeZ;

                Bitmap b = new Bitmap(Filenames[0]);
                sizeX = b.Width;
                sizeY = b.Height;
                sizeZ = Filenames.Length;

                float[, ,] mDensityGrid = new float[sizeZ, sizeY, sizeX];
                //mDensityGrid = new PhysicalArray(sizeX, sizeY, sizeZ, -1, 1, -1, 1, -1, 1);

                for (int z = 0; z < sizeZ; z++)
                {
                    b = new Bitmap(Filenames[z]);
                    double[,] Data = b.ConvertToDoubleArray(false);
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            mDensityGrid[z, y, x] = (float)Data[x, y];
                        }
                    }
                    b.Dispose();
                    b = null;
                    GC.Collect();
                }
                return mDensityGrid;
            }
            else if (Extension == ".ivg")
            {
                Filenames = Filenames.SortNumberedFiles();

                int sizeX, sizeY, sizeZ;

                float[, ,] b = LoadIVGFile(Filenames[0]);
                sizeX = b.GetLength(1);
                sizeY = b.GetLength(0);
                sizeZ = Filenames.Length;
                float[, ,] mDensityGrid = new float[sizeZ, sizeY, sizeX];

                for (int z = 0; z < sizeZ; z++)
                {
                    b = LoadIVGFile(Filenames[z]);
                    // double[,] Data = MathImageHelps. ConvertToDoubleArray(b, false);
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            mDensityGrid[z, y, x] = b[y, x, 0];// Data[y, x];
                        }
                    }
                }


                return mDensityGrid;
            }
            return null;
        }
        public static ushort[, ,] OpenDensityDataInt(string Filename)
        {
            string Extension = Path.GetExtension(Filename).ToLower();
            if (Path.GetExtension(Filename).ToLower() == ".cct")
            {
                #region Open Raw
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);
                int sizeX, sizeY, sizeZ;

                int ArrayRank = Reader.ReadInt32();

                sizeX = Reader.ReadInt32();
                sizeY = Reader.ReadInt32();
                sizeZ = Reader.ReadInt32();


                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();
                Reader.ReadDouble();


                ushort[, ,] mDensityGrid;
                if ((sizeX > 200 || sizeY > 200 || sizeZ > 200) && (IntPtr.Size != 8))
                {
                    mDensityGrid = new ushort[200, 200, 200];
                    double cX = 199d / sizeX;
                    double cY = 199d / sizeY;
                    double cZ = 199d / sizeZ;

                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[(int)(z * cZ), (int)(y * cY), (int)(x * cX)] = (ushort)Reader.ReadDouble();
                            }
                        }
                    }



                }
                else
                {
                    mDensityGrid = new ushort[sizeZ, sizeY, sizeX];

                    byte[] buffer = Reader.ReadBytes(mDensityGrid.Length * 8);
                    double[] BufD = new double[mDensityGrid.Length];
                    Buffer.BlockCopy(buffer, 0, BufD, 0, mDensityGrid.Length * 8);

                    double Max = double.MinValue;
                    double Min = double.MaxValue;


                    buffer = null;
                    unsafe
                    {
                        fixed (double* pDouble = BufD)
                        {
                            double* pIn = pDouble;
                            for (int i = 0; i < BufD.Length; i++)
                            {
                                if (*pIn > Max) Max = *pIn;
                                if (*pIn < Min) Min = *pIn;
                                pIn++;
                            }
                            double Length = (ushort.MaxValue - 1) / (Max - Min);
                            fixed (ushort* pData = mDensityGrid)
                            {
                                pIn = pDouble;
                                ushort* pOut = pData;
                                for (int i = 0; i < BufD.Length; i++)
                                {
                                    *pOut = (ushort)((*pIn - Min) * Length);
                                    pOut++;
                                    pIn++;
                                }

                            }
                        }
                    }
                    /*for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                mDensityGrid[z, y, x] = (float)Reader.ReadDouble();
                            }
                        }
                    }*/

                }

                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;

                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".dat")
            {
                #region Open dat
                int DataType = 0;
                int sizeX, sizeY, sizeZ;

                sizeX = 0;
                sizeY = 0;
                sizeZ = 0;
                Dictionary<string, string> Tags = new Dictionary<string, string>();

                using (StreamReader sr = new StreamReader(Filename))
                {

                    String line;
                    string[] parts;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() != "" && line.Trim().StartsWith("#") == false)
                        {
                            parts = line.Replace("\t", "").Split(':');
                            Tags.Add(parts[0].Trim().ToLower(), parts[1]);
                        }
                    }
                    /*  ObjectFileName:	ProjectionObject.raw
                      Resolution:	220 220 220
                      SliceThickness:	1 1 1
                      Format:	FLOAT
                      ObjectModel:	I*/
                    parts = Tags["resolution"].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(parts[0], out sizeX);
                    int.TryParse(parts[1], out sizeY);
                    int.TryParse(parts[2], out sizeZ);

                    if (Tags["format"].ToLower() == "float")
                        DataType = 1;
                    if (Tags["format"].ToLower() == "double")
                        DataType = 2;
                    if (Tags["format"].ToLower() == "ushort")
                        DataType = 3;
                }

                Filename = Path.GetDirectoryName(Filename) + "\\" + Tags["objectfilename"];
                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);


                ushort[, ,] mDensityGrid = new ushort[sizeX, sizeY, sizeZ];


                if (DataType == 2)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                mDensityGrid[x, y, z] = (ushort)Reader.ReadDouble();
                            }
                        }
                    }
                }
                else if (DataType == 1)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                mDensityGrid[x, y, z] = (ushort)Reader.ReadSingle();
                            }
                        }
                    }
                }
                else if (DataType == 3)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                mDensityGrid[x, y, z] = Reader.ReadUInt16();
                            }
                        }
                    }
                }


                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }
            else if (Path.GetExtension(Filename).ToLower() == ".raw")
            {
                #region Open Bin

                FileStream BinaryFile = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader Reader = new BinaryReader(BinaryFile);
                BinaryFile.Seek(0, SeekOrigin.Begin);

                long nPoints = (int)(BinaryFile.Length / 8d);
                nPoints = (long)Math.Pow((double)nPoints, (1d / 3d));

                int sizeX, sizeY, sizeZ;

                sizeX = (int)nPoints;
                sizeY = (int)nPoints;
                sizeZ = (int)nPoints;

                ushort[, ,] mDensityGrid = new ushort[sizeX, sizeY, sizeZ];

                for (int z = 0; z < sizeZ; z++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            mDensityGrid[x, y, z] = (ushort)Reader.ReadDouble();
                        }
                    }
                }
                Reader.Close();
                BinaryFile.Close();
                return mDensityGrid;
                #endregion
            }

            return null;

        }
    }
}
