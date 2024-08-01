using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
namespace ReconstructCells.Tools
{
    class StackHandler
    {
        /// <summary>
        /// omly use this one if you really know the location of the cell on the stack
        /// </summary>
        /// <param name="saveDir"></param>
        /// <param name="locations"></param>
        /// <param name="theBackground"></param>
        /// <param name="experimentFolder"></param>
        public static void CutStack(string saveDir, CellLocation[] locations, Image<Gray,float> theBackground, string experimentFolder)
        {
            OnDemandImageLibrary stack = new OnDemandImageLibrary(experimentFolder + "\\stack\\000", true, @"c:\temp", false);

            if (Directory.Exists(saveDir) == false)
                Directory.CreateDirectory(saveDir);
            else
            {
                try
                {
                    Directory.Delete(saveDir, true);
                    Directory.CreateDirectory(saveDir);
                }
                catch { }
            }

            CellLocation loc = locations[locations.Length-1];
            int cellSize =(int)( loc.CellSize *2+30);
            Rectangle cutRectangle = new Rectangle((int)loc.CellCenter.X - cellSize / 2, (int)loc.CellCenter.Y - cellSize / 2, cellSize, cellSize);


            for (int i = 0; i < stack.Count-1; i++)
            {
                Image<Gray, float> image = stack[i];
               
                var image2 = Background.RoughBackgrounds.RemoveBackground(image, theBackground, cutRectangle);

                ImageProcessing.ImageFileLoader.Save_16bit_TIFF(string.Format("{0}\\stackimage{1:000}.tif", saveDir, i), image2,1,0);
               // ImageProcessing.ImageFileLoader.Save_TIFF(string.Format("{0}\\stackimage{1:000}.tif", saveDir, i), image2);
            }
        }

        public static Image<Gray, float> CutStack(string saveDir, CellLocation[] locations, Image<Gray, float> theBackground, string experimentFolder, ReconstructCells.Registration.RoughRegister register)
        {
            OnDemandImageLibrary stack;
            try 
            {
                stack = new OnDemandImageLibrary(experimentFolder + "\\stack\\000", true, @"c:\temp", false);
            }
            catch 
            {
                Thread.Sleep(3000);
                stack = new OnDemandImageLibrary(experimentFolder + "\\stack\\000", true, @"c:\temp", false);
            }

            if (Directory.Exists(saveDir) == false)
                Directory.CreateDirectory(saveDir);
            else
            {
                try
                {
                    Directory.Delete(saveDir, true);
                    Directory.CreateDirectory(saveDir);
                }
                catch { }
            }

            //CellLocation loc = locations[locations.Length - 1];
            //int cellSize = (int)(loc.CellSize * 2 + 30);
            Rectangle cutRectangle = register.FindStackCell(stack[stack.Count / 2],locations[ locations.Length -1 ]);//new Rectangle((int)loc.CellCenter.X - cellSize / 2, (int)loc.CellCenter.Y - cellSize / 2, cellSize, cellSize);

            double StackFocus = double.MinValue;
            Image<Gray, float> BestStack=null;
            int tries = 0;
            for (int i = 0; i < stack.Count - 1; i++)
            {
                try
                {
                    Image<Gray, float> image = stack[i];

                    var image2 = Background.RoughBackgrounds.RemoveBackground(image, theBackground, cutRectangle);

                  
                    ImageProcessing.ImageFileLoader.Save_TIFF(string.Format("{0}\\stackimage{1:000}.tif", saveDir, i), image2);

                    double SF4 = ImageProcessing._2D.FocusScores.F4(image2);
                    if (SF4 > StackFocus)
                    {
                        BestStack = image2;
                        StackFocus = SF4;
                    }
                }
                catch (Exception ex)
                {

                    if (Directory.Exists(saveDir) == false)
                        Directory.CreateDirectory(saveDir);

                    if (tries < 4)
                    {
                        i = -1;
                    }

                    tries++;
                }
            }

            return BestStack;
        }

    }
}
