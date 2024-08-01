
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

namespace ReconstructCells
{
    public class RepeatScript
    {
        public static void RunReconScript(bool fluorImage)
        {
            Program.WriteTagsToLog("Starting", true);
            Program.WriteTagsToLog("NumberOfCells", 1);
            Program.WriteTagsToLog("Run Time", DateTime.Now);
            bool ColorImage = (Program.VGInfoReader.GetNode("Color").ToLower() == "true");


            Program.WriteTagsToLog("IsColor", ColorImage);
            Program.WriteTagsToLog("IsFluor", fluorImage);

            Program.WriteTagsToLog("Center Quality", 0.722690296022354);
            Program.WriteTagsToLog("Center Quality SD", 0.413005572786976);
            Program.WriteTagsToLog("Center Quality Variance", 1.58568483613873);

            Tools.LoadLibrary ll = new Tools.LoadLibrary();
            ll.SetExperimentFolder(Program.ExperimentFolder);
          //  ll.SetFluorImage(fluorImage);
            ll.SetPPReader(Program.VGInfoReader);
            ll.RunNode();



            //show the raw projections
            Program.ShowBitmaps(ll.GetOutput().Library);
            Image<Gray, float> BackgroundCheckImage = ll.GetOutput().Library[10].Clone();

            Image<Gray, float> theBackground;

           
            Registration.RoughRegister roughReg = new Registration.RoughRegister();

            if (!fluorImage)
            {
                //Start the processing again
                Background.RemoveCapillaryCurvature rcc = new Background.RemoveCapillaryCurvature();
                rcc.SetInput(ll.GetOutput());
                rcc.RunNode();
                var lib = rcc.GetOutput().Library;

                Background.RoughBackgrounds roughBack = new Background.RoughBackgrounds();
                roughBack.SetInput(rcc.GetOutput());
                roughBack.RunNode();

                theBackground = roughBack.getBackground();

             //   roughReg.setBackground(roughBack.getBackground());
                roughReg.setInfoReader(Program.VGInfoReader);
                roughReg.SetInput(roughBack.GetOutput());
                roughReg.RunNode();
            }
            else
            {
                theBackground =ll.GetOutput(). Library[0].CopyBlank();
                theBackground = theBackground.Add(new Gray(1d));

              //  roughReg.setBackground(theBackground);
                roughReg.setInfoReader(Program.VGInfoReader);
                roughReg.SetInput(ll.GetOutput());
                roughReg.RunNode();

            }

#if TESTING
            roughReg.SaveBitmaps(@"C:\temp\Visualized\images.bmp");
#endif 

            //if there are no cells, then make a background and then exit
            /*if (!Registration.RoughRegister.CheckIfCell(BackgroundCheckImage, roughBack.getBackground()) && roughBack.GetOutput().FluorImage == false)
            {
                Background.BackgroundFromEmptyPP.CreateBackground(Program.experimentFolder, @"V:\ASU_Recon\Backgrounds");
                return;
            }*/

            ImageProcessing.ImageFileLoader.Save_TIFF(Program.DataFolder + "\\background.tif", theBackground);


            roughReg.SaveExamples(Program.DataFolder);
            Program.WriteTagsToLog("CellSize", roughReg.GetOutput().Information["CellSize"]);

#if TESTING
            roughReg.GetOutput().Library.SaveImages(@"C:\temp\registered\image.tif");
            CellLocation.Save(@"c:\temp\testLocations.txt", roughReg.GetOutput().Locations);
#endif


            Registration.ZRegistration fr = new Registration.ZRegistration();
            fr.SetInput(roughReg.GetOutput());
            fr.RunNode();


#if TESTING
            fr.GetOutput().Library.SaveImages(@"C:\temp\Visualized\images.bmp");
#endif




            Background.DivideFlattenAndInvertBackgrounds divide = new Background.DivideFlattenAndInvertBackgrounds();
            divide.setBackground(theBackground);
            divide.SetInput(fr.GetOutput());
            divide.setSuggestedCellSize((int)(roughReg.getCellSize()*1.5));
            divide.RunNode();

#if TESTING
            roughReg.GetOutput().Library.SaveImages(@"C:\temp\registered\images.tif");
            roughReg.GetOutput().Library.SaveImages(@"C:\temp\Visualized\images.bmp");

            CellLocation.Save(@"c:\temp\locations.txt", roughReg.GetOutput().Locations);
#endif

            Program.WriteTagsToLog("backgroundRemovalMethod", "Average Background");

            Program.ShowBitmaps(divide.GetOutput().Library);

            if (!divide.GetOutput().FluorImage)
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
                smooth.MedianFilter(5);
            }
            else
            {
                Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(divide.GetOutput().Library);
                smooth.MedianFilter(3);
            }


            Registration.AlignByRecon fineAlign = new Registration.AlignByRecon();
            fineAlign.SetInput(divide.GetOutput());
            fineAlign.RunNode();

            fineAlign.GetOutput().Library.CreateAVIVideoEMGU(Program.DataFolder + "\\centering.avi", 10);

            Program.ShowBitmaps(fineAlign.GetOutput().Library);

            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.FilterType, Program.FilterLength);
            ps.SetInput(fineAlign.GetOutput());
            ps.RunNode();


            //ps.GetOutput().DensityGrid.ScrubReconVolume(ps.GetOutput().Library.Count);

            ps.GetOutput().DensityGrid.ScrubReconVolumeCloseCut(ps.GetOutput().Library.Count);

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(Program.DataFolder + "\\ProjectionObject.tif", ps.GetOutput().DensityGrid);

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

    }
}
