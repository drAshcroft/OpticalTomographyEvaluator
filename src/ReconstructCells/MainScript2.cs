
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
using System.Data.SQLite;
using RGiesecke.DllExport;

namespace ReconstructCells
{
    public class MainScript
    {


        /// <summary>
        /// Yes I know that the general rule is just to keep a proceedure less than a screen.  This is just a changeable script and it would be silly and more work to break it up.  
        /// the sections should be easily read and have been broken up visually.  Just make sure to keep the passdata updated as it is the token that controls all the flow. 
        /// </summary>
        /// <param name="registrationFolder"></param>
        /// <param name="LoadWait"></param>
 //       [DllExport("RunRecon")]
        public static void RunReconScript(string registrationFolder, bool LoadWait)
        {
            PassData pass = null;
            ///the database will hold all kinds of information about the results.  anything that is useful for record keeping should be put in this 
            try
            {
                Program.WriteTagsToLog("Starting", true);
                Program.WriteTagsToLog("Run Time", DateTime.Now);


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////Load the data from the network drive.   If loadwait has been enabled, the program will attempt to load the images after they appear
                ////////////////////////////////load the images and determine if it is a fluor sample

                #region loading
                Tools.LoadLibrary ll = new Tools.LoadLibrary();
                ll.setLoadWait(LoadWait);
                ll.SetExperimentFolder(Program.ExperimentFolder);
                ll.SetPPReader(Program.VGInfoReader);
                ll.RunNode();
                pass = ll.GetOutput();
                #endregion

                #region misc
                Program.WriteTagsToLog("Version", "2.0");
                Program.WriteTagsToLog("IsColor", pass.ColorImage);
                Program.WriteTagsToLog("IsFluor", pass.FluorImage);

                //show the raw projections
                Program.ShowBitmaps(pass.Library);

                OnDemandImageLibrary unsmoothed = null;

                #region Do Compression
                //the data needs to have a highly compressed solution.  This saves the most important part of the image, then compresses the rest of the image and 
                //puts them all together with the important information
                //unfortunately this is really slow
                if (Program.Compression)
                {
                   ReconstructCells.Registration.CompressRegister.SaveWholes(pass);
                }
                #endregion

                System.Diagnostics.Stopwatch sw = new Stopwatch();
                System.Diagnostics.Stopwatch wholeRecon = new Stopwatch();
                wholeRecon.Start();

                #endregion

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////                                             Background removal
                if (!pass.FluorImage)
                {
                    #region Background Removal

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///////////                                          First deal with the curvature
                    sw.Start();
                    //Start the processing again
                    Background.RemoveCapillaryCurvatureBig rcc = new Background.RemoveCapillaryCurvatureBig();
                    rcc.SetInput(pass);
                    rcc.RunNode();

                    pass = rcc.GetOutput();
                    pass.AddDatabaseQueue("secondsInCurvature", sw.Elapsed.TotalSeconds.ToString());
                    Console.WriteLine("Curvature " + sw.Elapsed.TotalSeconds.ToString());
                    sw.Reset();
                    sw.Start();

                    try
                    {
                        ImageProcessing.ImageFileLoader.Save_TIFF(Program.DehydrateFolder + "\\backgroundCurved.tif", rcc.getCurveImage());
                    }
                    catch { }


                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///////////                                          then remove the splotchy/speckle stuff


                    Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                    roughBack.SetInput(pass);

                    if (File.Exists(Program.DataFolder + "\\background.tif"))
                    {
                        pass.theBackground = ImageProcessing.ImageFileLoader.Load_Tiff(Program.DataFolder + "\\background.tif");
                        roughBack.SetBackground(pass.theBackground);
                    }
                    roughBack.SetDoIterations(true);

                    if (!ll.GetOutput().FluorImage)
                        roughBack.SetNumberProjections(25);
                    else
                        roughBack.SetNumberProjections(50);

                    roughBack.RunNode();

                    pass.AddDatabaseQueue("secondsInBackground", sw.Elapsed.TotalSeconds.ToString());
                    Console.WriteLine("Background " + sw.Elapsed.TotalSeconds.ToString());
                    sw.Reset();
                    pass = roughBack.GetOutput();

                    #endregion
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///Smoothing of fluor images
                #region Fluor changes
                if (pass.FluorImage)
                {
                    unsmoothed = new OnDemandImageLibrary(pass.Library);

                    Imaging.SmoothingFilters smooth2 = new Imaging.SmoothingFilters(pass.Library);
                    smooth2.MedianFilter(5);
                    //smooth2.MedianFilter(5);
                    //Imaging.SmoothingFilters smooth2 = new Imaging.SmoothingFilters(pass.Library);
                    //smooth2.filterKalman(pass.Library, .15f, .7f);
                    //pass.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);
                }
                #endregion
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //the data has an info file from the instrument that tells where the location of the first cell is.  reading this file makes everything else much easier

                Program.RetryVG();

                Program.ShowBitmaps(pass.Library);

                Registration.RoughRegister roughReg = new Registration.RoughRegister();
               
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                Console.WriteLine("Starting Registration");
                #region Registration 1
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
               Registration.AlignByRecon ar;

                if (File.Exists(Program.DehydrateFolder + "\\locations.csv"))
                {
                    pass.Locations = CellLocation.Open(Program.DehydrateFolder + "\\locations.csv");
                    pass.AddInformation("CellSize", pass.Locations[1].CellSize);
                }
                else
                {
                    sw.Reset();
                    sw.Start();


                    roughReg.SetInput(pass);
                    roughReg.setInfoReader(Program.VGInfoReader);
                    roughReg.setPP_Reader(Program.VGPPReader);
                    roughReg.RunNode();

                    Console.WriteLine("Register " + sw.Elapsed.TotalSeconds.ToString());
                    pass.AddDatabaseQueue("secondsInRegistration", sw.Elapsed.TotalSeconds.ToString());

                    roughReg.SaveExamples(Program.DataFolder);
                    pass = roughReg.GetOutput();

                    //The location of the cell should not be jerky, so smooth it off to prevent weird errors.
                    pass.Locations = CellLocation.SmoothPoly(pass.Locations);
                
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (!pass.FluorImage)
                    {
                        Console.WriteLine("Starting Noisy");
                        sw.Reset();
                        sw.Start();
                        ReconstructCells.Registration.nCOGRegister nCogRefine = new Registration.nCOGRegister();
                        nCogRefine.SetInput(pass);
                        nCogRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                        nCogRefine.RunNode();
                        pass = nCogRefine.GetOutput();

                        Console.WriteLine("nCOGRegister " + sw.Elapsed.TotalSeconds.ToString());
                        pass.AddDatabaseQueue("secondsInNoisy", sw.Elapsed.TotalSeconds.ToString());
                    }

                    Console.WriteLine("Starting align 1");
                    sw.Reset();
                    sw.Start();
                    //  pass.SavePassData(@"c:\temp\nnn1");
                    ar = new Registration.AlignByRecon();
                    ar.SetInput(pass);
                    ar.setNumberOfProjections(pass.Library.Count / 2);
                    ar.setAlreadyCut(false);
                    ar.setScale(1);
                    ar.RunNode();
                    pass = ar.GetOutput();
                    Console.WriteLine("secondsInAlign1 " + sw.Elapsed.TotalSeconds.ToString());
                    pass.AddDatabaseQueue("secondsInAlign1", sw.Elapsed.TotalSeconds.ToString());

                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", pass.theBackground);
                Program.WriteTagsToLog("CellSize", pass.GetInformation("CellSize"));


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                #endregion

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (pass.FluorImage)
                {
                    pass.Library = unsmoothed;
                }

                ///used to do the compression later
                OnDemandImageLibrary libUnMirrored = new OnDemandImageLibrary(pass.Library);

                #region Mirror Refine

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////// This is the first unrecoverable step.  the pps are averaged with their opposite and then cuts them down to easy size
                Console.WriteLine("Starting mirror");
                sw.Reset();
                sw.Start();
                ReconstructCells.Registration.mirrorAlignRegister mirrorRefine = new Registration.mirrorAlignRegister();
                mirrorRefine.SetInput(pass);
                mirrorRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
                mirrorRefine.setMergeMirror(true);
                mirrorRefine.RunNode();

                pass = mirrorRefine.GetOutput();
                Console.WriteLine("secondsInMirror " + sw.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("secondsInMirror", sw.Elapsed.TotalSeconds.ToString());

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                #endregion

              
                OnDemandImageLibrary smallUnsmoothed;

                if (Directory.Exists(Program.DataFolder + "\\projectionobject___TIK_16") == false)
                {
                    Console.WriteLine("Starting align 2");
                    sw.Reset();
                    sw.Start();
                    ar = new Registration.AlignByRecon();
                    ar.SetInput(pass);
                    ar.setNumberOfProjections(pass.Library.Count);
                    ar.setAlreadyCut(true);
                    ar.setScale(1);
                    ar.RunNode();
                    pass = ar.GetOutput();
                    Console.WriteLine("Finish align 2");

                    Console.WriteLine("secondsInAlign2 " + sw.Elapsed.TotalSeconds.ToString());
                    pass.AddDatabaseQueue("secondsInAlign2", sw.Elapsed.TotalSeconds.ToString());

                    #region misc
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    Program.ShowBitmaps(pass.Library);
                    #endregion
                    #region prefiltering
                    smallUnsmoothed = new OnDemandImageLibrary(pass.Library);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///is this needed for the tikh????
                    Console.WriteLine("Starting smooth");
                    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(pass.Library);
                    smooth.MedianFilter(3);
                    #endregion

                    #region Reconstruction

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    Console.WriteLine("Starting FBP 1");
                    sw.Reset();
                    sw.Start();
                    pass = FBP(pass, 128, "_128_m", 1, false, false, false);

                    Console.WriteLine("secondsInFBP1 " + sw.Elapsed.TotalSeconds.ToString());
                    pass.AddDatabaseQueue("secondsInFBP1", sw.Elapsed.TotalSeconds.ToString());

                    ar = new Registration.AlignByRecon();
                    ar.SetInput(pass);
                    ar.setNumberOfProjections(pass.Library.Count);
                    ar.setAlreadyCut(true);
                    ar.setAlreadyReconed(true);
                    ar.setScale(1);
                    ar.RunNode();
                    pass = ar.GetOutput();
                }
                else
                {
                    pass.DensityGrid = Tools.MatlabHelps.OpenVirtualStack(Program.DataFolder + "\\projectionobject___TIK_16", "*.tif");

                    ar = new Registration.AlignByRecon();
                    ar.SetInput(pass);
                    ar.setNumberOfProjections(pass.Library.Count);
                    ar.setAlreadyCut(true);
                    ar.setAlreadyReconed(true);
                    ar.setScale(1);
                    ar.RunNode();
                    pass = ar.GetOutput();

                    smallUnsmoothed = new OnDemandImageLibrary(pass.Library);
                    Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(pass.Library);
                    smooth.MedianFilter(3);
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                sw.Reset();
                sw.Start();
            
                Console.WriteLine("secondsInAlign3 " + sw.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("secondsInAlign3", sw.Elapsed.TotalSeconds.ToString());


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine("Starting FBP 2");
                sw.Reset();
                sw.Start();

                  pass = FBP(pass, 512, "_512_m", 1, false, true, true);

                Console.WriteLine("secondsInFBP2 " + sw.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("secondsInFBP2", sw.Elapsed.TotalSeconds.ToString());

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine("Creating Movie");
                sw.Reset();
                sw.Start();

                pass.Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 1);

                Console.WriteLine("secondsInMovie " + sw.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("secondsInMovie", sw.Elapsed.TotalSeconds.ToString());
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                Console.WriteLine("Starting Tik");
                sw.Reset();
                sw.Start();


                if (Directory.Exists(Program.DataFolder + "\\projectionobject_" + "_" + "_TIK_16") == false)
                    pass = OLD(pass, "_");

                Console.WriteLine("secondsInTikh " + sw.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("secondsInTikh", sw.Elapsed.TotalSeconds.ToString());


/*        if (Directory.Exists(Program.DataFolder + "\\projectionobject_" + "2" + "_TIK_16") == false)
                {
                    smallUnsmoothed.Minimize();
                    pass.Library = smallUnsmoothed;
                    Console.WriteLine("Starting FBP2");

                    pass = FBP(pass, 512, "_512_m", 1, false, true, true);

                  //  pass.SavePassData(@"c:\temp\secondTIKH", true);

                    float min = pass.DensityGrid.MinArray() / 3;

                    pass.DensityGrid.AddToArray(Math.Abs(min));
                    Console.WriteLine("Starting TIKH 2");
                    pass = TIK(pass,7, "2");
               }
*/
                    #endregion

                pass.AddDatabaseQueue("secondsInWholeRecon", wholeRecon.Elapsed.TotalSeconds.ToString());
                pass.AddDatabaseQueue("maxIntensity", pass.DensityGrid.MaxArray().ToString());
                pass.AddDatabaseQueue("aveIntensity", pass.DensityGrid.AverageArray().ToString());

                #region Do Compression
                Program.DoCompression(pass, libUnMirrored, smallUnsmoothed);

                smallUnsmoothed[0].ScaledBitmap.Save(Program.DataFolder + "\\FinalAngle0.png");
                smallUnsmoothed[(int)Math.Round(smallUnsmoothed.Count / 4.0)].ScaledBitmap.Save(Program.DataFolder + "\\FinalAngle90.png");

                #endregion
            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("Error", ex.Message);
                Program.WriteLine("Trace\n" + ex.StackTrace);

                if (ex.Message.Length > 254)
                    pass.AddDatabaseQueue("error", ex.Message.Substring(0, 254));
                else
                    pass.AddDatabaseQueue("error", ex.Message);

                if (ex.StackTrace.Length > 254)
                    pass.AddDatabaseQueue("stacktrace", ex.StackTrace.Substring(0, 254));
                else
                    pass.AddDatabaseQueue("stacktrace", ex.StackTrace);
            }
            finally
            {
  /*              #region clean and record


                pass.SaveToDatabase(@"z:\ASU_Recon\Eval_Experimental3.sqlite");

                Thread.Sleep(1000);

                pass.Library = null;
                pass.DensityGrid = null;
                pass.WriteInformationToLog();
                // pass.SavePassData(Program.dataFolder + "\\passData");

                //Tools.NetworkCommunication.StartNetworkWriter("Recon", 1234);
                //Tools.NetworkCommunication.SendMessage(Program.ExperimentFolder);
                Process ScriptRunner = new Process();
                ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
                ScriptRunner.StartInfo.FileName = @"matlab.exe";
                ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"fileDirectory='" + Program.ExperimentFolder + "';run('c:\\matlab\\DoProcess2.m');exit;\"";
                ScriptRunner.Start();

                try
                {
                    // GetExampleVG(pass, Program.DataFolder + "\\vgExample.png");
                    //string vg = 
                    // File.Copy(vg, Program.DataFolder + "\\vgExample.png");
                }
               catch { }

                Environment.Exit(0);
                throw new Exception("did not kill gracefully\n");
                #endregion
*/            }
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
        public static PassData FBP(PassData pd, int FilterWidth, string Tag, int smoothIt, bool doHalfProjections, bool makeMovie, bool save = false)
        {
            #region FBP
            //Tomography.PseudoSiddon ps = null;
            //ps = new Tomography.PseudoSiddon();
            //ps.setFilter(Program.FilterType, FilterWidth);
            //ps.SetInput(pd);
            //ps.setHalfProjections(doHalfProjections);
            //ps.RunNode();
            //if (rnd.NextDouble() > 1)
            //{
                Tomography.OptRecon ps = new Tomography.OptRecon();
                ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                ps.setFilter(Program.FilterType, FilterWidth);
                ps.SetInput(pd);
                ps.RunNode();

                pd = ps.GetOutput();
            //}
            //else
            //{
            //    Tomography.GPURecon ps = new Tomography.GPURecon();
            //    ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
            //    ps.setFilter(Program.FilterType, FilterWidth);
            //    ps.SetInput(pd);
            //    ps.RunNode();

            //    pd = ps.GetOutput();
            //}
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

            if (save)
            {
                try
                {
                    ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_fbp_16" + Tag + "\\image.tif", smallDensity, 0, 16);
                }
                catch
                {

                }
            }
            pd.DensityGrid = smallDensity;

            Bitmap[] CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X" + Tag + ".jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y" + Tag + ".jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z" + Tag + ".jpg");


            if (makeMovie)
            {
                Visualizations.MIP mip = new Visualizations.MIP();
                mip.setNumberProjections(40);
                mip.setFileName(Program.DataFolder + "\\mip.avi");
                mip.SetInput(pd);

                mip.RunNode();
            }
            #endregion


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
        public static PassData TIK(PassData ps, int iterations, string tag)
        {
            #region TIK
            //outputData = ps.GetOutput();
            //outputData.Library = new OnDemandImageLibrary(unsmoothed);
            //outputData.SavePassData(@"c:\temp\fbp");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Tomography.OptRecon ps2 = new Tomography.OptRecon();
            ps2.setReconType(Tomography.OptRecon.ReconTypes.TIKH);
            ps2.setNumIterations(iterations);
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
           

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_" + tag + "_TIK.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_" + tag + "_TIK.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_" + tag + "_TIK.jpg");

        
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

        public static PassData SIRT(PassData ps, string tag)
        {
            #region TIK
           
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Tomography.OptRecon ps2 = new Tomography.OptRecon();
            ps2.setReconType(Tomography.OptRecon.ReconTypes.SIRT);
            ps2.setNumIterations(5);
            ps2.setCleanProjections(true);
            ps2.SetInput(ps);
            ps2.RunNode();
            ps = ps2.GetOutput();

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


            return ps2.GetOutput();
            #endregion
        }
        public static PassData OLD(PassData ps, string tag)
        {
            #region TIK

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Tomography.OptRecon ps2 = new Tomography.OptRecon();
            ps2.setReconType(Tomography.OptRecon.ReconTypes.Old);
            ps2.setNumIterations(5);
            ps2.setCleanProjections(true);
            ps2.SetInput(ps);
            ps2.RunNode();
            ps = ps2.GetOutput();

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


            return ps2.GetOutput();
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
