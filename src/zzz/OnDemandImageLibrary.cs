using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageProcessing._2D;
using System.IO;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading.Tasks;
using Utilities;

namespace ReconstructCells
{
    [Serializable]
    public class OnDemandImageLibrary
    {
        private Image<Gray, float>[] mImageLibrary = null;
        private string[] mOriginalFilenames = null;


        private bool mPersistInMemory = false;
        private string mTempFolder = "";

        private object[] Locks = null;
        private int[] mImageGeneration;
        private int[] mSavedInGeneration;
        private object CriticalSectionLock = new object();
        private bool mVisionGateImage = false;

        private int SaveGeneration = -1;
        private bool DoSave = false;
        private string SaveFilename = "";
        private string SaveExten = "";
        //private object CriticalSectionLock = new object();
        /// <summary>
        /// Builds a threadsafe library of images.  Images are loaded when requested by default to spread out the loading 
        /// persist in memory does not do anything at the moment.
        /// </summary>
        /// <param name="OriginalFilenames"></param>
        /// <param name="PersistInMemory"></param>
        public OnDemandImageLibrary(string[] OriginalFilenames, bool PersistInMemory, string TempFolder, bool VisionGateImages)
        {
            mVisionGateImage = VisionGateImages;
            mOriginalFilenames = OriginalFilenames;
            mImageLibrary = new Image<Gray, float>[mOriginalFilenames.Length];
            mPersistInMemory = PersistInMemory;
            mTempFolder = TempFolder;

            Locks = new object[mOriginalFilenames.Length];
            mImageGeneration = new int[mOriginalFilenames.Length];
            mSavedInGeneration = new int[mOriginalFilenames.Length];
            for (int i = 0; i < Locks.Length; i++)
            {
                Locks[i] = new object();
                mSavedInGeneration[i] = -1;
                mImageGeneration[i] = 0;
            }
        }

        public OnDemandImageLibrary(string ImageDirectory, bool PersistInMemory, string TempFolder, bool VisionGateImages)
        {
            string[] OriginalFilenames = Directory.GetFiles(ImageDirectory);
            OriginalFilenames = OriginalFilenames.SortNumberedFiles();

            mVisionGateImage = VisionGateImages;
            mOriginalFilenames = OriginalFilenames;
            mImageLibrary = new Image<Gray, float>[mOriginalFilenames.Length];
            mPersistInMemory = PersistInMemory;
            mTempFolder = TempFolder;

            Locks = new object[mOriginalFilenames.Length];
            mImageGeneration = new int[mOriginalFilenames.Length];
            mSavedInGeneration = new int[mOriginalFilenames.Length];
            for (int i = 0; i < Locks.Length; i++)
            {
                Locks[i] = new object();
                mSavedInGeneration[i] = -1;
                mImageGeneration[i] = 0;
            }
        }


        public OnDemandImageLibrary(int numImages, bool PersistInMemory, string TempFolder, bool VisionGateImages)
        {
            mVisionGateImage = VisionGateImages;
            mOriginalFilenames = new string[numImages];
            mImageLibrary = new Image<Gray, float>[numImages];
            mPersistInMemory = PersistInMemory;
            mTempFolder = TempFolder;

            Locks = new object[mOriginalFilenames.Length];
            mImageGeneration = new int[mOriginalFilenames.Length];
            mSavedInGeneration = new int[mOriginalFilenames.Length];
            for (int i = 0; i < Locks.Length; i++)
            {
                Locks[i] = new object();
                mSavedInGeneration[i] = -1;
                mImageGeneration[i] = 0;
            }
        }

