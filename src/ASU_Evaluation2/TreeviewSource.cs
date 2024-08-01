using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Threading;

namespace ASU_Evaluation2
{

    public class TreeviewSource
    {


        /// <summary>
        /// this will return a list of machine or a list of dataset depending on the folder
        /// </summary>
        /// <param name="topFolder"></param>
        /// <returns></returns>
        public static object GetTreeview(string topFolder)
        {
            string[] dirs = Directory.GetDirectories(topFolder);
            List<Machine> Machines = new List<Machine>();
            bool Unorganized = true;
            for (int i = 0; i < dirs.Length; i++)
            {
                string dirname = Path.GetFileName(dirs[i]);
                if (dirname.Length == 6 && dirname.ToLower().StartsWith("cct") == true)
                {
                    Machine m = new Machine(dirname, dirs[i]);
                    Machines.Add(m);
                    Unorganized = false;
                }
            }

            if (Unorganized)
            {
                return GetTreeview(dirs, ViewTypes.ListView);

                Dictionary<string, List<string>> MachineNames = new Dictionary<string, List<string>>();
                for (int i = 0; i < dirs.Length; i++)
                {
                    string dirname = Path.GetFileName(dirs[i]);
                    if (dirname.ToLower().StartsWith("cct") == true)
                    {
                        string machineName = dirname.Substring(0, 6);
                        if (MachineNames.ContainsKey(machineName))
                        {
                            List<string> ddirs = MachineNames[machineName];
                            ddirs.Add(dirs[i]);
                        }
                        else
                        {
                            List<string> ddirs = new List<string>();
                            ddirs.Add(dirs[i]);
                            MachineNames.Add(machineName, ddirs);

                        }

                    }
                }

                foreach (KeyValuePair<string, List<string>> kvp in MachineNames)
                {
                    Machine m = new Machine(kvp.Key, kvp.Value.ToArray());
                    Machines.Add(m);
                }
            }
            return Machines;
        }

        public enum ViewTypes
        {
            Treeview, ListView

        }
        public static object GetTreeview(string[] topFolder, ViewTypes viewType)
        {
            string[] dirs = topFolder;

