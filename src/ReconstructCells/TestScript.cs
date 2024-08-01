
#if DEBUG
//#define TESTING
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
    public class TestScript
    {
        public static void RunReconScript(bool fluorImage, string registrationFolder)
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
            bool ColorImage = false;

            if (fluorImage)
                ColorImage = false;
            else
                ColorImage = (Program.VGInfoReader.GetNode("Color").ToLower() == "true");


            Program.WriteTagsToLog("IsColor", ColorImage);
            Program.WriteTagsToLog("IsFluor", fluorImage);

            Program.WriteTagsToLog("Center Quality", 0.722690296022354);
            Program.WriteTagsToLog("Center Quality SD", 0.413005572786976);
            Program.WriteTagsToLog("Center Quality Variance", 1.58568483613873);

            //     Tools.LoadLibrarySatir ll = new Tools.LoadLibrarySatir();
            //   ll.SetDataFolder(Program.dataFolder);

            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(Program.ExperimentFolder);
            // ll.SetExperimentFolder(Program.dataFolder );
         //   ll.SetFluorImage(fluorImage);
            //  ll.SetIsRotated(false );

            ll.SetPPReader(Program.VGInfoReader);
            ll.RunNode();

            var pass = ll.GetOutput();

            var im = pass.Library[20];
            int w = im.Width;
            //show the raw projections
            Program.ShowBitmaps(pass.Library);
            // Image<Gray, float> BackgroundCheckImage = ll.GetOutput().Library[10].Clone();
            Image<Gray, float> theBackground;
#if TESTING
            //        pass.SavePassData(@"c:\temp\flattened");
#endif

            Registration.RoughRegister roughReg = new Registration.RoughRegister();

            if (false )//!fluorImage)
            {
                //Start the processing again
                Background.RemoveCapillaryCurvature rcc = new Background.RemoveCapillaryCurvature();
                rcc.SetInput(ll.GetOutput());
                rcc.RunNode();
                var lib = rcc.GetOutput().Library;

#if TESTING
                rcc.GetOutput().SavePassData(@"c:\temp\backgroundRemoval");
                //    rcc.GetOutput().Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
                //  rcc.GetOutput().SavePassData(@"c:\temp\flattened");
                //  ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);
#endif
                Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                roughBack.SetInput(rcc.GetOutput());
                if ( false)// !fluorImage)
                {
                    // Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();

                    if (!fluorImage)
                        roughBack.SetNumberProjections(25);
                    else
                        roughBack.SetNumberProjections(50);

                    roughBack.RunNode();

                    theBackground = roughBack.getBackground();
                }
                else
                {
                    theBackground = rcc.GetOutput().Library[10].CopyBlank();
                    theBackground = theBackground.Add(new Gray(1));
                }
#if TESTING
                ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\background.tif", theBackground);
#endif
             //   roughReg.setBackground(theBackground);
                roughReg.setInfoReader(Program.VGInfoReader);
                roughReg.SetInput(roughBack.GetOutput());
                roughReg.RunNode();
                roughReg.SaveExamples(Program.DataFolder);
                pass = roughReg.GetOutput();
            }
            else
            {
                theBackground = pass.Library[10].CopyBlank();
                theBackground = theBackground.Add(new Gray(1));
                pass.Information.Add("CellSize",400);
            }
            //if there are no cells, then make a background and then exit
            /*if (!Registration.RoughRegister.CheckIfCell(BackgroundCheckImage, roughBack.getBackground()) && roughBack.GetOutput().FluorImage == false)
            {
                Background.BackgroundFromEmptyPP.CreateBackground(Program.experimentFolder, @"V:\ASU_Recon\Backgrounds");
                return;
            }*/

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", theBackground);


          
            Program.WriteTagsToLog("CellSize", pass.Information["CellSize"]);


            // Registration.ZRegistration fr = new Registration.ZRegistration();
            // fr.SetInput(roughReg.GetOutput());
            // fr.RunNode();

            if (registrationFolder != "")
            {
                //if the registration is already determined, then override the settings
                roughReg.GetOutput().Locations = CellLocation.Open(registrationFolder + @"\data\Locations.txt");

                roughReg.GetOutput().Information.Remove("CellSize");
                roughReg.GetOutput().Information.Add("CellSize", roughReg.getCellSize());
#if TESTING
                //       roughReg.SaveBitmaps(@"C:\temp\Visualized\images.bmp");
#endif
            }

            //       theBackground = theBackground.CopyBlank();
            //   theBackground=   theBackground.Add(new Gray(1));
#if TESTING
            //  roughReg.GetOutput().Locations = CellLocation.Open(@"C:\temp\testbad\data\Locations.txt");

            roughReg.GetOutput().SavePassData(@"c:\temp\flattened");
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\flattened\background.tif", theBackground);

#endif

            //theBackground = theBackground.CopyBlank();
            //theBackground = theBackground.Add(new Gray(1));
            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            divide.setBackground(theBackground);
            divide.SetInput(pass);
            divide.setSuggestedCellSize((int)pass.Information["CellSize"]);
            divide.RunNode();

#if TESTING

            roughReg.GetOutput().SavePassData(@"c:\temp\registered");
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);


#endif

            // divide.GetOutput().Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
            // Registration.SinogramAlign sa = new Registration.SinogramAlign();
            // sa.SetInput(divide.GetOutput());
            // sa.RunNode();

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(divide.GetOutput().Library);

