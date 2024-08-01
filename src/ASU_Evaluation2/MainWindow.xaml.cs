using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.Themes;
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace ASU_Evaluation2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string theme = e.AddedItems[0].ToString();

                // Window Level
                // this.ApplyTheme(theme);

                //Application Level
                Application.Current.ApplyTheme(theme);
            }
        }

        CellSelection CellSelector;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.ApplyTheme("ExpressionDark");
            SignIn si = new SignIn();
            si.ShowDialog();

            try
            {
                //Fixes.CreateBetterDatabase(DataStore.DataFolder + @"\Eval_Experimental7.sqlite");
                // Fixes.RecoverAllData(DataStore.DataFolder);
                // Fixes.UpgradeData();
                // Fixes.CopyData();
                // MessageBox.Show("Done");
                // Environment.Exit(0);

                //   return;
                //   Fixes.AddClipped(@"E:\cct001");
                //   Fixes.AddClipped(@"E:\cct002");
                //   Fixes.AddClipped(@"E:\cct004");

                // Fixes.SaveCellTypes();

                DataStore.OpenDatabase(System.IO.Path.Combine(DataStore.DataDrive, @"ASU_Recon\Eval_View3.sqlite"));
                // Fixes.AddClipped();
                //DataStore.RecoverAllData(@"z:\ASU_Recon\cct001");

                // DataStore.UpgradeDataML();
            }
            catch
            {
                System.Diagnostics.Debug.Print("");
            }
            //  DataStore.WriteNewData();

            themes.ItemsSource = ThemeManager.GetThemes();

            //return;
            themes.SelectedIndex = 3;

            timer.Tick += new EventHandler(timer_Tick);

            CellSelector = new CellSelection(this);
            CellSelector.Show();
            // SelectDataset(new Dataset("test", @"z:\ASU_Recon\cct001\201209\19\cct001_20120919_161641"));
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //Thread t = new Thread(delegate()
            // {
            try
            {
                // if (CenteringMovie != null && tabItem2.IsSelected)
                {
                    try
                    {
                        if (CenteringMovie != null)
                        {
                            System.Drawing.Bitmap b = CenteringMovie.QueryFrame().ToBitmap();
                            this.Dispatcher.Invoke((Action)(() => Centering.Source = b.CreateBitmapSourceFromBitmap()));
                        }
                    }
                    catch
                    {
                        if (CenteringMovie != null)
                            CenteringMovie.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                    }

                }
                if (KeepLoading == false)
                    return;
                // if (MIPMovie != null && tabItem1.IsSelected)
                {
                    try
                    {
                        if (MIPMovie != null)
                            this.Dispatcher.Invoke((Action)(() => MIP_Image.Source = MIPMovie.QueryFrame().ToBitmap().CreateBitmapSourceFromBitmap()));
                    }
                    catch
                    {
                        if (MIPMovie != null)
                            MIPMovie.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                    }
                }
                if (KeepLoading == false)
                    return;
                // if (currentSet != null && tabItem1.IsSelected)
                {
                    try
                    {
                        if (Fly_Through != null)
                            this.Dispatcher.Invoke((Action)(() => Fly_Through.Source = currentSet.Fly_Through));
                    }
                    catch { }
                }
            }
            catch { }
            // });
            //  t.Start();
        }

        DispatcherTimer timer = new DispatcherTimer();
        Dataset currentSet = null;
        Emgu.CV.Capture CenteringMovie = null;
        Emgu.CV.Capture MIPMovie = null;


        public void SelectDataset(Dataset dataSet)
        {
            try
            {

                if (MIPMovie != null)
                {
                    var t = MIPMovie;
                    MIPMovie = null;
                    t.Dispose();
                    t = null;

                }
                if (CenteringMovie != null)
                {
                    var t = CenteringMovie;
                    CenteringMovie = null;
                    t.Dispose();
                    t = null;

                }
                if (currentSet != null)
                {
                    currentSet.ClearMemory();
                }

                currentSet = dataSet;

                //dataSet  
                this.DataContext = dataSet;

                // MIP_Image.Source = new Uri( currentSet.MIP_Image,UriKind.Absolute);


                ThreadLoadImages();


                timer.Interval = new TimeSpan(1000 * 200);
            }
            catch { }
        }


       
        private void DoLoading()
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() => Axial_Image.Source = currentSet.Axial_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {

                this.Dispatcher.Invoke((Action)(() => Sag_Image.Source = currentSet.Sag_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Z_Image.Source = currentSet.Z_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Background.Source = currentSet.Background.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {

                this.Dispatcher.Invoke((Action)(() => MIP_Image.Source = currentSet.Axial_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Fly_Through.Source = currentSet.Axial_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {

                if (MIPMovie != null)
                {
                    MIPMovie.Dispose();
                }

                string mip = currentSet.MIP_Image;
                if (File.Exists(mip))
                    MIPMovie = new Emgu.CV.Capture(mip);
                else
                    MIPMovie = null;
                Thread.Sleep(20);

            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                timer.Start();
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {

                currentSet.StartLoading();
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Mask3D.Source = currentSet.MaskImage3D.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            try
            {
                this.Dispatcher.Invoke((Action)(() => UnMask. Source = currentSet.UnMaskImage3D.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }

            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Mask10.Source = currentSet.MaskImage10.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Mask40.Source = currentSet.MaskImage40.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            

            if (KeepLoading == false)
                return;
            try
            {

                if (CenteringMovie != null)
                {
                    CenteringMovie.Dispose();
                }
                string centering = currentSet.Centering;
                if (centering != "")
                    CenteringMovie = new Emgu.CV.Capture(centering);
                else
                    CenteringMovie = null;
                Thread.Sleep(20);

            }
            catch { }
            
            if (KeepLoading == false)
                return;
            try
            {

                this.Dispatcher.Invoke((Action)(() => PP1.Source = currentSet.PP1.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => PP2.Source = currentSet.PP2.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => PP3.Source = currentSet.PP3.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Centering.Source = currentSet.Axial_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;
            try
            {
                this.Dispatcher.Invoke((Action)(() => Stack_Image.Source = currentSet.Axial_Image.CreateBitmapSourceFromBitmap()));
                Thread.Sleep(20);
            }
            catch { }
            if (KeepLoading == false)
                return;

            try
            {
                currentSet.WaitForStack();
                BitmapSource[] exampleStacks = currentSet.GetExampleStacks();

                this.Dispatcher.Invoke((Action)(() => Stack0.Source = exampleStacks[0]));
                this.Dispatcher.Invoke((Action)(() => Stack1.Source = exampleStacks[1]));
                this.Dispatcher.Invoke((Action)(() => Stack2.Source = exampleStacks[2]));
            }
            catch { }
           
        }

        private bool KeepLoading = true;
        private Thread ThreadImageLoader = null, ThreadStackLoader = null;
        private void ThreadLoadImages()
        {
            // if (ThreadImageLoader == null && ThreadImageLoader.IsAlive == false)
            {
                Thread ThreadImageLoader = new Thread(delegate()
                     {
                         try
                         {
                             KeepLoading = false;
                             Thread.Sleep(100);
                             KeepLoading = true;
                             DoLoading();
                         }
                         catch { }
                     }
                 );

                ThreadImageLoader.Start();
            }



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                CellSelector.Close();

            }
            catch { }
            if (currentSet != null)
                currentSet.ClearMemory();
            DataStore.Close();
        }

        private void tabItem3_GotFocus(object sender, RoutedEventArgs e)
        {
            StackZPosition.Maximum = currentSet.NumberStackImages - 3;
            StackZPosition.Value = currentSet.NumberStackImages / 2;
            Stack_Image.Source = currentSet.GetStackImage((int)StackZPosition.Value);
        }

        private void StackZPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Stack_Image.Source = currentSet.GetStackImage((int)e.NewValue);
        }

        private void tbGoodCell_Click(object sender, RoutedEventArgs e)
        {
            //SelectDataset(new Dataset("test", @"z:\ASU_Recon\cct001\201209\19\cct001_20120919_161641"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }



        private void tabItem4_GotFocus(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            OrthoViewer.OrthoFrame of = new OrthoViewer.OrthoFrame();
            of.Show();
            of.DisplayData(currentSet.VolumeStack);
        }
    }
}