            if (viewType == ViewTypes.Treeview)
            {
                List<Machine> Machines = new List<Machine>();

                {
                    Dictionary<string, List<string>> MachineNames = new Dictionary<string, List<string>>();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        string dirname = Path.GetFileName(dirs[i]);
                        if (dirname.ToLower().StartsWith("cct") == true)
                        {
                            string machineName = dirname.Substring(0, 6);
                            if (MachineNames.ContainsKey(machineName))
                            {
                                List<string> ddirs = MachineNames[machineName];
                                ddirs.Add(dirs[i]);
                            }
                            else
                            {
                                List<string> ddirs = new List<string>();
                                ddirs.Add(dirs[i]);
                                MachineNames.Add(machineName, ddirs);

                            }

                        }
                    }

                    foreach (KeyValuePair<string, List<string>> kvp in MachineNames)
                    {
                        Machine m = new Machine(kvp.Key, kvp.Value.ToArray());
                        Machines.Add(m);
                    }
                }
                return Machines;
            }
            else
            {
                List<Day> Days = new List<Day>();

                for (int i = 0; i < topFolder.Length; i += 100)
                {
                    List<string> datasets = new List<string>();
                    for (int j = i; j < i + 100 && j < topFolder.Length; j++)
                        datasets.Add(topFolder[j]);
                    Day d = new Day((i).ToString() + "-" + (i + 100).ToString(), datasets.ToArray());
                    Days.Add(d);
                }
                return Days;
            }
        }
    }

    public class Machine
    {
        private List<Year> _Years = null;
        public List<Year> Years
        {
            get
            {
                if (_Years == null)
                {
                    string[] dirs = Directory.GetDirectories(TopFolder);
                    Dictionary<string, List<string>> byYear = new Dictionary<string, List<string>>();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        string folder = Path.GetFileName(dirs[i]);
                        string year = folder.Substring(0, 4);
                        if (byYear.ContainsKey(year) == false)
                        {
                            byYear.Add(year, new List<string>());
                        }
                        byYear[year].Add(dirs[i]);
                    }

                    _Years = new List<Year>();
                    foreach (KeyValuePair<string, List<string>> kvp in byYear)
                    {
                        _Years.Add(new Year(kvp.Key, kvp.Value.ToArray()));
                    }
                }

                return _Years;
            }

        }
        public string Name { get; private set; }
        string TopFolder;
        public Machine(string name, string topDirectory)
        {
            if (Directory.GetDirectories(topDirectory)[0] == "Absorption")
                topDirectory += "\\Absorption";

            TopFolder = topDirectory;
            Name = name;

        }


        public Machine(string name, string[] directories)
        {
            TopFolder = "";
            Name = name;


            _Years = new List<Year>();
            Dictionary<string, List<string>> byYear = new Dictionary<string, List<string>>();
            for (int i = 0; i < directories.Length; i++)
            {
                string dirName = "";
                string[] parts = null;

                string year = "";
                dirName = Path.GetFileNameWithoutExtension(directories[i]);
                //return dirName;
                parts = dirName.Split('_');

                year = parts[1].Substring(0, 4);

                if (byYear.ContainsKey(year) == false)
                {
                    byYear.Add(year, new List<string>());
                }
                byYear[year].Add(directories[i]);
            }


            foreach (KeyValuePair<string, List<string>> kvp in byYear)
            {
                _Years.Add(new Year(kvp.Key, kvp.Value.ToArray(), true));
            }
        }
    }

    public class Year
    {
        private List<Month> _Months = null;
        public List<Month> Months
        {
            get
            {

                if (_Months == null)
                {
                    List<Month> months = new List<Month>();
                    for (int i = 0; i < TopDirectory.Length; i++)
                    {
                        string dirname = Path.GetFileName(TopDirectory[i]);
                        if (dirname.Length == 6)
                        {
                            months.Add(new Month(dirname.Substring(4, dirname.Length - 4), TopDirectory[i]));
                        }

                    }
                    _Months = months;
                }
                return _Months;
            }

        }

        public string Name { get; private set; }

        private string[] TopDirectory;

        public Year(string name, string[] topDirectory, bool isFolderLoad = false)
        {
            Name = name;
            TopDirectory = topDirectory;


            if (isFolderLoad)
            {
                _Months = new List<Month>();
                Dictionary<string, List<string>> byMonth = new Dictionary<string, List<string>>();
                for (int i = 0; i < topDirectory.Length; i++)
                {
                    string dirName = "";
                    string[] parts = null;

                    string month = "";
                    dirName = Path.GetFileNameWithoutExtension(topDirectory[i]);
                    //return dirName;
                    parts = dirName.Split('_');

                    month = parts[1].Substring(4, 2);

                    if (byMonth.ContainsKey(month) == false)
                    {
                        byMonth.Add(month, new List<string>());
                    }
                    byMonth[month].Add(topDirectory[i]);
                }


                foreach (KeyValuePair<string, List<string>> kvp in byMonth)
                {
                    _Months.Add(new Month(kvp.Key, kvp.Value.ToArray()));
                }
            }
        }




    }

    public class Month
    {

        private List<Day> _Days;
        public List<Day> Days
        {
            get
            {
                if (_Days == null)
                {
                    string[] dirs = Directory.GetDirectories(TopDirectory);

                    _Days = new List<Day>();

                    foreach (string d in dirs)
                    {
                        _Days.Add(new Day(Path.GetFileName(d), d));
                    }
                }
                return _Days;
            }

        }
        public string Name { get; private set; }

        string TopDirectory;
        public Month(string name, string topDirectory)
        {
            Name = name;
            TopDirectory = topDirectory;
        }

        public Month(string name, string[] topDirectory)
        {
            Name = name;

            _Days = new List<Day>();
            Dictionary<string, List<string>> byDay = new Dictionary<string, List<string>>();
            for (int i = 0; i < topDirectory.Length; i++)
            {
                string dirName = "";
                string[] parts = null;

                string day = "";
                dirName = Path.GetFileNameWithoutExtension(topDirectory[i]);
                //return dirName;
                parts = dirName.Split('_');

                day = parts[1].Substring(6, 2);

                if (byDay.ContainsKey(day) == false)
                {
                    byDay.Add(day, new List<string>());
                }
                byDay[day].Add(topDirectory[i]);
            }


            foreach (KeyValuePair<string, List<string>> kvp in byDay)
            {
                _Days.Add(new Day(kvp.Key, kvp.Value.ToArray()));
            }
        }

    }

    public class Day
    {
        //private List<Dataset> _DataSets;
        ObservableCollection<Dataset> _DataSets;
        public ObservableCollection<Dataset> DataSets
        {
            get
            {
                if (_DataSets == null)
                {
                    _DataSets = new ObservableCollection<Dataset>();
                    _DataSets.Add(new Dataset("empty", ""));
                   // AddDataSets();
                }
                else if (_DataSets.Count == 1)
                {
                    _DataSets.Clear();
                    AddDataSets();
                }
                return _DataSets;
            }
        }


        private void AddDataSets()
        {
            Thread t = new Thread(delegate()
           {
               try
               {
                   string[] dirs = Directory.GetDirectories(TopDirectory);
                   System.Windows.Threading.Dispatcher disp = CellSelection.GetDispatcher();
                   foreach (string d in dirs)
                   {
                       try
                       {
                           Dataset ds = new Dataset(Path.GetFileName(d), d);
                           bool tt = ds.Cell_Good;
                           disp.BeginInvoke(new AddaDataset_Del(AddaDataset), ds);
                           Thread.Sleep(300);
                       }
                       catch { }
                   }
               }
               catch { }
           }
             );

            t.Start();
        }

        private delegate void AddaDataset_Del(Dataset ds);
        private void AddaDataset(Dataset ds)
        {
            _DataSets.Add(ds);
        }


        public string Name { get; private set; }

        private string TopDirectory;
        public Day(string name, string topDirectory)
        {
            Name = name;
            TopDirectory = topDirectory;
        }

        public Day(string name, string[] topDirectory)
        {
            Name = name;
            _DataSets = new ObservableCollection<Dataset>();
            System.Windows.Threading.Dispatcher disp = CellSelection.GetDispatcher();
            foreach (string d in topDirectory)
            {
                try
                {
                    disp.BeginInvoke(new AddaDataset_Del(AddaDataset), new Dataset(Path.GetFileName(d), d));
                }
                catch { }
                //Thread.Sleep(300);
            }
        }
    }






    //<HierarchicalDataTemplate DataType="{x:Type local:Machine}" ItemsSource="{Binding Years}">
    //                 <TextBlock Text="{Binding Path=Name}" />
    //             </HierarchicalDataTemplate>

    //         <HierarchicalDataTemplate DataType="{x:Type local:Year}" ItemsSource="{Binding Months}">
    //             <TextBlock Text="{Binding Path=Name}" />
    //         </HierarchicalDataTemplate>
    //         <HierarchicalDataTemplate DataType="{x:Type local:Month}" ItemsSource="{Binding Days}">
    //             <TextBlock Text="{Binding Path=Name}" />
    //         </HierarchicalDataTemplate>
    //         <DataTemplate DataType="{x:Type local:Dataset}" >
    //                 <Grid Margin="4">
    //                     <Grid.ColumnDefinitions>
    //                         <ColumnDefinition Width="240*" />
    //                         <ColumnDefinition Width="103*" />
    //                     </Grid.ColumnDefinitions>
    //                     <Grid.RowDefinitions>
    //                         <RowDefinition Height="30*" />
    //                         <RowDefinition Height="303*" />

    //                     </Grid.RowDefinitions>
    //                     <Label Content="{Binding DatasetName}" FontWeight="Bold"  />
    //                     <Label Content="{Binding Date}" Grid.Row="0" Grid.Column="1"   Name="lbDate"  />
    //                     <Image Grid.Row="2"  Margin="3,3" Source="{Binding ExampleImage}" VerticalAlignment="Center" HorizontalAlignment="Center" MinWidth="100" MinHeight="100"/>
    //                     <StackPanel Grid.Column="1" Grid.Row="1"   Margin="3,3" Name="stackPanel1" >
    //                         <ToggleButton Name="Recon_Succeed" Content="Recon Succeded" IsChecked="{Binding Recon_Succeeded}"/>
    //                         <ToggleButton Name="Cell_Good" Content="Cell Good" IsChecked="{Binding Cell_Good}"/>
    //                         <ToggleButton Name="Recon_Good" Content="Recon Good" IsChecked="{Binding Recon_Good}"/>
    //                     </StackPanel>

}