        /// <summary>
        /// returns the requested image.  If the image has not been loaded yet, it loads the requested image and then returns requested image
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Image<Gray, float> this[int index]
        {
            get
            {
               // if (File.Exists(mOriginalFilenames[index]) == false)
               //     Console.WriteLine("Waiting for PP to be created");
                lock (Locks[index])
                {
                    if (mPersistInMemory == true)
                    {
                        if (mImageLibrary[index] == null)
                        {
                            if (mOriginalFilenames[index] != "")
                            {
                                string otherLocation = "z" + mOriginalFilenames[index].Substring(1);
                                // Program.WriteTagsToLog("load ", "loading" + index );
                              //  while (File.Exists(mOriginalFilenames[index]) == false && File.Exists(otherLocation) == false)
                                {
                                 //   Thread.Sleep(100);
                                    //  Program.WriteTagsToLog("error", "File does not exit");
                                }
                                try
                                {
                                    //  Program.WriteTagsToLog("error", mOriginalFilenames[index]);
                                    mImageLibrary[index] = ImageProcessing.ImageFileLoader.LoadImage(mOriginalFilenames[index]);//new Image<Gray, float>(mOriginalFilenames[index]);
                                    //  Program.WriteTagsToLog("error", "done loading" + index );
                                }
                                catch (Exception ex)
                                {
                                    Program.WriteLine(ex.Message);
                                    Program.WriteLine(ex.StackTrace);
                                    Thread.Sleep(1000);
                                    mImageLibrary[index] = ImageProcessing.ImageFileLoader.LoadImage(otherLocation);// new Image<Gray, float>(otherLocation);
                                }

                                if (mVisionGateImage == true)
                                {
                                    //  Program.WriteTagsToLog("error", "visiongate");
                                    mImageLibrary[index] = ImageManipulation.FixBrokenVisiongate(mImageLibrary[index]);
                                }
                            }
                        }
                        return mImageLibrary[index];
                    }
                    else
                    {
                        if (mImageGeneration[index] == 0)
                        {
                          //  while (File.Exists(mOriginalFilenames[index]) == false)
                            {
                            //    Thread.Sleep(100);
                            }

                            if (mVisionGateImage == true)
                            {
                                return ImageManipulation.LoadBrokenVisiongate(mOriginalFilenames[index]);
                            }
                            else
                            {
                                return new Image<Gray, float>(mOriginalFilenames[index]);
                            }
                        }
                        else
                        {
                           // while (File.Exists(mOriginalFilenames[index]) == false)
                            {
                             //   Thread.Sleep(100);
                            }

                            string filename = string.Format("{0}_{3}_{1:0000}.{2}", mTempFolder + "temp", index, "raw", mImageGeneration[index] - 1);

                            if (mVisionGateImage == true)
                            {
                                return ImageManipulation.LoadBrokenVisiongate(mOriginalFilenames[index]);
                            }
                            else
                            {
                                return new Image<Gray, float>(mOriginalFilenames[index]);
                            }
                        }
                    }
                }
            }

            set
            {
                lock (CriticalSectionLock)
                {
                    lock (Locks[index])
                    {
                        if (mPersistInMemory == true)
                        {
                            mImageLibrary[index] = value;
                            mImageGeneration[index]++;
                        }
                        else
                        {
                            string filename = string.Format("{0}_{3}_{1:0000}.{2}", mTempFolder + "temp", index, "raw", mImageGeneration[index]);
                            value.Save(filename);
                            mImageGeneration[index]++;
                        }

                        if (DoSave == true && SaveGeneration == mImageGeneration[index])
                        {

                            mImageLibrary[index].Save(string.Format("{0}_{3}_{1:0000}.{2}", SaveFilename, index, SaveExten, SaveGeneration));
                            mSavedInGeneration[index] = mImageGeneration[index];
                        }
                    }
                }
            }

        }

        public float[, ,] GetDataArray(int index)
        {
            return this[index].Data;
        }
        public void SetDataArray(int index, float[,] Data)
        {
            Emgu.CV.Image<Gray, float> image = new Image<Gray, float>(Data.GetLength(0), Data.GetLength(1));
            Buffer.BlockCopy(Data, 0, image.Data, 0, Buffer.ByteLength(Data));
            this[index] = image;
        }


