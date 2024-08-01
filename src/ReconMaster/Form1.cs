using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ReconstructCells.Tools;

namespace ReconMaster
{
    public partial class Form1 : Form
    {
        private bool bPause = false;

        public Form1()
        {
            InitializeComponent();
        }
        /*
         * 
         * */

        #region Folders
        string Folders =
@"";

        #endregion

        private bool Rerun = false;
        private void Review_Click(object sender, EventArgs e)
        {
            string DirPath = @"z:\Raw PP\cct001\Absorption";

            string Folders2 = Folders + "\n" + rtbFolders.Text;
            string[] lines = Folders2.Split(new string[] { "\n", " ", "\t", "\r", "," }, StringSplitOptions.RemoveEmptyEntries);
            DateTime dates;// = new List<DateTime>();
            List<string> Dirs = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    dates = new DateTime(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]));
                    Dirs.Add(DirPath + string.Format("\\{0}{1:00}\\{2:00}", dates.Year, dates.Month, dates.Day));
                }
                catch { }
            }
            Rerun = true;

            //  Dirs.Sort();
            foreach (string s in Dirs)
            {
                try
                {
                    RunMonth(s, "");
                    if (formClosing == true)
                        return;
                }
                catch { }
            }

            Rerun = false;
            nProcesses.Value = 1;
            RunEverything_Click(this, EventArgs.Empty);
        }

        private void RunEverything_Click(object sender, EventArgs e)
        {
            // string DirPath = @"z:\Raw PP\cct001\Absorption";
            //  string DirPath = @"z:\Raw PP\cct002";
            // string DirPath = @"z:\Raw PP\cct001\Absorption\201208";
            // string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001";

            //  string DirPath = @"z:\Raw PP\cct001\Absorption\201302";
            //  string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001";
            //  string DirPath = @"X:\Rejected_Datasets";
            // string DirPath = @"z:\asu_recon\viveks_RT9";
            // string DirPath = @"S:\Research\Brian\Test cells for Recon\RawPP";
            // string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001\201303";
            //  RunMonth(DirPath, "");
            // Reruns();
            //return;
            string DirPath = @"z:\Raw PP\cct001\Absorption";
            //   DirPath = @"X:\Rejected_Datasets";
            //   RunMonth(DirPath); 

            // //string DirPath = @"Z:\";

            List<string> Dirs = new List<string>(Directory.GetDirectories(DirPath));

            Dirs.Sort();
            Dirs.Reverse();
            for (int i = 0; i < Dirs.Count; i++)
            {
                List<string> subdirs = new List<string>(Directory.GetDirectories(Dirs[i]));
                subdirs.Reverse();
                foreach (string s in subdirs)
                {
                    try
                    {
                        // if (s.Contains ("201202\\23") ==true)
                        //  if (s.Contains(@"201209") == true)
                        //string s2 = Path.GetDirectoryName(s);
                        //  int dt =int.Parse(s.Substring(s.Length-9,6))*100 + int.Parse( s.Substring(s.Length - 2));
                        // if (dt < 20130629)
                        {
                            int Day = int.Parse(s.Substring(s.Length - 2));
                            //  if (Day <= 15)
                            RunMonth(s, "");
                        }
                        if (formClosing == true)
                            return;
                    }
                    catch { }
                }
            }
        }

        Queue<Tuple<string, string>> SelectedHistoric = new Queue<Tuple<string, string>>();
        private class TicketProcess
        {
            public Process process;
            public Stopwatch watch;

            public TicketProcess(Process process)
            {
                this.process = process;
                watch = new Stopwatch();
                watch.Start();
            }

            public void Check(List<TicketProcess> parent)
            {
                if (watch.Elapsed.Minutes > 45)
                {
                    process.Kill();
                    watch.Stop();
                    process = null;
                    parent.Remove(this);
                }

                if (process.HasExited)
                {
                    process = null;
                    watch.Stop();
                    watch = null;
                    parent.Remove(this);
                }
            }
        }
        private List<TicketProcess> LeftOvers = new List<TicketProcess>();
        private void RunMonth(string MonthFolder, string arguments)
        {
            string STorage = MonthFolder;

            //List<string> DirAdds = 
            List<string> BadDirs = new List<string>();
            List<string> GoodDirs = new List<string>();
            bool FirstDir = true;
            string[] AllDirs;

            BadDirs.Add(STorage);
            while (FirstDir == true && BadDirs.Count > 0)
            {
                try
                {
                    string[] Dirs = Directory.GetDirectories(BadDirs[0]);
                    if (Dirs.Length > 0)
                    {

                        if (Dirs[0].Contains("cct") == true && Path.GetFileName(Dirs[0]).Length > 6)
                            GoodDirs.AddRange(Dirs);
                        else
                            BadDirs.AddRange(Dirs);
                    }
                }
                catch { }
                BadDirs.RemoveAt(0);
                Application.DoEvents();
            }

            AllDirs = GoodDirs.ToArray();
            string pPath;
            Queue<Tuple<string, string>> Selected = new Queue<Tuple<string, string>>();

            for (int i = 0; i < AllDirs.Length; i++)
            {
                try
                {
                    pPath = AllDirs[i].Replace("\"", "").Replace("'", "");

                    if (pPath.EndsWith("\\") == false)
                        pPath += "\\";

                    string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));
                    string[] parts = dirName.Split('_');
                    string Prefix = parts[0];
                    string Year = parts[1].Substring(0, 4);
                    string month = parts[1].Substring(4, 2);
                    string day = parts[1].Substring(6, 2);

                    string basePath;
                    basePath = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

                    string labelPath = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

                    //   basePath = Path.Combine(@"z:\asu_recon\sR4", dirName);
                    // basePath = @"c:\temp\testbad";
                    // if (dirName.Contains("0114")==true)
                    if (Directory.Exists(basePath + @"\data\projectionobject_2_TIK_16") == false)
                    {
                        Selected.Enqueue(new Tuple<string, string>(AllDirs[i], basePath));
                        SelectedHistoric.Enqueue(new Tuple<string, string>(AllDirs[i], basePath));
                    }
                    Application.DoEvents();
                }
                catch { }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.Minutes < 45)
            {
                Thread.Sleep(100);
                Application.DoEvents();

                if (LeftOvers.Count < 1)
                    return;

            }
            return;

            Process[] allProcesses = new Process[10];
            System.Diagnostics.Stopwatch[] watches = new Stopwatch[10];
            string s = Application.ExecutablePath;
            System.Diagnostics.Debug.Print(s);
            bool started = false;
            Random rnd2 = new Random();
            while (Selected.Count > 0)
            {
                for (int i = 0; i < nProcesses.Value; i++)
                {
                    if (allProcesses[i] == null || allProcesses[i].HasExited == true || (watches[i] != null && watches[i].Elapsed.Minutes > 30))
                    {
                        if (Selected.Count > 0)
                        {
                            try
                            {
                                Tuple<string, string> folders = Selected.Dequeue();
                                string folderExperiment = folders.Item1;
                                string folderData = folders.Item2;

                                bool folderExists = Directory.Exists(folderData);
                                bool TikExists = File.Exists(folderData + "\\data\\CrossSections_Z_2_TIK.jpg");
                                bool dataExists = Directory.Exists(folderExperiment + "\\pp") && Directory.GetFiles(folderExperiment + "\\pp").Length == 500;

                                string dirName = Path.GetFileNameWithoutExtension(folderExperiment);
                                string[] parts = dirName.Split('_');
                                string Prefix = parts[0];
                                string Year = parts[1].Substring(0, 4);
                                string month = parts[1].Substring(4, 2);
                                string day = parts[1].Substring(6, 2);

                                string basePath;
                                basePath = Path.Combine(@"e:\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

                                this.Text = dirName;
                                // if (Directory .Exists( basePath + "\\clippedPP")==false )
                                // if (Directory.Exists(folderData) == false )
                                // if (Rerun || ( ((folderExists == false || (folderExists == true && TikExists == false)) && dataExists )))
                                if (TikExists == false)
                                {

                                    //for (int j = 0; j < 12; j++)
                                    //{

                                    //    Application.DoEvents();
                                    //}

                                    //Process ScriptRunner = new Process();
                                    //ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    //ScriptRunner.StartInfo.FileName = @"ReconstructCells.exe";
                                    //ScriptRunner.StartInfo.Arguments = ("\"" + folderExperiment + "\" \"" + folderData + "\" \"RamLak\" \"512\" " + arguments).Trim();
                                    //ScriptRunner.Start();



                                    Process ScriptRunner = new Process();
                                    ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
                                    ScriptRunner.StartInfo.FileName = @"matlab.exe";
                                    ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"experimentFolder='" + folderExperiment + "';dataFolder='" + folderData + "';run('c:\\matlab\\DoReconstruction.m');exit;\"";
                                    ScriptRunner.Start();


                                    allProcesses[i] = ScriptRunner;
                                    watches[i] = new Stopwatch();
                                    watches[i].Start();
                                    this.Text = Path.GetFileNameWithoutExtension(folderExperiment);
                                    started = true;
                                    // ScriptRunner.WaitForExit();
                                    Stopwatch sleepwatch = new Stopwatch();
                                    sleepwatch.Start();
                                    while (sleepwatch.Elapsed.TotalSeconds < 60)
                                    {
                                        Thread.Sleep((int)(100));
                                        Application.DoEvents();
                                    }

                                    for (int j = 0; j < LeftOvers.Count; j++)
                                        LeftOvers[j].Check(LeftOvers);
                                }
                            }
                            catch { }
                        }
                    }


                    Application.DoEvents();
                    if (started)
                    {
                        while (bPause)
                            Application.DoEvents();

                        Thread.Sleep(200);
                    }
                    if (this.IsDisposed == true)
                        return;
                    if (formClosing == true)
                        return;
                }

            }

            for (int i = 0; i < allProcesses.Length; i++)
            {
                if (allProcesses[i] != null && allProcesses[i].HasExited == false)
                    LeftOvers.Add(new TicketProcess(allProcesses[i]));
            }


            if (LeftOvers.Count > 3)
            {

                sw = new Stopwatch();
                sw.Start();
                bool stillRunning = true;
                while (sw.Elapsed.Minutes < 30 && stillRunning == true)
                {
                    for (int i = 0; i < LeftOvers.Count; i++)
                    {
                        if (LeftOvers[i].process.HasExited == false)
                            stillRunning = true;
                        if (formClosing == true)
                            return;
                    }
                    Thread.Sleep(100);
                }
                for (int i = 0; i < LeftOvers.Count; i++)
                {
                    if (LeftOvers[i].process.HasExited == false)
                        LeftOvers[i].process.Kill();
                }
                LeftOvers = new List<TicketProcess>();
            }
            //bool processedExited = false;
            //Stopwatch sw2 = new Stopwatch();
            //sw2.Start();
            //while (processedExited == false && sw2.Elapsed.Minutes < 10)
            //{
            //    processedExited = true;
            //    for (int i = 0; i < nProcesses.Value; i++)
            //    {
            //        if (allProcesses[i] != null && allProcesses[i].HasExited == false)
            //        {
            //            processedExited = false;
            //        }
            //    }

            //    Application.DoEvents();
            //    Thread.Sleep(200);

            //    if (formClosing == true)
            //        return;
            //}
        }

        private void Reruns()
        {
            DataStore.OpenDatabase(@"z:\asu_recon\Eval.sqlite");
            Queue<Tuple<string, string>> Selected = DataStore.GetBadRecons();
            RunBad(Selected);

            //DataStore.OpenDatabase(@"z:\asu_recon\Eval.sqlite");
            //Selected = DataStore.GetBadRegisteredRecons();
            //RunBad(Selected);
        }
        private void RunBad(Queue<Tuple<string, string>> Selected)
        {
            Process[] allProcesses = new Process[10];
            System.Diagnostics.Stopwatch[] watches = new Stopwatch[10];
            string s = Application.ExecutablePath;
            System.Diagnostics.Debug.Print(s);
            bool started = false;
            while (Selected.Count > 0)
            {
                try
                {
                    for (int i = 0; i < nProcesses.Value; i++)
                    {
                        if (watches != null && watches[i] != null && watches[i].Elapsed.Minutes > 20)
                        {
                            LeftOvers.Add(new TicketProcess(allProcesses[i]));
                            allProcesses[i] = null;
                        }
                        if (allProcesses[i] == null || allProcesses[i].HasExited == true || (watches[i] != null && watches[i].Elapsed.Minutes > 30))
                        {
                            if (Selected.Count > 0)
                            {
                                Tuple<string, string> folders = Selected.Dequeue();
                                string folderExperiment = folders.Item1;
                                string folderData = folders.Item2;
                                {
                                    Process ScriptRunner = new Process();
                                    ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    ScriptRunner.StartInfo.FileName = @"ReconstructCells.exe";
                                    ScriptRunner.StartInfo.Arguments = ("\"" + folderExperiment + "\" \"" + folderData + "\" \"RamLak\" \"512\"").Trim();
                                    ScriptRunner.Start();
                                    allProcesses[i] = ScriptRunner;
                                    watches[i] = new Stopwatch();
                                    watches[i].Start();
                                    this.Text = Path.GetFileNameWithoutExtension(folderExperiment);
                                    started = true;
                                    // ScriptRunner.WaitForExit();


                                    for (int j = 0; j < LeftOvers.Count; j++)
                                        LeftOvers[j].Check(LeftOvers);
                                }
                            }
                        }


                        Application.DoEvents();
                        if (started)
                        {
                            while (bPause)
                                Application.DoEvents();

                            Thread.Sleep(200);
                        }
                        if (this.IsDisposed == true)
                            return;
                        if (formClosing == true)
                            return;
                    }
                }
                catch { }
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            bPause = !bPause;
        }

        private void bBrowse1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            tbWatchFolder1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void bBrowse2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            tbWatchFolder2.Text = folderBrowserDialog1.SelectedPath;
        }

        //
        List<String> Processing = new List<string>();
        List<Tuple<DateTime, string, string>> AlreadyProcessed = new List<Tuple<DateTime, string, string>>();
        Queue<Tuple<DateTime, string, string>> SelectedNetwork = new Queue<Tuple<DateTime, string, string>>();

        private void bWatch_Click(object sender, EventArgs e)
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
            LaunchProcessor();

            return;
            List<String> ExistingFolders = new List<string>();

            if (tbWatchFolder1.Text != "")
            {
                string[] dirs = Directory.GetDirectories(tbWatchFolder1.Text, "cct*.*", SearchOption.AllDirectories);
                ExistingFolders.AddRange(dirs);
            }
            if (tbWatchFolder2.Text != "")
            {
                ExistingFolders.AddRange(Directory.GetDirectories(tbWatchFolder2.Text, "cct*.*", SearchOption.AllDirectories));
            }


            foreach (string Foldername in ExistingFolders)
            {

                if (Foldername.ToLower().Contains("cct") == true)
                {
                    Processing.Add(Foldername);
                    string pPath = Foldername.Replace("\"", "").Replace("'", "");

                    if (pPath.EndsWith("\\") == false)
                        pPath += "\\";

                    string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));

                    if (dirName.Length > 6)
                    {
                        string[] parts = dirName.Split('_');
                        string Prefix = parts[0];
                        string Year = parts[1].Substring(0, 4);
                        string month = parts[1].Substring(4, 2);
                        string day = parts[1].Substring(6, 2);

                        string basePath;
                        basePath = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

                        DateTime dt = Directory.GetCreationTime(Foldername);
                        SelectedNetwork.Enqueue(new Tuple<DateTime, string, string>(dt, Foldername, basePath));
                    }
                }
            }


        }

        private delegate void SetTextEvent(string text);
        private void SetText(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextEvent(SetText), text);

            }
            else
            {
                this.Text = text;

            }


        }

        private void LaunchProcessor()
        {

            Thread t = new Thread(delegate()
                {
                    while (true)
                    {
                        try
                        {
                            int numRunning = Directory.GetFiles(@"C:\temp\running").Length;
                            if ((numRunning < nProcesses.Value + 1) && (LeftOvers.Count < (nProcesses.Value + 1) && (SelectedNetwork.Count > 0 || SelectedHistoric.Count > 0)))
                            {
                                Tuple<DateTime, string, string> folders = null;
                                DateTime foundTime;
                                string folderExperiment;
                                string folderData;

                                if (SelectedNetwork.Count > 0)
                                {
                                    folders = SelectedNetwork.Dequeue();
                                    foundTime = folders.Item1;
                                    folderExperiment = folders.Item2;
                                    folderData = folders.Item3;
                                    Console.WriteLine("FolderData Uploading: " + folderData);
                                }
                                else
                                {
                                    var tfolders = SelectedHistoric.Dequeue();
                                    foundTime = DateTime.Now;
                                    folderExperiment = tfolders.Item1;
                                    folderData = tfolders.Item2;
                                }

                                SetText(folderData);
                                Console.WriteLine("Starting FolderData: " + folderData);

                                bool folderExists = Directory.Exists(folderData);
                                bool TikExists = false;// File.Exists(folderData + "\\data\\CrossSections_Z_2_TIK.jpg");
                                bool dataExists = Directory.Exists(folderExperiment + "\\pp") && Directory.GetFiles(folderExperiment + "\\pp").Length == 500;

                                if (TikExists == false && dataExists)
                                {
                                    string dirName = Path.GetFileNameWithoutExtension(folderExperiment);
                                    Console.WriteLine("Processing " + dirName);
                                    Process ScriptRunner = new Process();
                                    ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
                                    ScriptRunner.StartInfo.FileName = @"matlab.exe";
                                    ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"experimentFolder='" + folderExperiment + "';dataFolder='" + folderData + "';DatasetName='" + dirName + "';" + tbShellScript.Text + ";exit;\"";
                                    ScriptRunner.Start();

                                    for (int z = 0; z < 10; z++)
                                        Thread.Sleep(60000);

                                    Stopwatch sw = new Stopwatch();
                                    sw.Start();

                                    LeftOvers.Add(new TicketProcess(ScriptRunner));

                                    if (folders != null)
                                    {
                                        folders = new Tuple<DateTime, string, string>(DateTime.Now, folders.Item2, folders.Item3);
                                        AlreadyProcessed.Add(folders);
                                    }
                                }
                            }
                            else
                                Thread.Sleep(100);

                            if (this.IsDisposed == true)
                                return;
                            if (formClosing == true)
                                return;

                            // Application.DoEvents();
                        }
                        catch { }
                    }
                });
            t.Start();
        }

        bool First = true;
        Thread DirCheck, Remove;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Interval = 10000;
                for (int j = 0; j < LeftOvers.Count; j++)
                    LeftOvers[j].Check(LeftOvers);

                if (DirCheck == null || DirCheck.IsAlive == false)
                {
                    DirCheck = new Thread(delegate()
                   {
                       try
                       {
                           List<string> Folder = new List<string>();
                           if (tbWatchFolder1.Text != "")
                               Folder.AddRange(Directory.GetDirectories(tbWatchFolder1.Text, "cct*.*", SearchOption.AllDirectories));
                           if (tbWatchFolder2.Text != "")
                               Folder.AddRange(Directory.GetDirectories(tbWatchFolder2.Text, "cct*.*", SearchOption.AllDirectories));


                           for (int i = 0; i < Folder.Count; i++)
                           {
                               #region put on todo list
                               if (Folder[i].Contains("cct") == true)
                               {
                                   bool found = false;
                                   for (int j = 0; j < Processing.Count; j++)///why not contains???
                                   {
                                       if (Processing[j] == Folder[i])
                                           found = true;
                                   }

                                   try
                                   {
                                       if (found == false)
                                       {
                                           Processing.Add(Folder[i]);

                                           string Foldername = Folder[i];
                                           string pPath = Foldername.Replace("\"", "").Replace("'", "");

                                           if (pPath.EndsWith("\\") == false)
                                               pPath += "\\";

                                           string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(pPath));
                                           string[] parts = dirName.Split('_');
                                           string Prefix = parts[0];
                                           string Year = parts[1].Substring(0, 4);
                                           string month = parts[1].Substring(4, 2);
                                           string day = parts[1].Substring(6, 2);

                                           string basePath;
                                           basePath = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                                           DateTime dt = Directory.GetCreationTime(Foldername);

                                           string backupFolder = Path.Combine(@"z:\raw pp", Prefix + "\\Absorption\\" + Year + month + "\\" + day + "\\" + dirName + "");

                                           if (First)
                                           {
                                               Thread.Sleep(3 * 60 * 1000);
                                               First = false;
                                           }

                                           //Thread t = new Thread(delegate()
                                           {

                                               try
                                               {
                                                   Console.WriteLine("Transfering : " + Foldername + " to " + backupFolder);
                                                   Utilities.FileUtilities.DoCopyFolder(Foldername, backupFolder);
                                                   Directory.Delete(Foldername, true);
                                               }
                                               catch (Exception ex2)
                                               {
                                                   Console.WriteLine(ex2.Message);
                                                   Console.WriteLine(ex2.StackTrace);
                                                   try
                                                   {
                                                       if (Directory.Exists(Path.GetDirectoryName(backupFolder)) == false)
                                                           Directory.CreateDirectory(Path.GetDirectoryName(backupFolder));

                                                       Directory.Move(Foldername, backupFolder);
                                                   }
                                                   catch (Exception ex)
                                                   {

                                                       Console.WriteLine(ex.Message);
                                                       Console.WriteLine(ex.StackTrace);
                                                   }
                                               }
                                               SelectedNetwork.Enqueue(new Tuple<DateTime, string, string>(dt, backupFolder, basePath));
                                           }
                                           //);
                                           //t.Start();
                                       }
                                   }
                                   catch { }
                               }
                               #endregion
                           }
                       }
                       catch { }
                   });
                    DirCheck.Start();
                }

                if (Remove == null || Remove.IsAlive == false)
                {
                    Remove = new Thread(delegate()
               {
                   // if (DateTime.Now.Hour > 1 && DateTime.Now.Hour < 3)

                   for (int i = 0; i < AlreadyProcessed.Count; i++)
                   {
                       DateTime dt = AlreadyProcessed[0].Item1;

                       if (Math.Abs(dt.Subtract(DateTime.Now).TotalMinutes) > 4)
                       {
                           var processed = AlreadyProcessed[0];
                           AlreadyProcessed.RemoveAt(0);
                           string folderExperiment = processed.Item2;
                           string dirName = Path.GetFileNameWithoutExtension(folderExperiment);
                           //return dirName;
                           string[] parts = dirName.Split('_');
                           string Prefix = parts[0];
                           string Year = parts[1].Substring(0, 4);
                           string month = parts[1].Substring(4, 2);
                           string day = parts[1].Substring(6, 2);

                           string backupFolder = Path.Combine(@"z:\raw pp", Prefix + "\\Absorption\\" + Year + month + "\\" + day + "\\" + dirName + "");
                           //string backupFolder = @"\\Doppelganger-0\readydrop\Raw PP\cct001\Absorption\" + Year + month + "\\" + day + "\\" + dirName;
                           //   if (Directory.Exists(Path.GetDirectoryName(backupFolder)) == false)
                           {
                               try
                               {
                                   Console.WriteLine("Moving directory: " + folderExperiment + " to " + backupFolder);
                                   Utilities.FileUtilities.DoCopyFolder(folderExperiment, backupFolder);
                                   Directory.Delete(folderExperiment, true);
                               }
                               catch (Exception ex2)
                               {
                                   Console.WriteLine(ex2.Message);
                                   Console.WriteLine(ex2.StackTrace);
                                   try
                                   {
                                       if (Directory.Exists(Path.GetDirectoryName(backupFolder)) == false)
                                           Directory.CreateDirectory(Path.GetDirectoryName(backupFolder));

                                       Directory.Move(folderExperiment, backupFolder);
                                   }
                                   catch (Exception ex)
                                   {

                                       Console.WriteLine(ex.Message);
                                       Console.WriteLine(ex.StackTrace);
                                   }
                               }
                           }

                       }
                   }
               });
                    // Remove.Start();
                }
            }
            catch { }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string DirPath = @"z:\Raw PP\cct001\Absorption";
            //  string DirPath = @"z:\Raw PP\cct001\Absorption\201009\13";
            // string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001";

            //  string DirPath = @"z:\Raw PP\cct001\Absorption\201302";
            //  string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001";
            //  string DirPath = @"X:\Rejected_Datasets";
            // string DirPath = @"z:\asu_recon\viveks_RT9";
            // string DirPath = @"S:\Research\Brian\Test cells for Recon\RawPP";
            // string DirPath = @"z:\Raw PP\cct001\Fluorescence\cct001\201303";
            //return;

            //   DirPath = @"X:\Rejected_Datasets";
            //  RunMonth(DirPath, "\"compress\"");
            // return;
            // //string DirPath = @"Z:\";

            List<string> Dirs = new List<string>(Directory.GetDirectories(DirPath));

            Dirs.Sort();
            // Dirs.Reverse();
            for (int i = 0; i < Dirs.Count; i++)
            {
                List<string> subdirs = new List<string>(Directory.GetDirectories(Dirs[i]));
                // subdirs.Reverse();
                foreach (string s in subdirs)
                {
                    try
                    {
                        // if (s.Contains ("201202\\23") ==true)
                        if (s.Contains(@"2010") == true)
                        {   //string s2 = Path.GetDirectoryName(s);
                            int Month = int.Parse(s.Substring(s.Length - 2));
                            // if (Month > 5)
                            {
                                //int Day = int.Parse(s.Substring(s.Length - 2));
                                //if (Day <= 6)
                                RunMonth(s, "\"compress\"");
                            }
                            if (formClosing == true)
                                return;
                        }
                    }
                    catch { }
                }
            }
        }

        bool formClosing = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string STorage = @"z:\ASU_Recon\cct001";

            //List<string> DirAdds = 
            List<string> BadDirs = new List<string>();
            List<string> GoodDirs = new List<string>();
            bool FirstDir = true;
            string[] AllDirs;

            BadDirs.Add(STorage);
            while (FirstDir == true && BadDirs.Count > 0)
            {
                try
                {
                    List<string> Dirs = new List<string>(Directory.GetDirectories(BadDirs[0]));
                    if (Dirs.Count > 0)
                    {
                        try
                        {
                            for (int i = 0; i < Dirs.Count; i++)
                            {
                                Application.DoEvents();
                                if (Dirs[i].Contains("fbp") == true)
                                {
                                    try
                                    {
                                        string d = Dirs[i];
                                        Directory.Delete(d, true);
                                        this.Text = "Deleting" + d;
                                        Application.DoEvents();
                                    }
                                    catch { }
                                    //GoodDirs.Add(Dirs[i]);
                                }
                                else
                                    BadDirs.Add(Dirs[i]);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                BadDirs.RemoveAt(0);
            }



        }

        private void Form1_Load(object sender, EventArgs e)
        {


            //Process ScriptRunner = new Process();
            //ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            //ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
            //ScriptRunner.StartInfo.FileName = @"matlab.exe";
            //ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"run('c:\\matlab\\loaddatabase.m');exit;\"";
            //ScriptRunner.Start();
            //ScriptRunner.WaitForExit();
            //Thread.Sleep(3 * 60 * 1000);

            Thread monitorThread = new Thread(delegate()
                {
                    try
                    {
                        while (true)
                        {
                            try
                            {
                                if (DateTime.Now.Hour == 12 && DateTime.Now.Minute == 0)
                                {

                                    Process ScriptRunner = new Process();
                                    ScriptRunner.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    ScriptRunner.StartInfo.WorkingDirectory = @"c:\matlab";
                                    ScriptRunner.StartInfo.FileName = @"matlab.exe";
                                    ScriptRunner.StartInfo.Arguments = "-nosplash -nodesktop -nojvm -r \"run('c:\\matlab\\loadDatabase.m');exit;\"";
                                    ScriptRunner.Start();
                                    ScriptRunner.WaitForExit();
                                    Thread.Sleep(3 * 60 * 1000);

                                    try
                                    {
                                        File.Copy(@"z:\ASU_Recon\Eval_View3.sqlite", @"t:\raw pp\Eval_View" + DateTime.Now.ToShortDateString() + ".sqlite", true);
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            this.Visible = true;
                            if (formClosing == true)
                                return;
                            Thread.Sleep(500);
                        }
                    }
                    catch
                    {

                    }
                }
            );

            monitorThread.Start();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
             string DirPath = @"Z:\ASU_Recon\cct001";
            List<string> Dirs = new List<string>(Directory.GetDirectories(DirPath));

            Dirs.Sort();
             Dirs.Reverse();
            for (int i = 0; i < Dirs.Count; i++)
            {
                List<string> subdirs = new List<string>(Directory.GetDirectories(Dirs[i]));
                for (int j = 0; j < subdirs.Count; j++)
                {

                    var days = new List<string>(Directory.GetDirectories(subdirs[j]));
                    for (int k = 0; k < days.Count; k++)
                    {
                        var recons = new List<string>(Directory.GetDirectories(days[k] + "\\data"));
                        for (int m = 0; m < recons.Count; m++)
                        {
                            if (recons[m].Contains("fbp") == true)
                            {
                                Console.WriteLine("Deleting: " + recons[m]);
                                Directory.Delete(recons[m], true);
                                Thread.Sleep(100);
                            }
                        }
                    }
                }

            }


        }


    }
}
