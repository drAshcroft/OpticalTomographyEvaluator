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
    public class ExampleRecon
    {
        OnDemandImageLibrary library;

        public float[, ,] GetImageData(int ImageNumber)
        {
            return library[ImageNumber].Data;
        }

        public void SaveImage(string Directory, int ImageNumber)
        {
            ImageProcessing.ImageFileLoader.Save_TIFF(Directory + "\\image" + ImageNumber + ".tif", library[ImageNumber]);
            //library[ImageNumber].Save();
        }

        public void SetImageData(int ImageNumber, float[, ,] imageData)
        {
            library[ImageNumber] = new Image<Gray, float>(imageData);
        }

        public void SetImageData2(int ImageNumber, float[, ] imageData)
        {
            float[, ,] ReformatedData = new float[imageData.GetLength(0), imageData.GetLength(1), 1];
            Buffer.BlockCopy(imageData, 0, ReformatedData, 0, Buffer.ByteLength(imageData));
            library[ImageNumber] = new Image<Gray, float>(ReformatedData);
        }

        public void  CleanProjections()
        {
            Image<Gray, float> roughBackground;
            if (true)
            {
                Background.RoughBackgrounds rb=null ;//= new Background.RoughBackgrounds(library);
                rb.RemoveCurvatures();

                roughBackground = rb.GetRoughBackground(25);
            }
            else
            {
                roughBackground = new Image<Gray, float>(library[0].Width, library[1].Height);// rb.GetRoughBackground(25);
                roughBackground = roughBackground.Add(new Gray(1));
            }


            Registration.RoughRegister register=null;// = new Registration.RoughRegister(library, ppReader);
            var locations = register.RegisterCells( 10, register.SuggestedCellSize);
            int EstimatedCellSize = (int)(register.EstimateCellSize() * 1.2);  //give the cell some room as the cytoplasm tends to get lost in against the nucleous

            Background.AveragingBackground reconBack = new Background.AveragingBackground(library);
            var FineBackground = reconBack.GetFineBackground(12, locations);

            reconBack.RemoveBackgrounds(FineBackground, locations, EstimatedCellSize);

            BatchUtilities utils = new BatchUtilities(library);
            utils.InvertAndFlatten();

        }

        public void  LoadImages()
        {
            string[] Files = Directory.GetFiles(@"C:\Development\CellCT\DataIN\cct001_20120210_160444\PP");
            Files = Files.SortNumberedFiles();
            library = new OnDemandImageLibrary(Files, true, @"C:\temp", false);
        }

        public float[, ,] testRecon( )
        {
            LoadImages();

            CleanProjections();

            return  RoughRecon();
        }

        public float[, ,] RoughRecon()
        {
            Tomography.RoughRecon roughRecon = new Tomography.RoughRecon(library);
            float[, ,] exampleGrid = roughRecon.DoProjections(20, .5);

            return exampleGrid;
        }

        public float[, ,] DoRecon()
        {
            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon(library);
            float[, ,] DensityGrid = ps.DoProjections("Han", 512);
            return DensityGrid;
        }

        public void RunTest()
        {
            var b= testRecon().ShowCross();

            Form1 f2 = new Form1();
            f2.Show();
            f2.ShowBitmaps(b);
        }
    }
}