        public void SetDataArrayAsImage(int index, string filePattern, float[,] Data)
        {
            Emgu.CV.Image<Gray, float> image = new Image<Gray, float>(Data.GetLength(1), Data.GetLength(0));

            //for (int i=0;i<Data.GetLength(0);i++)
            //    for (int j = 0; j < Data.GetLength(1); j++)
            //    {
            //        image.Data [
            //    }


            Buffer.BlockCopy(Data, 0, image.Data, 0, Buffer.ByteLength(Data));

            string extension = Path.GetExtension(filePattern).ToLower();
            string directory = Path.GetDirectoryName(filePattern);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            if (extension == "" || extension == ".tif" || extension == ".tiff")
            {
                string fileAndDir = Path.GetDirectoryName(filePattern) + "\\" + Path.GetFileNameWithoutExtension(filePattern);
                int i = index;
                string filename = string.Format("{0}{1:000}.tif", fileAndDir, i);
                ImageProcessing.ImageFileLoader.Save_TIFF(filename, image);
            }
            else
            {
                string fileAndDir = Path.GetDirectoryName(filePattern) + "\\" + Path.GetFileNameWithoutExtension(filePattern);
                int i = index;
                string filename = string.Format("{0}{1:000}{2}", fileAndDir, i, extension);
                ImageProcessing.ImageFileLoader.Save_Bitmap(filename, image);
            }
        }

        public void SaveDataArray(int index, string filePattern, float[,] Data)
        {
            string extension = Path.GetExtension(filePattern).ToLower();
            string directory = Path.GetDirectoryName(filePattern);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            string fileAndDir = Path.GetDirectoryName(filePattern) + "\\" + Path.GetFileNameWithoutExtension(filePattern);
            string filename = string.Format("{0}{1:000}.mbin", fileAndDir, index);

            FileStream BinaryFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BinaryWriter Writer = new BinaryWriter(BinaryFile);

            Writer.Write((int)(Data.GetLength(0)));
            Writer.Write((int)(Data.GetLength(1)));
            for (int z = 0; z < Data.GetLength(0); z++)
            {
                for (int y = 0; y < Data.GetLength(1); y++)
                {
                    Writer.Write((float)(Data[z, y]));
                }
            }
            Writer.Close();
            BinaryFile.Close();
        }


        public void SetDataArray(int index, float[, ,] Data)
        {
            this[index] = new Image<Gray, float>(Data);
        }

        public int Count
        {
            get { return mImageLibrary.Length; }
        }

        public int GetImageGeneration(int ImageIndex)
        {
            lock (CriticalSectionLock)
            {
                return mImageGeneration[ImageIndex];
            }
        }

        /// <summary>
        /// This function is designed for threaded operation.  It will set a flag that will save all the images
        /// that are placed into the image library to disk.  Once this generation is set, then the library will stop
        /// saving.  Must be called before the first image of the current generation is put into the library.
        /// </summary>
        /// <param name="FilePattern">This shows the filepattern to be used to save i.e.  c:\fileDir\CenteringImage.bmp The index numbers will be automatically added </param>
        public void SaveImageGenerationForward(string FilePattern)
        {
            lock (CriticalSectionLock)
            {

                SaveGeneration = mImageGeneration[0] + 1;
                SaveFilename = Path.GetFileNameWithoutExtension(FilePattern);
                SaveExten = Path.GetExtension(FilePattern);
                DoSave = true;
            }
        }

        /// <summary>
        /// This function will save all images that are in the library that are at a certain generation.
        /// </summary>
        /// <param name="ImageGeneration"></param>
        /// <param name="FilePattern"></param>
        public void SaveImageGenerationExisting(int ImageGeneration, string FilePattern)
        {
            lock (CriticalSectionLock)
            {
                SaveGeneration = ImageGeneration;
                SaveFilename = Path.GetDirectoryName(FilePattern) + "\\" + Path.GetFileNameWithoutExtension(FilePattern);
                SaveExten = Path.GetExtension(FilePattern);
                for (int i = 0; i < mOriginalFilenames.Length; i++)
                {
                    if (mSavedInGeneration[i] != ImageGeneration && mImageGeneration[i] == ImageGeneration)
                    {
                        mImageLibrary[i].Save(string.Format("{0}_{3}_{1:0000}.{2}", SaveFilename, i, SaveExten, ImageGeneration));
                        mSavedInGeneration[i] = ImageGeneration;
                    }
                }

            }
        }

