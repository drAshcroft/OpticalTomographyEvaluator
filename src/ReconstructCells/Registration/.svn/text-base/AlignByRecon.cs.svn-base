using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using ImageProcessing._3D;
using System.Threading.Tasks;

namespace ReconstructCells.Registration
{
    class AlignByRecon : ReconstructNodeTemplate
    {
        int nProjections = 25;
        int Scale = 1;
        bool UseHalf = false;
        bool AlreadyReconed = false;

        Image<Gray, float> BackGround;
        CellLocation[] Locations;
        int CellSize; //determined in code 
        //   float BackgroundAverage;
        OnDemandImageLibrary Library;
        OnDemandImageLibrary LibraryCut;

        bool AlreadyCut = false;

        float[] ImageMinValues;

        int Buffer = 40;

        #region Properties

        #region Set
        public void setAlreadyReconed(bool alreadyReconed)
        {
            AlreadyReconed = alreadyReconed;
        }
        public void setNumberOfProjections(int nProjections)
        {
            this.nProjections = nProjections;
        }

        public void setScale(int scale)
        {
            this.Scale = scale;
        }

        public void setUseHalf(bool useHalf)
        {
            this.UseHalf = useHalf;
        }


        public void setAlreadyCut(bool libraryisAlreadyCut)
        {
            AlreadyCut = libraryisAlreadyCut;

        }
        #endregion

        #region Get


        #endregion

        #endregion

        #region Code

        private void BatchInvertCut(int imageNumber)
        {
            try
            {

                var cellLocation = Locations[imageNumber];
                int minCellX = (int)(cellLocation.CellCenter.X - CellSize / 2 - Buffer);
                // int maxCellX = (int)(minCellX + CellSize + Buffer);
                int minCellY = (int)(cellLocation.CellCenter.Y - CellSize / 2 - Buffer);
                // int maxCellY = (int)(minCellY + CellSize + Buffer);

                Rectangle ROI = new Rectangle(minCellX, minCellY, CellSize + 2 * Buffer, CellSize + 2 * Buffer);
                int cellHalf = (int)((ROI.Width) / 2d);

                var image = Library[imageNumber];
                //   image = ReconstructCells.Background.RoughBackgrounds.RemoveBackground(image, BackGround, ROI);
                image = ImageProcessing._2D.ImageManipulation.CopyROI(image, ROI);
                ImageProcessing._2D.ImageManipulation.InvertImage(image);

                LibraryCut[imageNumber] = image.SmoothGaussian(5);

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }
        }


        /*
             // image = image.SmoothMedian(3);
               // image.MinMax(out min, out max, out pm, out pM);
               
                int sizeIncrease = mPassData.DataScaling;
               // image = image.Resize(2, Emgu.CV.CvEnum.INTER.CV_INTER_AREA );
            //   image = image.PyrUp();
               // var image2 = image.Copy();
              //  ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image2, true);
                ImageProcessing._2D.ImageManipulation.InvertImageAndClipOtsu(image, false);

                #region refine cutting
               // PointF p = NoisyFinder(image2);
                int nCellSize = (int)(Math.Floor(((double)ROI.Width * sizeIncrease - TrimSize * sizeIncrease) / 2));

              //  ROI = new Rectangle((int)(Math.Floor(p.X - nCellSize)), (int)(Math.Floor(p.Y - nCellSize)), 2 * nCellSize, 2 * nCellSize);

               // cellLocation.CellCenter.X += (int)(p.X - cellHalf);
               // cellLocation.CellCenter.Y += (int)(p.Y - cellHalf);

                // beforeRoughness[imageNumber] = cellLocation.CellCenter.X;
                // afterRoughness[imageNumber] = cellLocation.CellCenter.Y;
                if ((ROI.X >= 0 && ROI.Right < image.Width && ROI.Y >= 0 && ROI.Bottom < image.Height))
                {
                    
                    image = image  .Copy(ROI);
                }
                else
                {
                    #region boundry Problems ___ lazy problem
                    Image<Gray, float> nImage = new Image<Gray, float>(2 * nCellSize, 2 * nCellSize);

                    var ROIdest = new Rectangle(0, 0, 2 * nCellSize, 2 * nCellSize);
                    var ROIsrc = new Rectangle(ROI.X, ROI.Y, ROI.Width, ROI.Height);

                    if (ROI.X < 0)
                    {
                        ROIdest.X = -1 * ROI.X;
                        int right = ROI.Right;
                        ROIsrc.X = 0; ROIsrc.Width = right;
                        ROIdest.Width = right;
                    }

                    if (ROI.Y < 0)
                    {
                        ROIdest.Y = -1 * ROI.Y;
                        int bottom = ROI.Bottom;
                        ROIsrc.Y = 0; ROIsrc.Height = bottom;
                        ROIdest.Height = bottom;
                    }

                    if (ROI.Right > image.Width)
                    {
                        ROIsrc.Width = image.Width - ROI.X;
                        ROIdest.Width = ROIsrc.Width;
                    }

                    if (ROI.Bottom > image.Height)
                    {
                        ROIsrc.Height = image.Height - ROI.Y;
                        ROIdest.Height = ROIsrc.Height;
                    }

                    // nImage.ROI = ROIdest;
                    // image.ROI = ROIsrc;

                    //nImage = image.Copy();
                    int xx = ROIdest.X;
                    for (int x = ROIsrc.X; x < ROIsrc.Right; x++)
                    {
                        int yy = ROIdest.Y;
                        for (int y = ROIsrc.Y; y < ROIsrc.Bottom; y++)
                        {
                            nImage.Data[yy, xx, 0] = image.Data[y, x, 0];
                            yy++;
                        }
                        xx++;
                    }

                    //image.CopyTo(nImage);
                    image = nImage;
                    #endregion
                }

                Library[imageNumber] = image;//.PyrDown();
                #endregion

            }
            catch (Exception ex)
            {
                Program.WriteTagsToLog("divide error", ex.Message + "/n/n/n" + ex.StackTrace);
                Library[imageNumber] = Library[0].CopyBlank();

            }

         * */

