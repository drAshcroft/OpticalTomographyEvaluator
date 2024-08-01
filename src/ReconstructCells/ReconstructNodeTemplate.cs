using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SQLite;

namespace ReconstructCells
{
    public abstract class ReconstructNodeTemplate
    {
        protected bool NodeIsRun = false;
        protected PassData mPassData;

        public PassData GetOutput()
        {
            if (mPassData == null)
            {
                mPassData = new PassData();

            }
            return GetOutputImpl();
        }

        protected virtual PassData GetOutputImpl()
        {
            return mPassData;
        }

        public void SetInput(PassData value)
        {
            NodeIsRun = false;
            mPassData = value;
            SetInputImpl(value);
        }

        protected virtual void SetInputImpl(PassData value)
        {
            mPassData = value;
        }

        public void RunNode()
        {
            if (mPassData != null && mPassData.InvokeChain != null)
            {
                for (int i = 0; i < mPassData.InvokeChain.Count; i++)
                {
                    if (mPassData.InvokeChain[i].NodeIsRun == false)
                    {
                        for (int j = i; j < mPassData.InvokeChain.Count; j++)
                        {
                            if (mPassData.InvokeChain[j] == this)
                                break;
                            mPassData.InvokeChain[j].RunNode();
                        }
                        break;
                    }
                }
            }
            RunNodeImpl();
            NodeIsRun = true;
        }

        protected abstract void RunNodeImpl();
    }

    [Serializable]
    public class PassData : ICloneable
    {
        private Dictionary<string, string> Databaseables = new Dictionary<string, string>();


        public void AddDatabaseQueue(string key, string value)
        {
            try
            {
                if (Databaseables.ContainsKey(key) == false)
                {
                    Databaseables.Remove(key);

                }
                Databaseables.Add(key, value);
            }
            catch
            {
                try
                {

                    Databaseables.Remove(key);
                    Databaseables.Add(key, value);
                }
                catch
                {


                }

            }

        }

        public string GetFromDatabaseQueue(string key)
        {
            return Databaseables[key];
        }


        public object Clone()
        {
            PassData p = new PassData();
            p.DataScaling = DataScaling;
            p.DensityGrid = DensityGrid;
            p.FluorImage = FluorImage;
            p.mInformation = mInformation;
            p.InvokeChain = InvokeChain;
            p.Library = Library;
            p.Locations = Locations;
            p.PixelMap = PixelMap.Clone();
            p.SaveDirectory = SaveDirectory;
            p.Weights = Weights;
            p.StackImage = StackImage.Clone();
            p.Databaseables = Databaseables;
            return p;
        }

        public PassData()
        { }

        public PassData(int numImages)
        {

            Library = new OnDemandImageLibrary(500, true, @"c:\temp", false);
            Weights = new float[numImages];
        }

        [NonSerialized]
        public float[, ,] DensityGrid = null;

        [NonSerialized]
        public OnDemandImageLibrary Library = null;
        private Dictionary<string, object> mInformation = new Dictionary<string, object>();


        public string Now
        {
            get
            {
                return Utilities.SQLTools.DateTimeToSQL(DateTime.Now);
            }
        }

        public object GetInformation(string key)
        {
            return mInformation[key];
        }

        public void AddInformation(string key, object value)
        {
            try
            {
                if (mInformation.ContainsKey(key) == false)
                {
                    mInformation.Remove(key);
                }
                mInformation.Add(key, value);
            }
            catch
            {
                try
                {
                    mInformation.Remove(key);
                    mInformation.Add(key, value);
                }
                catch
                {
                }
            }
        }

        public void WriteInformationToLog()
        {
            foreach (KeyValuePair<string, object> kvp in mInformation)
            {
                Program.WriteTagsToLog(kvp.Key, kvp.Value);
            }
        }

        public CellLocation[] Locations = null;

        [NonSerialized]
        public List<ReconstructNodeTemplate> InvokeChain = new List<ReconstructNodeTemplate>();
        public Image<Gray, float> PixelMap = null;

        public Image<Gray, float> StackImage = null;

        public Image<Gray, float> theBackground = null;

        public float[] Weights;

        private string SaveDirectory;