        public void SaveImages(string Filepattern)
        {
            string extension = Path.GetExtension(Filepattern).ToLower();
            string directory = Path.GetDirectoryName(Filepattern);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            if (extension == "" || extension == ".tif" || extension == ".tiff")
            {

                string fileAndDir = Path.GetDirectoryName(Filepattern) + "\\" + Path.GetFileNameWithoutExtension(Filepattern);
                for (int i = 0; i < mImageLibrary.Length; i++)
                {
                    if (mImageLibrary[i] != null)
                    {
                        string filename = string.Format("{0}{1:000}.tif", fileAndDir, i);
                        ImageProcessing.ImageFileLoader.Save_TIFF(filename, mImageLibrary[i]);
                    }
                }
            }
            else
            {
                string fileAndDir = Path.GetDirectoryName(Filepattern) + "\\" + Path.GetFileNameWithoutExtension(Filepattern);
                for (int i = 0; i < mImageLibrary.Length; i++)
                {
                    if (mImageLibrary[i] != null)
                    {
                        string filename = string.Format("{0}{1:000}{2}", fileAndDir, i, extension);
                        ImageProcessing.ImageFileLoader.Save_Bitmap(filename, mImageLibrary[i]);
                    }
                }
            }
        }

        public void Save16bitImages(string Filepattern)
        {
            string extension = Path.GetExtension(Filepattern).ToLower();
            string directory = Path.GetDirectoryName(Filepattern);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            if (extension == "" || extension == ".tif" || extension == ".tiff")
            {

                string fileAndDir = Path.GetDirectoryName(Filepattern) + "\\" + Path.GetFileNameWithoutExtension(Filepattern);
                for (int i = 0; i < mImageLibrary.Length; i++)
                {
                    string index = string.Format("  {0}", i);
                    index = index.Substring(index.Length - 3);
                    string filename = string.Format("{0}{1}.tif", fileAndDir, index);
                    ImageProcessing.ImageFileLoader.Save_16bit_TIFF(filename, this[i], 1, 0);
                }
            }
            else
            {
                throw new Exception("Only supports 16bit tif");
            }
        }


        #region Cropping

        private Rectangle CropRegion;
        public void BatchCropImage(int ImageNumber)
        {
            mImageLibrary[ImageNumber] = mImageLibrary[ImageNumber ].Copy(CropRegion);
        }

        public void CropImagesCenter(int Width, int Height)
        {
            int minX = (int)(mImageLibrary[10].Width / 2 - Width / 2);
                int minY = (int)( mImageLibrary[10].Height / 2 - Height / 2);
            CropRegion = new Rectangle(minX ,minY ,Width, Height );

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = this.Count;

            Parallel.For(0, numberOfImages, po, x => BatchCropImage(x));
        }

        #endregion

        #region BatchLoad
        private void batchload(int imagenumber)
        {
            var image = this[imagenumber];
            int w = image.Width;
        }
        public void LoadLibrary()
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = this.Count;

            Parallel.For(0, numberOfImages, po, x => batchload(x));

        }

        #endregion

        public void CreateAVIVideoEMGU(string AVIFilename, int SkipFrames)
        {
            int w = 0;
            for (int i = 0; i < mImageLibrary.Length; i++)
                if (mImageLibrary[i] == null)
                    w = this[i].Width;

            ImageProcessing._2D.MovieMaker.CreateAVIVideoEMGU(AVIFilename, mImageLibrary, SkipFrames);

        }


        #region AverageImages
        [NonSerialized]
        double[][,] Averages;
         [NonSerialized]
        double[][,] AverageDiffsX;
         [NonSerialized]
        double[][,] AverageDiffsY;
         [NonSerialized]
        int nProcessors = 1;

        private void BatchAverageImages(int BlockNumber)
        {
            var tImage = this[10];
            Averages[BlockNumber] = new double[tImage.Height, tImage.Width];
            var lenth = Averages[BlockNumber].LongLength;
            unsafe
            {
                fixed (double* pSum = Averages[BlockNumber])
                {
                    for (int i = BlockNumber; i < this.mImageLibrary.Length; i += nProcessors)
                    {
                        double* pOut = pSum;
                        tImage = this[i];
                        fixed (float* pData = tImage.Data)
                        {
                            float* pIn = pData;
                            for (long j = 0; j < lenth; j++)
                            {
                                *pOut += *pIn;
                                pOut++;
                                pIn++;
                            }
                        }
                    }
                }
            }
        }


