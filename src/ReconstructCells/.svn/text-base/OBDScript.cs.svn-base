
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

    public class OBDScript
    {
        public static void RunReconScript(string registrationFolder)
        {

            try
            {
                string vg = GetExampleVG();
                File.Copy(vg, Program.DataFolder + "\\vgExample.png");
            }
            catch { }

            Program.WriteTagsToLog("Starting", true);
            Program.WriteTagsToLog("NumberOfCells", 1);
            Program.WriteTagsToLog("Run Time", DateTime.Now);

            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(Program.ExperimentFolder);
            ll.SetPPReader(Program.VGInfoReader);
            ll.RunNode();

            PassData pass = ll.GetOutput();

            bool fluorImage = ll.GetOutput().FluorImage;
            var im = pass.Library[20];
            int w = im.Width;


            bool ColorImage = false;

            if (fluorImage)
                ColorImage = false;
            else
                ColorImage = (Program.VGInfoReader.GetNode("Color").ToLower() == "true");


            Program.WriteTagsToLog("IsColor", ColorImage);
            Program.WriteTagsToLog("IsFluor", fluorImage);


            //show the raw projections
            Program.ShowBitmaps(pass.Library);
            Image<Gray, float> theBackground;

            Registration.RoughRegister roughReg = new Registration.RoughRegister();

            if (!fluorImage)
            {
                //Start the processing again
                Background.RemoveCapillaryCurvatureByAverage rcc = new Background.RemoveCapillaryCurvatureByAverage();
                rcc.SetInput(ll.GetOutput());
                rcc.RunNode();
                var lib = rcc.GetOutput().Library;

                Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                roughBack.SetInput(rcc.GetOutput());

                if (!fluorImage)
                    roughBack.SetNumberProjections(25);
                else
                    roughBack.SetNumberProjections(50);

                roughBack.RunNode();

                theBackground = roughBack.getBackground();

                pass = roughBack.GetOutput();
            }
            else
            {
                theBackground = ll.GetOutput().Library[10].CopyBlank();
                theBackground = theBackground.Add(new Gray(1));
            }


           // roughReg.setBackground(theBackground);
            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.SetInput(pass);
            roughReg.RunNode();
            roughReg.SaveExamples(Program.DataFolder);

            Program.ShowBitmaps(roughReg.GetOutput().Library);
            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", theBackground);
            Program.WriteTagsToLog("CellSize", roughReg.GetOutput().Information["CellSize"]);


          
            pass = roughReg.GetOutput();

            Background.DivideFlattenFileOut divide = new Background.DivideFlattenFileOut();
            divide.setBackground(theBackground);
            divide.SetInput(pass);
            divide.setSuggestedCellSize((int)pass.Information["CellSize"]);
            divide.RunNode();




            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(divide.GetOutput().Library);

            pass = divide.GetOutput();

            pass.SavePassData(Program.DataFolder + "\\passData");

            pass.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
            Program.ShowBitmaps(pass.Library);

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            DoFBP(0, pass, "");
        
            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");
        }



        public static void LoadReconScript(string registrationFolder)
        {

            try
            {
                string vg = GetExampleVG();
                File.Copy(vg, Program.DataFolder + "\\vgExample.png");
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

            //Tools.LoadLibrary ll = new Tools.LoadLibrary();
            //ll.SetExperimentFolder(Program.experimentFolder);
            //ll.SetPPReader(Program.ppReader);
            //ll.RunNode();
            //    ll.GetOutput().Library.SubLibrary(darkField);
            string passFolder = Program.DataFolder+ "\\passData";
            PassData pass = PassData.LoadPassData(passFolder);

            string[] dirs = Directory.GetDirectories(Program.DataFolder);

            for (int dirI = 0; dirI < dirs.Length; dirI++)
            {
                 string foldername = Path.GetFileName(dirs[dirI]);
                 string crossSec = Program.DataFolder + "\\CrossSections_X_sm_0__O_" + foldername + ".tif";
                if (dirs[dirI].Contains("\\OBD") == true && dirs[dirI].EndsWith("_L")  
                    && File.Exists(crossSec)==false)
                {
                    try
                    {
                        if (Directory.GetFiles(dirs[dirI]).Length == 500)
                        {
                           

                            Background.DivideFlatten_fileCOG divide = new Background.DivideFlatten_fileCOG();
                            divide.setFileFolder(foldername);
                            divide.setBackground(theBackground);
                            divide.SetInput(pass);

                            divide.RunNode();
                            var outputData = divide.GetOutput();

                            Program.ShowBitmaps(outputData.Library);

                            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

                            // divide.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);

                            outputData = DoFBP(0, outputData, "_O_" + foldername);
                            outputData = DoFBP(1, outputData, "_O_" + foldername);
                           // outputData = DoFBP(2, outputData, "_O_" + foldername);
                            //outputData = DoFBP(true, outputData);

                            // outputData = Tik(true, outputData);
                            // outputData = Tik(false, outputData);

                            // outputData = SIRT(true, outputData);
                            //    outputData = SIRT(false, outputData);
                        }
                    }
                    catch (Exception ex) 
                    {
                        Program.WriteTagsToLog("Error" + dirs[dirI], ex.Message);
                    }
                }
            }
        }

        private static PassData DoFBP(int smoothed, PassData outputData, string foldername)
        {

            // outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);

            if (smoothed == 2)
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);
                smooth.MedianFilter(3);
            }
            {
                if (smoothed == 1)
                {
                    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                    smooth.BilateralSmoothFilter(5);
                }
                else
                {
                    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                    smooth.MedianFilter(3);
                }
            }

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            Tomography.PseudoSiddon ps = null;
            ps = new Tomography.PseudoSiddon();
            // ps.setFilter(Program.filterType, 128);// Program.filterLength);
           
                ps.setFilter(Program.FilterType, 128);

            string T = "sm_" + smoothed;
            
            T = T + "_" + foldername;

            ps.SetInput(outputData);
            ps.RunNode();

            outputData = ps.GetOutput();
            float[, ,] smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();


            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16_" + T + "\\image.tif", smallDensity, 0, 16);
            }
            catch
            {
            }

            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            var c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X_" + T + ".tif", c2[0]);

            outputData = ps.GetOutput();

            return outputData;
        }

        private static PassData Tik(bool smoothed, PassData outputData)
        {


            outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);

            if (smoothed)
            {
                //Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                //smooth.MedianFilter(3);
            }
            Tomography.Tikhonov2Recon ps2 = new Tomography.Tikhonov2Recon();
            ps2.SetInput(outputData);
            ps2.RunNode();

            var smallDensity = ps2.GetOutput().DensityGrid;

            string T = "H";
            if (smoothed)
                T = "L";

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik__O_" + T + "_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }
           

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_Tik__O_" + T + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_Tik__O_" + T + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_Tik__O_" + T + ".jpg");

            var c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X_TIK_O_" + T + ".tif", c2[0]);
            return ps2.GetOutput();
        }

        private static PassData SIRT(bool smoothed, PassData outputData)
        {
           // outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);

            if (smoothed)
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);
            }

            string T = "H";
            if (smoothed)
                T = "L";
            Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
            ps3.SetInput(outputData);
            ps3.RunNode();

            var smallDensity = ps3.GetOutput().DensityGrid;

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_16__O_" + T + "\\image.tif", smallDensity, 0, 16);
            }
            catch { }
           

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_S__O_" + T + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_S__O_" + T + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_S__O_" + T + ".jpg");

            return ps3.GetOutput();

        }


        public static string GetExampleVG()
        {


            string pPath = Program.ExperimentFolder.Replace("\"", "").Replace("'", "");

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

}
