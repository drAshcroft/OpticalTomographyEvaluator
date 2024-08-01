
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
    public class PaperTestsScript2
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

#if TESTING
            //        pass.SavePassData(@"c:\temp\flattened");
#endif

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

#if TESTING
            //     ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\background.tif", theBackground);
#endif
            roughReg.SetInput(pass);
          //  roughReg.setBackground(theBackground);
            roughReg.setInfoReader(Program.VGInfoReader);
           
            roughReg.RunNode();
            roughReg.SaveExamples(Program.DataFolder);

            Program.ShowBitmaps(roughReg.GetOutput().Library);
            //if there are no cells, then make a background and then exit
            /*if (!Registration.RoughRegister.CheckIfCell(BackgroundCheckImage, roughBack.getBackground()) && roughBack.GetOutput().FluorImage == false)
            {
                Background.BackgroundFromEmptyPP.CreateBackground(Program.experimentFolder, @"V:\ASU_Recon\Backgrounds");
                return;
            }*/

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", theBackground);



            Program.WriteTagsToLog("CellSize", roughReg.GetOutput().Information["CellSize"]);


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
            //pass= roughReg.GetOutput();
            //roughReg.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            //ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif", theBackground);
            //CellLocation.Save(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv", roughReg.GetOutput().Locations);

         
#endif
            pass = roughReg.GetOutput();
            // pass.Library = lib2;
            //theBackground = theBackground.CopyBlank();
            //   theBackground=   theBackground.Add(new Gray(1));


            //theBackground = theBackground.CopyBlank();
            //theBackground = theBackground.Add(new Gray(1));
            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            divide.setBackground(theBackground);
            divide.SetInput(pass);
            divide.setSuggestedCellSize((int)pass.Information["CellSize"]);
            divide.RunNode();

#if TESTING
            pass = divide.GetOutput();
            var image = divide.MakeSingoGram(pass.Library[0].Width / 2);// Background.DivideFlattenAndInvertBackgrounds.MakeSinoGram(pass.Library[0].Width / 2, pass.Library, theBackground, pass.Locations);
            int wwww = image.Width;

            //divide.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            //ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif", theBackground);
            //CellLocation.Save(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv", roughReg.GetOutput().Locations);


           // var tImage = divide.MakeSingoGram(pass.Library[0].Width / 2);

           // int ww = tImage.Width;
            //roughReg.GetOutput().SavePassData(@"c:\temp\registered");
            //ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);


            //Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            //re.MirrorEvaluation(divide.GetOutput().Library);

