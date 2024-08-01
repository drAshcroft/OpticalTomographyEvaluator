
#if DEBUG
// #define TESTING
#endif

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

namespace ReconstructCells
{
#if PAPER_TESTS
    public class PaperTestsScript
    {
        public static void RunReconScript(string registrationFolder)
        {

            try
            {
                string vg = GetExampleVG();
                File.Copy(vg, Program.dataFolder + "\\vgExample.png");
            }
            catch { }

            Program.WriteTagsToLog("Starting", true);
            Program.WriteTagsToLog("NumberOfCells", 1);
            Program.WriteTagsToLog("Run Time", DateTime.Now);


            Program.WriteTagsToLog("Center Quality", 0.722690296022354);
            Program.WriteTagsToLog("Center Quality SD", 0.413005572786976);
            Program.WriteTagsToLog("Center Quality Variance", 1.58568483613873);


            Image<Gray, float> theBackground = null;
            Image<Gray, float> blankBackground = ImageProcessing.ImageFileLoader.LoadImage(@"c:\temp\brightfield.tif");
            Image<Gray, float> darkField = ImageProcessing.ImageFileLoader.LoadImage(@"V:\ASU_Recon\darkfield.tif");
            blankBackground = blankBackground.Sub(darkField);

            blankBackground = blankBackground.Mul(1d / blankBackground.GetAverage().Intensity);

            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(Program.experimentFolder);
            ll.SetPPReader(Program.ppReader);
            ll.RunNode();
            ll.GetOutput().Library.SubLibrary(darkField);
            PassData pass = ll.GetOutput();

            bool fluorImage = ll.GetOutput().FluorImage;
            var im = pass.Library[20];
            int w = im.Width;

            bool BackgroundTests = false;

            bool ColorImage = false;
            if (fluorImage)
                ColorImage = false;
            else
                ColorImage = (Program.ppReader.GetNode("Color").ToLower() == "true");

            if (Program.BackgroundTests)
            {
                Registration.RoughRegister roughReg = new Registration.RoughRegister();
                roughReg.setPPReader(Program.ppReader);
                Rectangle firstBox = roughReg.getFirstCellLocation();

                Background.RoughBackgrounds.TestSmoothness(pass.Library, blankBackground.CopyBlank().Add(new Gray(1)), "Raw Background", firstBox);
                Background.RoughBackgrounds.TestReadnoise(pass.Library);

                if (Program.BackgroundTests)
                    Background.RoughBackgrounds.TestSmoothness(pass.Library, blankBackground, "Blank Background", firstBox);

                //show the raw projections
                Program.ShowBitmaps(pass.Library);
                // Image<Gray, float> BackgroundCheckImage = ll.GetOutput().Library[10].Clone();
                // theBackground = blankBackground;
                //Start the processing again
                Background.RemoveCapillaryCurvatureByAverage rcc = new Background.RemoveCapillaryCurvatureByAverage();
                rcc.SetInput(ll.GetOutput());
                rcc.RunNode();
                var lib = rcc.GetOutput().Library;

                if (Program.BackgroundTests)
                    Background.RoughBackgrounds.TestSmoothness(pass.Library, blankBackground, "Capillary Blank Background", firstBox);

                for (int paperI = 0; paperI < 6; paperI++)
                {
                    try
                    {
                        Program.TestMode = paperI.ToString();

                        Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                        roughBack.setBox(firstBox);
                        roughBack.SetInput(rcc.GetOutput());

                        if (!fluorImage)
                            roughBack.SetNumberProjections(25);
                        else
                            roughBack.SetNumberProjections(50);

                        roughBack.RunNode();

                        theBackground = roughBack.getBackground();

                        pass = roughBack.GetOutput();
                    }
                    catch { }
                }

                roughReg.setBackground(theBackground);

                roughReg.SetInput(pass);
                roughReg.RunNode();
                roughReg.SaveExamples(Program.dataFolder);

                Program.ShowBitmaps(roughReg.GetOutput().Library);

                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\background.tif", theBackground);

                roughReg.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);
                ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif", theBackground);
                CellLocation.Save(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv", roughReg.GetOutput().Locations);


                Program.WriteTagsToLog("CellSize", roughReg.GetOutput().Information["CellSize"]);
            }
            // return;
            int cellSize = 219;// (int)roughReg.GetOutput().Information["CellSize"];
            PassData outputData = null;

            for (int paperI = 0; paperI <4; paperI++)
            {
                try
                {

                    pass = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
                    theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif");
                    try
                    {
                        pass.Information.Add("CellSize", cellSize);
                    }
                    catch { }
                    pass.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");


                    Program.TestMode = paperI.ToString();
                    Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
                    divide.setBackground(theBackground);
                    divide.SetInput(pass);
                    divide.setSuggestedCellSize(cellSize);
                    divide.RunNode();
                    outputData = divide.GetOutput();

                    if (paperI != 5 && paperI != 6)
                    {
                        var smooth = new Imaging.SmoothingFilters(outputData.Library);
                        smooth.MedianFilter(5);
                    }

                    var ps = new Tomography.PseudoSiddon();
                    ps.setFilter(Program.filterType,128);// Program.filterLength);
                    ps.SetInput(outputData);
                    ps.RunNode();

                    var smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();

                    Bitmap[] CrossSections = smallDensity.ShowCross();

                    CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_" + paperI + ".jpg");
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_fbp_16_" + paperI + "\\image.tif", smallDensity, 0, 16);
                }
                catch (Exception ex)
                {
                    Program.WriteLine(ex.Message);
                    Program.WriteLine(ex.StackTrace);
                }
            }


            return;

            // divide.GetOutput().Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
            // Registration.SinogramAlign sa = new Registration.SinogramAlign();
            // sa.SetInput(divide.GetOutput());
            // sa.RunNode();

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(outputData.Library);


            outputData.Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);


