//#if DEBUG
////#define TESTING
//#endif

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Emgu.CV;
//using Emgu.CV.Structure;
//using System.Threading.Tasks;
//using MathLibrary.Signal_Processing;
//using System.Drawing;
//using ImageProcessing._2D;
//using MathLibrary;
//using System.Windows.Forms;


//namespace ReconstructCells.Background
//{
//    class RemoveCapillaryTV : ReconstructNodeTemplate
//    {
//        private OnDemandImageLibrary Library;
//        private Image<Gray, float> PixelMap;


//        #region Code


//        float variation;
//        #region Linear

      

//        private void BatchRemoveCurves(int imageNumber)
//        {
//            var image = Library[imageNumber];



//            float[] background;
//            // image = RemoveCurvature3(image, out background, imageNumber);

//            Library[imageNumber] = image;


//        }
//        #endregion


//        #endregion

//        Image<Gray, float> roughBack;



//        protected override void RunNodeImpl()
//        {
//            Library = mPassData.Library;
//            ImageProcessing.ImageFileLoader.Save_TIFF(@"c:\temp\TVtestB.tif", Library[0]);
//            DoTV(Library[0], 20);
//           // ParallelOptions po = new ParallelOptions();
//            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

//           // numberOfImages = Library.Count;

//           // Parallel.For(0, numberOfImages, po, x => BatchRemoveCurves(x));

//        }

//    }
//}
