
#if DEBUG
//#define TESTING
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using MathLibrary;
using System.IO;

namespace ReconstructCells.Registration
{
    class CompressRegister : ReconstructNodeTemplate
    {
        OnDemandImageLibrary Library;
        Point FirstCellCenter = Point.Empty;
        int CellSize;
        int SuggestedCellSize = 300;
        CellLocation[] Locations;
        float[] minIntensity;
        int FrameSkip = 10;
        Image<Gray, float> RoughBackground;
        xmlFileReader VG_InfoReader = null;
        xmlFileReader VG_PP_Reader = null;
        eRegistrationSmoothing RegistrationSmoothing = eRegistrationSmoothing.Kalman;


        OnDemandImageLibrary cLibrary;
        int defaultThreshold = 200;

        int[] BorderTouches;

        #region Properties

        #region Set
        public void setPP_Reader(xmlFileReader ppReader)
        {
            VG_PP_Reader = ppReader;

        }
        public void setInfoReader(xmlFileReader infoReader)
        {
            this.VG_InfoReader = infoReader;
            bool ColorImage = false;

            if (infoReader != null && mPassData.FluorImage == false)
            {
                try
                {
                    Library = mPassData.Library;
                }
                catch
                {
                    throw new Exception("input must be set before this one is set");///todo: it would be better to do this code at the run node procedure
                }

                ColorImage = (infoReader.GetNode("Color").ToLower() == "true");


                int x = int.Parse(infoReader.GetNode("SPECIMEN/CellXPos"));
                int y = int.Parse(infoReader.GetNode("SPECIMEN/CellYPos"));
                int w = int.Parse(infoReader.GetNode("SPECIMEN/BoxWidth"));
                int h = int.Parse(infoReader.GetNode("SPECIMEN/BoxHeight"));

                if (ColorImage)
                {
                    x /= 2; y /= 2; w /= 2; h /= 2;
                }

                if (w == 0 || h == 0)
                {
                    w = 100;
                    h = 100;
                }
                Rectangle FirstCellGuess = new Rectangle(x, y, w, h);

                FirstCellCenter = new Point(FirstCellGuess.X, FirstCellGuess.Y);// -firstGuess.Width / 2;

                SuggestedCellSize = (int)(FirstCellGuess.Width * 2.5);
                CellSize = SuggestedCellSize;
                Locations = new CellLocation[1];
                Locations[0] = new CellLocation(new PointF(FirstCellCenter.X, FirstCellCenter.Y), CellSize, 0);

                Size tempS = estCellSize2(0);

                SuggestedCellSize = (int)(tempS.Width * 1.1);
                CellSize = SuggestedCellSize;
#if TESTING


#else
                SuggestedCellSize = (int)(FirstCellGuess.Width * 1.2);
                CellSize = SuggestedCellSize;
#endif
            }
            else
            {


            }
        }

        public void set_Locations(CellLocation[] Locations)
        {
            this.Locations = Locations;
        }

        public void setLibrary(OnDemandImageLibrary library)
        {
            cLibrary = library;

        }

        public enum eRegistrationSmoothing
        {
            None, Median, Gaussian, Kalman
        }
        public void setRegistrationSmoothing(eRegistrationSmoothing regSmoothingTechnique)
        {
            RegistrationSmoothing = regSmoothingTechnique;
        }

        #endregion

        #region Get
        public Rectangle getFirstCellLocation()
        {
            var r = new Rectangle(FirstCellCenter.X - SuggestedCellSize / 2, FirstCellCenter.Y - SuggestedCellSize / 2, SuggestedCellSize, SuggestedCellSize);
            r.Inflate(50, 50);
            return r;

        }
        public int getCellSize()
        {
            int maxSize = 0;
            for (int i = 0; i < Locations.Length; i += 100)
            {
                Size tCellSize = estCellSize(i);
                if (tCellSize.Width > maxSize) maxSize = tCellSize.Width;
                if (tCellSize.Height > maxSize) maxSize = tCellSize.Height;
            }

            if (mPassData.FluorImage)
                SuggestedCellSize = (int)(maxSize * 1.5);
            else
                SuggestedCellSize = (int)(maxSize * 1.2);

            if (SuggestedCellSize > 450)
                SuggestedCellSize = 400;

            CellSize = SuggestedCellSize;

            return SuggestedCellSize;
        }


        private Size estCellSize(int imageNumber)
        {
            int i = imageNumber;
            Point centerOnImage = new Point((int)Locations[i].CellCenter.X, (int)Locations[i].CellCenter.Y);
            Rectangle ROI = new Rectangle(centerOnImage.X - CellSize / 2, centerOnImage.Y - CellSize / 2, CellSize, CellSize);
            //cleanup the background 
            // var image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[i], RoughBackground, ROI);
            var image = ImageProcessing._2D.ImageManipulation.CopyROI(Library[i], ROI);
            Size tCellSize;
            ImageProcessing.BlobFinding.CenterOfGravity.BlobSize(image, out tCellSize);
            return tCellSize;
        }

