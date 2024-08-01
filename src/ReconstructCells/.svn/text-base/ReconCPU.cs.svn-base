using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using ImageProcessing._3D;

namespace ReconstructCells
{
    public class ReconCPU
    {
        OnDemandImageLibrary library;
        private string ExperimentFolder;
        private string DataOutFolder;
        private xmlFileReader ppReader;
        public CellLocation[] Locations;
        public Rectangle StartROI;
        //public Image<Gray, float> GoodBackground;
        public Image<Gray, float> smoothBackground;
        private Image<Gray, float> pixelMap;
        private bool FluorImage = false;


        private StreamWriter logFile = null;

        public StreamWriter LogFile
        {
            set { logFile = value; }
        }

        public OnDemandImageLibrary ImageLibrary
        {
            get { return library; }
        }

        private void LoadFolder()
        {
            string[] Files = Directory.GetFiles(ExperimentFolder + "\\pp");
            Files = Files.SortNumberedFiles();

            bool ColorImage = (ppReader.GetNode("Color").ToLower() == "true");

            library = new OnDemandImageLibrary(Files, true, @"C:\temp", ColorImage);

            if (FluorImage)
            {
                BatchUtilities utils = new BatchUtilities(library);
                utils.FixFluorImages(false );
            }

            if (logFile != null)
                logFile.WriteLine("<Files Loaded><True/>");
        }

        public ReconCPU(string ExperimentFolder, string DataOutFolder, xmlFileReader ppReader, bool fluorImage)
        {
            this.ppReader = ppReader;
            this.ExperimentFolder = ExperimentFolder;
            this.DataOutFolder = DataOutFolder;
            this.FluorImage = fluorImage;
            LoadFolder();


            if (fluorImage)
            {
                pixelMap = library[0].CopyBlank();
                pixelMap = pixelMap.Add(new Gray(1));
            }
            else
            {
                try
                {
                    pixelMap = Background.BackgroundFromEmptyPP.GetClosestBackground(ExperimentFolder);
                }
                catch
                {
                    pixelMap = library[0].CopyBlank();
                    pixelMap = pixelMap.Add(new Gray(1));
                }

                if (pixelMap == null)
                {
                    pixelMap = library[0].CopyBlank();
                    pixelMap = pixelMap.Add(new Gray(1));
                }
               //     throw new Exception("No good background found");
            }
        }

        public Bitmap[] ExamplePP;

        public bool DatasetIsBackground = false;

        public void RegisterAndRemoveBackground()
        {
         /*   //first estimate a background to get rid of the varations that mask 
            //the location of the cell
            Background.RoughBackgrounds rb = new Background.RoughBackgrounds(library, pixelMap);

            Image<Gray, float> BackgroundCheckImage = library[10].Clone();

            if (!FluorImage)
            {
                rb.RemoveCurvatures();

               // library.SaveImages(@"c:\temp\curved\image");
             //   library  = new OnDemandImageLibrary(Directory.GetFiles(@"c:\temp\curved\"), true, @"c:\temp", false);
             //   library.LoadLibrary();
            //    var t = library[1].ScaledBitmap;
                smoothBackground = rb.GetRoughBackground(25);
            }
            else
                smoothBackground = pixelMap;

            if (false  )
            {
                ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\smoothBackground.tif", smoothBackground);
                library.SaveImages(@"c:\temp\onlySmoothed\image");
            }

            //now do a registration of the cell motion, using center of gravity
            Registration.RoughRegister register = new Registration.RoughRegister(smoothBackground, library, ppReader);

            if (!register.CheckIfCell(BackgroundCheckImage) && FluorImage == false)
            {
                  DatasetIsBackground = true;
                  return;
            }

            //return;
           // int suggestedCellSize = (int)(register.SuggestedCellSize * 1.5);
            if (FluorImage)
                suggestedCellSize *= 2;

            Locations = register.RegisterCells(10, suggestedCellSize);
            int EstimatedCellSize = (int)(register.EstimateCellSize() * 1.2);  //give the cell some room as the cytoplasm tends to get lost in against the nucleous

            if (logFile != null)
                logFile.WriteLine("<CellSize><" + EstimatedCellSize + @"/>");

            //save 4 projection examples that have been nicely cut down
            ExamplePP = new Bitmap[4];
            for (int i = 0; i < 4; i++)
            {
                library[i * 50].ROI = new Rectangle(Locations[i * 50].CellCenter.X - EstimatedCellSize, 0, EstimatedCellSize * 2, library[0].Height);
                ExamplePP[i] = library[i * 50].Copy().Bitmap;
                library[i * 50].ROI = Rectangle.Empty;
            }
            StartROI = new Rectangle(Locations[0].CellCenter.X - EstimatedCellSize / 2, Locations[0].CellCenter.Y - EstimatedCellSize / 2, EstimatedCellSize, EstimatedCellSize);

           
          //  library.SaveImages(@"c:\temp\registered\image");
          //  CellLocation.Save(@"c:\temp\locations.txt",Locations );

            Registration.ZRegistration fr = new Registration.ZRegistration(library);
            fr.ZRegisterCell(Locations, EstimatedCellSize);

            //now that the exact location of the cell is known, refine the background
            Background.AveragingBackground reconBack = new Background.AveragingBackground(library, FluorImage);
            //var FineBackground = reconBack.GetFineBackground(12, Locations);

            //last take this background off of all the PPs and clip out the desired area
            reconBack.RemoveBackgrounds(smoothBackground, Locations, EstimatedCellSize);

            //clean up the PPs, invert the image and flatten all the edges
            BatchUtilities utils = new BatchUtilities(library);
            utils.InvertAndFlatten();

            if (logFile != null)
                logFile.WriteLine("<backgroundRemovalMethod><Average Background/>");*/
        }



        public void AlignByRecon()
        {
           
            /*
            //generate an example reconstrunction at half size
            Tomography.RoughRecon roughRecon = new Tomography.RoughRecon(library);
            float[, ,] exampleGrid = roughRecon.DoProjections(25, .5);

            //now create the forward projections
            Tomography.PseudoSiddonForward psf = new Tomography.PseudoSiddonForward(exampleGrid);
            int nForwardProjections = 60;
            Image<Gray, float>[] ForwardProjections = psf.DoForwardProjections(nForwardProjections, library[0].Width);

            Registration.FineRegistration fr = new Registration.FineRegistration(library);

            //now align the projections to the recon
            fr.RegisterCell(ForwardProjections);*/


        }

        public void MedianFilter(int radius)
        {
            Imaging.SmoothingFilters smooth = new Imaging.SmoothingFilters(library);
            smooth.MedianFilter(radius);
        }

       /* public float[, ,] DoRecon(string filterType, int filterLength)
        {
            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon(library);
            float[, ,] DensityGrid = ps.DoProjections(filterType, filterLength);
            return DensityGrid;
        }*/
    }
}