        public Image<Gray, float> AverageLibrary()
        {
            //now that the cell positions are estimated, fill in all the other cells
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            nProcessors = po.MaxDegreeOfParallelism;


            Averages = new double[nProcessors][,];

            int numberOfImages = nProcessors;

            Parallel.For(0, numberOfImages, po, x => BatchAverageImages(x));

            var outImage = this[10].CopyBlank();

            var lenth = Averages[0].LongLength;

            double nImages = mImageLibrary.Length;
            unsafe
            {
                fixed (float* pSum = outImage.Data)
                {
                    for (int i = 0; i < nProcessors; i++)
                    {
                        float* pOut = pSum;
                        fixed (double* pData = Averages[i])
                        {
                            double* pIn = pData;
                            for (long j = 0; j < lenth; j++)
                            {
                                *pOut += (float)(*pIn / nImages);
                                pOut++;
                                pIn++;
                            }
                        }
                    }
                }
            }

            return outImage;

        }

        private double IntensityTarget = 1;

        private void BatchBackgroundImages(int BlockNumber)
        {

            int BlockWidth = mImageLibrary.Length / nProcessors;

            int startI = BlockNumber * BlockWidth;
            int endI = startI + BlockWidth;


            var tImage = this[10];
            Averages[BlockNumber] = new double[tImage.Height, tImage.Width];
            double[,] Weights = new double[tImage.Height, tImage.Width];

            var lenth = Averages[BlockNumber].LongLength;
            unsafe
            {
                fixed (double* pSum = Averages[BlockNumber])
                fixed (double* pWeights = Weights)
                {
                    for (int i = startI; i < endI; i++)
                    {
                        double* pOut = pSum;
                        double* pW = pWeights;
                        tImage = this[i];


                        double sum = 0;

                        for (int x = 0; x < tImage.Width; x++)
                            for (int y = 0; y < 10; y++)
                            {
                                sum += tImage.Data[y, x, 0];
                                sum += tImage.Data[tImage.Height - y - 1, x, 0];
                            }

                        sum = IntensityTarget / sum;
                        fixed (float* pData = tImage.Data)
                        {
                            float* pIn = pData;
                            for (long j = 0; j < lenth; j++)
                            {
                                *pOut += *pIn * (*pIn) * sum;
                                *pW += *pIn;
                                pOut++;
                                pIn++;
                                pW++;
                            }
                        }
                    }

                    {
                        double* pOut = pSum;
                        double* pW = pWeights;
                        for (long j = 0; j < lenth; j++)
                        {
                            *pOut = (*pOut) / (*pW);
                            pOut++;
                            pW++;
                        }
                    }

                }
            }
        }

        public Image<Gray, float> FindBackground()
        {
            var tImage = this[10];

            IntensityTarget = 0;

            for (int i = 0; i < tImage.Width; i++)
                for (int j = 0; j < 10; j++)
                {
                    IntensityTarget += tImage.Data[j, i, 0];
                    IntensityTarget += tImage.Data[tImage.Height - j - 1, i, 0];
                }


            //now that the cell positions are estimated, fill in all the other cells
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            nProcessors = 25;

            int BlockWidth = mImageLibrary.Length / nProcessors;

            Averages = new double[nProcessors][,];

            int numberOfImages = nProcessors;

            Parallel.For(0, numberOfImages, po, x => BatchBackgroundImages(x));

            var outImage = this[10].CopyBlank();

            double[] f = new double[Averages.Length];
            List<double> fList = new List<double>(f);

            for (int x = 0; x < outImage.Width; x++)
                for (int y = 0; y < outImage.Height; y++)
                {


                    for (int i = 0; i < Averages.Length; i++)
                    {
                        fList[i] = Averages[i][y, x];
                    }
                    fList.Sort();
                    outImage.Data[y, x, 0] = (float)(fList[fList.Count - 2]);
                }


            return outImage;

        }


