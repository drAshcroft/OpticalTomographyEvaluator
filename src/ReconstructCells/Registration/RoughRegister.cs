
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
    public class RoughRegister : ReconstructNodeTemplate
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
                Locations = new CellLocation[500];
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
              //  SuggestedCellSize = (int)(maxSize * 1.5);
                SuggestedCellSize = (int)(maxSize);
            else
                SuggestedCellSize = (int)(maxSize * 1.2);

           // if (SuggestedCellSize > 450)
           //     SuggestedCellSize = 400;

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

        protected override void RunNodeImpl()
        {
            RoughBackground = mPassData.theBackground;
            Library = mPassData.Library;


            if (VG_PP_Reader != null && mPassData.Locations == null)
            {
                mPassData = GetVGSuggestion(mPassData, VG_PP_Reader);
            }
            else
            {
                Locations = new CellLocation[Library.Count];

                if (mPassData.FluorImage == true)
                {
                    SuggestedCellSize = 200;// cellPos.CellSize;
                    CellSize = SuggestedCellSize;
                }

                mPassData.Locations = Locations;

            }

            mPassData.Locations = RegisterCells();
            SuggestedCellSize = getCellSize();

            //if (SuggestedCellSize % 2 == 1)
            //    SuggestedCellSize += 1;

            mPassData.AddInformation("CellSize", SuggestedCellSize);
            mPassData.AddInformation("RoughRegistration.FrameSkip", FrameSkip.ToString());
            mPassData.AddInformation("RoughRegistration.RegistrationSmoothing", RegistrationSmoothing.ToString());
            mPassData.AddInformation("RoughRegistration.CellSize", SuggestedCellSize.ToString());

        }

        #region Code
        private void BatchFindCells(int imageNumber)
        {
            try
            {
                if (imageNumber == -1 || Locations[imageNumber] == null)
                {
                    int index;
                    Point centerOnImage;
                    Rectangle ROI;
                    Image<Gray, float> image;
                    if (imageNumber == -1)
                    {
                        //handle the fact that this the first test
                        index = 0;
                        imageNumber = 0;

                        //now set up the search region
                        centerOnImage = new Point(FirstCellCenter.X, FirstCellCenter.Y);
                        ROI = new Rectangle(centerOnImage.X - CellSize / 2, centerOnImage.Y - CellSize / 2, CellSize, CellSize);

                        double[] max, min;
                        Point[] p1, p2;
                        RoughBackground.MinMax(out min, out max, out p1, out p2);
                        max[0] = RoughBackground.GetAverage().Intensity;
                        // System.Diagnostics.Debug.Print(min[0].ToString());

                        //  image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[imageNumber], RoughBackground, ROI);
                        image = ImageProcessing._2D.ImageManipulation.CopyROI(Library[imageNumber], ROI);
                        //now estimate that the cell will be located at the center of the image and search for the cell

                        ImageProcessing.BlobFinding.CenterOfGravity.FindOTSUThreshold(image, out defaultThreshold);//.CenterOfGravity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);

                    }
                    else
                    {
                        if (Locations[imageNumber] == null)
                        {
                            //get the location of the last found cell
                            index = (int)Math.Floor((double)imageNumber / FrameSkip) * FrameSkip;

                            //if the location has not been set yet, this is a setup step
                            if (Locations[index] == null)
                                index -= FrameSkip;
                        }
                        else
                            index = imageNumber;

                        //now set up the search region
                        centerOnImage = new Point((int)Locations[index].CellCenter.X, (int)Locations[index].CellCenter.Y);
                        ROI = new Rectangle(centerOnImage.X - CellSize / 2, centerOnImage.Y - CellSize / 2, CellSize, CellSize);
                    }


                    //cleanup the background 
                    //image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[imageNumber], RoughBackground, ROI);
                    image = ImageProcessing._2D.ImageManipulation.CopyROI(Library[imageNumber], ROI);

                    //now estimate that the cell will be located at the center of the image and search for the cell
                    Point center = new Point((int)(image.Width / 2.0), (int)(image.Height / 2.0));
                    int localCellSize = 0;
                    bool touchesBorder = false;
                    // ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);
                    // if (mPassData.FluorImage == false)
                    //     ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravityThreshold_Centered(image, center, defaultThreshold, out centerOnImage, out localCellSize, out touchesBorder);
                    // else
                    try
                    {
                        ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.Print("");

                    }
                    //double[] min,max;
                    //Point[] pm,pM;
                    //image.MinMax(out min,out  max, out pm, out pM);

                    // minIntensity[imageNumber] = (float)min[0];

                    if (touchesBorder)
                    {
                        ROI.X += center.X / 4;
                        ROI.Y += center.Y / 4;
                        // Rectangle ROI2 = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);
                        ROI.Inflate(30, 30);

                        image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(Library[imageNumber], RoughBackground, ROI);

                        //now estimate that the cell will be located at the center of the image and search for the cell
                        center = new Point((int)(image.Width / 2.0), (int)(image.Height / 2.0));
                        localCellSize = 0;
                        touchesBorder = false;
                        //ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravityIntensity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);
                        //ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);
                        //  if (mPassData.FluorImage == false)
                        //      ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravityThreshold_Centered(image, center, defaultThreshold, out centerOnImage, out localCellSize, out touchesBorder);
                        //  else
                        ImageProcessing.BlobFinding.CenterOfGravity.CenterOfGravity_Centered(image, center, out centerOnImage, out localCellSize, out touchesBorder);

                        BorderTouches[imageNumber] = 1;
                    }

                    //transform back to the whole picture coords and return the result
                    centerOnImage = new Point(ROI.X + centerOnImage.X, ROI.Y + centerOnImage.Y);
                    var loc = new CellLocation(centerOnImage, localCellSize, imageNumber);
                    loc.FV = ImageProcessing._2D.FocusScores.F4(image);

                    Locations[imageNumber] = loc;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);


            }
        }

        public Rectangle FindFirstCell(Image<Gray, float> image)
        {
            int minSize = 100;
            if (mPassData.FluorImage == true)
                ///Sets the initial size of the cell
                minSize = 3; //Changed from 30 to 3 to accomodate beads
            /// Finds the blobs using the Watershed algorithm; if multiple blobs (cells) are present, finds all of them
            ImageProcessing.BlobFinding.Blob[] Blobs = ImageProcessing.BlobFinding.Watershed.FindBlobs(image, true);

            int minCenter = (int)Math.Abs(Blobs[0].Center.X - image.Width / 2);
            ImageProcessing.BlobFinding.Blob bestCenter = Blobs[0];
            /// Finds the blob that is most close to the image center
            for (int i = 0; i < Blobs.Length; i++)
            {
                if (Blobs[i].MinSize > minSize && Blobs[i].Center.X > 0)
                {
                    int centerDist = (int)Math.Abs(Blobs[i].Center.X - image.Width / 2);
                    if (centerDist < minCenter)
                    {
                        bestCenter = Blobs[i];
                        minCenter = centerDist;
                    }
                }
            }
            ///Assign the blob closest to the center to "first" cell
            FirstCellCenter = bestCenter.Center;
            //Increases the cell size by a the indicated factor
            //CellSize = (int)(bestCenter.MaxSize * 1.6);
            CellSize = (int)(bestCenter.MaxSize);
            //if (CellSize > 450)
            //    CellSize = 400;
            SuggestedCellSize = CellSize;

            return new Rectangle(FirstCellCenter.X - CellSize / 2, FirstCellCenter.Y - CellSize / 2, CellSize, CellSize);
        }


        public Rectangle FindStackCell(Image<Gray, float> image, CellLocation lastLocation)
        {
            int minSize = 100;
            if (mPassData.FluorImage == true)
                minSize = 30;

            ImageProcessing.BlobFinding.Blob[] Blobs = ImageProcessing.BlobFinding.Watershed.FindBlobs(image, true);


            double minCenter = Math.Round(Math.Abs(Blobs[0].Center.X - lastLocation.CellCenter.X + .0001) + Math.Abs(Blobs[0].Center.Y - lastLocation.CellCenter.Y + .0001));

            ImageProcessing.BlobFinding.Blob bestCenter = Blobs[0];
            for (int i = 0; i < Blobs.Length; i++)
            {
                if (Blobs[i].MinSize > minSize && Blobs[i].Center.X > 0)
                {
                    int centerDist = (int)Math.Abs(Blobs[i].Center.X - lastLocation.CellCenter.X) + (int)Math.Abs(Blobs[i].Center.Y - lastLocation.CellCenter.Y);
                    if (centerDist < minCenter)
                    {
                        bestCenter = Blobs[i];
                        minCenter = centerDist;
                    }
                }
            }
            FirstCellCenter = bestCenter.Center;

            CellSize = (int)(bestCenter.MaxSize * 1.6);
            if (CellSize > 450)
                CellSize = 400;
            SuggestedCellSize = CellSize;

            return new Rectangle(FirstCellCenter.X - CellSize / 2, FirstCellCenter.Y - CellSize / 2, CellSize, CellSize);
        }



        private CellLocation[] RegisterCells()
        {

            if (FirstCellCenter == Point.Empty && VG_PP_Reader == null)
                FindFirstCell(Library[0]);

            BorderTouches = new int[Library.Count];
            minIntensity = new float[Library.Count];
            BatchFindCells(-1);
            // BatchFindCells(456);

            int i;

            for (i = FrameSkip; i < Library.Count; i += FrameSkip)
            {
                BatchFindCells(i);
            }

            //now that the cell positions are estimated, fill in all the other cells
            // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchFindCells(x));

            //for ( i = 0; i < Library.Count; i++)
            //{
            //    if (i == 7)
            //        System.Diagnostics.Debug.Print("");

            //     BatchFindCells(i);// Library[i].Sub(new Gray(minIntensity[i]));
            //}

            //   var b = BorderTouches.DrawGraph();
            // int w = b.Width;

            return Locations;
        }

        #endregion

        #region Extras
        public static bool CheckIfCell(Image<Gray, float> testImage, Image<Gray, float> RoughBackground)
        {
            //cleanup the background 
            var image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(testImage, RoughBackground, new Rectangle(0, 0, testImage.Width, testImage.Height));

            double Average = (image.Data.MaxArray() + 2 * image.GetAverage().Intensity) / 3;
            double Threshold = Average * .8;
            int underCount = 0;
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.Data[y, x, 0] < Threshold)
                        underCount++;
                }

            if (underCount > Math.PI * 100 * 100)
                return true;
            else
                return false;
        }

        public void SaveExamples(string dataFolder)
        {
            var ExamplePP = new Bitmap[4];
            var exampleROI = new Bitmap[4];
      //      for (int i = 0; i < 4; i++) LK added 7252014
            for (int i = 0; i < 4; i++)
            {
                // Library[i * 50].ROI = new Rectangle((int)(Locations[i * 50].CellCenter.X - SuggestedCellSize), 0, SuggestedCellSize * 2, Library[0].Height);
          //      var image = Library[i * 50].Copy(); LK 7252014
                var image = Library[i * 10].Copy();
                ExamplePP[i] = image.ScaledBitmap;
          //      var roi = Locations[i * 50].ToRectangle(); LK added 7252014
                var roi = Locations[i * 10].ToRectangle();
                roi.Inflate(20, 20);
                image.ROI = roi;
                exampleROI[i] = image.Copy().ScaledBitmap;
                // Library[i * 50].ROI = Rectangle.Empty;
            }

            //a series of images are created at removebackground that show the area around the object
            ExamplePP[0].Save(dataFolder + "\\projection1.jpg");
            ExamplePP[1].Save(dataFolder + "\\projection2.jpg");
            ExamplePP[2].Save(dataFolder + "\\projection3.jpg");
            ExamplePP[3].Save(dataFolder + "\\projection4.jpg");

            //save examples of the pp's cut down
            ImageProcessing.ImageFileLoader.Save_Bitmap(dataFolder + "\\FirstPP.bmp", exampleROI[0]);
            ImageProcessing.ImageFileLoader.Save_Bitmap(dataFolder + "\\HalfPP.bmp", exampleROI[1]);
            ImageProcessing.ImageFileLoader.Save_Bitmap(dataFolder + "\\LastQuarterPP.bmp", exampleROI[2]);
            ImageProcessing.ImageFileLoader.Save_Bitmap(dataFolder + "\\QuarterPP.bmp", exampleROI[3]);
        }


        public void SaveBitmaps(string filePattern)
        {

            string extension = Path.GetExtension(filePattern).ToLower();
            string directory = Path.GetDirectoryName(filePattern);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            int halfCell = CellSize / 2;
            if (extension == "" || extension == ".tif" || extension == ".tiff")
            {


                for (int i = 0; i < Library.Count; i++)
                {
                    string filename = string.Format("{0}{1:000}.tif", filePattern, i);
                    Library[i].ROI = new Rectangle((int)Locations[i].CellCenter.X - halfCell, (int)Locations[i].CellCenter.Y - halfCell, CellSize, CellSize);
                    var image = Library[i].Copy();
                    Library[i].ROI = Rectangle.Empty;

                    ImageProcessing.ImageFileLoader.Save_TIFF(filename, image);
                }
            }
            else
            {
                string fileAndDir = Path.GetDirectoryName(filePattern) + "\\" + Path.GetFileNameWithoutExtension(filePattern);
                for (int i = 0; i < Library.Count; i++)
                {
                    string filename = string.Format("{0}{1:000}{2}", fileAndDir, i, extension);
                    Rectangle roi = new Rectangle((int)Locations[i].CellCenter.X - halfCell, (int)Locations[i].CellCenter.Y - halfCell, CellSize, CellSize);
                    Library[i].ROI = roi;
                    var image = Library[i].Copy();
                    Library[i].ROI = Rectangle.Empty;

                    ImageProcessing.ImageFileLoader.Save_Bitmap(filename, image);
                }
            }

        }

        private void MarkCenters(CellLocation[] Locations)
        {
            for (int i = 0; i < Library.Count; i++)
            {
                var image = Library[i];
                image.Draw(new CircleF(Locations[i].CellCenter, 50), new Gray(0), 5);
                Library[i] = image;
            }

        }

        #endregion
    }
}