        private void InvertAndCutImages()
        {


           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertCut(x));
        }

        private Random rnd = new Random();
        protected override void RunNodeImpl()
        {
            CellSize = (int)mPassData.GetInformation("CellSize");
            Library = mPassData.Library;
            Locations = mPassData.Locations;
            BackGround = mPassData.Library[10].CopyBlank().Add(new Gray(1));
            ImageMinValues = new float[Library.Count];


            var origLibrary = mPassData.Library;
            if (AlreadyCut == false)
            {
                LibraryCut = new OnDemandImageLibrary(Library.Count, true, @"c:\temp", false);
                InvertAndCutImages();
                mPassData.Library = LibraryCut;
                Library = LibraryCut;

                //  mPassData.SavePassData(@"c:\temp\noise2");
            }

            if (mPassData.Weights == null)
            {
                mPassData.Weights = new float[mPassData.Library.Count];
                for (int i = 0; i < mPassData.Weights.Length; i++)
                    mPassData.Weights[i] = 1;
            }

            OnDemandImageLibrary forwards=null;

            if (AlreadyReconed == true)
            {
                // Console.WriteLine("             Starting PseudoSiddon");
                //Tomography.OptRecon ps = new Tomography.OptRecon();
                //ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                //ps.setFilter(Program.FilterType, 256);
                //ps.SetInput(mPassData);
                //ps.setSkipProjections(10);
                //ps.RunNode();

                //Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
                //ps.setFilter(Program.FilterType, 256);
                //ps.SetInput(mPassData);
                //ps.setSkipProjections(10);
                //ps.setHalfProjections(true);
                //ps.RunNode();

                // mPassData = ps.GetOutput();

                Console.WriteLine("             Starting PseudoSiddonForward");


                Tomography.OptRecon psf = new Tomography.OptRecon();
                psf.setReconType(Tomography.OptRecon.ReconTypes.ForwardOnly);
                psf.SetInput(mPassData);
                psf.setSkipProjections(1);
                psf.RunNode();

                forwards = psf.getForwardProjections();

                mPassData = psf.GetOutput();
            }
            else
            {
                if (rnd.NextDouble() >0)
                {
                    Console.WriteLine("             Starting PseudoSiddon");
                    Tomography.OptRecon ps = new Tomography.OptRecon();
                    ps.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                    ps.setFilter(Program.FilterType, 256);
                    ps.SetInput(mPassData);
                    ps.setSkipProjections(10);
                    ps.RunNode();
                
                    Console.WriteLine("             Starting PseudoSiddonForward");

                    Tomography.OptRecon psf = new Tomography.OptRecon();
                    psf.setReconType(Tomography.OptRecon.ReconTypes.ForwardOnly);
                    psf.SetInput(mPassData);
                    psf.setSkipProjections(1);
                    psf.RunNode();

                    forwards = psf.getForwardProjections();

                    mPassData = psf.GetOutput();
                }
                else
                {
                    Console.WriteLine("             Starting GPU PseudoSiddon ");
                    var ps2 = new Tomography.GPURecon();
                    ps2.setReconType(Tomography.OptRecon.ReconTypes.FBP);
                    ps2.setNumIterations(1);
                    ps2.setCleanProjections(false);
                    ps2.setGetForwardProjections(true);
                    ps2.SetInput(mPassData);
                    ps2.RunNode();

                    forwards = ps2.getForwardProjections();

                    mPassData = ps2.GetOutput();
                }
            }
        

            Console.WriteLine("             Starting FineRegistration");
            //now align the projections to the recon
            Registration.FineRegistration fr = new Registration.FineRegistration();
            fr.setTargets(forwards);
            fr.setDoHalf(false);
            fr.setCutRegistered(AlreadyCut);
            fr.setAlignmentScale(Scale);
            fr.SetInput(mPassData);
            fr.RunNode();

            mPassData = fr.GetOutput();

            if (AlreadyCut == false)
            {
                mPassData.Library = origLibrary;
            }
        }

        #endregion
    }
}
