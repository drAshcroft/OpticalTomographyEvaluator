﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;

namespace ReconstructCells.Tools
{
    class LoadLibrary : ReconstructNodeTemplate
    {
        private string ExperimentFolder;
        private xmlFileReader ppReader;
        private bool FluorImage;
        private bool LoadWait = false;
        #region Properties
        public void setLoadWait(bool loadWait)
        {
            LoadWait = loadWait;
        }

        public void SetExperimentFolder(string experimentFolder)
        {
            this.ExperimentFolder = experimentFolder;
        }

        public void SetPPReader(xmlFileReader ppReader)
        {
            this.ppReader = ppReader;
        }

        //public void SetFluorImage(bool fluorImage)
        //{
        //    FluorImage = fluorImage;
        //}
        #endregion

        Image<Gray, float>[] Images = new Image<Gray, float>[500];
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void BatchLoadFiles(int fileNumber)
        {
            while (sw.Elapsed.Minutes < 10)
            {
                Console.WriteLine("Searching for image " + fileNumber.ToString());

                if (Images[fileNumber] == null)
                {
                    if (File.Exists(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber)) || File.Exists(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber)))
                    {
                        try
                        {
                            if (File.Exists(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber)))
                            {

                                Images[fileNumber] = ImageProcessing.ImageFileLoader.LoadImage(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber));
                                Console.WriteLine(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber));
                                return;
                            }
                            else if (File.Exists(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber)))
                            {
                                Images[fileNumber] = ImageProcessing.ImageFileLoader.LoadImage(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber));
                                Console.WriteLine(ExperimentFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber));
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    else if (File.Exists(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber)) || File.Exists(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber)))
                    {
                        try
                        {
                            if (File.Exists(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber)))
                            {
                                Images[fileNumber] = ImageProcessing.ImageFileLoader.LoadImage(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber));
                                Console.WriteLine(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.ivg", fileNumber));
                                return;
                            }
                        }
                        catch
                        { System.Threading.Thread.Sleep(3000); }

                        try
                        {
                            if (File.Exists(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber)))
                            {
                                Images[fileNumber] = ImageProcessing.ImageFileLoader.LoadImage(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber));
                                Console.WriteLine(Program.StorageFolder + "\\pp\\" + string.Format("{0:000}.png", fileNumber));
                                return;
                            }
                        }
                        catch
                        { System.Threading.Thread.Sleep(3000); }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }


        protected override void RunNodeImpl()
        {
            if (mPassData == null)
                mPassData = new PassData();


            string[] Files = null;
            bool ColorImage = false;

            //Akshay added
            //string ext = ".ivg";    //Set the file extension to the files you want to count towards your dataset
            int count;

            if (Directory.Exists(ExperimentFolder + "\\pp"))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(ExperimentFolder + "\\pp");
                count = dir.GetFiles().Length;
            }
            else
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(ExperimentFolder);
                count = dir.GetFiles().Length;
            }
            int numberOfImages = count;

            if (LoadWait == false)
            {
                if (Directory.Exists(ExperimentFolder + "\\pp"))
                {
                    Files = Directory.GetFiles(ExperimentFolder + "\\pp");
                }
                else
                {
                    Files = Directory.GetFiles(ExperimentFolder);
                }
                
                Files = Files.SortNumberedFiles();
                Console.WriteLine("FileCount: ", count);

                //if (Files.Length < 500)
                //{
                //    List<string> nFiles = new List<string>(Files);
                //    string lFile = nFiles[nFiles.Count - 1];
                //    while (nFiles.Count < 500)
                //        nFiles.Add(lFile);

                //    Files = nFiles.ToArray();
                //}

                if (ppReader != null)
                    ColorImage = (ppReader.GetNode("Color").ToLower() == "true");

                mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", ColorImage);
            }
            else
            {
                while (Directory.Exists(ExperimentFolder + "\\pp") == false)
                {
                    System.Threading.Thread.Sleep(100);
                }

                System.Threading.Thread.Sleep(1000);
                while (Directory.GetFiles(ExperimentFolder + "\\pp").Length == 0)
                    System.Threading.Thread.Sleep(100);

                mPassData.Library = new OnDemandImageLibrary(500, true, @"C:\temp", false);

                sw.Start();

                //now that the cell positions are estimated, fill in all the other cells
                // ParallelOptions po = new ParallelOptions();
                // Program.threadingParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

                //int numberOfImages = 500;

                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchLoadFiles(x));


                if (sw.Elapsed.Minutes > 10)
                    throw new Exception("Unable to load files: timeout");

                for (int i = 0; i < Images.Length; i++)
                {
                    mPassData.Library[i] = Images[i];
                }

                Files = new string[500];
                for (int i = 0; i < 500; i++)
                    Files[i] = ExperimentFolder + string.Format("\\pp\\{0:000}.ivg", i);
            }

            //  mPassData.Library[10].ROI = new System.Drawing.Rectangle(50, mPassData.Library[10].Height /2, 50, 50);

            if (Files[1].Contains("cct004") == true)
            {

                mPassData.Library.LoadLibrary();

                mPassData.Library[10].ROI = System.Drawing.Rectangle.Empty;

                float max = MathLibrary.ArrayExtensions.MaxArray(mPassData.Library[11].Data);
                for (int i = 0; i < mPassData.Library.Count; i++)
                {
                    var image = mPassData.Library[i];
                    image = image.Add(new Gray(60000 - max));
                    mPassData.Library[i] = image;
                }
                FluorImage = false;
            }
            else
            {
                double average = mPassData.Library[10].GetAverage().Intensity;
                var testFluor = mPassData.Library[10];

                var t = new Image<Bgr, byte>(testFluor.ScaledBitmap);

                var a = t.GetAverage();
                ///Threshold using the OTSU method and calulate the percentage of pixels that are above the threshold

                t = t.ThresholdOTSU(new Bgr(a.Blue + 10, a.Green + 10, a.Red + 10), new Bgr(255, 255, 255));

                var sum = t.GetSum();
                ///calculate the percentage of pixels above the threshold
                double percent = (double)sum.Blue / testFluor.Data.LongLength * 100 / 255;

                // = mPassData.Library[10].Copy();
                ///The follwing two parameters-6000 and 50-are arbitrary; they need to be adjusted for actual fluorescence PPs with lower SBR!
                if (average < 6000 && percent < 50)
                    FluorImage = true;

                //FluorImage = (testFluor.GetAverage().Intensity < 30000);
            }

            mPassData.Library[10].ROI = System.Drawing.Rectangle.Empty;
            mPassData.FluorImage = FluorImage;
            mPassData.ColorImage = ColorImage;
            mPassData.theBackground = mPassData.Library[10].CopyBlank().Add(new Gray(1));

            if (FluorImage)
            {
                BatchUtilities utils = new BatchUtilities(mPassData.Library);
                utils.FixFluorImages(false);
                //  utils.FixRotateFluorImages();
            }

            Image<Gray, float> pixelMap = null;
            if (FluorImage)
            {
                pixelMap = mPassData.Library[0].CopyBlank();
                pixelMap = pixelMap.Add(new Gray(1));
            }
            else
            {
                try
                {
                    pixelMap = Background.BackgroundFromEmptyPP.GetClosestBackground(ExperimentFolder);
                }
                catch
                {
                    pixelMap = mPassData.Library[0].CopyBlank();
                    pixelMap = pixelMap.Add(new Gray(1));
                }
                pixelMap = null;
                if (pixelMap == null)
                {
                    pixelMap = mPassData.Library[0].CopyBlank();
                    pixelMap = pixelMap.Add(new Gray(1));
                }
                //     throw new Exception("No good background found");
            }

            mPassData.PixelMap = pixelMap;

            Program.WriteTagsToLog("Files Loaded", true);


            try
            {
                using (StreamReader sr = new StreamReader(@"X:\logs\" + Program.ExperimentTag + ".txt"))
                {
                    String lines = sr.ReadToEnd();
                    Console.WriteLine(lines);

                    if (lines.Contains("LogComments"))
                    {
                        string[] cutLines = lines.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < cutLines.Length; i++)
                        {
                            string line = cutLines[i];
                            if (line.Contains("CellType"))
                            {
                                int startIDX = line.IndexOf("CellType") + 9;
                                if (line.Contains("Recon Mode"))
                                {
                                    int endIDX = line.IndexOf("Recon Mode");
                                    mPassData.AddDatabaseQueue("cellType", line.Substring(startIDX, endIDX - startIDX).Trim());
                                }
                                else
                                {
                                    int endIDX = line.IndexOf("\n", startIDX + 1);
                                    mPassData.AddDatabaseQueue("cellType", line.Substring(startIDX, endIDX - startIDX).Trim());
                                }
                            }
                            else
                            {
                                int startIDX = line.IndexOf("LogComments") + 12;
                                int endIDX = line.IndexOf("EndLogComments", startIDX + 1);
                                mPassData.AddDatabaseQueue("cellType", line.Substring(startIDX, endIDX - startIDX).Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

        }
    }
}