        private void BatchRegularizedBackgroundImages(int BlockNumber)
        {

            int BlockWidth = mImageLibrary.Length / nProcessors;

            int startI = BlockNumber * BlockWidth;
            int endI = startI + BlockWidth;


            var tImage = this[10];
            Averages[BlockNumber] = new double[tImage.Height, tImage.Width];
            AverageDiffsX[BlockNumber] = new double[tImage.Height, tImage.Width];
            AverageDiffsY[BlockNumber] = new double[tImage.Height, tImage.Width];
            double[,] Weights = new double[tImage.Height, tImage.Width];

            double[,] DiffsX = AverageDiffsX[BlockNumber];
            double[,] DiffsY = AverageDiffsY[BlockNumber];

            var lenth = Averages[BlockNumber].LongLength;
            unsafe
            {
                fixed (double* pSum = Averages[BlockNumber])
                fixed (double* pWeights = Weights)
                {
                    for (int i = startI; i < endI; i++)
                    {
                        double* pOut = pSum;
                        double* pW = pWeights;
                        tImage = this[i];

                        double sum = 0;

                        for (int x = 0; x < tImage.Width; x++)
                            for (int y = 0; y < 10; y++)
                            {
                                sum += tImage.Data[y, x, 0];
                                sum += tImage.Data[tImage.Height - y - 1, x, 0];
                            }

                        sum = IntensityTarget / sum;
                        fixed (float* pData = tImage.Data)
                        {
                            float* pIn = pData;
                            for (long j = 0; j < lenth; j++)
                            {
                                *pOut += *pIn * (*pIn) * sum;
                                *pW += *pIn;
                                pOut++;
                                pIn++;
                                pW++;
                            }
                        }

                        for (int x = 1; x < DiffsX.GetLength(1) - 1; x++)
                            for (int y = 1; y < DiffsX.GetLength(0) - 1; y++)
                            {
                                DiffsX[y, x] += 2 * tImage.Data[y, x, 0] - tImage.Data[y, x - 1, 0] - tImage.Data[y, x + 1, 0];
                                DiffsY[y, x] += 2 * tImage.Data[y, x, 0] - tImage.Data[y - 1, x, 0] - tImage.Data[y + 1, x, 0];
                            }
                    }

                    {
                        double* pOut = pSum;
                        double* pW = pWeights;
                        for (long j = 0; j < lenth; j++)
                        {
                            *pOut = (*pOut) / (*pW);
                            pOut++;
                            pW++;
                        }
                    }

                }
            }
        }

        public Image<Gray, float> FindBackgroundRegularized()
        {
            var tImage = this[10];

            IntensityTarget = 0;

            for (int i = 0; i < tImage.Width; i++)
                for (int j = 0; j < 10; j++)
                {
                    IntensityTarget += tImage.Data[j, i, 0];
                    IntensityTarget += tImage.Data[tImage.Height - j - 1, i, 0];
                }


            //now that the cell positions are estimated, fill in all the other cells
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            nProcessors = 25;

            int BlockWidth = mImageLibrary.Length / nProcessors;

            Averages = new double[nProcessors][,];
            AverageDiffsX = new double[nProcessors][,];
            AverageDiffsY = new double[nProcessors][,];

            int numberOfImages = nProcessors;

            Parallel.For(0, numberOfImages, po, x => BatchRegularizedBackgroundImages(x));

            var outImage = this[10].CopyBlank();

            double[] f = new double[Averages.Length];
            List<double> fList = new List<double>(f);

            #region calcMax
            for (int x = 0; x < outImage.Width; x++)
                for (int y = 0; y < outImage.Height; y++)
                {
                    for (int i = 0; i < Averages.Length; i++)
                    {
                        fList[i] = Averages[i][y, x];
                    }
                    fList.Sort();
                    outImage.Data[y, x, 0] = (float)(fList[fList.Count - 2]);
                }

            #endregion

            #region Diffs
            var lenth = Averages[0].LongLength;
            Image<Gray, float> i_diffX = outImage.CopyBlank();
            Image<Gray, float> i_diffY = outImage.CopyBlank();

            float[, ,] diffX = i_diffX.Data;
            float[, ,] diffY = i_diffY.Data;

            double nImages = mImageLibrary.Length;
            unsafe
            {
                fixed (float* pSumX = diffX)
                fixed (float* pSumY = diffY)
                {
                    for (int i = 0; i < nProcessors; i++)
                    {
                        float* pOutX = pSumX;
                        float* pOutY = pSumY;

                        fixed (double* pDataX = AverageDiffsX[i])
                        fixed (double* pDataY = AverageDiffsY[i])
                        {
                            double* pInX = pDataX;
                            double* pInY = pDataY;

                            for (long j = 0; j < lenth; j++)
                            {
                                *pOutX += (float)(*pInX / nImages);
                                pOutX++;

                                *pOutY += (float)(*pInY / nImages);
                                pOutY++;

                                pInX++;
                                pInY++;
                            }
                        }
                    }
                }
            }
            #endregion

            #region regularization
            int nIterations = 5;
            float alpha = 0.1f;
            float dx, dy;
            float val;
            for (int i = 0; i < nIterations; i++)
            {

                for (int x = 1; x < outImage.Width - 1; x++)
                    for (int y = 1; y < outImage.Height - 1; y++)
                    {
                        val = 2 * outImage.Data[y, x, 0];
                        dx = val - outImage.Data[y, x - 1, 0] - outImage.Data[y, x + 1, 0];
                        dx = (dx - diffX[y, x, 0]) * alpha;

                        outImage.Data[y, x, 0] -= dx;
                    }

                for (int x = 1; x < outImage.Width - 1; x++)
                    for (int y = 1; y < outImage.Height - 1; y++)
                    {
                        val = 2 * outImage.Data[y, x, 0];

                        dy = val - outImage.Data[y - 1, x, 0] - outImage.Data[y + 1, x, 0];
                        dy = (dy - diffY[y, x, 0]) * alpha;

                        outImage.Data[y, x, 0] -= dy;
                    }
            }
            #endregion

            return outImage;

        }

