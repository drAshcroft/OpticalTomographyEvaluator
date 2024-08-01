using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using ImageProcessing._2D;
using MathLibrary;

namespace ReconstructCells.Evaulation
{
    public class VolumeEvaluation 
    {
        private float[, ,] Volume;
        string StackPath;
        string StackExtension;
        string VGFolder;
        bool ColorImage;
        Rectangle CellROI;
        StreamWriter logFile;

        public static void SaveEvaluation(string filename,Dictionary<string, string> TaggedElements)
        {

           string junk = "";
           foreach (KeyValuePair<string, string> kvp in TaggedElements)
            {
               
                junk += kvp.Key + ", " + kvp.Value  + "\n";
            }

            System.IO.File.WriteAllText(filename, junk);

        }

        public VolumeEvaluation(StreamWriter logFile, string experimentFolder, bool colorImage, Rectangle cellROI)
        {
            CellROI = cellROI;

            ColorImage = colorImage;
            this.logFile = logFile;
            StackPath = experimentFolder + "\\stack\\000";

            if (File.Exists(StackPath + "\\000_0000m.ivg"))
                StackExtension = ".ivg";
            if (File.Exists(StackPath + "\\000_0000m.png"))
                StackExtension = ".png";

            string dirName = Path.GetFileNameWithoutExtension(experimentFolder);
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

           // string basePath;
            VGFolder = Path.Combine("v:\\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

            if (Directory.Exists(VGFolder + "\\500pp\\recon_cropped_16bit") == true)
                VGFolder = VGFolder + "\\500pp\\recon_cropped_16bit";
            else
                VGFolder = VGFolder + "\\500pp\\recon_cropped_8bit";
        }

        #region StackEval
        const int nStackTries = 20;
        double[] StackFocus = new double[nStackTries];
        Image<Gray, float>[] stackImages = new Image<Gray, float>[nStackTries];

     

        private string StackFilename(int imageNumber)
        {
            int imageIndex = imageNumber - nStackTries / 2;
            string imagePath = StackPath;
            if (imageIndex < 0)
                imagePath += "\\000_" + string.Format("{0:0000}n{1}", Math.Abs(imageIndex), StackExtension);
            else if (imageIndex == 0)
                imagePath += "\\000_0000m" + StackExtension;
            else
                imagePath += "\\000_" + string.Format("{0:0000}p{1}", Math.Abs(imageIndex), StackExtension);
            return imagePath;
        }
        private void BatchFindBestStack(int imageNumber)
        {
            string imagePath = StackFilename(imageNumber);
           // System.Diagnostics.Debug.Print(imageNumber + "," + imagePath);
            Image<Gray, float> stackImage = ImageProcessing.ImageFileLoader.LoadImage(imagePath);
            if (ColorImage)
            {
                stackImage = ImageManipulation.FixBrokenVisiongate(stackImage);
            }
           
            stackImage.ROI = CellROI;
            stackImages[imageNumber] = stackImage.Copy();

            try
            {
                //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\stack\\image" +string.Format("{0:000}.tif",imageNumber ), stackImage);
               // ImageProcessing.ImageFileLoader.SaveFits(Program.DataFolder + "\\stack\\image" + string.Format("{0:000}.fit", imageNumber), stackImage);
            }
            catch { }

            StackFocus[imageNumber] = ImageProcessing._2D.FocusScores.F4(stackImages[imageNumber]);
        }

        private Image<Gray, float> BestStackImage(out double bestFV)
        {
            try
            {
                Directory.CreateDirectory(Program.DataFolder + "\\stack");
            }
            catch { }
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = nStackTries;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindBestStack(x));

            Image<Gray, float> stackImage = null;
            double mFV = double.MinValue;

            for (int i = 0; i < StackFocus.Length; i++)
            {
                if (mFV < StackFocus[i])
                {
                    mFV = StackFocus[i];
                    stackImage = stackImages[i];
                }

            }
            bestFV = mFV;
            return stackImage;
        }
        #endregion

