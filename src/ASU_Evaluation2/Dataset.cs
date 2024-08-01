using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ASU_Evaluation2
{
    public class Dataset
    {

        public ImageSource ExampleImage
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_X___TIK.jpg";

                if (File.Exists(exampleImage))
                {
                    ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    image.Freeze();
                    return image;
                }
                else
                    return null;
            }
        }

        public string DatasetName { get; private set; }
        public string TopDirectory { get; private set; }


        string VGFolder;
        string StorageFolder;
        string DehydrateFolder;
        string BackupFolder;
        string ColdBackupFolder;
        string ColdBackupFolder2;
        string StackFolder;
        string PremadeStack;
        string StackReportFilePath;
        string InfoFileFilePath;

        private void BuildPaths()
        {
            string dirName = Path.GetFileNameWithoutExtension(TopDirectory);
            //return dirName;
            string[] parts = dirName.Split('_');
            string Prefix = parts[0];
            string Year = parts[1].Substring(0, 4);
            string month = parts[1].Substring(4, 2);
            string day = parts[1].Substring(6, 2);

            VGFolder = Path.Combine(DataStore.ProcessedDrive, Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            StorageFolder = Path.Combine(DataStore.StorageDrive, Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            DehydrateFolder = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            BackupFolder = Path.Combine(Path.GetDirectoryName(DataStore.DataFolder), @"BackupCompleted\" + Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
            ColdBackupFolder = Path.Combine(DataStore.ColdStorageDrive, @"Raw PP\" + Prefix + "\\Absorption\\" + Year + month + "\\" + day + "\\" + dirName);
            ColdBackupFolder2 = Path.Combine(DataStore.ColdStorageDrive2,  @"cellctdata$\" +   Year + month + "\\" + day + "\\" + dirName);

            if (Prefix.ToLower() == "cct001")
                //z:\Raw PP\cct001\Absorption\201207\09\cct001_20120709_124409
                InfoFileFilePath = Path.Combine(Path.GetDirectoryName(DataStore.DataFolder), "raw pp\\" + Prefix + "\\absorption\\" + Year + month + "\\" + day + "\\" + dirName + "\\info.xml");
            else
                InfoFileFilePath = Path.Combine(Path.GetDirectoryName(DataStore.DataFolder), "raw pp\\" + Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + "\\info.xml");



            if (Prefix.ToLower() == "cct001")
            {
                StackFolder = Path.Combine((DataStore.DataDrive) + @"\Raw PP\cct001\Absorption\", Year + month + "\\" + day + "\\" + dirName + "\\STACK\\000");
            }
            else
                StackFolder = Path.Combine((DataStore.DataDrive) + @"Raw PP\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + "\\STACK\\000");

            if (Directory.Exists(StackFolder) == false)
            {
                StackFolder =ColdBackupFolder + "\\STACK\\000";
                if (Directory.Exists(StackFolder) == false)
                    StackFolder = ColdBackupFolder2 + "\\STACK\\000";
                System.Diagnostics.Debug.Print(Directory.Exists(StackFolder).ToString());
            }


            StackReportFilePath = VGFolder + "\\FixedStackReport.xml";

            //Z:\Compressed\cct001\201302\11\cct001_20130211_090244\stack
            PremadeStack = DataStore.DataDrive + @"\Compressed\" + Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName + "\\STACK";
        }

        public Dataset(string name, string path, bool dataLoaded = false)
        {
            DataLoaded = dataLoaded;
            DatasetName = name;
            // _Evaluator = DataStore.User;
            _Evaluator = "Undone";
            TopDirectory = path;
            GoodStain = true;
            Clipping = false;
            Interesting = false;
            Noisy = false;
            Rings = false;
            InterferingObject = 0;
            Comments = "  ";

            Cell_Good = true;
            CellType = " ";
            Recon_Good = true;
            AlignmentGood = true;
            FocusGood = true;
            MaskCorrect = true;
            MaskCorrect10 = true;
            MaskCorrect40 = true;

            try
            {
                CollectionDate = DataStore.GetCollectionDate(name);
            }
            catch { }
            _Evaluator = "Undone";
        }

        public Bitmap MaskImage3D
        {
            get
            {
                try
                {
                    string path = DataStore.DataDrive + @"\ASU_Recon\ImageProcessingArchive\" + DatasetName;
                    string[] files = Directory.GetFiles(path, "*Segmentation*.tif");
                    return new Bitmap(files[0]);
                }
                catch
                {
                    return null;
                }
            }
        }

        public Bitmap UnMaskImage3D
        {
            get
            {
                try
                {
                    string path = DataStore.DataDrive + @"\ASU_Recon\ImageProcessingArchive\" + DatasetName;
                    string[] files = Directory.GetFiles(path, "*Original*.tif");
                    return new Bitmap(files[0]);
                }
                catch
                {
                    return null;
                }
            }
        }

        public Bitmap MaskImage10
        {
            get
            {
                try
                {
                    string path = DataStore.DataDrive + @"\ASU_Recon\ImageProcessingArchive\" + DatasetName + "\\10x";
                    string[] files = Directory.GetFiles(path, "*Segmentation*.tif");
                    return new Bitmap(files[0]);
                }
                catch
                {
                    return null;
                }
            }
        }

        public Bitmap MaskImage40
        {
            get
            {
                try
                {
                    string path = DataStore.DataDrive + @"\ASU_Recon\ImageProcessingArchive\" + DatasetName + "\\40x";
                    string[] files = Directory.GetFiles(path, "*Segmentation*.tif");
                    return new Bitmap(files[0]);
                }
                catch
                {
                    return null;
                }
            }
        }

        private string _Evaluator = "undone";
        public string Evaluator
        {
            get { return _Evaluator; }
            // set { _Evaluator = value; }
        }
        public void setEvaluator(string evaluator, bool force = false)
        {
            if (force)
                _Evaluator = evaluator;
            else
            {
                string s = _Evaluator.ToLower();
                if (s == "themachine" || s == "undone")
                    _Evaluator = evaluator;
            }
        }

        public DateTime ReconDate { get; set; }
        public string BackGround { get; set; }
        public DateTime CollectionDate { get; set; }

        public DateTime EvaluationDate { get; set; }

        public string CellType { get; set; }
        public bool GoodStain { get; set; }
        public bool Clipping { get; set; }
        public bool Interesting { get; set; }
        public bool Noisy { get; set; }
        public bool Rings { get; set; }
        public bool FocusGood { get; set; }
        public bool MaskCorrect { get; set; }
        public bool MaskCorrect10 { get; set; }
        public bool MaskCorrect40 { get; set; }
        public bool AlignmentGood { get; set; }
        public int InterferingObject { get; set; }
        public string Comments { get; set; }
        public bool Recon_Succeeded
        {
            get { return true; }
        }
        bool DataLoaded = false;
        bool _Cell_Good = true;
        bool _Recon_Good = true;
        bool _Cell_Repeat = false;
        public bool Cell_Good
        {
            get
            {
                if (DataLoaded == false)
                {
                    DataStore.LoadDataset(this);
                    DataLoaded = true;
                }
                return _Cell_Good;
            }
            set
            {

                _Cell_Good = value;
                setEvaluator(DataStore.User);
            }
        }

        public bool Cell_Repeat
        {
            get
            {
                if (DataLoaded == false)
                {
                    DataStore.LoadDataset(this);
                    DataLoaded = true;
                }
                return _Cell_Repeat;
            }
            set
            {
                _Cell_Repeat = value;
                setEvaluator(DataStore.User);
            }
        }


        public bool Recon_Good
        {
            get
            {
                if (DataLoaded == false)
                {
                    DataStore.LoadDataset(this);
                    DataLoaded = true;
                }
                return _Recon_Good;
            }
            set
            {
                _Recon_Good = value;
                setEvaluator(DataStore.User);
            }
        }
        public string MIP_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\mip.avi";

                if (File.Exists(exampleImage))
                    return exampleImage;
                else
                    return "";
            }
        }

        public Bitmap Axial_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_X___TIK.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }
        public Bitmap Sag_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Y___TIK.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }
        public Bitmap Z_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Z___TIK.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }

        bool stopLoading = false;

        public void ClearMemory()
        {
            DataStore.SaveDataset(this);
            stopLoading = true;
            //VolumeImages = null;
            //StackImages = null;
            //VolRawImage = null;
        }
        BitmapSource[] VolumeImages;
        Image<Gray, float>[] VolRawImage;
        string[] VolumeSources;
        int index = 0;

        public ImageSource Fly_Through
        {
            get
            {
                if (VolumeSources != null)
                {
                    index = ((index + 1) % VolumeSources.Length);

                    if (VolumeImages != null)
                    {

                        if (VolumeImages[index] != null)
                            return VolumeImages[index];
                    }
                }
                return null;
            }
        }

        public int StackProgress
        {
            get { return (int)((double)StackIndex / StackImages.Length * 100d); }

        }

        int StackIndex;
        string[] StackFiles;
        BitmapSource[] StackImages;
        bool StillLoading = true;

        private float[][] bm;
        private float[][] cm;
        private float[][] ChangeColor;
        private float[][] Multiply(float[][] f1, float[][] f2)
        {
            float[][] X = new float[5][];
            for (int d = 0; d < 5; d++)
                X[d] = new float[5];

            int size = 5;
            float[] column = new float[5];
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    column[k] = f1[k][j];
                }
                for (int i = 0; i < 5; i++)
                {
                    float[] row = f2[i];
                    float s = 0;
                    for (int k = 0; k < size; k++)
                    {
                        s += row[k] * column[k];
                    }
                    X[i][j] = s;
                }
            }

            return X;
        }

        private Bitmap DrawImage(float[][] Matrix, Bitmap image)
        {
            ColorMatrix m = new ColorMatrix(Matrix);
            ImageAttributes ia = new ImageAttributes();
            ia.SetColorMatrix(m);

            Rectangle rMy = new Rectangle(0, 0, image.Width, image.Height);

            Bitmap bm = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage((Image)bm);
            g.Clear(System.Drawing.Color.Black);

            g.DrawImage(image, rMy, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);

            return bm;
        }
        private void NewMatrices()
        {
            // Initialize the matices;
            bm = new float[5][];
            cm = new float[5][];
            for (int i = 0; i < 5; i++)
            {
                bm[i] = new float[5];
                cm[i] = new float[5];
            }

            // Set the values of the brightness matrix
            float brightness = .6f;
            bm[0][0] = 1; bm[0][1] = 0; bm[0][2] = 0; bm[0][3] = 0; bm[0][4] = 0;
            bm[1][0] = 0; bm[1][1] = 1; bm[1][2] = 0; bm[1][3] = 0; bm[1][4] = 0;
            bm[2][0] = 0; bm[2][1] = 0; bm[2][2] = 1; bm[2][3] = 0; bm[2][4] = 0;
            bm[3][0] = 0; bm[3][1] = 0; bm[3][2] = 0; bm[3][3] = 1; bm[3][4] = 0;
            bm[4][0] = brightness; bm[4][1] = brightness; bm[4][2] = brightness; bm[4][3] = 1; bm[4][4] = 1;

            // Set the values of contrast matrix
            float contrast = 2f;
            float T = 0.5f * (1f - contrast);

            cm[0][0] = contrast; cm[0][1] = 0; cm[0][2] = 0; cm[0][3] = 0; cm[0][4] = 0;
            cm[1][0] = 0; cm[1][1] = contrast; cm[1][2] = 0; cm[1][3] = 0; cm[1][4] = 0;
            cm[2][0] = 0; cm[2][1] = 0; cm[2][2] = contrast; cm[2][3] = 0; cm[2][4] = 0;
            cm[3][0] = 0; cm[3][1] = 0; cm[3][2] = 0; cm[3][3] = 1; cm[3][4] = 0;
            cm[4][0] = T; cm[4][1] = T; cm[4][2] = T; cm[4][3] = 1; cm[4][4] = 1;
        }


        public int NumberStackImages
        {
            get
            { return StackFiles.Length; }
        }

        private string[] DiscoverStack(string stackFolder)
        {
            //stackFolder = stackFolder.ToLower().Replace("\\absorption\\", "\\");
            string[] Files = Directory.GetFiles(stackFolder);

            List<string> forwards = new List<string>();
            List<string> backs = new List<string>();
            string MidPoint = "";

            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Path.GetFileNameWithoutExtension(Files[i]);
                if (filename.EndsWith("n") == true)
                {
                    backs.Add(Files[i]);
                }
                else if (filename.EndsWith("p") == true)
                    forwards.Add(Files[i]);
                else if (filename.EndsWith("m") == true)
                    MidPoint = Files[i];
            }

            backs.Sort();
            forwards.Sort();

            string[] SortedFiles = new string[backs.Count + 1 + forwards.Count];
            int cc = 0;
            for (int i = 0; i < backs.Count; i++)
            {
                SortedFiles[cc] = backs[backs.Count - 1 - i];
                cc++;
            }
            SortedFiles[cc] = MidPoint;
            cc++;
            for (int i = 0; i < forwards.Count; i++)
            {
                SortedFiles[cc] = forwards[i];
                cc++;
            }

            return SortedFiles;

        }

        public Image<Gray, float>[] VolumeStack
        {
            get { return VolRawImage; }
        }

        private void BatchLoadVolume(int index)
        {
            try
            {
                if (VolumeImages != null && stopLoading == false)
                {
                    double p = LoadingProgress;
                    p += 1d / VolumeSources.Length;
                    LoadingProgress = p;
                    var image = ImageProcessing.ImageFileLoader.LoadImage(VolumeSources[index]);
                    VolRawImage[index] = image;
                    Bitmap b = image.Bitmap;
                    //  b = DrawImage(ChangeColor, b);
                    if (VolumeImages != null)
                        VolumeImages[index] = (b).CreateBitmapSourceFromBitmap();
                }
            }
            catch { }
        }

        private void BatchLoadCompressedStack(int index)
        {
            try
            {
                if (StackImages != null && stopLoading == false)
                {
                    Bitmap b;

                    var image = ImageProcessing.ImageFileLoader.LoadImage(StackFiles[index]);
                    b = image.ScaledBitmap;


                    if (StackImages != null)
                        StackImages[index] = b.CreateBitmapSourceFromBitmap();
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.Print(ex.Message);
            }
        }


        private void BatchLoadStack(int index)
        {
            try
            {
                if (StackImages != null && stopLoading == false)
                {
                    Bitmap b;
                    // if (StackFiles[index].Contains(".ivg"))
                    {
                        var image = ImageProcessing.ImageFileLoader.LoadImage(StackFiles[index]);
                        b = image.ScaledBitmap;
                    }
                    //  else
                    //     b = new System.Drawing.Bitmap(StackFiles[index]);
                    Bitmap b2;
                    if (StackROI[0].Width != -1)
                    {
                        b2 = new Bitmap(StackROI[0].Width, StackROI[0].Height, b.PixelFormat);
                        Graphics g = Graphics.FromImage(b2);
                        g.DrawImage(b, new Rectangle(0, 0, b2.Width, b2.Height), StackROI[0], GraphicsUnit.Pixel);
                    }
                    else
                        b2 = b;
                    // b2.Save(@"c:\temp\stack\image" + string.Format("{0:000}.png", index));

                    if (StackImages != null)
                        StackImages[index] = b2.CreateBitmapSourceFromBitmap();
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        public BitmapSource GetStackImage(int index)
        {
            if (StackImages != null)
                return StackImages[index];
            else
                return null;
        }

        private Rectangle[] StackROI;

        public double LoadingProgress { get; private set; }
        private bool StackLoaded = false;
        public void StartLoading()
        {
            LoadingProgress = 0;
            BuildPaths();
            StackLoaded = false;

            NewMatrices();
            ChangeColor = Multiply(bm, cm);

            Thread t2 = new Thread(delegate()
            {
                try
                {
                    VolumeSources = Directory.GetFiles(TopDirectory + @"\data\projectionobject___TIK_16");
                    VolumeImages = new BitmapSource[VolumeSources.Length];
                    VolRawImage = new Image<Gray, float>[VolumeSources.Length];
                    try
                    {
                        StackFiles = DiscoverStack(StackFolder);
                        StackImages = new BitmapSource[StackFiles.Length];
                    }
                    catch { }

                    // Z:\Compressed\cct001\201302\11\cct001_20130211_090244\stack
                    if (File.Exists(PremadeStack + @"\stackimage001.tif"))
                    {
                        LoadKnownStack();
                    }
                    else
                        LoadUnknownStack();

                    ParallelOptions po = new ParallelOptions();
                    po.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 + 1;
                    Parallel.For(0, VolumeSources.Length, po, x => BatchLoadVolume(x));

                    if (stopLoading == true)
                        return;



                }
                catch { }
            }
                );
            t2.Start();
        }

        public void LoadKnownStack()
        {

            StackFiles=   Directory.GetFiles(PremadeStack);

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 + 1;
            Parallel.For(0, StackFiles.Length, po, x => BatchLoadCompressedStack(x));

            StackLoaded = true;
        }
        public void LoadUnknownStack()
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 + 1;
            try
            {
                Utilities.StackReportReader srr = new Utilities.StackReportReader(StackReportFilePath);

                StackROI = srr.GetLocations();
            }
            catch { }

            if (stopLoading == true)
                return;

            if (StackROI == null || StackROI.Length < 5)
            {
                try
                {

                    Utilities.xmlFileReader infoReader = new Utilities.xmlFileReader(InfoFileFilePath);
                    int x = int.Parse(infoReader.GetNode("SPECIMEN/CellXPos"));
                    int y = int.Parse(infoReader.GetNode("SPECIMEN/CellYPos"));
                    int w = int.Parse(infoReader.GetNode("SPECIMEN/BoxWidth"));
                    int h = int.Parse(infoReader.GetNode("SPECIMEN/BoxHeight"));

                    StackROI = new Rectangle[StackFiles.Length];

                    Rectangle rect = new Rectangle(x - 400, y - 400, 800, 800);
                    if (rect.X < 0) rect.X = 0;
                    if (rect.Y < 0) rect.Y = 0;
                    if (rect.Left > 1600) rect.Width = 1600 - rect.X;
                    if (rect.Bottom > 800) rect.Height = 800 - rect.Y;

                    for (int i = 0; i < StackFiles.Length; i++)
                    {
                        StackROI[i] = rect;
                    }

                }
                catch
                {
                    Thread.Sleep(100);
                }

            }
            if (stopLoading == true)
                return;

            Parallel.For(0, StackFiles.Length, po, x => BatchLoadStack(x));

            StackLoaded = true;

        }

        public void WaitForStack()
        {
            while (StackLoaded == false && StillLoading == true && stopLoading == false)
                Thread.Sleep(100);
        }

        public Bitmap Background
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\background.tif";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }
        public Bitmap PP1
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection1.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }
        public Bitmap PP2
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection2.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }
        public Bitmap PP3
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\projection3.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
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


        public BitmapSource[] GetExampleStacks()
        {
            while (StackFiles == null || StackROI == null)
                Thread.Sleep(10);

            BitmapSource[] examples = new BitmapSource[3];

            try
            {
                if (StackImages != null && StackImages[(int)(StackFiles.Length / 2)] == null)
                    BatchLoadStack((int)(StackFiles.Length / 2));

                examples[0] = StackImages[(int)(StackFiles.Length / 2)];
            }
            catch { }


            try
            {

                if (StackImages != null && StackImages[(int)(StackFiles.Length / 2) - 20] == null)
                    BatchLoadStack((int)(StackFiles.Length / 2) - 20);

                examples[1] = StackImages[(int)(StackFiles.Length / 2) - 20];
            }
            catch { }

            try
            {

                if (StackImages != null && StackImages[(int)(StackFiles.Length / 2) + 20] == null)
                    BatchLoadStack((int)(StackFiles.Length / 2) + 20);

                examples[2] = StackImages[(int)(StackFiles.Length / 2) + 20];
            }
            catch { }

            return examples;
        }


        public Bitmap Stack_Image
        {
            get
            {
                string exampleImage = TopDirectory + @"\data\CrossSections_Y___TIK.jpg";

                if (File.Exists(exampleImage))
                {
                    // ImageSource image = BitmapFrame.Create(new Uri(exampleImage, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    // image.Freeze();
                    // return image;
                    return new Bitmap(exampleImage);
                }
                else
                    return null;
            }
        }

    }
}