            outputData.SavePassData(@"C:\temp\Divided\" + Program.ExperimentTag);
            ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\Divided\" + Program.ExperimentTag + @"\background.tif", theBackground);
            CellLocation.Save(@"c:\temp\Divided\" + Program.ExperimentTag + @"\locations.csv", outputData.Locations);


            try
            {
                Directory.CreateDirectory(Program.dataFolder + "\\back_TIF");
                Directory.CreateDirectory(Program.dataFolder + "\\back_FIT");

                for (int imageNumber = 0; imageNumber < outputData.Library.Count; imageNumber++)
                {
                    ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\back_TIF\\imageTIFs" + string.Format("{0:000}.tif", imageNumber), outputData.Library[imageNumber]);
                    try
                    {
                        ImageProcessing.ImageFileLoader.SaveFits(Program.dataFolder + "\\back_FIT\\imageFits" + string.Format("{0:000}.fit", imageNumber), outputData.Library[imageNumber]);
                    }
                    catch { }
                }
            }
            catch { }

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            var vol = DoFBP(TaggedEvals);

            try
            {
                vol = DoTik(TaggedEvals, vol);
            }
            catch { }


            DoSIRT(TaggedEvals, vol);

            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");

        }

        private static float[, ,] DoSIRT(Dictionary<string, string> TaggedEvals, float[, ,] densityGrid)
        {
            var outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);
            outputData.DensityGrid = densityGrid;
            {
                Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
                ps3.SetInput(outputData);
                ps3.RunNode();

                var smallDensity = ps3.GetOutput().DensityGrid;

                var CrossSections = smallDensity.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_S_0.jpg");
                CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_S_0.jpg");
                CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_S_0.jpg");

                try
                {
                    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
                    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "SIRT");

                    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
                }
                catch { }
            }

            outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);
            outputData.DensityGrid = densityGrid;
            {
                var smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);

                Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
                ps3.SetInput(outputData);
                ps3.RunNode();

                var smallDensity = ps3.GetOutput().DensityGrid;

                var CrossSections = smallDensity.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_S_10.jpg");
                CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_S_10.jpg");
                CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_S_10.jpg");

