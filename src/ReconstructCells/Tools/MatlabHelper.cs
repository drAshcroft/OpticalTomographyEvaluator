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
using MathLibrary;
using ImageProcessing;

namespace ReconstructCells.Tools
{
    class MatlabHelper
    {
        public static OnDemandImageLibrary GetLibraryForMatlab(string LoadFolder)
        {
            PassData mPassData = new PassData();
            string[] Files = Directory.GetFiles(LoadFolder);

            Files = Files.SortNumberedFiles();

            return (new OnDemandImageLibrary(Files, true, @"C:\temp", false));
        }


       

        public float[,,]  MatlabTest(OnDemandImageLibrary library, string WriteFolder)
        {
            PassData mPassData = new PassData();
            mPassData.Library = library;
            mPassData.Locations = CellLocation.Open(@"c:\temp\testLocations.txt");
            mPassData.Information.Add("CellSize", 170);

            Tomography.PseudoSiddon ps = new Tomography.PseudoSiddon();
            ps.setFilter(Program.filterType, Program.filterLength);
            ps.SetInput(mPassData);
            ps.RunNode();

            ImageProcessing.ImageFileLoader.Save_Tiff_Stack(WriteFolder + "\\ProjectionObject.tif", ps.GetOutput().DensityGrid);
            ImageProcessing.ImageFileLoader.SaveDensityData(WriteFolder + "\\ProjectionObject.raw", ps.GetOutput().DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.UInt16);

            Bitmap[] CrossSections = ps.GetOutput().DensityGrid.ShowCross();
            Program.ShowBitmaps(CrossSections);

            CrossSections[0].Save(WriteFolder + "\\CrossSections_X.jpg");
            CrossSections[1].Save(WriteFolder + "\\CrossSections_Y.jpg");
            CrossSections[2].Save(WriteFolder + "\\CrossSections_Z.jpg");
            return mPassData.DensityGrid;

        }




    


    }
}
