using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using ReconstructCells;

namespace ReconMaster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string dirName = "";
                string[] parts = null;
                string Prefix = "";
                string Year = "";
                string month = "";
                string day = "";

                dirName = args[0];
                parts = dirName.Split('_');
                Prefix = parts[0];
                Year = parts[1].Substring(0, 4);
                month = parts[1].Substring(4, 2);
                day = parts[1].Substring(6, 2);

               string VGFolder = Path.Combine(@"v:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
               string StorageFolder = Path.Combine(@"w:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
               string DehydrateFolder = Path.Combine(@"z:\Compressed", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
               string DataFolder = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

               string PPFolder = DehydrateFolder + @"\clippedMirror";
               string ReconFolder = DataFolder + @"\data\projectionobject___TIK_8";
               Console.Write("Starting MIP Movie");
               try
               {
                   PassData pd = new PassData();
                   pd.DensityGrid = ReconstructCells.Tools.MatlabHelps.OpenVirtualStack(ReconFolder, "*.tif");
                   ReconstructCells.Visualizations.MIP mip = new ReconstructCells.Visualizations.MIP();
                   mip.setNumberProjections(40);
                   mip.setFileName(DataFolder + "\\data\\mip.avi");
                   mip.SetInput(pd);

                   mip.RunNode();
               }
               catch (Exception ex) 
               {
                   Console.WriteLine(ex.Message);
               }

               Console.WriteLine("Starting Centering");
               try
               {
                   var bRecon = new OnDemandImageLibrary(PPFolder, true, @"c:\temp", false);
                   bRecon.CreateAVIVideoEMGU(DataFolder + "\\data\\centering.avi", 10);
               }
               catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            else
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
