using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;

namespace ReconstructCells.Tools
{
    class LoadLibrarySatir : ReconstructNodeTemplate
    {
        private string ExperimentFolder;
        private string DataFolder;
        private xmlFileReader ppReader;
        private bool FluorImage;
        private bool Rotated;

        #region Properties
        public void SetExperimentFolder(string experimentFolder)
        {
            this.ExperimentFolder = experimentFolder;
        }

        public void SetDataFolder(string dataFolder)
        {
            this.DataFolder = dataFolder;
        }

        public void SetIsRotated(bool rotated)
        {
            this.Rotated = rotated;
        }

        public void SetPPReader(xmlFileReader ppReader)
        {
            this.ppReader = ppReader;
        }

        public void SetFluorImage(bool fluorImage)
        {
            FluorImage = fluorImage;
        }
        #endregion

        protected override void RunNodeImpl()
        {
            if (mPassData == null)
                mPassData = new PassData();

            mPassData.FluorImage = FluorImage;

            string[] Files = Directory.GetFiles(ExperimentFolder + "\\pp");
            Files = Files.SortNumberedFiles();
            Files[0] = Files[1];

            if (Directory.Exists(DataFolder + "\\filtered") == false)
            {
                Directory.CreateDirectory(DataFolder + "\\converted");
                Directory.CreateDirectory(DataFolder + "\\filtered");

                for (int i = 0; i < Files.Length; i++)
                {
                    string index = string.Format("  {0}", i);
                    index = index.Substring(index.Length - 3);
                    ImageProcessing.ImageFileLoader.ConvertTo_16bit_TIFF(Files[i], DataFolder + "\\converted\\image" + index + ".tif");
                }

                Process p = new Process();
                p.StartInfo.FileName = (@"C:\Program Files (x86)\ndsafir\ndsafir.exe");
                p.StartInfo.Arguments = "-i " + DataFolder + "\\converted\\image%3d.tif -first 0 -last " + (Files.Length - 1) + " -o " + DataFolder + "\\filtered\\image%3d.tif  -sampling 2 - iter 2";

                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                p.Start();

                p.WaitForExit();

                Directory.Delete(DataFolder + "\\converted", true);
            }

            //Files = Directory.GetFiles(DataFolder + "\\filtered");
            //Files = Files.SortNumberedFiles();
            //Files[0] = Files[1];

            bool ColorImage = false;
            if (ppReader != null)
                ColorImage = (ppReader.GetNode("Color").ToLower() == "true");

            mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", ColorImage);
           // mPassData.Library.CreateAVIVideoEMGU(@"c:\temp\test.avi", 10);

            if (Rotated && FluorImage ==false )
            {

                BatchUtilities utils = new BatchUtilities(mPassData.Library);
                utils.FixRotateImages();
            }

            if (FluorImage)
            {
                if (Rotated == false)
                {
                    BatchUtilities utils = new BatchUtilities(mPassData.Library);
                    utils.FixFluorImages(false );

                    for (int i = 1; i < mPassData.Library.Count; i++)
                    {
                        mPassData.Library[i] = mPassData.Library[i].Rotate(90, new Gray(Int16.MaxValue));
                    }
                    //mPassData.Library.CreateAVIVideoEMGU(@"c:\temp\test.avi", 10);
                }
                else
                {
                    BatchUtilities utils = new BatchUtilities(mPassData.Library);
                    utils.FixFluorImages(false );

                    for (int i = 1; i < mPassData.Library.Count; i++)
                    {
                        mPassData.Library[i] = mPassData.Library[i].Rotate(90, new Gray(Int16.MaxValue));
                    }
                }
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

                if (pixelMap == null)
                {
                    pixelMap = mPassData.Library[0].CopyBlank();
                    pixelMap = pixelMap.Add(new Gray(1));
                }
                //     throw new Exception("No good background found");
            }

            mPassData.PixelMap = pixelMap;

            Program.WriteTagsToLog("Files Loaded", true);
        }
    }
}