        public bool FluorImage = false;
        public bool ColorImage = false;

        public int DataScaling = 1;


        public List<string> matLab_DatabaseString(bool insert)
        {

            if (insert)
            {

                string sqlKeys = @" minClipping, maxClipping, datasetDate ,reconDate, cellGood,reconGood, reconSucceeded, colorCamera, fluorCamera, backgroundStolen,reconVersion, cellSize";

                foreach (KeyValuePair<string, string> kvp in Databaseables)
                {
                    sqlKeys += ("," + kvp.Key);
                }


                int[] extremes = CellLocation.MaxY(Locations);

                string sqlValues =  (extremes[0]).ToString() + "," + (extremes[1]).ToString() + ",'" + Utilities.SQLTools.GetCollectionDate(Program.ExperimentTag) + "','"
                    + Utilities.SQLTools.DateTimeToSQL(DateTime.Now) + "'," + "1,1,1,";

                if (ColorImage)
                    sqlValues += "1,";
                else
                    sqlValues += "0,";
                if (FluorImage)
                    sqlValues += "1,";
                else
                    sqlValues += "0,";

                sqlValues += "0,'2.1'," + Locations[0].CellSize;

                foreach (KeyValuePair<string, string> kvp in Databaseables)
                {
                    string val;
                    if (kvp.Value.Trim() == "")
                        val = "0";
                    else if  (kvp.Key.ToLower() =="celltype")
                        val = "'" + kvp.Value + "'";
                    else
                        val = kvp.Value;

                    if (kvp.Key == "error" || kvp.Key == "stacktrace")
                        sqlValues += (",'" + val + "'");
                    else
                        sqlValues += ("," + val);
                }

                return new List<string>(new string[] { sqlKeys, sqlValues });
            }
            else
            {
                int[] extremes = CellLocation.MaxY(Locations);
                string sql = @"minClipping=" + (extremes[0]).ToString() + @",maxClipping=" + (extremes[1]).ToString() + @",reconDate='" + Utilities.SQLTools.DateTimeToSQL(DateTime.Now) + @"',";

                foreach (KeyValuePair<string, string> kvp in Databaseables)
                {
                    if (kvp.Key.ToLower() =="celltype")
                        sql += (kvp.Key + "='" + kvp.Value + "',");
                    else 
                        sql += (kvp.Key + "=" + kvp.Value + ",");
                }

                if (ColorImage)
                    sql += "colorCamera=1,";
                else
                    sql += "colorCamera=0,";
                if (FluorImage)
                    sql += "fluorCamera=1,";
                else
                    sql += "fluorCamera=0,";

                sql += "reconVersion='2.1',cellSize=" + Locations[0].CellSize;

                Console.WriteLine(sql);

                return new List<string>(new string[] { sql });
            }
        }