#if TESTING
            divide.GetOutput().SavePassData(@"c:\temp\registered");
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);
#endif

            if (!divide.GetOutput().FluorImage)
            {
                // Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
                //  smooth.MedianFilter(3);
            }
            else
            {
                //   Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
                //  smooth.MedianFilter(3);
            }

            PassData outputData = divide.GetOutput();
            if (registrationFolder == "")
            {
                /*  Registration.AlignByRecon fineAlign = new Registration.AlignByRecon();
                  fineAlign.SetInput(divide.GetOutput());
                  fineAlign.RunNode();

                  CellLocation.Save(Program.dataFolder + @"\locations.txt", fineAlign.GetOutput().Locations);

                  outputData = fineAlign.GetOutput();*/

#if TESTING

                /*   outputData.Library.SaveImages(@"C:\temp\registered\images.tif");
            outputData.Library.SaveImages(@"C:\temp\Visualized\images.bmp");

            CellLocation.Save(@"c:\temp\locations.txt", outputData.Locations);*/
#endif
            }


#if TESTING
            //   outputData.Library.SaveImages(@"C:\temp\sinogram\images.tif");

#endif

            outputData.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
            Program.ShowBitmaps(outputData.Library);
            Tomography.PseudoSiddon ps = null;

            outputData.SavePassData(@"C:\temp\testHoldW");
            return;
            outputData = PassData.LoadPassData(@"C:\temp\testHold");

          
            for (int i = 0; i < 12; i++)
            {
                
                if (  i == 4)
                {
                    outputData = PassData.LoadPassData(@"C:\temp\testHold");
                    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                    smooth.MedianFilter(5);
                }

                if (i == 8)
                {
                    outputData = PassData.LoadPassData(@"C:\temp\testHold");

                   // Denoising.MRFVolume mrf = new Denoising.MRFVolume();
                   //outputData.Library = mrf.CleanSinogram(outputData.Library, 500, 2000, .3, 5);
                }

                switch (i % 4)
                {
                    case 0:
                        Program.FilterType = "RamLak";
                        Program.FilterLength = 512;
                        break;
                    case 1:
                        Program.FilterType = "Han";
                        Program.FilterLength = 256;
                        break;
                    case 2:
                        Program.FilterType = "Han";
                        Program.FilterLength = 512;
                        break;
                    case 3:
                        Program.FilterType = "Han";
                        Program.FilterLength = 1024;
                        break;
                }
                // Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
                //  smooth.MedianFilter(3);


                ps = new Tomography.PseudoSiddon();
                ps.setFilter(Program.FilterType, Program.FilterLength);
                ps.SetInput(outputData);
                ps.RunNode();


                ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

                //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

                // ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_FBP.tif", ps.GetOutput().DensityGrid);
                // ImageProcessing.ImageFileLoader.SaveDensityData(Program.dataFolder + "\\ProjectionObject.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);


                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16_2_" + i + "\\image.tif", ps.GetOutput().DensityGrid, 0, 16);
                }
                catch
                {
                }

            }


            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);

            }
            catch
            {
                System.Diagnostics.Debug.Print("");
            }

            Bitmap[] CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z.jpg");


            var c2 = ps.GetOutput().DensityGrid.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X.tif", c2[0]);
            //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Y.tif", c2[1]);
            //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Z.tif", c2[2]);

            Visualizations.MIP mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip.avi");
            mip.SetInput(ps.GetOutput());

            mip.RunNode();

            outputData = ps.GetOutput();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");



            return;

            Tomography.TikanhovRecon ps2 = new Tomography.TikanhovRecon();
            ps2.SetInput(outputData);
            ps2.RunNode();


            // ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_Tik.tif", ps2.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_Tik.raw", ps2.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_16\\image.tif", ps.GetOutput().DensityGrid, 0, 16);

            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_8\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);

            }
            catch
            {

                System.Diagnostics.Debug.Print("");
            }

            CrossSections = ps2.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_Tik.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_Tik.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_Tik.jpg");

            mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip_Tik.avi");
            mip.SetInput(ps2.GetOutput());

            mip.RunNode();


            Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
            ps3.SetInput(outputData);
            ps3.RunNode();



            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_S.raw", ps3.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);


            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_16\\image.tif", ps.GetOutput().DensityGrid, 0, 16);
            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_8\\image.jpg", ps.GetOutput().DensityGrid, 0, 8);
            }
            catch { }

            CrossSections = ps3.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_S.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_S.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_S.jpg");

            mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip_S.avi");
            mip.SetInput(ps3.GetOutput());

            mip.RunNode();




            /* Evaulation.VolumeEvaluation volEval = new Evaulation.VolumeEvaluation(Program.logFile , Program.experimentFolder, ColorImage, rCPU.StartROI);
             try
             {
                 volEval.EvaluateFocus(rCPU.ImageLibrary, rCPU.Locations);
             }
             catch { }
             try
             {
                 volEval.OutOfRange(rCPU.ImageLibrary, rCPU.Locations);
             }
             catch { }
             try
             {
                 var stackImage = volEval.EvaluatedRecon(Volume);
                 ImageProcessing.ImageFileLoader.Save_Bitmap(Program.dataFolder + "\\stack.bmp", stackImage);
                 ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\stack.tif", stackImage);
             }
             catch { }*/

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