        static Image<Gray, float> bestStack = null;
        #region VolumeEvals
        /// <summary>
        /// checks the focus value against a number of items and then returns the best stack image
        /// </summary>
        /// <param name="volume"></param>
        /// <returns>returns the best stack image for the given volume</returns>
        public Image<Gray,float> EvaluatedRecon(float[, ,] volume,ref  Dictionary<string,string> TaggedInfo, string identifier )
        {
            this.Volume = volume;
            double stackF4 = 1;
            double onAxisF4 = 1;
            double overallFocus = 1;

            if (bestStack == null)
            {
                bestStack = BestStackImage(out stackF4);
                ImageProcessing._2D.ImageManipulation.InvertFlattenImage(bestStack);
            }
            try
            {
                EvaluateRecon(bestStack, out onAxisF4, out overallFocus);

                TaggedInfo.Add("Eval_F4" + identifier, overallFocus.ToString());
                TaggedInfo.Add("Stack_F4" + identifier, stackF4.ToString());
                TaggedInfo.Add("ReconVsStack" + identifier, (overallFocus / stackF4).ToString());

                logFile.WriteLine("<Eval F4><" + overallFocus + "\\>");
                logFile.WriteLine("<Stack F4><" + stackF4 + "\\>");
                logFile.WriteLine("<ReconVsStack><" + (overallFocus / stackF4) + "\\>");
            }
            catch
            {

            }


            try
            {
                string[] files = Directory.GetFiles(VGFolder);
               files =  Utilities.StringExtensions.SortNumberedFiles(files);
                Image<Gray, float> ih = ImageProcessing.ImageFileLoader.LoadImage(files[files.Length / 2]);

                double FV = ImageProcessing._2D.FocusScores.F4(ih);

                TaggedInfo.Add("VG_VsStack_" + identifier, (FV / stackF4).ToString());
                TaggedInfo.Add("VG_VsASU_" + identifier, (onAxisF4 / FV).ToString());

                logFile.WriteLine("<VG_VsStack><" + (FV / stackF4).ToString() + "\\>");
                logFile.WriteLine("<VG_VsASU><" + (onAxisF4 / FV).ToString() + "\\>");
            }
            catch { }
            return bestStack;
        }


        private void EvaluateRecon(Image<Gray, float> stackImage, out double onAxisF4, out double overallFocus)
        {
            int ZSlices = Volume.GetLength(0);
            int XSlices = Volume.GetLength(1);
            int YSlices = Volume.GetLength(2);

            double[,] Slice = new double[XSlices, YSlices];
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    Slice[x, y] = Volume[ZSlices / 2, x, y];
                }
            }
            double[,] SliceX = new double[XSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    SliceX[x, z] = Volume[z, x, YSlices / 2];
                }
            }
            double[,] SliceY = new double[YSlices, ZSlices];
            for (int z = 0; z < ZSlices; z++)
            {
                for (int y = 0; y < YSlices; y++)
                {
                    SliceY[y, z] = Volume[z, XSlices / 2, y];
                }
            }


            double sumSlice = Slice.SumArray();
            string[] s = new string[6];

            onAxisF4 = 1;
            try
            {
                if (stackImage != null)
                {
                    onAxisF4 = ImageProcessing._2D.FocusScores.F4(Slice);

                }
            }
            catch { }

            overallFocus = 0;

            try
            {
                overallFocus = ImageProcessing._2D.FocusScores.F4(Slice);
                overallFocus += ImageProcessing._2D.FocusScores.F4(SliceX);
                overallFocus += ImageProcessing._2D.FocusScores.F4(SliceY);
            }
            catch { }

            overallFocus /= 3;

        }

        #endregion

        #region registration eval
        public void OutOfRange(OnDemandImageLibrary library, CellLocation[] Locations)
        {

            bool YRangeMissed = false;
            int height = library[0].Width / 2;
            for (int i = 0; i < Locations.Length; i++)
            {
                int Top =(int)( Locations[i].CellCenter.Y - height);
                int Bottom =(int)( Locations[i].CellCenter.Y + height);
                if (Top < 0 || Bottom > library[0].Height)
                {
                    YRangeMissed = true;
                    break;
                }
            }
            if (logFile != null)
                logFile.WriteLine("<OutOfImageRange><" + YRangeMissed.ToString() + "/>");
        }

        public void EvaluateFocus(OnDemandImageLibrary library, CellLocation[] Locations)
        {
            double average = 0;
            for (int i = 0; i < library.Count; i++)
            {
                Locations[i].FV = ImageProcessing._2D.FocusScores.F4(library[i]);
            }
            double[] EffectiveFV = new double[library.Count / 2];
            for (int i = 0; i < library.Count / 2; i++)
            {
                if (Locations[i].FV > Locations[i + library.Count / 2].FV)
                    EffectiveFV[i] = Locations[i].FV;
                else
                    EffectiveFV[i] = Locations[i + library.Count / 2].FV;

                average += EffectiveFV[i];
            }

            average /= (library.Count/2);
            double sd = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < library.Count; i++)
            {
                double f4 = EffectiveFV[i];
                if (f4 > max) max = f4;
                if (f4 < min) min = f4;
                f4 = f4 - average;
                sd += f4 * f4;
            }

            sd = Math.Sqrt(sd / (library.Count/2));
            logFile.WriteLine("<Focus Original><" + average + "/>");
            logFile.WriteLine("<Focus Original SD><" + sd + "/>");
            logFile.WriteLine("<Focus Original Variance><" + (max - min) + "/>");

        }
        #endregion
    }
}
