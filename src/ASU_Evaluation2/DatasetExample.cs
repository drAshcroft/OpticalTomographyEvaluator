using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Drawing;

namespace ASU_Evaluation2
{
    class DatasetExample
    {
        public string Date { get { return "3/3/3"; } }
        public ImageSource ExampleImage
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_X___TIK.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public bool Recon_Succeeded { get { return true; } }
        public bool Cell_Good
        {
            get;
            set;
        }
        public bool Recon_Good
        {
            get;
            set;
        }
        public string DatasetName { get; private set; }
        public string TopDirectory { get; private set; }


        string VGFolder;
        string StorageFolder;
        string DehydrateFolder;
        string BackupFolder;
        string StackFolder;
        string StackReportFilePath;

        private void BuildPaths()
        {
            string dirName = Path.GetFileNameWithoutExtension(TopDirectory);
            //return dirName;
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            VGFolder = Path.Combine(@"y:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            StorageFolder = Path.Combine(@"z:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            DehydrateFolder = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            BackupFolder = Path.Combine(@"V:\BackupCompleted\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

            if (Prefix.ToLower() == "cct001")
            {
                StackFolder = Path.Combine(@"V:\Raw PP\cct001\Absorption\", Year + month + "\\" + day + "\\" + dirName + "\\STACK\\000");
            }
            else
                StackFolder = Path.Combine(@"V:\Raw PP\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + "\\STACK\\000");

            StackReportFilePath = VGFolder + "\\FixedStackReport.xml";
        }

        public DatasetExample(string name, string path)
        {
            DatasetName = name;
            TopDirectory = path;
            GoodStain = true;
            Clipping = false;
            Interesting = false;
            Noisy = false;
            Rings = false;
            InterferingObject = 0;
            Comments = "  ";
            Evaluator = "Brian";
            Cell_Good = true;
            CellType = " ";
            Recon_Good = true;

        }

        public string Evaluator { get; set; }
        public string CellType { get; set; }
        public bool GoodStain { get; set; }
        public bool Clipping { get; set; }
        public bool Interesting { get; set; }
        public bool Noisy { get; set; }
        public bool Rings { get; set; }
        public int InterferingObject { get; set; }
        public string Comments { get; set; }



        public ImageSource Axial_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_X___TIK.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public ImageSource Sag_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Y___TIK.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public ImageSource Z_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Z___TIK.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }


        public void ClearMemory()
        {



        }

        public ImageSource Background
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\background.tif";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public ImageSource PP1
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection1.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public ImageSource PP2
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection2.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public ImageSource PP3
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection3.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }
        public string Centering
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\centering.avi";

                if (File.Exists(exampleImage))
                    return exampleImage;
                else
                    return "";
            }
        }

        public ImageSource Stack_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Y___TIK.jpg";

                if (File.Exists(exampleImage))
                    return BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    return null;
            }
        }

    }
}
