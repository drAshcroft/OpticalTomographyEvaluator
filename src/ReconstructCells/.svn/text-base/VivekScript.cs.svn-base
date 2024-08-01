
#if DEBUG
#define TESTING
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
    public class VivekScript
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


            Program.WriteTagsToLog("Center Quality", 0.722690296022354);
            Program.WriteTagsToLog("Center Quality SD", 0.413005572786976);
            Program.WriteTagsToLog("Center Quality Variance", 1.58568483613873);

            //     Tools.LoadLibrarySatir ll = new Tools.LoadLibrarySatir();
            //   ll.SetDataFolder(Program.dataFolder);

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
            // Image<Gray, float> BackgroundCheckImage = ll.GetOutput().Library[10].Clone();
            Image<Gray, float> theBackground;


            Registration.RoughRegister roughReg = new Registration.RoughRegister();

            if (!fluorImage)
            {
                //Start the processing again
                Background.RemoveCapillaryCurvatureByAverage rcc = new Background.RemoveCapillaryCurvatureByAverage();
                //  Background.RemoveCapillaryCurvature rcc = new Background.RemoveCapillaryCurvature();
                rcc.SetInput(ll.GetOutput());
                rcc.RunNode();
                var lib = rcc.GetOutput().Library;

#if TESTING
                //  rcc.GetOutput().SavePassData(@"c:\temp\backgroundRemoval");
                //    rcc.GetOutput().Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
                //  rcc.GetOutput().SavePassData(@"c:\temp\flattened");
                //  ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);
#endif
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

          //  roughReg.setBackground(theBackground);
            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.SetInput(pass);
            roughReg.RunNode();
            roughReg.SaveExamples(Program.DataFolder);

            Program.ShowBitmaps(roughReg.GetOutput().Library);


            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", theBackground);



            Program.WriteTagsToLog("CellSize", roughReg.GetOutput().Information["CellSize"]);



            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            divide.setBackground(theBackground);
            divide.SetInput(roughReg.GetOutput());
            divide.setSuggestedCellSize((int)roughReg.GetOutput().Information["CellSize"]);
            divide.RunNode();

#if TESTING

            divide.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif", theBackground);
            CellLocation.Save(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv", roughReg.GetOutput().Locations);
#endif

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(divide.GetOutput().Library);

            divide.GetOutput().Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
            Program.ShowBitmaps(divide.GetOutput().Library);

            var outputData = divide.GetOutput();


            outputData = DoFBP(false, outputData);
          //  outputData = DoFBP(true, outputData);

           // outputData = Tik(true, outputData);
            outputData = Tik(false, outputData);

          //  outputData = SIRT(true, outputData);
            outputData = SIRT(false, outputData);
        }


        private static PassData DoFBP(bool smoothed, PassData outputData)
        {

            outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);


            if (smoothed)
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(5);
            }
            else
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);
            }

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            Tomography.PseudoSiddon ps = null;
            ps = new Tomography.PseudoSiddon();
            // ps.setFilter(Program.filterType, 128);// Program.filterLength);
            if (smoothed)
                ps.setFilter(Program.FilterType, 128);
            else
                ps.setFilter(Program.FilterType, 256);

            string T = "";
            if (smoothed)
                T = "L";

            ps.SetInput(outputData);
            ps.RunNode();

            outputData = ps.GetOutput();
            float[, ,] smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObject" + T + ".raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);


            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8" + T + "\\image.tif", smallDensity, 0, 8);

            }
            catch
            {
                System.Diagnostics.Debug.Print("");
            }

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16" + T + "\\image.tif", smallDensity, 0, 16);
            }
            catch
            {
            }



            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X" + T + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y" + T + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z" + T + ".jpg");


            var c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X" + T + ".tif", c2[0]);


            outputData = ps.GetOutput();

            return outputData;
        }

        private static PassData Tik(bool smoothed, PassData outputData)
        {


            outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);

            if (smoothed)
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                smooth.MedianFilter(3);
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
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_" + T + "_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_" + T + "_8\\image.tif", smallDensity, 0, 8);

            }
            catch
            {

                System.Diagnostics.Debug.Print("");
            }

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_Tik_" + T + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_Tik_" + T + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_Tik_" + T + ".jpg");


            return ps2.GetOutput();
        }

        private static PassData SIRT(bool smoothed, PassData outputData)
        {
            outputData.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + "\\library", true, @"c:\temp", false);

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
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_16_" + T + "\\image.tif", smallDensity, 0, 16);
            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_8_" + T + "\\image.tif", smallDensity, 0, 8);
            }
            catch { }

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_S_" + T + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_S_" + T + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_S_" + T + ".jpg");

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
