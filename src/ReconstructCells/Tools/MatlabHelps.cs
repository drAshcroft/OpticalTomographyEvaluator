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
using System.Xml;
using System.Diagnostics;

namespace ReconstructCells.Tools
{
    public class MatlabHelps
    {

        public static void LaunchMovieMaker(string datasetname)
        {
            Process ScriptRunner = new Process();
            ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            ScriptRunner.StartInfo.FileName = @"C:\Development\CellCT\Tomographic_Imaging - Current\Tomography Simplified_New\bin\Debug\ReconMaster.exe";
            ScriptRunner.StartInfo.Arguments = "\"" + datasetname + "\"";
            ScriptRunner.Start();
            ScriptRunner.WaitForExit();

        }
        public static void CutOutCell(bool LoadWait)
        {
            PassData pass = null;
            try
            {
                Program.WriteTagsToLog("Starting", true);
                Program.WriteTagsToLog("NumberOfCells", 1);
                Program.WriteTagsToLog("Run Time", DateTime.Now);

                Program.WriteTagsToLog("Center Quality", 0.722690296022354);
                Program.WriteTagsToLog("Center Quality SD", 0.413005572786976);
                Program.WriteTagsToLog("Center Quality Variance", 1.58568483613873);

                // Tools.StackHandler.CutStack(Program.dataFolder + "\\stack", null, Program.experimentFolder);

                ///load the images and determine if it is a fluor sample
                Tools.LoadLibrary ll = new Tools.LoadLibrary();
                ll.setLoadWait(LoadWait);
                ll.SetExperimentFolder(Program.ExperimentFolder);
                ll.SetPPReader(Program.VGInfoReader);
                ll.RunNode();

                pass = ll.GetOutput();

                try
                {
                    // string vg = MainScript.GetExampleVG();
                    // File.Copy(vg, Program.DataFolder + "\\vgExample.png");
                }
                catch { }


                bool ColorImage = false;

                if (pass.FluorImage)
                    ColorImage = false;
                else if (Program.VGInfoReader != null)
                    ColorImage = (Program.VGInfoReader.GetNode("Color").ToLower() == "true");


                Program.WriteTagsToLog("IsColor", ColorImage);
                Program.WriteTagsToLog("IsFluor", pass.FluorImage);


                //show the raw projections
                Program.ShowBitmaps(pass.Library);


                Registration.RoughRegister roughReg = new Registration.RoughRegister();

                OnDemandImageLibrary unsmoothed = null;


#if TESTING
#else

                if (true)  // if (!pass.FluorImage)
                {

                    //Start the processing again
                    Background.RemoveCapillaryCurvatureBig rcc = new Background.RemoveCapillaryCurvatureBig();
                    //  Background.RemoveCapillaryCurvature rcc = new Background.RemoveCapillaryCurvature();
                    rcc.SetInput(pass);
                    rcc.RunNode();

                    pass = rcc.GetOutput();


                    Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                    roughBack.SetInput(pass);

                    if (!ll.GetOutput().FluorImage)
                        roughBack.SetNumberProjections(25);
                    else
                        roughBack.SetNumberProjections(50);

                    roughBack.RunNode();
                    pass = roughBack.GetOutput();
                    // pass.Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
                    //theBackground = ll.GetOutput().Library[10].CopyBlank();
                    //theBackground = theBackground.Add(new Gray(1));
                }
                //else
                //{
                //    theBackground = ll.GetOutput().Library[10].CopyBlank();
                //    theBackground = theBackground.Add(new Gray(1));
                //}


                //  pass.SavePassData(@"C:\temp\postcurve\" + Program.ExperimentTag);
                //  ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\postcurve\background.tif", theBackground);
#endif

                if (pass.FluorImage)
                {
                    unsmoothed = new OnDemandImageLibrary(pass.Library);
                    Imaging.SmoothingFilters smooth2 = new Imaging.SmoothingFilters(pass.Library);
                    smooth2.filterKalman(pass.Library, .15f, .7f);
                    pass.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
                }


                if (Program.VGInfoReader == null)
                {
                    while (Program.VGInfoReader == null)
                    {
                        try
                        {
                            Program.VGInfoReader = new xmlFileReader(Program.ExperimentFolder + "\\info.xml");// Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName));
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                        try
                        {
                            Program.VGInfoReader = new xmlFileReader(Program.StorageFolder + "\\info.xml");// Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName));
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

                roughReg.SetInput(pass);
                roughReg.setInfoReader(Program.VGInfoReader);
                roughReg.setPP_Reader(Program.VGPPReader);
                roughReg.RunNode();
                roughReg.SaveExamples(Program.DataFolder);

                pass = roughReg.GetOutput();

#if TESTING


#else
                //     pass.SavePassData(@"C:\temp\FBP\");
                //  ReconstructCells.Registration.nCOGRegister nCogRefine = new Registration.nCOGRegister();
                ReconstructCells.Registration.guassRegister nCogRefine = new Registration.guassRegister();
                nCogRefine.SetInput(pass);
                nCogRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                nCogRefine.RunNode();

                pass = nCogRefine.GetOutput();


                // pass.SavePassData(@"C:\temp\FBP1\");
                pass.Locations = CellLocation.SmoothPoly(pass.Locations);
                // pass.SavePassData(@"C:\temp\FBP2\");
#endif

                if (pass.FluorImage)
                {
                    pass.Library = unsmoothed;
                }


                Program.ShowBitmaps(pass.Library);
                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", pass.theBackground);
                Program.WriteTagsToLog("CellSize", pass.GetInformation("CellSize"));


                //  Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
                Background.InvertAndCut divide = new Background.InvertAndCut();
                //  divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.nCOG);

                divide.SetInput(pass);
                divide.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                divide.RunNode();

                pass = divide.GetOutput();

                pass.SavePassData(Program.DehydrateFolder);

                Registration.AlignByRecon ar = new Registration.AlignByRecon();
                ar.SetInput(pass);
                ar.setNumberOfProjections(125);
                ar.setScale(1);
                ar.RunNode();
                pass.DataScaling = 1;

                pass = ar.GetOutput();

                Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

                Program.ShowBitmaps(pass.Library);



                if (pass.FluorImage)
                {
                    pass.Library.RotateLibrary(-90);
                }


            }
            catch (Exception ex)
            {

                Program.WriteTagsToLog("Error", ex.Message);
                Program.WriteLine("Trace\n" + ex.StackTrace);

                throw ex;
            }

        }

        public static int numberRawFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.raw").Length;
        }

        public static string RawFileName(string directory, int number)
        {
            return Directory.GetFiles(directory, "*.raw")[number];
        }

        public static float[, ,] OpenVirtualStack(string directory, string filter)
        {

            string[] Filenames = Directory.GetFiles(directory, filter);
            Filenames = Filenames.SortNumberedFiles();

            return ImageProcessing.ImageFileLoader.OpenDensityDataFloat(Filenames);
        }


        static Queue<string> NetworkMessages = new Queue<string>();
        public static void StartListeningToNetwork()
        {
            Tools.NetworkCommunication.StartNetworkListener(NetworkMessages, 1234);
            //Thread.Sleep(500);
            //Tools.NetworkCommunication.StartNetworkWriter("Recon", 1234);
            //Tools.NetworkCommunication.SendMessage("testing");
            Thread.Sleep(500);
        }

        private static object networkLock = new object();
        public static bool MessageReady()
        {
            return NetworkMessages.Count != 0;
        }

        public static ProcessedDataset GetProcessedDataset(string rootDehydrateDirectory, string reconDirectory, string imageFilter)
        {
            string Directory = "";
            lock (networkLock)
            {
                Directory = NetworkMessages.Dequeue();
            }
            return new ProcessedDataset(rootDehydrateDirectory, Directory, reconDirectory, imageFilter);
        }

        public static ProcessedDataset NetworkOpenProcessedDataset(string rootDehydrateDirectory,  string reconDirectory, string imageFilter)
        {
            string Directory = "";
            ProcessedDataset pd = null;
            lock (networkLock)
            {
                while (Directory == "")
                {
                    while (NetworkMessages.Count == 0)
                        Thread.Sleep(400);

                    Directory = NetworkMessages.Dequeue();

                    pd =  new ProcessedDataset(rootDehydrateDirectory, Directory, reconDirectory, imageFilter);

                    if (!(File.Exists(pd.ReconFolder + "\\image0000.tif") && File.Exists(pd.PPFolder + "\\image000.tif")))
                        Directory ="";
                }
            }
            return pd;
        }


        public static ProcessedDataset OpenProcessedDataset(string rootDehydrateDirectory, string Directory, string reconDirectory, string imageFilter)
        {
            return new ProcessedDataset(rootDehydrateDirectory, Directory, reconDirectory, imageFilter);
        }

        public class ProcessedDataset
        {
            public string Datafolder { get; private set; }
            public string PPFolder { get; private set; }
            public string ReconFolder { get; private set; }
            public string StackFolder { get; private set; }
            public string DehydrateFolder { get; private set; }
            public string DatasetName { get; private set; }
            public string ImageFilter { get; private set; }

            public string Now
            {
                get
                {
                    return Utilities.SQLTools.DateTimeToSQL(DateTime.Now);
                }
            }

            public double[,] theBackground
            {
                get
                {
                    return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(Datafolder + "\\data\\background.tif").Data);
                }
            }

            public double[,] stackExample
            {
                get
                {
                    if (File.Exists(Datafolder + "\\data\\stackExample.tif"))
                        return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(Datafolder + "\\data\\stackExample.tif").Data);
                    else
                    {
                        string[] files = Directory.GetFiles(StackFolder);
                        Image<Gray, float> BestStack=null;
                        double StackFocus=double.MinValue;
                        for (int i = 0; i < files.Length; i++)
                        {
                            var image2 = ImageProcessing.ImageFileLoader.LoadImage(files[i]);
                            double SF4 = ImageProcessing._2D.FocusScores.F4(image2);
                            if (SF4 > StackFocus)
                            {
                                BestStack = image2;
                                StackFocus = SF4;
                            }
                        }

                        return MathLibrary.ArrayExtensions.ToDouble(BestStack.Data);
                    }
                }
            }

            public double[,] PP0
            {
                get
                {
                    return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(Datafolder + "\\data\\FirstPP.bmp").Data);
                }
            }

            public double[,] PP90
            {
                get
                {
                    return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(Datafolder + "\\data\\HalfPP.bmp").Data);
                }
            }