        public void SaveToDatabase(string SQLFile)
        {
            try
            {
                #region databaseUpdate

                // string SQLFile = @"z:\ASU_Recon\Eval_Experimental3.sqlite";
                string datasource = @"Data Source='" + SQLFile + "';Version=3;";

                System.Data.SQLite.SQLiteConnection m_dbConnection = new SQLiteConnection(datasource);
                m_dbConnection.Open();

                try
                {
                    string sql = @"Select datasetName from CellQuality where datasetName='" + Program.ExperimentTag + "';";

                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    var reader = command.ExecuteReader();

                    if (reader.HasRows == false)
                    {
                        sql = @"INSERT OR REPLACE INTO CellQuality (datasetName, EvaluatorName, minClipping, maxClipping, datasetDate ,reconDate, cellGood,reconGood, reconSucceeded, colorCamera, fluorCamera, backgroundStolen,reconVersion, cellSize";

                        foreach (KeyValuePair<string, string> kvp in Databaseables)
                        {
                            sql += ("," + kvp.Key);
                        }

                        int[] extremes = CellLocation.MaxY(Locations);

                        sql += ") VALUES ('" + Program.ExperimentTag + "', 'unrated',"
                            + (extremes[0]).ToString() + "," + (extremes[1]).ToString() + ","
                            + "'" + Utilities.SQLTools.GetCollectionDate(Program.ExperimentTag) + "',"
                            + "'" + Utilities.SQLTools.DateTimeToSQL(DateTime.Now) + "',"
                            + "1,1,1,";
                        if (ColorImage)
                            sql += "1,";
                        else
                            sql += "0,";
                        if (FluorImage)
                            sql += "1,";
                        else
                            sql += "0,";

                        sql += "0,'2.0'," + Locations[0].CellSize;

                        foreach (KeyValuePair<string, string> kvp in Databaseables)
                        {
                            if (kvp.Key == "error" || kvp.Key == "stacktrace")
                                sql += (",'" + kvp.Value + "'");
                            else
                                sql += ("," + kvp.Value);
                        }

                        sql += ");";
                        Console.WriteLine(sql);
                        command = new SQLiteCommand(sql, m_dbConnection);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        int[] extremes = CellLocation.MaxY(Locations);
                        sql = @"update  CellQuality SET minClipping=" + (extremes[0]).ToString() + @",maxClipping=" + (extremes[1]).ToString() + @",reconDate='" + Utilities.SQLTools.DateTimeToSQL(DateTime.Now) + @"',";

                        foreach (KeyValuePair<string, string> kvp in Databaseables)
                        {
                            sql += (kvp.Key + "=" + kvp.Value + ",");
                        }

                        if (ColorImage)
                            sql += "colorCamera=1,";
                        else
                            sql += "colorCamera=0,";
                        if (FluorImage)
                            sql += "fluorCamera=1,";
                        else
                            sql += "fluorCamera=0,";
                        sql += "reconVersion='2.1',cellSize=" + Locations[0].CellSize + @" WHERE datasetName='" + Program.ExperimentTag + "';";

                        Console.WriteLine(sql);

                        command = new SQLiteCommand(sql, m_dbConnection);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                    Program.WriteTagsToLog("error", ex.Message);
                    Program.WriteTagsToLog("trace", ex.StackTrace);

                }


                #endregion

                m_dbConnection.Close();
            }
            catch { }
        }

        public void SavePassData(string dirName, bool EraseExisting = true)
        {
            SaveDirectory = dirName;

            try
            {
                if (Directory.Exists(dirName) && EraseExisting)
                    Directory.Delete(dirName, true);
            }
            catch { }
            try
            {
                Directory.CreateDirectory(dirName);
            }
            catch { }

            try
            {
                Directory.CreateDirectory(dirName + "\\library");
            }
            catch { }

            if (this.DensityGrid != null)
                ImageProcessing.ImageFileLoader.SaveDensityData(dirName + "\\ProjectionObject.raw", this.DensityGrid, ImageProcessing.ImageFileLoader.RawFileTypes.Float32);


            if (Library != null)
            {
                Library.LoadLibrary();
                Library.SaveImages(dirName + "\\library\\image.tif");
            }

            if (Locations != null)
                CellLocation.Save(dirName + "\\cellLocations.csv", Locations);

            if (theBackground != null)
                ImageProcessing.ImageFileLoader.Save_TIFF(dirName + "\\theBackground.tif", theBackground);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(dirName + "\\passData.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static PassData LoadPassData(string dirName)
        {
            PassData obj;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dirName + "\\passData.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                obj = (PassData)formatter.Deserialize(stream);
                stream.Close();
            }
            catch
            {
                obj = new PassData();

            }

            if (File.Exists(dirName + "\\ProjectionObject.raw"))
            {
                obj.DensityGrid = ImageProcessing.ImageFileLoader.OpenDensityDataFloat(dirName + "\\ProjectionObject.raw");
            }

            if (File.Exists(dirName + "\\theBackground.tif"))
            {
                obj.theBackground = ImageProcessing.ImageFileLoader.Load_Tiff(dirName + "\\theBackground.tif");
            }

            if (Directory.Exists(dirName + "\\library"))
            {
                obj.Library = new OnDemandImageLibrary(dirName + "\\library", true, "c:\temp", false);
                obj.Library.LoadLibrary();
            }

            if (obj.Locations == null && File.Exists(dirName + "\\cellLocations.csv"))
            {
                obj.Locations = CellLocation.Open(dirName + "\\cellLocations.csv");

            }
            return obj;
        }
    }

}