         [NonSerialized]
        private Image<Gray, float> SegmentBack;
         [NonSerialized]
        private int[,] segment;
        private void Batch_BackgroundImages_Value_Segment(int segmentIndex)
        {
            int BlockWidth = mImageLibrary.Length / nProcessors;


            for (int x = segmentIndex; x < segment.GetLength(1); x += nProcessors)
                for (int y = 0; y < segment.GetLength(0); y++)
                {
                    int BlockNumber = segment[y, x];
                    int startI = BlockNumber * BlockWidth;
                    int endI = startI + BlockWidth;

                    double sum = 0;

                    for (int i = startI; i < endI; i++)
                    {
                        //var tImage = mImageLibrary[i];
                        sum += mImageLibrary[i].Data[y, x, 0];
                    }

                    SegmentBack.Data[y, x, 0] = (float)(sum / BlockWidth);
                }
        }

        public Image<Gray, float> FindBackground_Segment()
        {
            var tImage = this[10];

            IntensityTarget = 0;

            for (int i = 0; i < tImage.Width; i++)
                for (int j = 0; j < 10; j++)
                {
                    IntensityTarget += tImage.Data[j, i, 0];
                    IntensityTarget += tImage.Data[tImage.Height - j - 1, i, 0];
                }


            //now that the cell positions are estimated, fill in all the other cells
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            nProcessors = 25;

            int BlockWidth = mImageLibrary.Length / nProcessors;

            Averages = new double[nProcessors][,];

            int numberOfImages = nProcessors;

            Parallel.For(0, numberOfImages, po, x => BatchBackgroundImages(x));

            var outImage = this[10].CopyBlank();
            SegmentBack = outImage;
            segment = new int[outImage.Height, outImage.Width];
            AverageHolder[] f = new AverageHolder[Averages.Length];
            for (int i = 0; i < Averages.Length; i++)
                f[i] = new AverageHolder(i, 0);

            List<AverageHolder> fList = new List<AverageHolder>(f);

            for (int x = 0; x < outImage.Width; x++)
                for (int y = 0; y < outImage.Height; y++)
                {

                    for (int i = 0; i < Averages.Length; i++)
                    {
                        fList[i].index = i;
                        fList[i].value = Averages[i][y, x];
                    }
                    fList.Sort(delegate(AverageHolder a1, AverageHolder a2) { return a1.value.CompareTo(a2.value); });
                    segment[y, x] = fList[fList.Count - 2].index;
                }

            Parallel.For(0, numberOfImages, po, x => Batch_BackgroundImages_Value_Segment(x));

            return outImage;

        }

        private class AverageHolder
        {
            public int index;
            public double value;
            public AverageHolder(int index, double value)
            {
                this.index = index;
                this.value = value;
            }
        }
        #endregion
    }
}
