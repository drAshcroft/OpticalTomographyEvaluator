﻿//#if DEBUG
//#define TESTING
//#endif

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
using ReconstructCells.Tools;
using MathLibrary;
using ImageProcessing;
using ImageProcessing._3D;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using MathLibrary;
using ImageProcessing;
using System.Diagnostics;

namespace ReconstructCells
{
    public static class Program
    {
        #region Figure Making

        private static void FigureResults()
        {

            var outputData = PassData.LoadPassData(@"C:\temp\testHoldW");


            for (int i = 0; i < 12; i++)
            {

                try
                {
                    if (i == 4)
                    {
                        outputData = PassData.LoadPassData(@"C:\temp\testHoldW");
                        Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(outputData.Library);
                        smooth.MedianFilter(5);
                    }

                    if (i == 8)
                    {
                        outputData = PassData.LoadPassData(@"C:\temp\testHoldW");

                        Denoising.MRFVolume mrf = new Denoising.MRFVolume();
                        outputData.Library = mrf.CleanSinogram(outputData.Library, 600, 6000, .1, 5);
                        outputData.SavePassData(@"c:\temp\mrfhold");
                    }
                    else
                        if (i > 8)
                        {
                            outputData = PassData.LoadPassData(@"c:\temp\mrfhold");
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

                    Tomography.PseudoSiddon ps = null;
                    ps = new Tomography.PseudoSiddon();
                    ps.setFilter(Program.FilterType, Program.FilterLength);
                    ps.SetInput(outputData);
                    ps.RunNode();

                    outputData = ps.GetOutput();
                    outputData.DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);


                    //   ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_fbp_16\\image.tif", ps.GetOutput().DensityGrid, 0, 16);

                    ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObject_W_" + i + ".raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);


                    Tomography.SIRTRecon ps3 = new Tomography.SIRTRecon();
                    ps3.SetInput(outputData);
                    ps3.RunNode();
                    ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_Sirt_W_" + i + ".raw", ps3.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);


                    try
                    {
                        Tomography.StatisticalRecon ps4 = new Tomography.StatisticalRecon();
                        ps4.SetInput(outputData);
                        ps4.RunNode();
                        ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered_Stat_W_" + i + ".raw", ps4.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);
                    }
                    catch { }
                }
                catch { }

            }



        }

        #endregion
        #region TestCode
        private static void RunTest()
        {
            //TestTiff();
            // TestTIK();
            //var lib = MainScript.GetLibraryForMatlab(@"C:\temp\cleanedbetter");
            //MainScript.MatlabTest(lib, @"c:\temp\testBad");
            //TestMakeMovies();
            // TestSinogramAlign2();
            //  TestZRegistration();
            //  TestSiddon();
            // TestNDSafir();
            // TestFilteredSiddon();
            // FigureResults();
            // TestBackground();
            //  TestCenterandDivide();
            //  TestFilteredSiddon();
            //  Test_All_Regs();
            // TestsrSIRT();
            //  ConvertTo8();
            // CheckPostProcess();
            // var s=   ReconstructCells.Tools.MatlabHelps.GetDataDirs();
            // ReconstructCells.Tools.MatlabHelps.GetVGFolder(s[0]);
            // TestNewRecon();
            //TestGetVG();

            MatlabHelps.OpenVirtualStack(@"z:\ASU_Recon\cct001\201209\22\cct001_20120922_163839\data\projectionobject___TIK_16", "*.tif");

            TestSegmentation();
        }



        private static void TestSegmentation()
        {
            float[, ,] densityData = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(Directory.GetFiles(@"C:\temp\testbad\data\projectionobject___TIK_16"));

            ILNumerics.ILArray<float> a = densityData;
            ILNumerics.ILMatFile m = new ILNumerics.ILMatFile(a);
            m.Write(@"c:\temp\test3.mat");

            //ImageProcessing.Segmentation.LevelSets ls = new ImageProcessing.Segmentation.LevelSets();
            //ls.SegmentVolumeConfidenceConnectedImageFilter(densityData);

            var image = ImageProcessing.ImageFileLoader.Load_Tiff(@"z:\ASU_Recon\viveks_RT31\cct001_20110527_174730\data\projectionobject_fbp_16_512_m\image0143.tif");

            var seg = ImageProcessing.Segmentation.Otsu.MultiOtsu(densityData, 5);

            Bitmap[] b = seg.ShowCross();

            System.Diagnostics.Debug.Print(b[0].Width.ToString());
            //System.Diagnostics.Debug.Print(seg.Width.ToString());

        }
        private static PassData passTest;
        private static void BatchILTest(int ImageNumber)
        {



            var b = MathLibrary.Signal_Processing.ilFilters.SmoothGuassian(passTest.Library[ImageNumber].Data, 61);
            Image<Gray, float> bb = new Image<Gray, float>(b);


            int w = bb.Width;
        }

        private static void TestIL()
        {
            passTest = PassData.LoadPassData(@"C:\temp\nnn2");

            //   ParallelOptions po = new ParallelOptions();
            //  po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            //   Parallel.For(0, passTest.Library.Count, po, x => BatchILTest(x));

            // il.SetSurface(MathLibrary.Signal_Processing.ilFilters.SmoothGuassianT(pass.Library[1].Data, 61));
            // il.SetSurface(MathLibrary.Signal_Processing.ilFilters.CreateGuassianFilterI(122, 61));

            // Application.Run(il);

            var imageThresh = passTest.Library[1].Convert<Bgr, byte>();

            var ave2 = imageThresh.GetAverage();
            var m2 = imageThresh.ThresholdOTSU(ave2, new Bgr(Color.White));

            var m3 = m2.SmoothGaussian(121);

            var m4 = m2.Convert<Gray, float>();

            var b = MathLibrary.Signal_Processing.ilFilters.SmoothGuassian(m4.Data, 61);
            Image<Gray, float> bb = new Image<Gray, float>(b);

            int w = bb.Width + m3.Width;

        }

