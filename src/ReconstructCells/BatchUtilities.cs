using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Threading.Tasks;

namespace ReconstructCells
{
    public class BatchUtilities
    {
        OnDemandImageLibrary Library;
        public BatchUtilities(OnDemandImageLibrary library)
        {
            Library = library;
        }

        private float TargetAverageIntensity = Int16.MaxValue * .9f;
        private void BatchInvertAndFlatten(int imageNumber)
        {
            //  ImageProcessing._2D.ImageManipulation.InvertFlattenNormalizeImage(Library[imageNumber], TargetAverageIntensity);
            ImageProcessing._2D.ImageManipulation.InvertFlattenImage(Library[imageNumber]);
        }

        public void InvertAndFlatten()
        {
            ImageProcessing._2D.ImageManipulation.InvertFlattenImage(Library[0]);
            TargetAverageIntensity = (float)Library[0].GetAverage().Intensity;

            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(1, numberOfImages, Program.threadingParallelOptions, x => BatchInvertAndFlatten(x));
        }


        private void BatchInvertToMax(int imageNumber)
        {
            var image = Library[imageNumber];
            ImageProcessing._2D.ImageManipulation.InvertToMax(image, Int16.MaxValue);

            Library[imageNumber] = Library[imageNumber].Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
        }

        private const int TrimSize = 15;
        private void BatchInvertToMaxAndClip(int imageNumber)
        {
            var image = Library[imageNumber];

            image.ROI = new Rectangle(TrimSize, TrimSize, image.Width - 2 * TrimSize, image.Height - 2 * TrimSize);
            image = image.Copy();
            ImageProcessing._2D.ImageManipulation.InvertToMax(image, Int16.MaxValue);

            Library[imageNumber] = image.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
        }

        private void BatchInvertToMaxAndRotate(int imageNumber)
        {
            var image = Library[imageNumber];

            image.ROI = new Rectangle(TrimSize, TrimSize, image.Width - 2 * TrimSize, image.Height - 2 * TrimSize);
            image = image.Copy();
            ImageProcessing._2D.ImageManipulation.InvertToMax(image, Int16.MaxValue);

            Library[imageNumber] = image.Rotate(90, new Gray(0), true); //image.Flip(Emgu.CV.CvEnum.FLIP);
        }

        private void BatchInvertToMax2(int imageNumber)
        {
            var image = Library[imageNumber];

            image.ROI = new Rectangle(TrimSize, TrimSize, image.Width - 2 * TrimSize, image.Height - 2 * TrimSize);
            image = image.Copy();
            ImageProcessing._2D.ImageManipulation.InvertToMax(image, Int16.MaxValue);

            Library[imageNumber] = image;// image.Rotate(90, new Gray(0), true); //image.Flip(Emgu.CV.CvEnum.FLIP);
        }

        public void FixFluorImages(bool Rotate)
        {
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            if (Rotate)
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertToMaxAndRotate(x));
            else
                Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertToMax2(x));
        }


        private void BatchInvertToMaxAndClipRotate(int imageNumber)
        {
            var image = Library[imageNumber];

            //  image.ROI = new Rectangle(30, 30, image.Width - 60, image.Height - 60);
            //  image = image.Copy();
            ImageProcessing._2D.ImageManipulation.InvertToMax(image, Int16.MaxValue);

            //var nImage = new Image<Gray, float>(image.Height, image.Width);

            //var data = nImage.Data;
            //for (int i = 0; i < image.Width; i++)
            //    for (int j = 0; j < image.Height; j++)
            //    {
            //        data[i, j, 0] = image.Data[j, i, 0];

            //    }

            Library[imageNumber] = image.Rotate(90, new Gray(0)); ;
        }

        public void FixRotateFluorImages()
        {
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            for (int i = 0; i < Library.Count; i++)
                BatchInvertToMaxAndClipRotate(i);

            //  Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertToMaxAndClipRotate(x));
        }



        private void BatchRotate(int imageNumber)
        {
            var image = Library[imageNumber];

            var nImage = new Image<Gray, float>(image.Height, image.Width);

            var data = nImage.Data;
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    data[i, j, 0] = image.Data[j, i, 0];

                }

            Library[imageNumber] = nImage;
        }

        public void FixRotateImages()
        {
            //now that the cell positions are estimated, fill in all the other cells
           // ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, Program.threadingParallelOptions, x => BatchInvertToMaxAndClipRotate(x));
        }
    }
}
