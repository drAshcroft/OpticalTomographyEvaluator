﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using System.IO;

namespace ReconstructCells.Background
{
    public class BackgroundFromEmptyPP
    {

        //OnDemandImageLibrary Library;
        /* public BackgroundFromEmptyPP(OnDemandImageLibrary library)
         {
             Library = library;

         }*/

        public static Image<Gray, float> GetClosestBackground(string experimentFolder)
        {
            string[] files = Directory.GetFiles(@"z:\ASU_Recon\Backgrounds", "*.tif");

            string dirName = Path.GetFileNameWithoutExtension(experimentFolder);
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            DateTime experimentTime = new DateTime(int.Parse(Year), int.Parse(month), int.Parse(day));

            string bestFile = "";
            long BestTime = long.MaxValue;
            for (int i = 0; i < files.Length; i++)
            {
                dirName = Path.GetFileNameWithoutExtension(files[i]);
                parts = dirName.Split('_');
                Year = parts[1];
                month = parts[2];
                day = parts[3];

                DateTime backTime = new DateTime(int.Parse(Year), int.Parse(month), int.Parse(day));

                long days =Math.Abs (experimentTime.Subtract(backTime).Days);
                if (days < BestTime)// && days >= 0)
                {
                    BestTime = days;
                    bestFile = files[i];
                }
            }

            if (BestTime == long.MaxValue)
                return null;

            return ImageProcessing.ImageFileLoader.LoadImage(bestFile);
        }


        public static void CreateBackground(string experimentFolder, string outputFolder)
        {
            OnDemandImageLibrary library = new OnDemandImageLibrary(Directory.GetFiles(experimentFolder + "\\pp"), true, @"c:\temp", false);
            var image = GetBackground(library);


            string dirName = Path.GetFileNameWithoutExtension(experimentFolder);
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            ImageProcessing.ImageFileLoader.Save_TIFF(outputFolder + "\\background_" + Year + "_" + month + "_" + day + ".tif", image);
        }

        private static Image<Gray, float> GetBackground(OnDemandImageLibrary library)
        {
            Image<Gray, float> average = library[0].CopyBlank();

            double target = library[0].GetAverage().Intensity;

            double factor = 1d / library.Count;
            for (int i = 0; i < library.Count; i++)
            {
                var image = library[i];
                double alpha = target / image.GetAverage().Intensity;
                image = image.Mul(alpha);
                average = average.Add(image);
            }

            average = average.Mul(factor);
            double ave = 1 / average.GetAverage().Intensity;

            average = average.Mul(ave);


            var lowFreq = average.SmoothGaussian(51);

            var testImage = lowFreq.CopyBlank();
            var num = average.Data;// library[0].Data;
            unchecked
            {
                for (int x = 0; x < testImage.Width; x++)
                    for (int y = 0; y < testImage.Height; y++)
                    {
                        testImage.Data[y, x, 0] = num[y, x, 0] / lowFreq.Data[y, x, 0];
                    }
            }

            return testImage;
        }




    }
}