        private static void TestNewRecon()
        {
            PassData pass = PassData.LoadPassData(@"C:\temp\beforeFPB");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();


            //var psT = new Tomography.OptRecon();
            //psT.setReconType(Tomography.OptRecon.ReconTypes.FBP);
            //psT.setNumIterations(1);
            //psT.setCleanProjections(false);
            //psT.SetInput(pass);
            //   psT.RunNode();

            sw.Stop();
            string cpu = sw.ElapsedTicks.ToString();


            System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
            sw2.Reset();
            sw2.Start();

            var ps2 = new Tomography.GPURecon();
            ps2.setReconType(Tomography.OptRecon.ReconTypes.FBP);
            ps2.setNumIterations(1);
            ps2.setCleanProjections(false);
            ps2.setGetForwardProjections(true);
            ps2.SetInput(pass);
            ps2.RunNode();

            var forwards = ps2.getForwardProjections();


            int w = forwards.Count;

            sw2.Stop();
            string gpu = sw2.ElapsedTicks.ToString();

            pass = ps2.GetOutput();
            var CrossSections2 = pass.DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections2);

            var Tag2 = "GPU";
            CrossSections2[0].Save(Program.DataFolder + "\\CrossSections_X" + Tag2 + ".jpg");
            CrossSections2[1].Save(Program.DataFolder + "\\CrossSections_Y" + Tag2 + ".jpg");
            CrossSections2[2].Save(Program.DataFolder + "\\CrossSections_Z" + Tag2 + ".jpg");
        }

        private static void TestGetVG()
        {
            PassData pass = PassData.LoadPassData(@"c:\temp\alldone");
            try
            {
                MainScript.GetExampleVG(pass, Program.DataFolder + "\\vgExample.png");
                //string vg = 
                // File.Copy(vg, Program.DataFolder + "\\vgExample.png");
            }
            catch { }

            try
            {
                ReconstructCells.Tools.StackHandler.CutStack(Program.DataFolder + "\\stack", pass.Locations, pass.theBackground/*.CopyBlank().Add(new Gray(1))*/, Program.ExperimentFolder);

            }
            catch { }


        }

        private static void TestAlign()
        {
            PassData pass = PassData.LoadPassData(@"c:\temp\nnn1");

            //ReconstructCells.Registration.nCOGRegister nCogRefine = new Registration.nCOGRegister();
            //nCogRefine.SetInput(pass);
            //nCogRefine.setSuggestedCellSize((int)pass.Information["CellSize"]);
            //nCogRefine.RunNode();

            //pass = nCogRefine.GetOutput();



            Registration.AlignByRecon ar = new Registration.AlignByRecon();
            ar.SetInput(pass);
            ar.setNumberOfProjections(pass.Library.Count / 2);
            ar.setAlreadyCut(false);
            ar.setScale(1);
            ar.RunNode();
            pass = ar.GetOutput();



            //pass.SavePassData(@"c:\temp\raw2");

            ReconstructCells.Registration.mirrorAlignRegister mirrorRefine = new Registration.mirrorAlignRegister();
            mirrorRefine.SetInput(pass);
            mirrorRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
            mirrorRefine.setMergeMirror(true);
            mirrorRefine.RunNode();

            //Background.InvertAndCut cutter = new Background.InvertAndCut();
            //cutter.setSuggestedCellSize((int)pass.Information["CellSize"]);
            //cutter.SetInput(pass);
            //cutter.RunNode();
            //cutter.GetOutput().SavePassData(@"c:\temp\noise2");
            //return;

            // pass.SavePassData(@"c:\temp\raw3");
            // return;
            ar = new Registration.AlignByRecon();
            ar.SetInput(pass);
            ar.setNumberOfProjections(pass.Library.Count);
            ar.setAlreadyCut(true);
            ar.setScale(1);
            ar.RunNode();
            pass = ar.GetOutput();
            //Background.InvertAndCut Cutter = new Background.InvertAndCut();
            //Cutter.SetInput(pass);
            //Cutter.setSuggestedCellSize((int)pass.Information["CellSize"]);
            //Cutter.RunNode();


            //   pass.SavePassData(@"c:\temp\nnn4");
            Program.ShowBitmaps(pass.Library);



            pass.Library.SaveImages(Program.DehydrateFolder + "\\image.tif");
            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DehydrateFolder + "\\theBackground.tif", pass.theBackground);

            Program.ShowBitmaps(pass.Library);

            if (pass.FluorImage)
            {
                pass.Library.RotateLibrary(-90);
            }



            Program.ShowBitmaps(pass.Library);

            Dictionary<string, string> TaggedEvals = new Dictionary<string, string>();

            Console.WriteLine("Starting smooth");
            Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(pass.Library);
            smooth.MedianFilter(3);

            //pass = FBP(pass, 128, "d_m_128_", 0, false, false);
            // pass = FBP(pass, 512, "d_m", 3, false, false);
            Console.WriteLine("Starting FBP 1");
            pass = MainScript.FBP(pass, 512, "_512_m", 2, false, true);


            ar = new Registration.AlignByRecon();
            ar.SetInput(pass);
            ar.setNumberOfProjections(pass.Library.Count);
            ar.setAlreadyCut(true);
            ar.setScale(1);
            ar.RunNode();
            pass = ar.GetOutput();

            Console.WriteLine("Starting FBP 2");
            pass = MainScript.FBP(pass, 128, "_128_m", 1, false, true);
            float[, ,] smoothRecon = pass.DensityGrid;

            Console.WriteLine("Starting FBP 3");
            pass = MainScript.FBP(pass, 512, "_512", 0, false, true);

            Console.WriteLine("Starting Combined");
            pass.DensityGrid = MainScript.SaveCombined(pass, smoothRecon);
            pass.SavePassData(@"c:\temp\mirrored");
        }

        private static void AlignWithVG()
        {

            string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT20", "projectionobject_*", SearchOption.AllDirectories);

            foreach (string s in dirs)
            {
                if (s.Contains("aligned") == false)
                {
                    try
                    {

                        string parent = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(Path.GetDirectoryName(s)));
                        var bRecon = new OnDemandImageLibrary(s, true, @"c:\temp", false);
                        OnDemandImageLibrary vRecon;

                        try
                        {
                            vRecon = new OnDemandImageLibrary(@"S:\Research\Brian\Test cells for Recon\VGRecon\" + parent + @"\500PP\recon_cropped_16bit", true, @"c:\temp", false);
                        }
                        catch
                        {
                            vRecon = new OnDemandImageLibrary(@"S:\Research\Brian\Test cells for Recon\VGRecon\" + parent + @"\500PP\recon_cropped_8bit", true, @"c:\temp", false);


                        }
                        var targetI = vRecon[vRecon.Count / 2];

                        float[, ,] target = vRecon[vRecon.Count / 2].Data;

                        int midPoint = bRecon.Count / 2;
                        Image<Gray, float> bImage = bRecon[midPoint];

                        int bcIndex = midPoint;

                        double xx = 0, yy = 0, zz = 0, d;
                        double sWeight = 0;
                        var average = bRecon[midPoint].Convert<Bgr, byte>().GetAverage();
                        var image = bRecon[midPoint].Convert<Bgr, byte>().ThresholdOTSU(average, new Bgr(Color.White));
                        float threshold = 0;
                        for (int i = 20; i < image.Width - 20; i++)
                        {
                            if (image.Data[image.Height / 2, i, 0] == 0 && image.Data[image.Height / 2, i + 1, 0] != 0)
                            {
                                threshold = bRecon[midPoint].Data[image.Height / 2, i + 1, 0];
                                break;
                            }
                        }
                        for (int i = 0; i < bRecon.Count; i++)
                        {
                            var image2 = bRecon[i];
                            // image = bRecon[i].Convert<Bgr, byte>().ThresholdOTSU(average, new Bgr(Color.White));
                            for (int y = 0; y < image.Height; y++)
                                for (int x = 0; x < image.Width; x++)
                                {
                                    d = image2.Data[y, x, 0];
                                    if (d > threshold)
                                    {
                                        xx += x * d;
                                        yy += y * d;
                                        zz += i * d;
                                        sWeight += d;
                                    }
                                }
                        }

                        int xxB = (int)(xx / sWeight);
                        int yyB = (int)(yy / sWeight);
                        int zzB = (int)(zz / sWeight);

                        double xx2 = 0, yy2 = 0, zz2 = 0;
                        sWeight = 0;

                        average = vRecon[vRecon.Count / 2].Convert<Bgr, byte>().GetAverage();
                        image = vRecon[vRecon.Count / 2].Convert<Bgr, byte>().ThresholdOTSU(average, new Bgr(Color.White));

                        for (int i = 20; i < image.Width - 20; i++)
                        {
                            if (image.Data[image.Height / 2, i, 0] == 0 && image.Data[image.Height / 2, i + 1, 0] != 0)
                            {
                                threshold = vRecon[vRecon.Count / 2].Data[image.Height / 2, i + 1, 0];
                                break;
                            }
                        }
                        for (int i = 0; i < vRecon.Count; i++)
                        {
                            var image2 = vRecon[i];
                            // var image = vRecon[i].ThresholdOTSU(average, new Gray(Int16.MaxValue / 2));
                            //image = vRecon[i].Convert<Bgr, byte>().ThresholdOTSU(average, new Bgr(Color.White));
                            for (int y = 0; y < image.Height; y++)
                                for (int x = 0; x < image.Width; x++)
                                {
                                    d = image2.Data[y, x, 0];
                                    if (d > threshold)
                                    {
                                        xx2 += x * d;
                                        yy2 += y * d;
                                        zz2 += i * d;
                                        sWeight += d;
                                    }
                                }
                        }

                        int xxV = (int)(xx2 / sWeight);
                        int yyV = (int)(yy2 / sWeight);
                        int zzV = (int)(zz2 / sWeight);

                        Rectangle ROI = new Rectangle(xxB - xxV, yyB - yyV, vRecon[0].Width, vRecon[0].Height);
                        //v=B+b b=v-B
                        int diff = zzB - zzV;
                        OnDemandImageLibrary libOut = new OnDemandImageLibrary(vRecon);
                        Image<Gray, float> iB, iV;
                        for (int i = 0; i < vRecon.Count; i++)
                        {
                            int V = i + diff;
                            if (V > 0 && V < bRecon.Count)
                            {
                                iB = bRecon[V];
                                iV = vRecon[i].CopyBlank();

                                for (int x = ROI.Left; x < ROI.Right; x++)
                                    for (int y = ROI.Top; y < ROI.Bottom; y++)
                                    {
                                        if (x > 0 && x < iB.Width && y > 0 && y < iB.Height)
                                            iV.Data[y - ROI.Top, x - ROI.Left, 0] = iB.Data[y, x, 0];
                                    }
                                libOut[i] = iV;
                            }
                        }

                        libOut.Save16bitImages(s + @"_aligned\image.tif");

                        System.Diagnostics.Debug.Print((xx + yy + zz).ToString());
                    }
                    catch { }
                }
            }

        }

        private static void Test_All_Regs()
        {
            PassData pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();

            OnDemandImageLibrary unprocessed = new OnDemandImageLibrary(pd.Library);

            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            var theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\background.tif");
            theBackground = theBackground.Copy().Add(new Gray(1));

            Registration.RoughRegister roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.RunNode();


            //  unprocessed = new OnDemandImageLibrary(@"C:\temp\postcurve\cct001_20110527_174730\library", true, @"c:\temp", false);
            //  unprocessed.LoadLibrary();
            pd.Library = new OnDemandImageLibrary(unprocessed);
            CheckRegCOG(pd, "COG", theBackground);

            pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();

            unprocessed = new OnDemandImageLibrary(pd.Library);

            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\background.tif");
            theBackground = theBackground.Copy().Add(new Gray(1));

            roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.RunNode();


            pd.Library = new OnDemandImageLibrary(unprocessed);
            CheckRegmCOG(pd, "mCOG", theBackground);

            pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();

            unprocessed = new OnDemandImageLibrary(pd.Library);

            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\background.tif");
            theBackground = theBackground.Copy().Add(new Gray(1));

            roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.RunNode();

            pd.Library = new OnDemandImageLibrary(unprocessed);
            CheckRegtCC(pd, "tCC", theBackground);


            pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();

            unprocessed = new OnDemandImageLibrary(pd.Library);

            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\background.tif");
            theBackground = theBackground.Copy().Add(new Gray(1));

            roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.RunNode();
            pd.Library = new OnDemandImageLibrary(unprocessed);

            CheckRegCC(pd, "CC", theBackground);


            pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();

            unprocessed = new OnDemandImageLibrary(pd.Library);

            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\background.tif");
            theBackground = theBackground.Copy().Add(new Gray(1));

            roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);
            roughReg.RunNode();
            pd.Library = new OnDemandImageLibrary(unprocessed);

            CheckRegnCOG(pd, "nCOG", theBackground);
            pd.Library = new OnDemandImageLibrary(unprocessed);
        }


        private static void CheckPostProcess()
        {

            // string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT17", "data.*", SearchOption.AllDirectories);

            //   foreach (string s in dirs)
            {
                string s = @"c:\temp\testbad\data";
                Program.DataFolder = s;

                string Tag = "_POST";
                var smallDensity = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(Program.DataFolder + "\\ProjectionObject.raw");
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(null);
                float[, ,] X = smooth.filterKalmanX(smallDensity, .05f, .7f, false);
                Bitmap[] CrossSections = X.ShowCross();
                Program.ShowBitmaps(CrossSections);
                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X" + Tag + ".jpg");


                float[, ,] Y = smooth.filterKalmanY(smallDensity, .05f, .7f, false);
                CrossSections = Y.ShowCross();
                Program.ShowBitmaps(CrossSections);
                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_Y" + Tag + ".jpg");

                float[, ,] Z = smooth.filterKalmanZ(smallDensity, .05f, .7f, false);
                CrossSections = Z.ShowCross();
                Program.ShowBitmaps(CrossSections);
                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_Z" + Tag + ".jpg");

                float[, ,] tD = smooth.filterKalman3D(smallDensity, .05f, .7f);
                CrossSections = tD.ShowCross();
                Program.ShowBitmaps(CrossSections);
                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_3N" + Tag + ".jpg");


                smooth.filterKalman(ref smallDensity, .05f, .7f);
                CrossSections = smallDensity.ShowCross();
                Program.ShowBitmaps(CrossSections);
                CrossSections[0].Save(Program.DataFolder + "\\CrossSections_N" + Tag + ".jpg");

            }

            try
            {

                //Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(null);
                // smallDensity = smooth.SmoothRecon(smallDensity, 5);

                // ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_fbp_16_Post_BIL" + Tag + "\\image.tif", smallDensity, 0, 16);

            }
            catch
            {

            }

            try
            {
                //    Denoising.MRFVolume mrf = new Denoising.MRFVolume();
                //   smallDensity = mrf.CleanSinogram(smallDensity, 200, 2000, 1d / 6, 1);

                //  ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.dataFolder + "\\projectionobject_fbp_16_Post_MRF" + Tag + "\\image.tif", smallDensity, 0, 16);

            }
            catch
            {


            }


        }

        private static void CheckRegCC(PassData pd, string expName, Image<Gray, float> theBackground)
        {
            Background.DivideFlatten_CC divide = new Background.DivideFlatten_CC();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            //divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.COG);
            divide.setBackground(theBackground);
            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();

            var lib = divide.GetOutput().Library;
            Rectangle ROI = new Rectangle(40, 40, lib[0].Width - 80, lib[0].Height - 80);
            for (int i = 0; i < lib.Count; i++)
            {
                lib[i].ROI = ROI;
                lib[i] = lib[i].Copy();
                lib[i].ROI = Rectangle.Empty;
            }


            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(lib, expName);
        }

        private static void CheckRegnCOG(PassData pd, string expName, Image<Gray, float> theBackground)
        {
            Background.InvertAndCut divide = new Background.InvertAndCut();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            // divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.COG);

            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();

            var lib = divide.GetOutput().Library;
            Rectangle ROI = new Rectangle(40, 40, lib[0].Width - 80, lib[0].Height - 80);
            for (int i = 0; i < lib.Count; i++)
            {
                lib[i].ROI = ROI;
                lib[i] = lib[i].Copy();
                lib[i].ROI = Rectangle.Empty;
            }


            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(lib, expName);
        }

        public static void CheckRegCOG(PassData pd, string expName, Image<Gray, float> theBackground)
        {
            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.COG);
            divide.setBackground(theBackground);
            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();

            var lib = divide.GetOutput().Library;
            Rectangle ROI = new Rectangle(40, 40, lib[0].Width - 80, lib[0].Height - 80);
            for (int i = 0; i < lib.Count; i++)
            {
                lib[i].ROI = ROI;
                lib[i] = lib[i].Copy();
                lib[i].ROI = Rectangle.Empty;
            }


            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(lib, expName);


        }

        private static void CheckRegtCC(PassData pd, string expName, Image<Gray, float> theBackground)
        {
            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //   Background.DivideFlatten_rCOG divide = new Background.DivideFlatten_rCOG();
            divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.tCC);
            divide.setBackground(theBackground);
            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();
            var lib = divide.GetOutput().Library;
            Rectangle ROI = new Rectangle(40, 40, lib[0].Width - 80, lib[0].Height - 80);
            for (int i = 0; i < lib.Count; i++)
            {
                lib[i].ROI = ROI;
                lib[i] = lib[i].Copy();
                lib[i].ROI = Rectangle.Empty;
            }


            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(lib, expName);
        }

        private static void CheckRegmCOG(PassData pd, string expName, Image<Gray, float> theBackground)
        {
            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            divide.setMethod(Background.DivideFlattenAndInvertBackgrounds.AlignMethods.mCOG);
            divide.setBackground(theBackground);
            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();

            var lib = divide.GetOutput().Library;
            Rectangle ROI = new Rectangle(30, 30, lib[0].Width - 60, lib[0].Height - 60);
            for (int i = 0; i < lib.Count; i++)
            {
                lib[i].ROI = ROI;
                lib[i] = lib[i].Copy();
                lib[i].ROI = Rectangle.Empty;
            }


            Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            re.MirrorEvaluation(lib, expName);
        }

        private static void TestSVD()
        {
            Tomography.SVDRecon svd = new Tomography.SVDRecon();

            svd.DoProjections(null);
        }

        private static void ConvertTo8()
        {

            string[] dirs = Directory.GetDirectories(@"z:\ASU_Recon\viveks_RT8", "projectionobject_*_16.*", SearchOption.AllDirectories);

            foreach (string s in dirs)
            {
                float[, ,] density = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(Directory.GetFiles(s));

                string nFolderName = s.Replace("_16", "_8_4");

                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(nFolderName + "\\image.tif", density, 0, 8);


                nFolderName = s.Replace("_8_4", "_16_4");

                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(nFolderName + "\\image.tif", density, 0, 8);

            }
        }

        private static void TestsrSIRT()
        {
            //OnDemandImageLibrary lib = new OnDemandImageLibrary(@"c:\temp\nnn2\library", true, @"c:\temp",false);

            var im1 = ImageProcessing.ImageFileLoader.Load_Tiff(@"c:\temp\cut\image000.tif");
            var im2 = ImageProcessing.ImageFileLoader.Load_Tiff(@"c:\temp\cut\image250.tif");

            //  ImageProcessing._2D.TV td = new ImageProcessing._2D.TV();
            //  td.TVDenoise(im1, 2);

            ImageProcessing._2D.Deconvolution dc = new ImageProcessing._2D.Deconvolution();
            dc.obDeconvolve(new Image<Gray, float>[] { im1, im2 }, 200, 1);

            PassData pass = PassData.LoadPassData(@"c:\temp\nnn2");
            pass.DataScaling = 1;

            ReconstructCells.Registration.mirrorAlignRegister mirrorRefine = new Registration.mirrorAlignRegister();
            mirrorRefine.SetInput(pass);
            mirrorRefine.setMergeMethod(Registration.mirrorAlignRegister.MergeMethodEnum.SuperRes);
            mirrorRefine.setSuggestedCellSize((int)pass.GetInformation("CellSize"));
            mirrorRefine.setMergeMirror(true);
            mirrorRefine.RunNode();



        }
        private static void TestTIK()
        {
            PassData outputData = PassData.LoadPassData(@"C:\temp\pseudo\cct001_20110120_152935");
            Tomography.TikanhovRecon ps2 = new Tomography.TikanhovRecon();
            ps2.SetInput(outputData);
            ps2.RunNode();

            var smallDensity = ps2.GetOutput().DensityGrid;

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_L_16\\image.tif", smallDensity, 0, 16);

            }
            catch { }
            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_tik_L_8\\image.tif", smallDensity, 0, 8);

            }
            catch
            {

                System.Diagnostics.Debug.Print("");
            }

            var CrossSections = smallDensity.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_Tik_L.jpg");

        }
        private static void TestTiff()
        {
            float[, ,] density = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(@"C:\temp\testbad\data\ProjectionObject.raw");

            ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(@"c:\temp\testbad\stack\image.tif", density, 0, 16);

        }

        private static void TestRegistration()
        {
            //roughReg.GetOutput().SavePassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            //ImageProcessing.ImageFileLoader.Save_TIFF(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif", theBackground);
            //CellLocation.Save(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv", roughReg.GetOutput().Locations);


            PassData pd = PassData.LoadPassData(@"C:\temp\flattened\" + Program.ExperimentTag);
            pd.Library = new OnDemandImageLibrary(@"C:\temp\flattened\" + Program.ExperimentTag + @"\library", true, @"c:\temp", false);
            pd.Library.LoadLibrary();
            pd.Locations = CellLocation.Open(@"c:\temp\flattened\" + Program.ExperimentTag + @"\locations.csv");
            var theBackground = ImageProcessing.ImageFileLoader.LoadImage(@"C:\temp\flattened\" + Program.ExperimentTag + @"\background.tif");


            Registration.RoughRegister roughReg = new Registration.RoughRegister();
            roughReg.SetInput(pd);

            roughReg.setInfoReader(Program.VGInfoReader);

            roughReg.RunNode();
            roughReg.SaveExamples(Program.DataFolder);


            var image = Background.DivideFlattenAndInvertBackgrounds.MakeSinoGram(pd.Library[0].Width / 2, pd.Library, theBackground, pd.Locations);
            int wwww = image.Width;

            image.ScaledBitmap.Save(@"c:\temp\sinogram.tif");
            //for (int paperI = 3; paperI < 4; paperI++)
            //{
            //    Program.TestMode = paperI.ToString();
            //    Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //    divide.setBackground(theBackground);
            //    divide.SetInput(pd);
            //    divide.setSuggestedCellSize(300);
            //    divide.RunNode();
            //}

        }

        private static void TestBackground()
        {
            PassData pd = PassData.LoadPassData(@"C:\temp\backgroundRemoval");

            Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
            roughBack.SetInput(pd);

            roughBack.SetNumberProjections(25);

            roughBack.RunNode();

            var theBackground = roughBack.getBackground();
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\testBAck.tif", theBackground);

        }

        private static void TestNDSafir()
        {

            Program.DataFolder = @"C:\temp\testBad";

            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(@"C:\Dehydrated\cct001_20120229_105314");

            Files = Files.SortNumberedFiles();

            mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", false);
            for (int i = 0; i < mPassData.Library.Count; i++)
            {
                var image = mPassData.Library[i];
                image.ROI = new Rectangle(30, 30, image.Width - 60, image.Height - 60);
                image = image.Copy();

                //double  Background =0;
                double Background = double.MaxValue;
                double max = double.MinValue;
                for (int j = 0; j < image.Height; j++)
                {
                    //Background += image.Data[j, 1, 0];
                    if (Background > image.Data[j, 1, 0])
                        Background = image.Data[j, 1, 0];

                    if (max < image.Data[j, 1, 0])
                        max = image.Data[j, 1, 0];


                }
                Background = 0;

                //float min =(float)( Background / image.Height);

                string index = string.Format("  {0}", i);
                index = index.Substring(index.Length - 3);

                ImageProcessing.ImageFileLoader.Save_16bit_TIFF(@"C:\temp\ndsafirAbsorb\image" + index + ".tif", image, 1, (float)Background);
            }
        }

        private static void TestMakeMovies()
        {
            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(@"C:\temp\registered");
            Files = Files.SortNumberedFiles();

            mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", false);
            mPassData.Locations = CellLocation.Open(@"c:\temp\testLocations.txt");
            mPassData.AddInformation("CellSize", 170);

            mPassData.Library.CreateAVIVideoEMGU(@"C:\temp\testBad\centering.avi", 10);

            mPassData.DensityGrid = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(@"C:\temp\testBad\ProjectionObject.tif");

            Visualizations.MIP mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(@"C:\temp\testBad\mip.avi");
            mip.SetInput(mPassData);

            mip.RunNode();
        }

        private static void TestZRegistration()
        {
            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(@"C:\temp\registered");
            //  string[] Files = Directory.GetFiles(@"z:\Raw PP\cct001\Absorption\201209\11\cct001_20120911_092738\PP");
            Files = Files.SortNumberedFiles();

            mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", false);
            mPassData.Locations = CellLocation.Open(@"c:\temp\testLocations.txt");
            mPassData.AddInformation("CellSize", 170);

            Registration.ZRegistration fr = new Registration.ZRegistration();

            fr.SetInput(mPassData);
            fr.RunNode();
        }

        private static void TestSiddon()
        {
            PassData mPassData = PassData.LoadPassData(@"c:\temp\registered");

            // Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(mPassData.Library);
            // smooth.MedianFilter(5);

            //Denoising.AverageVolume aveVole = new Denoising.AverageVolume();
            //mPassData.Library = aveVole.CleanSinogram(mPassData.Library, 1, false,100000);
            for (int i = 0; i < mPassData.Library.Count; i++)
                mPassData.Library[i] = mPassData.Library[i].SmoothBilatral(5, 1, 3);

            //Denoising.MedianVolume meanVole = new Denoising.MedianVolume();
            //mPassData.Library = meanVole.CleanSinogram(mPassData.Library, 1);

            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, 1024);
            ps.SetInput(mPassData);
            ps.RunNode();

            mPassData = ps.GetOutput();

            //Denoising.MedianVolume meadVole = new Denoising.MedianVolume();
            //mPassData.DensityGrid = meadVole.CleanSinogram(mPassData.DensityGrid, 1);

            //Denoising.AverageVolume aveVole = new Denoising.AverageVolume();
            //mPassData.DensityGrid = aveVole.CleanSinogram(mPassData.DensityGrid, 1,false ,0);

            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.DataFolder + "\\ProjectionObject.tif", ps.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObject.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            Bitmap[] CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z.jpg");

            Visualizations.MIP mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip.avi");
            mip.SetInput(ps.GetOutput());

            mip.RunNode();
        }

        private static void TestFilteredSiddon()
        {
            PassData mPassData = PassData.LoadPassData(@"c:\temp\FBP");

            //mPassData.Library = new OnDemandImageLibrary(Program.dataFolder + "\\prefiltered", false, @"c:\temp", false);

            ReconstructCells.Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(mPassData.Library);
            smooth.TotalVariation(10);

            var ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, Program.FilterLength);
            ps.SetInput(mPassData);
            ps.RunNode();


            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.DataFolder + "\\ProjectionObject_FBP_P.tif", ps.GetOutput().DensityGrid);
            var CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_FBP_P.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_FBP_P.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_FBP_P.jpg");

            var mip = new Visualizations.MIP();
            mip.setNumberProjections(40);
            mip.setFileName(Program.DataFolder + "\\mip_FBP_P.avi");
            mip.SetInput(ps.GetOutput());

            mip.RunNode();

            //  outputData = ps.GetOutput();


        }


        private static void TestSART()
        {


            Program.DataFolder = @"C:\temp\testbad";
            Bitmap[] CrossSections;


            PassData mPassData = PassData.LoadPassData(@"C:\temp\FBP");
            mPassData.DensityGrid = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(@"c:\temp\testbad\data\ProjectionObject_512_.raw");

            //  mPassData.Library.PyrDown();

            Tomography.MRF_IRT ps2 = new Tomography.MRF_IRT();
            ps2.SetInput(mPassData);
            ps2.RunNode();

            CrossSections = mPassData.DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X_K.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y_K.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z_K.jpg");

            try
            {
                ImageProcessing.ImageFileLoader.Save_Tiff_VirtualStack(Program.DataFolder + "\\projectionobject_kal_16\\image.tif", mPassData.DensityGrid, 0, 16);

            }
            catch { }
        }


        private static void TestSinogramAlign()
        {
            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(@"C:\temp\sinogram");
            Files = Files.SortNumberedFiles();

            mPassData.Library = new OnDemandImageLibrary(Files, true, @"C:\temp", false);
            mPassData.Locations = CellLocation.Open(@"c:\temp\testLocations.txt");
            mPassData.AddInformation ("CellSize", 170);

            Registration.SinogramAlign sa = new Registration.SinogramAlign();
            sa.SetInput(mPassData);
            sa.RunNode();
        }

        private static void TestSinogramAlign2()
        {
            PassData pd = PassData.LoadPassData(@"C:\temp\flattened");


            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            //divide.setBackground(theBackground);
            //divide.SetInput(roughReg.GetOutput());
            //divide.setSuggestedCellSize((int)roughReg.GetOutput().Information["CellSize"]);
            //divide.RunNode();

            //Evaulation.RegistrationEvaulation re = new Evaulation.RegistrationEvaulation();
            //re.MirrorEvaluation(pd.Library );
        }

        private static void TestCenterandDivide()
        {
            PassData pd = PassData.LoadPassData(@"C:\temp\flattened");

            var theBackground = ImageProcessing.ImageFileLoader.Load_Tiff(@"c:\temp\flattened\background.tif");

            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            divide.setBackground(theBackground);
            divide.SetInput(pd);
            divide.setSuggestedCellSize((int)pd.GetInformation("CellSize"));
            divide.RunNode();

            divide.GetOutput().Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);

            divide.GetOutput().SavePassData(@"c:\temp\flattened");
            // ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\registered\background.tif", theBackground);

            Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
            smooth.MedianFilter(3);

            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, Program.FilterLength);
            ps.SetInput(divide.GetOutput());
            ps.RunNode();


            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            //  ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.DataFolder + "\\ProjectionObject.tif", ps.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(Program.DataFolder + "\\ProjectionObjectFiltered.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            Bitmap[] CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(Program.DataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(Program.DataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(Program.DataFolder + "\\CrossSections_Z.jpg");

        }

        #endregion

        public static System.IO.StreamWriter logFile = null;


        static object IO_Lock = new object();
        public static void WriteLine(string text)
        {
            try
            {
                lock (IO_Lock)
                {
                    if (logFile != null)
                    {
                        logFile.WriteLine(text);
                        logFile.Flush();
                        Console.WriteLine(text);
                    }
                }
            }
            catch { }

        }

        public static void WriteTagsToLog(string Tag, object Info)
        {
            try
            {
                lock (IO_Lock)
                {
                    if (logFile != null)
                    {
                        logFile.WriteLine("<" + Tag + @"/><" + Info.ToString() + @"/>");
                        logFile.Flush();
                        Console.WriteLine("<" + Tag + @"/><" + Info.ToString() + @"/>");
                    }
                }
            }
            catch { }
        }

        public static void ShowBitmaps(OnDemandImageLibrary library)
        {
            if (DisplayForm != null)
                DisplayForm.ShowBitmaps(library);
        }

        public static void ShowBitmaps(Bitmap[] library)
        {
            if (DisplayForm != null)
            {
                DisplayForm.ShowBitmaps(library);
            }
        }

        public static void ShowBitmaps(float[, ,] library)
        {
            if (DisplayForm != null)
            {

                DisplayForm.ShowBitmaps(library);
            }
        }

        private volatile static Form1 DisplayForm = null;

        public static string ExperimentFolder = "";
        public static string BaseDataFolder = "";
        public static string DataFolder = "";
        public static string RegistrationFolder = "";
        public static string VGFolder = "";
        public static string StorageFolder = "";
        public static string DehydrateFolder = "";
        public static string BackupFolder = "";

        public static string FilterType = "Han";
        public static int FilterLength = 512;
        public static xmlFileReader VGInfoReader = null;
        public static xmlFileReader VGPPReader = null;
        public static string TimeStamp;
        public static string DateStamp;
        public static string ExperimentTag = "";
        public static string WorkTags = "Rough_Capillary_Average_Iterative_BlackOut";
        public static bool Compression = true;


        public static ParallelOptions threadingParallelOptions = new ParallelOptions();

        static bool LoadWait = false;