            public double[,] GetClippedPP(int index)
            {

                return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(DehydrateFolder + string.Format("\\clippedMirror\\image{0:000}.tif", index)).Data);
            }

            public double[,] GetClippedStack(int index)
            {

                return MathLibrary.ArrayExtensions.ToDouble(ImageProcessing.ImageFileLoader.LoadImage(DehydrateFolder + string.Format("\\stack\\stackimage{0:000}.tif", index)).Data);
            }

            public float[, ,] VirtualStack
            {
                get
                {
                    return MatlabHelps.OpenVirtualStack(ReconFolder, ImageFilter);
                }
            }
            public ProcessedDataset(string rootDehydrateDirectory, string Directory, string reconDirectory, string filter)
            {
                ImageFilter = filter;

                string dirName = Path.GetFileNameWithoutExtension(Directory);
                //return dirName;
                string[] parts = dirName.Split('_');
                string Prefix = parts[0];
                string Year = parts[1].Substring(0, 4);
                string month = parts[1].Substring(4, 2);
                string day = parts[1].Substring(6, 2);

                Datafolder = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                DehydrateFolder = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                PPFolder = DehydrateFolder + ("\\clippedPP");
                ReconFolder =Datafolder+ "\\data\\" + reconDirectory;
                StackFolder = DehydrateFolder + ("\\stack");

                DatasetName = dirName;

            }
        }



        public static float[, ,] OpenRawFile(string directory, string filename)
        {
            return ImageProcessing.ImageFileLoader.OpenDensityDataFloat(directory + "\\" + filename);
        }

        public static string GetTimeStamp()
        {
            DateTime dt = DateTime.Now;
            return string.Format("cct001_{0}{1:00}{2:00}_{3:00}{4:00}{5:00}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        public static void CreateInfoFile(string outputFilename, string TimeStamp, int xPosition, int yPosition, int Width, int Height)
        {
            String line;

            using (StreamReader sr = new StreamReader(@"c:\temp\info.xml"))
            {
                line = sr.ReadToEnd();
                line = line.Replace("XXXXXX", xPosition.ToString());
                line = line.Replace("YYYYYYY", yPosition.ToString());
                line = line.Replace("WWWWWWW", Width.ToString());
                line = line.Replace("HHHHHH", Height.ToString());


                line = line.Replace("DDDDDDDD", TimeStamp);
            }

            using (StreamWriter sr = new StreamWriter(outputFilename))
            {
                sr.Write(line);
            }

        }

        public static OnDemandImageLibrary CreateLibrary(int numberImages)
        {
            return (new OnDemandImageLibrary(numberImages, true, @"C:\temp", false));
        }

        public static OnDemandImageLibrary GetLibraryForMatlab(string LoadFolder)
        {
            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(LoadFolder);

            Files = Files.SortNumberedFiles();

            return (new OnDemandImageLibrary(Files, true, @"C:\temp", false));
        }
        public static void MatlabTest(OnDemandImageLibrary library, string WriteFolder)
        {
            PassData mPassData = new PassData();
            mPassData.Library = library;
            mPassData.Locations = CellLocation.Open(@"c:\temp\testLocations.txt");
            mPassData.AddInformation("CellSize", 170);


            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, Program.FilterLength);
            ps.SetInput(mPassData);
            ps.RunNode();


            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(WriteFolder + "\\ProjectionObject.tif", ps.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(WriteFolder + "\\ProjectionObject.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            Bitmap[] CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(WriteFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(WriteFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(WriteFolder + "\\CrossSections_Z.jpg");

            /*      Visualizations.MIP mip = new Visualizations.MIP();
                  mip.setNumberProjections(40);
                  mip.setFileName(WriteFolder + "\\mip.avi");
                  mip.SetInput(ps.GetOutput());

                  mip.RunNode();*/
        }

        static xmlFileReader ppReader = null;
        public static PassData Load(string experimentFolder, bool fluorImage)
        {


            if (!fluorImage)
                ppReader = new xmlFileReader(experimentFolder + "\\info.xml");

            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(experimentFolder);
            // ll.SetExperimentFolder(Program.dataFolder );
            //  ll.SetFluorImage(fluorImage);
            //  ll.SetIsRotated(false );

            ll.SetPPReader(ppReader);
            ll.RunNode();

            return ll.GetOutput();

        }

        public static PassData RemoveCapillary(PassData passData)
        {
            //Start the processing again
            Background.RemoveCapillaryCurvature rcc = new Background.RemoveCapillaryCurvature();
            rcc.SetInput(passData);
            rcc.RunNode();
            return rcc.GetOutput();
        }

        public static string GetDatasetName(string path)
        {
            string pPath = path.Replace("\"", "").Replace("'", "");

            if (pPath.EndsWith("\\") == false)
                pPath += "\\";

            string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);


            return dirName;
        }

        public static string[] DataFolders()
        {

            string STorage = @"z:\ASU_Recon\cct001";

            //List<string> DirAdds = 
            List<string> BadDirs = new List<string>();
            List<string> GoodDirs = new List<string>();
            bool FirstDir = true;
            string[] AllDirs;

            BadDirs.Add(STorage);
            while (FirstDir == true && BadDirs.Count > 0)
            {
                try
                {
                    string[] Dirs = Directory.GetDirectories(BadDirs[0]);
                    if (Dirs.Length > 0)
                    {
                        if (Dirs[0].Contains("cct") == true && Path.GetFileName(Dirs[0]).Length > 6)
                            GoodDirs.AddRange(Dirs);
                        else
                            BadDirs.AddRange(Dirs);
                    }
                }
                catch { }
                BadDirs.RemoveAt(0);
            }

            AllDirs = GoodDirs.ToArray();
            string pPath;
            List<string> selected = new List<string>();
            for (int i = 0; i < AllDirs.Length; i++)
            {
                try
                {
                    pPath = AllDirs[i].Replace("\"", "").Replace("'", "");

                    if (pPath.EndsWith("\\") == false)
                        pPath += "\\";

                    string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));
                    string[] parts = dirName.Split('_');
                    string Prefix = parts[0];
                    string Year = parts[1].Substring(0, 4);
                    string month = parts[1].Substring(4, 2);
                    string day = parts[1].Substring(6, 2);

                    string basePath = AllDirs[i];// +@"\data\projectionobject___TIK_16";
                    // z:\ASU_Recon\cct001\201209\21\cct001_20120921_123526\data\projectionobject___TIK_16
                    if (Directory.Exists(basePath))
                        selected.Add(basePath);
                }
                catch { }
            }

            return selected.ToArray();
        }

        public static Image<Gray, float> Getbackground(PassData passData)
        {

            Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
            roughBack.SetInput(passData);

            roughBack.SetNumberProjections(25);


            roughBack.RunNode();

            return roughBack.getBackground();
        }

        public static PassData RoughRegister(Image<Gray, float> Background, PassData passData)
        {
            Registration.RoughRegister roughReg = new Registration.RoughRegister();
            //  roughReg.setBackground(Background);
            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.SetInput(passData);
            roughReg.RunNode();
            return roughReg.GetOutput();
        }

        public static PassData DivideAndFlatten(Image<Gray, float> theBackground, PassData passData)
        {

            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            divide.setBackground(theBackground);
            divide.SetInput(passData);
            divide.setSuggestedCellSize((int)passData.GetInformation("CellSize"));
            divide.RunNode();
            return divide.GetOutput();
        }

        public static PassData AlignByRecon(PassData passData)
        {
            Registration.AlignByRecon fineAlign = new Registration.AlignByRecon();
            fineAlign.SetInput(passData);
            fineAlign.RunNode();

            return fineAlign.GetOutput();
        }

        public static PassData SiddonRecon(PassData passData, int FilterLength)
        {

            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, FilterLength);// Program.filterLength);
            ps.SetInput(passData);
            ps.RunNode();
            return ps.GetOutput();
        }

        public static double[,] Look(PassData passData, string datafolder)
        {
            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(datafolder + "\\ProjectionObject.tif", passData.DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(datafolder + "\\ProjectionObjectFiltered.raw", passData.DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            Bitmap[] CrossSections = passData.DensityGrid.ShowCross();


            CrossSections[0].Save(datafolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(datafolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(datafolder + "\\CrossSections_Z.jpg");

            var DensityGrid = passData.DensityGrid;

            int ZSlices = DensityGrid.GetLength(0);
            int XSlices = DensityGrid.GetLength(1);
            int YSlices = DensityGrid.GetLength(2);


            double[,] Slice = new double[XSlices, YSlices];
            for (int y = 0; y < YSlices; y++)
            {
                for (int x = 0; x < XSlices; x++)
                {
                    Slice[x, y] = DensityGrid[ZSlices / 2, x, y];
                }
            }

            return Slice;
        }

        public static Image<Gray, float> ConvertToEMGU(float[,] data)
        {
            float[, ,] Data = new float[data.GetLength(0), data.GetLength(1), 1];
            Buffer.BlockCopy(data, 0, Data, 0, Buffer.ByteLength(data));
            var image = new Image<Gray, float>(Data);

            return image;
        }

        public static void Save16BitTiff(string filename, float[,] data)
        {
            float[, ,] Data = new float[data.GetLength(0), data.GetLength(1), 1];
            Buffer.BlockCopy(data, 0, Data, 0, Buffer.ByteLength(data));
            var image = new Image<Gray, float>(Data);

            double[] min, max;
            Point[] minI, maxI;
            image.MinMax(out min, out max, out minI, out maxI);


            ImageProcessing.ImageFileLoader.Save_16bit_TIFF(filename, image, (float)(Int16.MaxValue / (max[0] - min[0])), (float)(min[0]));
        }

        public static void SaveTiff(string filename, float[,] data)
        {
            float[, ,] Data = new float[data.GetLength(0), data.GetLength(1), 1];
            Buffer.BlockCopy(data, 0, Data, 0, Buffer.ByteLength(data));
            var image = new Image<Gray, float>(Data);

            ImageProcessing.ImageFileLoader.Save_TIFF(filename, image);
        }

        public static void SaveTiff(string directory, int index, float[,] data)
        {
            float[, ,] Data = new float[data.GetLength(0), data.GetLength(1), 1];
            Buffer.BlockCopy(data, 0, Data, 0, Buffer.ByteLength(data));
            var image = new Image<Gray, float>(Data);

            ImageProcessing.ImageFileLoader.Save_TIFF(directory + string.Format("\\image{0:000}.tif", index), image);
        }


        public static float[, ,] Open16BitTiff(string filename)
        {
            return ImageProcessing.ImageFileLoader.Load_Tiff(filename).Data;
        }

        public class CellPositions
        {
            public double[] X_Positions;
            public double[] Y_Positions;
            public int CellSize;
            public CellPositions(int nProjections)
            {
                X_Positions = new double[nProjections];
                Y_Positions = new double[nProjections];
            }
        }
        public static CellPositions OpenXMLAndProjectionLocations(string Filename)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(Filename);

            XmlNodeList nodes = xmlDoc.SelectNodes("DataSet/PseudoProjection/BoundingBox");
            // nodes = xmlDoc.GetElementsByTagName("PseudoProjection");
            CellPositions positions = new CellPositions(500);
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    int Bottom = int.Parse(node.Attributes["bottom"].Value.ToString());
                    int Top = int.Parse(node.Attributes["top"].Value.ToString());
                    int Left = int.Parse(node.Attributes["left"].Value.ToString());
                    int Right = int.Parse(node.Attributes["right"].Value.ToString());

                    positions.X_Positions[i] = (Left + Right) / 2;
                    positions.Y_Positions[i] = (Top + Bottom) / 2;

                    positions.CellSize += Math.Abs(Left - Right);
                }

            }

            positions.CellSize /= 500;
            return positions;

        }

        public static PassData NormalizeVolume(PassData passData)
        {
            double targetSum;
            targetSum = passData.Library[10].GetSum().Intensity;

            double[] max, min;
            Point[] mP, MP;
            passData.Library[10].MinMax(out min, out max, out mP, out MP);


            var densityGrid = passData.DensityGrid;
            double sum = 0;
            int hX, hY, hZ, tX, tY, tZ;
            double R;
            float val = 0;
            hX = densityGrid.GetLength(0) / 2;
            hY = densityGrid.GetLength(1) / 2;
            hZ = densityGrid.GetLength(2) / 2;

            R = (hX * hX);
            unsafe
            {
                double maxLine = 0;
                double sumLine = 0;

                for (int x = 0; x < densityGrid.GetLength(0); x++)
                {
                    tX = x - hX;
                    tX = tX * tX;
                    for (int y = 0; y < densityGrid.GetLength(1); y++)
                    {
                        tY = y - hY;
                        tY = tY * tY;
                        sumLine = 0;
                        for (int z = 0; z < densityGrid.GetLength(2); z++)
                        {
                            val = densityGrid[x, y, z];
                            if (val < 0)
                            {
                                val = 0;
                                densityGrid[x, y, z] = 0;
                            }
                            else
                            {
                                tZ = z - hZ;
                                if (R < tX + tY + tZ * tZ)
                                {
                                    val = 0;
                                    densityGrid[x, y, z] = 0;
                                }
                            }
                            sum += val;
                            sumLine += val;
                        }
                        if (sumLine > maxLine)
                        {
                            maxLine = sumLine;
                        }
                    }


                }

                fixed (float* pData = densityGrid)
                {
                    float* pD = pData;
                    float conver = (float)(targetSum / sum);
                    conver = (float)(max[0] / maxLine);
                    for (long i = 0; i < densityGrid.LongLength; i++)
                    {
                        *pD *= conver;
                        pD++;
                    }
                }
            }

            return passData;

        }

        public static PassData SirtRecon(PassData passData)
        {
            Tomography.SIRTRecon ps = new Tomography.SIRTRecon();

            ps.SetInput(passData);
            ps.RunNode();
            return ps.GetOutput();
        }

        public static PassData SARTRecon(PassData passData)
        {
            Tomography.SART ps = new Tomography.SART();

            ps.SetInput(passData);
            ps.RunNode();
            return ps.GetOutput();
        }

        public static PassData StatRecon(PassData passData)
        {
            Tomography.StatisticalRecon ps = new Tomography.StatisticalRecon();

            ps.SetInput(passData);
            ps.RunNode();
            return ps.GetOutput();
        }

        public static PassData TikanhovRecon(PassData passData)
        {
            Tomography.TikanhovRecon ps = new Tomography.TikanhovRecon();

            ps.SetInput(passData);
            ps.RunNode();
            return ps.GetOutput();
        }


        public static string[] GetPaperDirectories(string TopDirectory, string subDir)
        {
            string[] dirs = Directory.GetDirectories(TopDirectory);
            for (int i = 0; i < dirs.Length; i++)
                dirs[i] += "\\data";
            return dirs;
        }

        public static string[] GetDataDirs8()
        {
            string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT20", "projectionobject_*", SearchOption.AllDirectories);
            List<string> dataFolders = new List<string>();
            foreach (string s in dirs)
            {
                if (s.Contains("aligned") == false && s.Contains("_8_") == true)
                {
                    dataFolders.Add(s);
                }
                else
                {
                    System.Diagnostics.Debug.Print(s);
                    //  Directory.Delete(s, true);
                }
            }
            return dataFolders.ToArray();
        }

        public static string[] GetDataDirs16()
        {
            string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT20", "projectionobject_*", SearchOption.AllDirectories);
            List<string> dataFolders = new List<string>();
            foreach (string s in dirs)
            {
                if (s.Contains("aligned") == false && s.Contains("16") == true && Directory.Exists(s + "_aligned") == false)
                {
                    dataFolders.Add(s);
                }
                else
                {
                    System.Diagnostics.Debug.Print(s);
                    //  Directory.Delete(s, true);
                }
            }
            return dataFolders.ToArray();
        }

        public static string[] GetDataDirsNew()
        {
            string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT26", "projectionobject_*", SearchOption.AllDirectories);
            List<string> dataFolders = new List<string>();
            foreach (string s in dirs)
            {
                if (s.Contains("aligned") == false)
                {
                    dataFolders.Add(s);
                }
                else
                {
                    System.Diagnostics.Debug.Print(s);
                    //  Directory.Delete(s, true);
                }
            }
            return dataFolders.ToArray();
        }

        public static string GetVGFolder(string bFolder)
        {

            string ExperimentFolder = Path.GetDirectoryName(Path.GetDirectoryName(bFolder));

            string dirName = Path.GetFileNameWithoutExtension(ExperimentFolder);
            //return dirName;
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            string VGFolder = Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + @"\500PP\recon_cropped_16bit");

            if (Directory.Exists(VGFolder) == false)
            {
                VGFolder = Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + @"\500PP\recon_cropped_8bit");
            }

            return VGFolder;
        }

        public static void SaveVolume(string SaveFile, float[, ,] data)
        {
            // MessageBox.Show(SaveFile);
            // MessageBox.Show(data.GetLength(0).ToString() + data.GetLength(1).ToString() + data.GetLength(2).ToString());
            // MessageBox.Show(data.ToString());
            ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(SaveFile, data, 0, 16);
        }


        static Emgu.CV.Capture movieFile;
        public static Emgu.CV.Capture OpenMovie(string filename)
        {
            try
            {
                movieFile = new Emgu.CV.Capture(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw ex;
            }
            return movieFile;
        }

        public static Image<Gray, float> QueryFrame()
        {
            Image<Gray, byte> frame = movieFile.QueryGrayFrame();

            return frame.Convert<Gray, float>();
            //if (frame == null)
            //    throw new Exception("Out of frames");

            //double [,] image = ImageProcessing.ArraysToBitmaps.unmakeBitmap(frame.Bitmap);
            //  return image;
        }

        public static int[,] BitmapToArrayInt(Bitmap b)
        {
            return ImageProcessing.ArraysToBitmaps.unmakeBitmapInt(b);
        }

        public static double[,] BitmapToArray(Bitmap b)
        {
            return ImageProcessing.ArraysToBitmaps.unmakeBitmap(b);
        }
    }
}
