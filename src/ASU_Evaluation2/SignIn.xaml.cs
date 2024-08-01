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
using System.Windows.Shapes;
using System.IO;

namespace ASU_Evaluation2
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        public SignIn()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] drives = System.IO.DriveInfo.GetDrives();

            for (int i = 0; i < drives.Length; i++)
            {
                try
                {
                    if (drives[i].VolumeLabel == "BDA_CELLCT")
                    {
                        tbDataFolder.Text = drives[i].Name + "ASU_Recon";
                        DataStore.DataDrive = drives[i].Name;
                    }

                    if (drives[i].VolumeLabel.ToLower() .Contains("processed"))
                        tbProcessed.Text = drives[i].Name;

                    if (drives[i].VolumeLabel.ToLower() .Contains("storage"))
                        tbStorage.Text = drives[i].Name;

                    if (drives[i].VolumeLabel.ToLower().Contains("doppel"))
                        tbColdStorage.Text = drives[i].Name;

                    if (drives[i].VolumeLabel.ToLower().Contains("nas-03"))
                        tbColdStorage2.Text = drives[i].Name;


                    System.Diagnostics.Debug.Print(drives[i].VolumeLabel);
                }
                catch { }
            }

          
        }

        private void bDone_Click(object sender, RoutedEventArgs e)
        {
            DataStore.User = tbUserName.Text;
            DataStore.DataFolder = tbDataFolder.Text;
            DataStore.ProcessedDrive = tbProcessed.Text;
            DataStore.StorageDrive = tbStorage.Text;
            DataStore.ColdStorageDrive = tbColdStorage.Text;
            DataStore.ColdStorageDrive2 = tbColdStorage2.Text;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataStore.User = tbUserName.Text;
            DataStore.DataFolder = tbDataFolder.Text;
            DataStore.ProcessedDrive = tbProcessed.Text;
            DataStore.StorageDrive = tbStorage.Text;
            DataStore.ColdStorageDrive = tbColdStorage.Text;
            DataStore.ColdStorageDrive2 = tbColdStorage2.Text;
        }
    }
}
