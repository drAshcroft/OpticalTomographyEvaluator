﻿
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
using System.Diagnostics;

namespace ReconstructCells
{
    public class MainScriptSR
    {

        public static void RunReconScript(string registrationFolder, bool LoadWait)
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


                // pass.SavePassData(@"c:\temp\nnn1");

                bool ColorImage = false;

                if (pass.FluorImage)
                    ColorImage = false;
                else if (Program.VGInfoReader != null)
                    ColorImage = (Program.VGInfoReader.GetNode("Color").ToLower() == "true");


                Program.WriteTagsToLog("Version", "2.0");

                Program.WriteTagsToLog("IsColor", ColorImage);
                Program.WriteTagsToLog("IsFluor", pass.FluorImage);


                //show the raw projections
                Program.ShowBitmaps(pass.Library);

                OnDemandImageLibrary unsmoothed;//= new OnDemandImageLibrary(pass.Library);

#if TESTING
#else

                if (!pass.FluorImage)
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
                // return;

                if (pass.FluorImage)
                {
                    unsmoothed = new OnDemandImageLibrary(pass.Library);

                    Imaging.SmoothingFilters smooth2 = new Imaging.SmoothingFilters(pass.Library);
                    smooth2.MedianFilter(5);
                    smooth2.MedianFilter(5);
                    //Imaging.SmoothingFilters smooth2 = new Imaging.SmoothingFilters(pass.Library);
                    //smooth2.filterKalman(pass.Library, .15f, .7f);
                    //pass.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
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
                        if (Program.VGInfoReader == null)
                        {
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
                }

                Program.ShowBitmaps(pass.Library);

                Console.WriteLine("Starting Registartion");

                Registration.RoughRegister roughReg = new Registration.RoughRegister();
                roughReg.SetInput(pass);
                roughReg.setInfoReader(Program.VGInfoReader);
                roughReg.setPP_Reader(Program.VGPPReader);
                roughReg.RunNode();
                roughReg.SaveExamples(Program.DataFolder);

                pass = roughReg.GetOutput();


                pass.Locations = CellLocation.SmoothPoly(pass.Locations);

                Console.WriteLine("Starting Noisy");

                if (!pass.FluorImage)
                {
                    ReconstructCells.Registration.nCOGRegister nCogRefine = new Registration.nCOGRegister();
                    nCogRefine.SetInput(pass);
                    nCogRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                    nCogRefine.RunNode();
                    pass = nCogRefine.GetOutput();
                }

                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", pass.theBackground);
                Program.WriteTagsToLog("CellSize", pass.GetInformation("CellSize"));

                Console.WriteLine("Starting align 1");
                //  pass.SavePassData(@"c:\temp\nnn1");
                Registration.AlignByRecon ar = new Registration.AlignByRecon();
                ar.SetInput(pass);
                ar.setNumberOfProjections(pass.Library.Count / 2);
                ar.setAlreadyCut(false);
                ar.setScale(1);
                ar.RunNode();
                pass = ar.GetOutput();


                //  if (pass.FluorImage)
                {
                    //  pass.Library = unsmoothed;
                }

                  pass.SavePassData(@"c:\temp\nnn2");
                // return;
                Console.WriteLine("Starting mirror");
                ReconstructCells.Registration.mirrorAlignRegister mirrorRefine = new Registration.mirrorAlignRegister();
                mirrorRefine.SetInput(pass);
                mirrorRefine.setMergeMethod(Registration.mirrorAlignRegister.MergeMethodEnum.SuperRes);
                mirrorRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                mirrorRefine.setMergeMirror(true);
                mirrorRefine.RunNode();
                // return;//

                // pass.SavePassData(@"c:\temp\nnn3");
                Console.WriteLine("Starting align 2");
                ar = new Registration.AlignByRecon();
                ar.SetInput(pass);
                ar.setNumberOfProjections(pass.Library.Count);
                ar.setAlreadyCut(true);
                ar.setScale(1);
                ar.RunNode();
                pass = ar.GetOutput();
                Console.WriteLine("Finish align 2");

             

                //pass.Library.SaveImages(Program.DehydrateFolder + "\\image.tif");

                pass.Library.SaveImages(Program.DataFolder + "\\aligned\\image.tif");

                try
                {
                    ImageProcessing.ImageFileLoader.Save_TIFF(Program.DehydrateFolder + "\\theBackground.tif", pass.theBackground);
                }
                catch { }

                Program.ShowBitmaps(pass.Library);

                unsmoothed = new OnDemandImageLibrary(pass.Library);

                Program.ShowBitmaps(pass.Library);

                Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

                Console.WriteLine("Starting smooth");
              //  Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(pass.Library);
               // smooth.MedianFilter(3);

                //pass = FBP(pass, 128, "d_m_128_", 0, false, false);
                // pass = FBP(pass, 512, "d_m", 3, false, false);
                Console.WriteLine("Starting FBP 1");

                //  pass.SavePassData(@"c:\temp\beforeFPB");
                // return;
                pass = FBP(pass, 512, "_512_m", 0, false, false);

                // return;
                ar = new Registration.AlignByRecon();
                ar.SetInput(pass);
                ar.setNumberOfProjections(pass.Library.Count);
                ar.setAlreadyCut(true);
                ar.setAlreadyReconed(true);
                ar.setScale(1);
                ar.RunNode();
                pass = ar.GetOutput();

                try
                {
                    CellLocation.Save(Program.DehydrateFolder + "\\locations.csv", pass.Locations);
                }
                catch { }

                Console.WriteLine("Starting FBP 2");
                pass = FBP(pass, 512, "_512_a", 0, false, false);
                float[, ,] smoothRecon = pass.DensityGrid;

               

                ar = new Registration.AlignByRecon();
                ar.SetInput(pass);
                ar.setNumberOfProjections(pass.Library.Count);
                ar.setAlreadyCut(true);
                ar.setAlreadyReconed(true);
                ar.setScale(1);
                ar.RunNode();
                pass = ar.GetOutput();

                Console.WriteLine("Starting Tik");

                pass = TIK(pass, "_");

            }
            catch (Exception ex)
            {

                Program.WriteTagsToLog("Error", ex.Message);
                Program.WriteLine("Trace\n" + ex.StackTrace);
            }
            finally
            {
                try
                {
                    GetExampleVG(pass, Program.DataFolder + "\\vgExample.png");
                    //string vg = 
                    // File.Copy(vg, Program.DataFolder + "\\vgExample.png");
                }
                catch { }

                try
                {
                    //    ReconstructCells.Tools.StackHandler.CutStack(Program.DataFolder + "\\stack\\image.tif", pass.Locations, pass.theBackground, Program.ExperimentFolder);

                }
                catch { }



                pass.Library = null;
                pass.DensityGrid = null;
                pass.WriteInformationToLog();
                // pass.SavePassData(Program.dataFolder + "\\passData");
                Environment.Exit(0);
                throw new Exception("did not kill gracefully\n");
            }
        }


