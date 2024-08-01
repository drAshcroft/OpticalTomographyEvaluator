
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
    public class MainScriptFile
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

            string fileSource = "OBD1";
            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(@"c:\temp\obd");
            // ll.SetExperimentFolder(Program.dataFolder );
            //  ll.SetFluorImage(fluorImage);
            //  ll.SetIsRotated(false );

            ll.SetPPReader(Program.VGInfoReader);
            ll.RunNode();

            PassData pass = ll.GetOutput();

            var theBackground = pass.Library[10].CopyBlank().Add(new Gray(1));
            pass.Information.Add("CellSize", (int)(theBackground.Width * .9));

            Background.DivideFlatten_NoDivide divide = new Background.DivideFlatten_NoDivide();
            divide.setBackground(theBackground);
            divide.SetInput(pass);
            divide.setSuggestedCellSize((int)pass.Information["CellSize"]);
            divide.RunNode();

            PassData outputData = divide.GetOutput();

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(outputData.Library);

            Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
            smooth.MedianFilter(5);


            outputData.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
            Program.ShowBitmaps(outputData.Library);

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            Tomography.PseudoSiddon ps = null;


            ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, 128);
            ps.SetInput(outputData);
            ps.RunNode();


            float[, ,] smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16" + fileSource + "\\image.tif", smallDensity, 0, 16);
            }
            catch
            {
            }


            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X" + fileSource + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y" + fileSource + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z" + fileSource + ".jpg");




            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");

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