        private Size estCellSize2(int imageNumber)
        {
            int i = imageNumber;
            Point centerOnImage = new Point((int)Locations[i].CellCenter.X, (int)Locations[i].CellCenter.Y);
            Rectangle ROI = new Rectangle(centerOnImage.X - CellSize / 2, centerOnImage.Y - CellSize / 2, CellSize, CellSize);
            //cleanup the background 
            var image = ImageProcessing._2D.ImageManipulation.CopyROI(Library[i], ROI);  // ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[i], RoughBackground, ROI);
            Size tCellSize;
            ImageProcessing.BlobFinding.CenterOfGravity.BlobSizeCloseClip(image, out tCellSize);
            return tCellSize;
        }

        public int getCellSizeO()
        {
            double sum = 0;
            double weight = 0;

            for (int i = 0; i < Locations.Length; i++)
            {
                if (Locations[i].FV != 0)
                {
                    sum += Locations[i].FV * Locations[i].CellSize;
                    weight += Locations[i].FV;
                }
            }

            if (mPassData.FluorImage == false)
                SuggestedCellSize = (int)(sum / weight * 1.5);
            else
                SuggestedCellSize = (int)(sum / weight * 1.5);

            return SuggestedCellSize;
        }


        #endregion
        #endregion

        public static PassData GetVGSuggestion(PassData pass, xmlFileReader vG_PP_Reader)
        {

            string s = "DataSet/PseudoProjection/BoundingBox";
            CellLocation[] Locations = new CellLocation[pass.Library.Count];
            for (int i = 0; i < pass.Library.Count; i++)
            {
                string[] box = vG_PP_Reader.GetNodes(s, i, new string[] { "bottom", "top", "right", "left" });//="684" top="340" right="836" left="496"/>"));
                int b = int.Parse(box[0]);
                int t = int.Parse(box[1]);
                int r = int.Parse(box[2]);
                int l = int.Parse(box[3]);

                box = vG_PP_Reader.GetNodes("DataSet/PseudoProjection/ObjectCenter", i, new string[] { "x", "z" });
                int x = int.Parse(box[0]);
                int z = int.Parse(box[1]);

                CellLocation cl = new CellLocation(new PointF(z, x), (int)(.8 * (Math.Abs(r - l) + Math.Abs(t - b)) / 2d), i);
                Locations[i] = cl;
            }

            //  SuggestedCellSize = getCellSize();
            //  CellSize = SuggestedCellSize;

            // MarkCenters(Locations);
            pass.Locations = Locations;
            // mPassData.SavePassData("c:\\temp\\marked");
            return pass;
        }

        public static void SaveWholes(PassData pass)
        {
            try
            {
    //            pass.Library.SaveImagesCompressed(Program.DehydrateFolder + "\\wholes\\image.jpg", 90);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        protected override void RunNodeImpl()
        {
            RoughBackground = mPassData.theBackground;
            bool alreadyCut = false;
            if (cLibrary != null)
            {
                Library = cLibrary;
                alreadyCut = true;
            }
            else
                Library = mPassData.Library;


            //if (VG_PP_Reader != null && mPassData.Locations==null)
            //{
            //    mPassData = GetVGSuggestion(mPassData, VG_PP_Reader);
            //}

            int firstWidth = mPassData.Locations[0].ToRectangle().Width;
            int firstHeight = mPassData.Locations[0].ToRectangle().Height;
            string saveDir = Program.DehydrateFolder + "\\clippedPP";


            if (!alreadyCut)
            {

                try
                {
                    Directory.CreateDirectory(saveDir);
                }
                catch { }
                int CellSize = Locations[0].CellSize + 30;
                for (int i = 0; i < Library.Count; i++)
                {
                    Rectangle roi = mPassData.Locations[i].ToFixedRectangle(CellSize);
                   
                    Image<Gray, float> image = Library[i];
                    var image2 = Background.RoughBackgrounds.RemoveBackground(image, RoughBackground, roi);
                    ImageProcessing.ImageFileLoader.Save_TIFF(string.Format("{0}\\image{1:000}.tif", saveDir, i), image2);
                }
            }
            else
            {
                saveDir = Program.DehydrateFolder + "\\clippedMirror";

                try
                {
                    Directory.CreateDirectory(saveDir);
                }
                catch { }
                for (int i = 0; i < Library.Count; i++)
                {
                    Image<Gray, float> image2 = Library[i];
                    ImageProcessing.ImageFileLoader.Save_TIFF(string.Format("{0}\\image{1:000}.tif", saveDir, i), image2);
                }
            }
        }
    }
}