#if PAPER_TESTS
        public static string TestMode="";
        public static bool BackgroundTests = false  ;
#endif

        // static Thread DisplayThread = null;

        private static void StartDisplay()
        {
            try
            {
                if (DisplayForm == null)
                    DisplayForm = new Form1();
            }
            catch
            {
                DisplayForm = new Form1();

            }
            DisplayForm.Show();
        }
        private static bool DoneRunning = false;
        private static void ThreadRun(bool fluorImage)
        {
            DoneRunning = false;
            Thread t = new Thread(delegate()
            {
                try
                {
                    MainScript.RunReconScript(RegistrationFolder, LoadWait);
                }
                catch (Exception ex)
                {
                    logFile.WriteLine(@"<Error Encountered><" + Utilities.StringExtensions.FormatException(ex) + "/>");

                    logFile.WriteLine(ex.StackTrace);
                    logFile.WriteLine(@"<Finished All Threads><true/>");
                    logFile.WriteLine(@"<ErrorMessage><False/>");

                    //    displayForm.ShowText(ex.Message);

                    Application.DoEvents();
                    Thread.Sleep(1000 * 45);
                }
                DoneRunning = true;
            });
            t.Start();

            while (DoneRunning == false)
            {
                Application.DoEvents();
                Thread.Sleep(300);
            }
        }

        public static void RetryVG()
        {
            #region GetFirstCell
  /*          if (Program.VGInfoReader == null)
            {
                while (Program.VGInfoReader == null)
                {
                    try
                    {
                        Program.VGInfoReader = new xmlFileReader(Program.ExperimentFolder + "\\info.xml");
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                    if (Program.VGInfoReader == null)
                    {
                        try
                        {
                            Program.VGInfoReader = new xmlFileReader(Program.StorageFolder + "\\info.xml");
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
           }
   */
            #endregion
        }

        public static void CloseFiles()
        {

            try
            {
                logFile.Close();
            }
            catch { }
            VGInfoReader = null;
            VGPPReader = null;
        }

        public static void SetupRun(string experimentFolder, string baseDataFolder, bool loadWait)
        {
            string dirName = "";
            string[] parts = null;
            string Prefix = "";
            string Year = "";
            string month = "";
            string day = "";

            threadingParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            ExperimentFolder = experimentFolder;
            BaseDataFolder = baseDataFolder;

            Console.WriteLine(ExperimentFolder);
            Console.WriteLine(baseDataFolder);

            LoadWait = loadWait;

            DataFolder = baseDataFolder + "\\data";
            StartDisplay();
            dirName = Path.GetFileNameWithoutExtension(ExperimentFolder);
            //return dirName;
 //           parts = dirName.Split('_');
 //           Prefix = parts[0];
 //           Year = parts[1].Substring(0, 4);
 //           month = parts[1].Substring(4, 2);
 //           day = parts[1].Substring(6, 2);

  //          VGFolder = Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
  //          StorageFolder = Path.Combine(@"z:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);


 //           DehydrateFolder = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
 //           BackupFolder = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

            if (File.Exists(DehydrateFolder) ==false)
            {
                DehydrateFolder = Path.Combine(@"e:\Compressed\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                BackupFolder = Path.Combine(@"e:\Compressed\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            }
           

            try
            {
     //           Directory.CreateDirectory(DehydrateFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            
            }


            DateStamp = Year + "_" + month + "_" + day;
            TimeStamp = dirName;

            ExperimentTag = dirName;

            if (Directory.Exists(DataFolder) == false)
                Directory.CreateDirectory(DataFolder);

            DisplayForm.ShowCaption(Path.GetFileNameWithoutExtension(ExperimentFolder));

            logFile = new System.IO.StreamWriter(DataFolder + "\\comments.txt");

            logFile.AutoFlush = true;

            CvInvoke.logFile = logFile;


            try
            {
     //           VGInfoReader = new xmlFileReader(Program.ExperimentFolder + "\\info.xml");// Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName));
            }
            catch
            {
     //           VGInfoReader = null;
            }

            try
            {
                if (File.Exists((VGFolder + "\\500pp\\visualization_preview.jpg")))
                    VGPPReader = new xmlFileReader(VGFolder + "\\PPDetailReport.xml");// Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName));
            }
            catch
            {
                VGPPReader = null;
            }

        }


        public static void DoAMove(string ExperimentFolder)
        {
            string dirName = Path.GetFileNameWithoutExtension(ExperimentFolder);
            //return dirName;
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            string backupFolder = Path.Combine(@"f:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);


            try
            {
                Utilities.FileUtilities.DoCopyFolder(ExperimentFolder, backupFolder);

                Directory.Delete(ExperimentFolder, true);
            }
            catch
            {
                if (Directory.Exists(Path.GetDirectoryName(Program.BackupFolder)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(Program.BackupFolder));

                Directory.Move(Program.ExperimentFolder, Program.BackupFolder);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {@"S:\Research\Brian\Test cells for Recon\RawPP\cct001_20110120_104112",
                    @"c:\temp\testbad", "Han", "512", "Load-Wait" };
            }


            //  Tools.MatlabHelps.OpenMovie(@"S:\Research\Matt Stanley\matts_new_vids_2013\vids_from_05_19_13\0520 good hoechst 60x 20ms _ 2v_15mhz_k562_post_cover_slide_adjustment.avi");

            //  Tools.MatlabHelps.QueryFrame();



            //   Thread.Sleep(500);


            // Thread.Sleep(500);
            // try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null && args[i].ToLower() == "loadwait")
                        LoadWait = true;
                    if (args[i] != null && args[i].ToLower() == "compress")
                        Compression = true;
                }

                ExperimentFolder = args[0];
                BaseDataFolder = args[1];


                //Tools.NetworkCommunication.StartNetworkWriter("Recon", 1234);
                //Tools.NetworkCommunication.SendMessage(ExperimentFolder);


                SetupRun(ExperimentFolder, BaseDataFolder, LoadWait);
            }


            // catch { }

#if TESTING
            RunTest();
            return;
#endif

            try
            {
                Compression = true;
                //if (File.Exists(DehydrateFolder + "\\clippedPP\\image000.tif") && Compression)
                //{
                //    Console.WriteLine("Already Done");

                //    Thread.Sleep(30000);

                //    //Tools.NetworkCommunication.StartNetworkWriter("Recon", 1234);
                //    //Tools.NetworkCommunication.SendMessage(Program.ExperimentFolder);
                //    Process ScriptRunner = new Process();
                //    ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                //    ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
                //    ScriptRunner.StartInfo.FileName = @"matlab.exe";
                //    ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"fileDirectory='" + Program.ExperimentFolder + "';run('c:\\matlab\\DoProcess2.m');exit;\"";
                //    ScriptRunner.Start();
                //}
                //else
                MainScript.RunReconScript(RegistrationFolder, LoadWait);
            }
            catch (Exception ex)
            {
                DoneRunning = true;
                WriteLine(@"<Error Encountered><" + Utilities.StringExtensions.FormatException(ex) + "/>");

                WriteLine(ex.StackTrace);
                WriteLine(@"<Finished All Threads><true/>");
                WriteLine(@"<ErrorMessage><False/>");

                //    displayForm.ShowText(ex.Message);

                Application.DoEvents();
                Thread.Sleep(1000 * 5);
            }
            CloseFiles();

            //if (Compression)
            //    DoAMove(ExperimentFolder);

        }


        public static void DoCompression(PassData pass, OnDemandImageLibrary libUnMirrored, OnDemandImageLibrary smallUnsmoothed)
        {

            Registration.RoughRegister roughReg = null;
            try
            {
                CellLocation.Save(Program.DehydrateFolder + "\\locations.csv", pass.Locations);
            }
            catch { }

            //the data needs to have a highly compressed solution.  This saves the most important part of the image, then compresses the rest of the image and 
            //puts them all together with the important information
            if (Program.Compression)
            {
                try
                {
                    pass.Library = libUnMirrored;
                    Registration.CompressRegister compress = new Registration.CompressRegister();
                    compress.SetInput(pass);
                    //  compress.setLibrary(smallUnsmoothed);
                    compress.setInfoReader(Program.VGInfoReader);
                    compress.setPP_Reader(Program.VGPPReader);
                    compress.RunNode();

                    compress = new Registration.CompressRegister();
                    compress.SetInput(pass);
                    compress.setLibrary(smallUnsmoothed);
                    compress.setInfoReader(Program.VGInfoReader);
                    compress.setPP_Reader(Program.VGPPReader);
                    compress.RunNode();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }

                try
                {
                    if (roughReg == null)
                        roughReg = new Registration.RoughRegister();

                    roughReg.SetInput(pass);

                    pass.StackImage = Tools.StackHandler.CutStack(Program.DehydrateFolder + "\\stack", pass.Locations, pass.theBackground, Program.ExperimentFolder, roughReg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);

                }

                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\stackExample.tif", pass.StackImage);
                ImageProcessing.ImageFileLoader.Save_TIFF(Program.DehydrateFolder + "\\background.tif", pass.theBackground);
            }

        }
    }
}
