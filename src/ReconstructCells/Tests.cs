using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace ReconstructCells
{
    class Tests
    {
        /*    private static void TestBackgroundSeperation()
        {

            var roughBackground = ImageProcessing.ImageFileLoader.LoadImage(@"c:\temp\pixelmap.tif");

            var lowFreq = roughBackground.SmoothGaussian(51);

            var testImage = lowFreq.CopyBlank();
            var num = roughBackground.Data;// library[0].Data;
            unchecked
            {
                for (int x = 0; x < testImage.Width; x++)
                    for (int y = 0; y < testImage.Height; y++)
                    {
                        testImage.Data[y, x, 0] = num[y, x, 0] / lowFreq.Data[y, x, 0];
                    }
            }

            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\pixelOnly.tif", testImage);

            var image = ImageProcessing.ImageFileLoader.LoadImage(@"C:\Development\CellCT\DataIN\cct001_20120210_160444\PP\000.png");
            var testImage2 = image.CopyBlank();
            num = image.Data;// library[0].Data;
            unchecked
            {
                for (int x = 0; x < testImage2.Width; x++)
                    for (int y = 0; y < testImage2.Height; y++)
                    {
                        testImage2.Data[y, x, 0] = num[y, x, 0] / testImage.Data[y, x, 0];
                    }
            }
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\testBR2.tif", testImage2);

        }
        private static void TestBackgroundRecognition( PPFileReader ppReader)
        {

            OnDemandImageLibrary library = new OnDemandImageLibrary(Directory.GetFiles(@"c:\temp\cct001_20120829_214526\pp"), true, @"c:\temp", false);
            library.LoadLibrary();

            //library.SaveImages(@"c:\temp\flatPPs");
           // var roughBackground =   ImageProcessing.ImageFileLoader.LoadImage(@"c:\temp\roughBackground.tif");
            //  ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\roughBackground.tif", roughBackground);



            //now do a registration of the cell motion, using center of gravity
            //Registration.RoughRegister register = new Registration.RoughRegister(roughBackground, library, ppReader);

           // register.CheckIfCell();

            var averageImage = Background.BackgroundFromEmptyPP.GetBackground(library);

            //averageImage = ImageProcessing._2D.ImageManipulation.UnSharp(averageImage, 41, .9f);

            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\pixelMap.tif", averageImage);

             var image =  ImageProcessing.ImageFileLoader.LoadImage(@"C:\Development\CellCT\DataIN\cct001_20120210_160444\PP\000.png");
            var testImage2 = library[0].CopyBlank();
            var  num = image.Data;// library[0].Data;
            unchecked
            {
                for (int x = 0; x < testImage2.Width; x++)
                    for (int y = 0; y < testImage2.Height; y++)
                    {
                        testImage2.Data[y, x, 0] = num[y, x, 0] / averageImage.Data[y, x, 0];
                    }
            }
            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\testBR.tif", testImage2);
        }*/
        /*
        private static void TestEvaluation()
        {
            string experimentFolder = @"C:\Development\CellCT\DataIN\cct001_20120210_160444";
            string dataFolder = @"C:\Development\CellCT\DataIN\cct001_20120210_160444\\data";

            float[, ,] Volume = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(dataFolder + "\\ProjectionObject.tif");

            Bitmap[] CrossSections = Volume.ShowCross();

            dataFolder = @"c:\temp";
            CrossSections[0].Save(dataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(dataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(dataFolder + "\\CrossSections_Z.jpg");

            Visualizations.MIP mip = new Visualizations.MIP(Volume);
            mip.DoForwardProjections(40, true, dataFolder + "\\mip.avi");

            System.IO.StreamWriter logFile = new System.IO.StreamWriter(dataFolder + "\\comments.txt");
            Evaulation.VolumeEvaluation volEval = new Evaulation.VolumeEvaluation(logFile, experimentFolder, false, new Rectangle(591, 500, 234, 234));

            var stackImage = volEval.EvaluatedRecon(Volume);
            ImageProcessing.ImageFileLoader.Save_Bitmap(dataFolder + "\\stack.bmp", stackImage);
            ImageProcessing.ImageFileLoader.Save_TIFF(dataFolder + "\\stack.tif", stackImage);

        }

        private static void TestZRegistration()
        {
            OnDemandImageLibrary library = new OnDemandImageLibrary(Directory.GetFiles(@"c:\temp\registered"), true, @"c:\temp", false);
            //library.SaveImages(@"c:\temp\registered\image");
            var Locations = CellLocation.Open(@"c:\temp\locations.txt");
            int EstimatedCellSize = 188;
            Registration.ZRegistration fr = new Registration.ZRegistration(library);
            fr.ZRegisterCell(Locations, EstimatedCellSize);

        }

        private static void TestRegistration(PPFileReader ppReader)
        {


            //  ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\smoothBackground.tif",smoothBackground );
            //    library.SaveImages(@"c:\temp\onlySmoothed\image");
            OnDemandImageLibrary library = new OnDemandImageLibrary(Directory.GetFiles(@"c:\temp\onlySmoothed"), true, @"c:\temp", false);
            var smoothBackground = ImageProcessing.ImageFileLoader.LoadImage(@"c:\temp\smoothBackground.tif");

            //now do a registration of the cell motion, using center of gravity
            Registration.RoughRegister register = new Registration.RoughRegister(smoothBackground, library, ppReader);

            int suggestedCellSize = (int)(register.SuggestedCellSize * 1.5);

            var Locations = register.RegisterCells(10, suggestedCellSize);

            int EstimatedCellSize = (int)(register.EstimateCellSize() * 1.2);  //give the cell some room as the cytoplasm tends to get lost in against the nucleous


            //save 4 projection examples that have been nicely cut down
            var ExamplePP = new Bitmap[4];
            for (int i = 0; i < 4; i++)
            {
                library[i * 50].ROI = new Rectangle(Locations[i * 50].CellCenter.X - EstimatedCellSize, 0, EstimatedCellSize * 2, library[0].Height);
                ExamplePP[i] = library[i * 50].Copy().Bitmap;
                library[i * 50].ROI = Rectangle.Empty;
            }
            var StartROI = new Rectangle(Locations[0].CellCenter.X - EstimatedCellSize / 2, Locations[0].CellCenter.Y - EstimatedCellSize / 2, EstimatedCellSize, EstimatedCellSize);

            //now that the exact location of the cell is known, refine the background
            Background.AveragingBackground reconBack = new Background.AveragingBackground(library, false);
            //var FineBackground = reconBack.GetFineBackground(12, Locations);

            //last take this background off of all the PPs and clip out the desired area
            reconBack.RemoveBackgrounds(smoothBackground, Locations, EstimatedCellSize);

            //clean up the PPs, invert the image and flatten all the edges
            BatchUtilities utils = new BatchUtilities(library);
            utils.InvertAndFlatten();

        }
        private static void TestRegistration2()
        {
            OnDemandImageLibrary library = new OnDemandImageLibrary(Directory.GetFiles(@"c:\temp\pps"), true, @"c:\temp", false);
            float[, ,] vol = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(@"c:\temp\projectionobject.tif");

            Tomography.PseudoSiddonForward psf = new Tomography.PseudoSiddonForward(vol);
            int nForwardProjections = 40;
            Image<Gray, float>[] ForwardProjections = psf.DoForwardProjectionsHalf(nForwardProjections, library[0].Width);

            // library.SaveImages(@"c:\temp\pps\PPs");
            Registration.FineRegistration fr = new Registration.FineRegistration(library);

            fr.RegisterCell(ForwardProjections);

            library.CreateAVIVideoEMGU(@"c:\temp\centering.avi", 10);
        }
        private static void TestForwardProjection()
        {
            float[, ,] vol = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(@"c:\temp\projectionobject.tif");
            Tomography.PseudoSiddonForward psf = new Tomography.PseudoSiddonForward(vol);

            int nForwardProjections = 40;

            Bitmap[] testImages = psf.testForwardProjections(nForwardProjections, vol.GetLength(0) * 2);
            testImages[0].Save(@"c:\temp\test0.bmp");
            testImages[2].Save(@"c:\temp\test1.bmp");

            Image<Gray, float>[] ForwardProjections = psf.DoForwardProjectionsHalf(nForwardProjections, vol.GetLength(0) * 2);

            ImageProcessing._2D.MovieMaker.CreateAVIVideoEMGU(@"c:\temp\test.avi", ForwardProjections, 1);
        }

        private static void TestRecon()
        {
            OnDemandImageLibrary library = new OnDemandImageLibrary(
                Directory.GetFiles(@"C:\temp\CompletelyAligned"),
                true, @"c:\temp", false);

            //   for (int i = 0; i < library.Count; i++)
            //     library[i] = library[i].PyrDown();
            //      library[i] = library[i].Resize(120, 120, Emgu.CV.CvEnum.INTER.CV_INTER_NN);

            library[1].Save(@"c:\temp\image.bmp");
            // Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon(library);

            Tomography.RoughRecon rough = new Tomography.RoughRecon(library);
            var densityGrid = rough.DoProjections(50);

            Tomography.SIRT sirt = new Tomography.SIRT(library);
            float[, ,] DensityGrid = sirt.DoProjections(densityGrid);

            Tomography.StatisticalRecon stat = new Tomography.StatisticalRecon(library);
            DensityGrid = stat.DoProjections(DensityGrid);

            Bitmap[] b = DensityGrid.ShowCross();

            b[0].Save(@"c:\temp\view1.bmp");
            b[1].Save(@"c:\temp\view2.bmp");
            b[2].Save(@"c:\temp\view3.bmp");
            // b.SaveArray(@"c:\temp", "testrecond");

        }


        private static void TestMIP()
        {
            string dataFolder = @"C:\temp\testBad\data";
            string volumestring = dataFolder + "\\projectionobject.tif";
            var volume = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(volumestring);

            //volume.RemoveSphere();

            Bitmap[] CrossSections = volume.ShowCross();

            CrossSections[0].Save(dataFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(dataFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(dataFolder + "\\CrossSections_Z.jpg");

            Visualizations.MIP mip = new Visualizations.MIP(volume);
            mip.DoForwardProjections(40, true, @"C:\temp\testBad\data\mip.avi");

        }
       */
    }
}