                return smallDensity;
            }

        }

        private static float[, ,] DoTik(Dictionary<string, string> TaggedEvals, float[, ,] densityGrid)
        {
            var outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);
            outputData.DensityGrid = densityGrid;

            float[, ,] smallDensity;
            {
                Tomography.TikanhovRecon ps2 = new Tomography.TikanhovRecon();
                ps2.SetInput(outputData);
                ps2.RunNode();

                smallDensity = ps2.GetOutput().DensityGrid;

                var CrossSections = smallDensity.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_Tik_0.jpg");
                CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_Tik_0.jpg");
                CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_Tik_0.jpg");

                try
                {
                    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
                    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "TIK");
                    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
                }
                catch { }
            }

            outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);
            outputData.DensityGrid = densityGrid;
            {
                var smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);

                Tomography.TikanhovRecon ps2 = new Tomography.TikanhovRecon();
                ps2.SetInput(outputData);
                ps2.RunNode();

                smallDensity = ps2.GetOutput().DensityGrid;

                var CrossSections = smallDensity.ShowCross();
                Program.ShowBitmaps(CrossSections);

                CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_Tik_1.jpg");
                CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_Tik_1.jpg");
                CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_Tik_1.jpg");

                return ps2.GetOutput().DensityGrid;
            }

        }

        private static float[, ,] DoFBP(Dictionary<string, string> TaggedEvals)
        {
            float[, ,] smallDensity;
            {
                for (int paperI = 3; paperI < 9; paperI += 2)
                {
                    Program.TestMode = paperI.ToString();

                    var outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);

                    var smooth = new Imaging.SmoothingFilters(outputData.Library);
                    smooth.MedianFilter(paperI);

                    Program.ShowBitmaps(outputData.Library);
                    int[] sizeFilter = new int[] { 128, 512, 1024 };
                    for (int i = 0; i < sizeFilter.Length; i++)
                    {
                        Tomography.PseudoSiddon ps = null;

                        ps = new Tomography.PseudoSiddon();
                        ps.setFilter(Program.filterType, sizeFilter[i]);// Program.filterLength);
                        ps.SetInput(outputData);
                        ps.RunNode();

                        smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();

                        Bitmap[] CrossSections = smallDensity.ShowCross();

                        CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_" + paperI + "_" + sizeFilter[i] + ".jpg");
                    }
                }
            }

            {
                var outputData = PassData.LoadPassData(@"C:\temp\divided\" + Program.ExperimentTag);

                var smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);

                Program.ShowBitmaps(outputData.Library);

                {
                    Tomography.PseudoSiddon ps = null;

                    ps = new Tomography.PseudoSiddon();
                    ps.setFilter(Program.filterType, 256);// Program.filterLength);
                    ps.SetInput(outputData);
                    ps.RunNode();

                    smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();

                    outputData = ps.GetOutput();

                    Bitmap[] CrossSections = smallDensity.ShowCross();
                    Program.ShowBitmaps(CrossSections);

                    CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X.jpg");
                    CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y.jpg");
                    CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z.jpg");
                }


                var c2 = smallDensity.ShowCrossHigh();

                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_X.tif", c2[0]);
                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Y.tif", c2[1]);
                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Z.tif", c2[2]);

                try
                {
                    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
                    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "Siddon");


                    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
                }
                catch { }

                return outputData.DensityGrid;
            }

        }

        public static string GetExampleVG()
        {


            string pPath = Program.experimentFolder.Replace("\"", "").Replace("'", "");

            if (pPath.EndsWith("\\") == false)
                pPath += "\\";

            string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            string basePath;
            basePath = Path.Combine(@"y:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName) + "\\";


            if (Directory.Exists(basePath + @"500PP\recon_cropped_8bit"))
            {
                string[] files = Directory.GetFiles(basePath + @"500PP\recon_cropped_8bit");
                files = Utilities.StringExtensions.SortNumberedFiles(files);
                string filename = basePath + @"500PP\recon_cropped_8bit\reconCrop8bit_" + string.Format("{0:000}.png", files.Length / 2);

                return (filename);
            }

            return "";


        }
    }
#endif
}