#endif

            // return;
            // divide.GetOutput().Library.CreateAVIVideoEMGU(Program.dataFolder + "\\centering.avi", 10);
            // Registration.SinogramAlign sa = new Registration.SinogramAlign();
            // sa.SetInput(divide.GetOutput());
            // sa.RunNode();

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(divide.GetOutput().Library);

            pass = divide.GetOutput();

            OnDemandImageLibrary unsmoothed = new OnDemandImageLibrary(pass.Library.Count, true, @"c:\temp", false);
            for (int i = 0; i < pass.Library.Count; i++)
            {
                pass.Library[i].ROI = Rectangle.Empty;
                unsmoothed[i] = pass.Library[i].Copy();
            }

            Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);

            smooth.MedianFilter(3);
            // smooth.AddGaussianNoiseFilter(500);
            if (!divide.GetOutput().FluorImage)
            {

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

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            #region FBP
            Tomography.PseudoSiddon ps = null;

            //  return;

            ps = new Tomography.PseudoSiddon();
            // ps.setFilter(Program.filterType, 128);// Program.filterLength);
            ps.setFilter(Program.FilterType, 128);
            ps.SetInput(outputData);
            ps.RunNode();




            // ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);
            float[, ,] smallDensity = ps.GetOutput().DensityGrid;//.InsideSphere();
            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            // ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_FBP.tif", ps.GetOutput().DensityGrid);
            //ImageProcessing.ImageFileLoader.SaveDensityData(Program.dataFolder + "\\ProjectionObject.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObject.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16\\image.tif", smallDensity, 0, 16);
            }
            catch
            {
            }

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8\\image.tif", smallDensity, 0, 8);

            }
            catch
            {
                System.Diagnostics.Debug.Print("");
            }

            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z.jpg");


            var c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X.tif", c2[0]);
            //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Y.tif", c2[1]);
            //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_Z.tif", c2[2]);

            Visualizations.MIP mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip.avi");
            mip.SetInput(ps.GetOutput());

            mip.RunNode();
            #endregion
            return;
            //#region TIK
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            ////outputData.SavePassData(@"c:\temp\fbp");
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Program.WriteTagsToLog("Finished All Threads", true);
            //Program.WriteTagsToLog("ErrorMessage", "Succeeded");
            //// return;
            ////try
            ////{
            ////    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            ////    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "Siddon");


            ////    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            ////}
            ////catch { }

            //Tomography.Tikhonov2Recon ps2 = new Tomography.Tikhonov2Recon();
            //ps2.SetInput(outputData);
            //ps2.RunNode();

            //smallDensity = ps2.GetOutput().DensityGrid;
            //// ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_Tik.tif", ps2.GetOutput().DensityGrid);
            //ImageProcessing.ImageFileLoader.SaveDensityData(Program.dataFolder + "\\ProjectionObjectFiltered_Tik.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            //try
            //{
            //    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_tik_16\\image.tif", smallDensity, 0, 16);

            //}
            //catch { }
            //try
            //{
            //    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_tik_8\\image.tif", smallDensity, 0, 8);

            //}
            //catch
            //{

            //    System.Diagnostics.Debug.Print("");
            //}

            //CrossSections = smallDensity.ShowCross();
            //Program.ShowBitmaps(CrossSections);

            //CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_Tik.jpg");
            //CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_Tik.jpg");
            //CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_Tik.jpg");

            ////mip = new Visualizations.MIP();
            ////mip.setNumberProjections(40);
            ////mip.setFileName(Program.dataFolder + "\\mip_Tik.avi");
            ////mip.SetInput(ps2.GetOutput());

            ////mip.RunNode();

            //#endregion
            //            //try
            //            //{
            //            //    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            //            //    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "TIK");


            //            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //            //}
            //            //catch { }




            outputData = ps.GetOutput();
            outputData.Library = new OnDemandImageLibrary(unsmoothed);
            #region deconv
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");


            Tomography.obdSIRTRecon3 psO = new Tomography.obdSIRTRecon3();
            psO.SetInput(outputData);
            psO.setRealign(true);
            psO.RunNode();

            smallDensity = psO.GetOutput().DensityGrid;
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_O.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_O_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }


            CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_O.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_O.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_O.jpg");

            c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X_o.tif", c2[0]);

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_O.avi");
            //mip.SetInput(psO.GetOutput());

            //mip.RunNode();
            #endregion

            return;

            outputData = ps.GetOutput();
            outputData.Library = new OnDemandImageLibrary(unsmoothed);
            //#region deconv2
            ////outputData.SavePassData(@"c:\temp\fbp");
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Program.WriteTagsToLog("Finished All Threads", true);
            //Program.WriteTagsToLog("ErrorMessage", "Succeeded");


            // psO = new Tomography.obdSIRTRecon();
            // psO.setRealign(false);
            //psO.SetInput(outputData);
            //psO.RunNode();

            //smallDensity = psO.GetOutput().DensityGrid;
            //ImageProcessing.ImageFileLoader.SaveDensityData(Program.dataFolder + "\\ProjectionObjectFiltered_O.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            //try
            //{
            //    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_O_16\\image.tif", smallDensity, 0, 16);

            //}
            //catch { }


            //CrossSections = smallDensity.ShowCross();
            //Program.ShowBitmaps(CrossSections);

            //CrossSections[0].Save(Program.dataFolder + "\\CrossSections_X_O.jpg");
            //CrossSections[1].Save(Program.dataFolder + "\\CrossSections_Y_O.jpg");
            //CrossSections[2].Save(Program.dataFolder + "\\CrossSections_Z_O.jpg");

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_O.avi");
            //mip.SetInput(ps2.GetOutput());

            //mip.RunNode();
            //#endregion
            return;

            #region SIRT
            outputData = ps.GetOutput();
            outputData.Library = new OnDemandImageLibrary(unsmoothed);
            Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
            ps3.SetInput(outputData);
            ps3.RunNode();

            smallDensity = ps3.GetOutput().DensityGrid;

            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_S.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);


            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_16\\image.tif", smallDensity, 0, 16);
            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_sirt_8\\image.tif", smallDensity, 0, 8);
            }
            catch { }

            CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_S.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_S.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_S.jpg");

            mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip_S.avi");
            mip.SetInput(ps3.GetOutput());

            mip.RunNode();

            //try
            //{
            //    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            //    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "SIRT");


            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //}
            //catch { }

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
            #endregion

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