        public static float[, ,] SaveCombined(PassData pass, float[, ,] smoothRecon)
        {
            pass.DensityGrid = ImageProcessing._3D.Filters3D.GuassianBlur(pass.DensityGrid);

            smoothRecon.AddToArray(pass.DensityGrid);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16_HC\\image.tif", smoothRecon, 0, 16);
            }
            catch
            {
            }

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8_HC\\image.tif", smoothRecon, 0, 8);
            }
            catch
            {
            }
            Bitmap[] CrossSections = smoothRecon.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_HC.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_HC.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_HC.jpg");

            return smoothRecon;
        }
        static Random rnd = new Random();
        public static PassData FBP(PassData pd, int FilterWidth, string Tag, int smoothIt, bool doHalfProjections, bool makeMovie)
        {
            #region FBP
            //Tomography.PseudoSiddon ps = null;
            //ps = new Tomography.PseudoSiddon();
            //ps.setFilter(Program.FilterType, FilterWidth);
            //ps.SetInput(pd);
            //ps.setHalfProjections(doHalfProjections);
            //ps.RunNode();
            if (rnd.NextDouble() > 0)
            {
                Tomography.OptRecon ps = new Tomography.OptRecon();
                ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                ps.setFilter(Program.FilterType, FilterWidth);
                ps.SetInput(pd);
                ps.RunNode();

                pd = ps.GetOutput();
            }
            else
            {
                Tomography.GPURecon ps = new Tomography.GPURecon();
                ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                ps.setFilter("RamLak", FilterWidth);
                ps.SetInput(pd);
                ps.RunNode();

                pd = ps.GetOutput();
            }
            float[, ,] smallDensity = pd.DensityGrid; //.InsideSphere();

            if (smoothIt == 555)
            {
                smallDensity = ImageProcessing._3D.Filters3D.GuassianBlur555(smallDensity);
            }
            else
            {
                for (int i = 0; i < smoothIt; i++)
                {
                    smallDensity = ImageProcessing._3D.Filters3D.GuassianBlur(smallDensity);
                }
            }

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16" + Tag + "\\image.tif", smallDensity, 0, 16);
            }
            catch
            {
            }

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8" + Tag + "\\image.tif", smallDensity, 0, 8);
            }
            catch
            {
            }

            pd.DensityGrid = smallDensity;

            //// ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObject" + Tag + ".raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);
            //if (smoothIt > 0)
            //{
            //    try
            //    {
            //        ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16" + Tag + "_s" + smoothIt + "\\image.tif", smallDensity, 0, 16);
            //    }
            //    catch
            //    { }

            //    try
            //    {
            //        ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8" + Tag + "_s" + smoothIt + "\\image.tif", smallDensity, 0, 8);
            //    }
            //    catch
            //    { }
            //}
            //try
            //{
            //    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_8" + Tag + "\\image.tif", smallDensity, 0, 8);
            //}
            //catch
            //{
            //}


            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X" + Tag + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y" + Tag + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z" + Tag + ".jpg");

            //var c2 = smallDensity.ShowCrossHigh();

            //ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_X" + Tag + ".tif", c2[0]);

            //try
            //{

            //    //   pd.SavePassData(@"c:\temp\finalFilter");

            //    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(null);
            //    //  smooth.filterKalman(ref smallDensity, .15f, .7f);
            //    smallDensity = ImageProcessing._3D.Filters3D.GuassianBlur555(smallDensity);
            //    //  Denoising.MRFVolume mrf = new Denoising.MRFVolume();

            //    // float covar =(float)( smallDensity.MaxArray()*.05f);

            //    //  mrf.CleanSinogram(smallDensity, covar, covar*4, 1f / 6f, 20);


            //    Bitmap[] CrossSections2 = smallDensity.ShowCross();
            //    Program.ShowBitmaps(CrossSections2);

            //    CrossSections2[0].Save(Program.dataFolder + "\\CrossSections_X_g_" + Tag + ".jpg");
            //    CrossSections2[1].Save(Program.dataFolder + "\\CrossSections_Y_g_" + Tag + ".jpg");
            //    CrossSections2[2].Save(Program.dataFolder + "\\CrossSections_Z_g_" + Tag + ".jpg");

            //    c2 = smallDensity.ShowCrossHigh();

            //    ImageProcessing.ImageFileLoader.Save_TIFF(Program.dataFolder + "\\CrossSections_X_g_" + Tag + ".tif", c2[0]);

            //    pd.DensityGrid = smallDensity;

            //    try
            //    {
            //        ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_fbp_16_g_" + Tag + "\\image.tif", smallDensity, 0, 16);
            //    }
            //    catch
            //    {
            //    }
            //}
            //catch { }

            if (makeMovie)
            {
                Visualizations.MIP mip = new Visualizations.MIP();
                mip.setNumberProjections(40);
                mip.setFileName(Program.DataFolder + "\\mip.avi");
                mip.SetInput(pd);

                mip.RunNode();
            }
            #endregion

            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");
            // return;
            //try
            //{
            //    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            //    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "Siddon");


            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //}
            //catch { }
            return pd;
        }


        public static PassData OPT(PassData pd, int FilterWidth, string Tag, int smoothIt, bool doHalfProjections, bool makeMovie)
        {

            Tomography.OptRecon ps = new Tomography.OptRecon();
            ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
            ps.setFilter(Program.FilterType, FilterWidth);
            ps.SetInput(pd);

            ps.RunNode();
            pd = ps.GetOutput();
            float[, ,] smallDensity = pd.DensityGrid; //.InsideSphere();

            pd.DensityGrid = smallDensity;

            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_XO" + Tag + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_YO" + Tag + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_ZO" + Tag + ".jpg");


            return pd;
        }

        public static PassData TIK(PassData ps, string tag)
        {
            #region TIK
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Tomography.OptRecon ps2 = new Tomography.OptRecon();
            ps2.setReconType(Tomography.OptRecon.ReconTypes.SIRT);
            ps2.setNumIterations(5);
            ps2.setCleanProjections(true);
            ps2.SetInput(ps);
            ps2.RunNode();
            ps = ps2.GetOutput();

            //Tomography.TIKRecon4 ps2 = new Tomography.TIKRecon4();
            //ps2.SetInput(ps);
            //ps2.RunNode();

            var smallDensity = ps2.GetOutput().DensityGrid;

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_" + tag + "_TIK_16\\image.tif", smallDensity, 0, 16);
            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_" + tag + "_TIK_8\\image.tif", smallDensity, 0, 8);
            }
            catch { }


            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_" + tag + "_TIK.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_" + tag + "_TIK.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_" + tag + "_TIK.jpg");

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_Tik.avi");
            //mip.SetInput(ps2.GetOutput());

            //mip.RunNode();
            //try
            //{
            //   Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            // ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "TIK");


            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //}
            //catch { }
            return ps2.GetOutput();
            #endregion

        }

        public static PassData BIL(PassData ps, string tag)
        {
            #region BIL
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Tomography.Bil_IRT ps2 = new Tomography.Bil_IRT();
            ps2.SetInput(ps);
            ps2.RunNode();

            var smallDensity = ps2.GetOutput().DensityGrid;

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_" + tag + "Bil_IRT_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }


            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_" + tag + "Bil_IRT.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_" + tag + "Bil_IRT.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_" + tag + "Bil_IRT.jpg");

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_Tik.avi");
            //mip.SetInput(ps2.GetOutput());

            //mip.RunNode();
            //try
            //{
            //    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            //    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "TIK");


            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //}
            //catch { }
            return ps2.GetOutput();
            #endregion

        }


        public static PassData MRF(PassData ps, string tag)
        {
            #region TIK
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            Tomography.MRF_IRT ps2 = new Tomography.MRF_IRT();
            ps2.SetInput(ps);
            ps2.RunNode();

            var smallDensity = ps2.GetOutput().DensityGrid;
            // ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.dataFolder + "\\ProjectionObject_Tik.tif", ps2.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_" + tag + "_MRF_IRT.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_" + tag + "MRF_IRT_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_" + tag + "MRF_IRT_8\\image.tif", smallDensity, 0, 8);

            }
            catch
            {

                System.Diagnostics.Debug.Print("");
            }

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_" + tag + "MRF_IRT.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_" + tag + "MRF_IRT.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_" + tag + "MRF_IRT.jpg");

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_Tik.avi");
            //mip.SetInput(ps2.GetOutput());

            //mip.RunNode();
            //try
            //{
            //    Evaulation.VolumeEvaluation ve = new Evaulation.VolumeEvaluation(Program.logFile, Program.experimentFolder, false, outputData.Locations[1].ToRectangle());
            //    ve.EvaluatedRecon(smallDensity, ref TaggedEvals, "TIK");


            //    Evaulation.VolumeEvaluation.SaveEvaluation(@"C:\temp\EvaulationMetrics\" + Program.ExperimentTag + "_reconEval_feature_" + Program.WorkTags + ".csv", TaggedEvals);
            //}
            //catch { }
            return ps2.GetOutput();
            #endregion

        }


        public static PassData Deconv3(PassData pd)
        {





            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            #region deconv
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Program.WriteTagsToLog("Finished All Threads", true);
            Program.WriteTagsToLog("ErrorMessage", "Succeeded");


            Tomography.obdSIRTRecon3 psO = new Tomography.obdSIRTRecon3();
            psO.SetInput(pd);
            psO.setRealign(true);
            psO.RunNode();

            var smallDensity = psO.GetOutput().DensityGrid;
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_O.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_O_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }


            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_O.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_O.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_O.jpg");

            var c2 = smallDensity.ShowCrossHigh();

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\CrossSections_X_o.tif", c2[0]);

            //mip = new Visualizations.MIP();
            //mip.setNumberProjections(40);
            //mip.setFileName(Program.dataFolder + "\\mip_O.avi");
            //mip.SetInput(psO.GetOutput());

            //mip.RunNode();
            #endregion

            return psO.GetOutput();
        }

        public static PassData Deconv2(PassData pd)
        {

            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            #region deconv2



            var psO = new Tomography.obdSIRTRecon();
            psO.setRealign(false);
            psO.SetInput(pd);
            psO.RunNode();

            var smallDensity = psO.GetOutput().DensityGrid;
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_O.raw", smallDensity, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_O_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }


            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_O.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_O.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_O.jpg");

            var mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip_O.avi");
            mip.SetInput(psO.GetOutput());

            mip.RunNode();
            #endregion
            return psO.GetOutput();
        }

        public static PassData SIRT(PassData pd)
        {

            #region SIRT
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            Tomography.SIRTRecon4 ps3 = new Tomography.SIRTRecon4();
            ps3.SetInput(pd);
            ps3.RunNode();

            var smallDensity = ps3.GetOutput().DensityGrid;

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

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_S.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_S.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_S.jpg");

            //var    mip = new Visualizations.MIP();
            //   mip.setNumberProjections(40);
            //   mip.setFileName(Program.dataFolder + "\\mip_S.avi");
            //   mip.SetInput(ps3.GetOutput());

            //   mip.RunNode();

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

            return ps3.GetOutput();
            #endregion
        }



        public static void GetExampleVG(PassData pass, string outFilename)
        {
            //Bitmap[] CrossSections = pass.DensityGrid.ShowCross();

            //Image<Gray, float> asu = new Image<Gray, float>(CrossSections[0]);
            //asu = asu.Mul(1d / asu.GetAverage().Intensity);



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
            basePath = Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName) + "\\";


            if (Directory.Exists(basePath + @"500PP\recon_cropped_8bit"))
            {
                string[] files = Directory.GetFiles(basePath + @"500PP\recon_cropped_8bit");
                files = Utilities.StringExtensions.SortNumberedFiles(files);


                int mid = (int)(files.Length / 2d);


                //double[] comps = new double[2 * 18 + 1];
                //int cc = 0;
                //double  maxComp =0;
                int maxIndex = mid;
                //for (int i = mid - 18; i < mid + 18; i++)
                //{
                //    string filename =files[ i];
                //    Image<Gray, float> VG = new Image<Gray, float>(filename);

                //    for (int x=0;x<VG.Width ;x++)
                //        for (int y = 0; y < VG.Height; y++)
                //        {
                //            float v = VG.Data[y, x, 0]-15;
                //            if (v < 0) v = 0;
                //            VG.Data[y, x, 0] = v;
                //        }

                //    double inten = 1d / VG.GetAverage().Intensity;
                //    VG = VG.Mul(inten );
                //  //  System.Diagnostics.Debug.Print(inten.ToString());

                //    double[] min, max;
                //    Point[] pm, pM;

                //    var image3 = asu.CopyBlank();
                //    MathLibrary.FFT.MathFFTHelps.FFT_cnv2(asu.Data, VG.Data, image3.Data);

                //    //avoid weird spikes
                //    image3 = image3.SmoothGaussian(5);
                //    image3.MinMax(out min, out max, out pm, out pM);

                //    comps[cc] = max[0];
                //    System.Diagnostics.Debug.Print(max[0].ToString());
                //    cc++;

                //    if (max[0] > maxComp)
                //    {
                //        maxComp = max[0];
                //        maxIndex = i;
                //    }
                //}

                Image<Gray, byte> best = new Image<Gray, byte>(files[maxIndex]);
                best.Save(outFilename);
            }
        }
    }
}
