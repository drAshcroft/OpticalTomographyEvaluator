using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace ASU_Evaluation2
{
    public class DataStore
    {

        public static string DataFolder;
        public static string DataDrive;
        public static string User;
        public static string ProcessedDrive;
        public static string StorageDrive;
        public static string ColdStorageDrive;
        public static string ColdStorageDrive2;

        public static Random RND = new Random((int)DateTime.Now.Ticks);

        private static SQLiteConnection m_dbConnection;

        public static void OpenDatabase(string filename)
        {

            string SQLFile = filename;

            if (File.Exists(SQLFile) == true)
            {
                string datasource = @"Data Source='" + SQLFile + "';Version=3;";

                m_dbConnection = new SQLiteConnection(datasource);
                m_dbConnection.Open();

                //                string sql = @"create table ReconQuality (
                //                             datasetName varchar(20) UNIQUE, 
                //Hist1 INT,Hist2 INT,Hist3 INT,Hist4 INT,Hist5 INT,Hist6 INT,Hist7 INT,Hist8 INT,Hist9 INT,Hist10 INT,Hist11 INT,Hist12 INT,Hist13 INT,Hist14 INT,Hist15 INT,Hist16 INT,Hist17 INT,Hist18 INT,Hist19 INT,Hist20 INT,Hist21 INT,Hist22 INT,Hist23 INT,Hist24 INT,Hist25 INT,Hist26 INT,Hist27 INT,Hist28 INT,Hist29 INT,Hist30 INT,Hist31 INT,Hist32 INT,Hist33 INT,Hist34 INT,Hist35 INT,Hist36 INT,Hist37 INT,Hist38 INT,Hist39 INT,Hist40 INT,Hist41 INT,Hist42 INT,Hist43 INT,Hist44 INT,Hist45 INT,Hist46 INT,Hist47 INT,Hist48 INT,Hist49 INT,Hist50 INT,Hist51 INT,Hist52 INT,Hist53 INT,Hist54 INT,Hist55 INT,Hist56 INT,Hist57 INT,Hist58 INT,Hist59 INT,Hist60 INT,Hist61 INT,Hist62 INT,Hist63 INT,Hist64 INT,Hist65 INT,Hist66 INT,Hist67 INT,Hist68 INT,Hist69 INT,Hist70 INT,Hist71 INT,Hist72 INT,Hist73 INT,Hist74 INT,Hist75 INT,Hist76 INT,Hist77 INT,Hist78 INT,Hist79 INT,Hist80 INT,Hist81 INT,Hist82 INT,Hist83 INT,Hist84 INT,Hist85 INT,Hist86 INT,Hist87 INT,Hist88 INT,Hist89 INT,Hist90 INT,Hist91 INT,Hist92 INT,Hist93 INT,Hist94 INT,Hist95 INT,Hist96 INT,Hist97 INT,Hist98 INT,Hist99 INT,Hist100 INT,Hist101 INT,Hist102 INT,Hist103 INT,Hist104 INT,Hist105 INT,Hist106 INT,Hist107 INT,Hist108 INT,Hist109 INT,Hist110 INT,Hist111 INT,Hist112 INT,Hist113 INT,Hist114 INT,Hist115 INT,Hist116 INT,Hist117 INT,Hist118 INT,Hist119 INT,Hist120 INT,Hist121 INT,Hist122 INT,Hist123 INT,Hist124 INT,Hist125 INT,Hist126 INT,Hist127 INT,Hist128 INT,Hist129 INT,Hist130 INT,Hist131 INT,Hist132 INT,Hist133 INT,Hist134 INT,Hist135 INT,Hist136 INT,Hist137 INT,Hist138 INT,Hist139 INT,Hist140 INT,Hist141 INT,Hist142 INT,Hist143 INT,Hist144 INT,Hist145 INT,Hist146 INT,Hist147 INT,Hist148 INT,Hist149 INT,Hist150 INT,Hist151 INT,Hist152 INT,Hist153 INT,Hist154 INT,Hist155 INT,Hist156 INT,Hist157 INT,Hist158 INT,Hist159 INT,Hist160 INT,Hist161 INT,Hist162 INT,Hist163 INT,Hist164 INT,Hist165 INT,Hist166 INT,Hist167 INT,Hist168 INT,Hist169 INT,Hist170 INT,Hist171 INT,Hist172 INT,Hist173 INT,Hist174 INT,Hist175 INT,Hist176 INT,Hist177 INT,Hist178 INT,Hist179 INT,Hist180 INT,Hist181 INT,Hist182 INT,Hist183 INT,Hist184 INT,Hist185 INT,Hist186 INT,Hist187 INT,Hist188 INT,Hist189 INT,Hist190 INT,Hist191 INT,Hist192 INT,Hist193 INT,Hist194 INT,Hist195 INT,Hist196 INT,Hist197 INT,Hist198 INT,Hist199 INT,Hist200 INT,Hist201 INT,Hist202 INT,Hist203 INT,Hist204 INT,Hist205 INT,Hist206 INT,Hist207 INT,Hist208 INT,Hist209 INT,Hist210 INT,Hist211 INT,Hist212 INT,Hist213 INT,Hist214 INT,Hist215 INT,Hist216 INT,Hist217 INT,Hist218 INT,Hist219 INT,Hist220 INT,Hist221 INT,Hist222 INT,Hist223 INT,Hist224 INT,Hist225 INT,Hist226 INT,Hist227 INT,Hist228 INT,Hist229 INT,Hist230 INT,Hist231 INT,Hist232 INT,Hist233 INT,Hist234 INT,Hist235 INT,Hist236 INT,Hist237 INT,Hist238 INT,Hist239 INT,Hist240 INT,Hist241 INT,Hist242 INT,Hist243 INT,Hist244 INT,Hist245 INT,Hist246 INT,Hist247 INT,Hist248 INT,Hist249 INT,Hist250 INT,Hist251 INT,Hist252 INT,Hist253 INT,Hist254 INT,Hist255 INT,TotalVolume INT,NucSurfacePercent INT,NucSurfaceArea INT,NucSpectrumMax1 INT,NucSpectrumMax2 INT,NucSpectrumMax3 INT,NucSpectrumMax4 INT,NucSpectrumMax5 INT,NucSpectrumMax6 INT,NucSpectrumMax7 INT,NucSpectrumMax8 INT,NucSpectrumMax9 INT,NucSpectrumMax10 INT,NucSpectrumMax11 INT,NucSpectrumMax12 INT,NucSpectrumMax13 INT,NucSpectrumMax14 INT,NucSpectrumMax15 INT,NucSpectrumMax16 INT,NucSpectrumMax17 INT,NucSpectrumMax18 INT,NucSpectrumMax19 INT,NucSpectrumMax20 INT,NucSpectrumMean1 INT,NucSpectrumMean2 INT,NucSpectrumMean3 INT,NucSpectrumMean4 INT,NucSpectrumMean5 INT,NucSpectrumMean6 INT,NucSpectrumMean7 INT,NucSpectrumMean8 INT,NucSpectrumMean9 INT,NucSpectrumMean10 INT,NucSpectrumMean11 INT,NucSpectrumMean12 INT,NucSpectrumMean13 INT,NucSpectrumMean14 INT,NucSpectrumMean15 INT,NucSpectrumMean16 INT,NucSpectrumMean17 INT,NucSpectrumMean18 INT,NucSpectrumMean19 INT,NucSpectrumMean20 INT,NucSpectrumVar1 INT,NucSpectrumVar2 INT,NucSpectrumVar3 INT,NucSpectrumVar4 INT,NucSpectrumVar5 INT,NucSpectrumVar6 INT,NucSpectrumVar7 INT,NucSpectrumVar8 INT,NucSpectrumVar9 INT,NucSpectrumVar10 INT,NucSpectrumVar11 INT,NucSpectrumVar12 INT,NucSpectrumVar13 INT,NucSpectrumVar14 INT,NucSpectrumVar15 INT,NucSpectrumVar16 INT,NucSpectrumVar17 INT,NucSpectrumVar18 INT,NucSpectrumVar19 INT,NucSpectrumVar20 INT,NucSurfaceSpreadRoughness INT,NucSurfacePsuedoEccentricity INT,NucSurfaceDistanceToCentroid INT,NucSurfaceSTDDistanceToCentroid INT,NucSurfIntensitySTD INT,NucNumberGaussians INT,NucSTDLabels INT,NucRatioGaussians INT,NucSTDIntensities INT,NucKurtIntensities INT,NucSkewIntensities INT,NucNumberVoxelsHigh INT,NucSurfTexture1 INT,NucSurfTexture2 INT,NucSurfTexture3 INT,NucSurfTexture4 INT,NucSurfTexture5 INT,NucSurfTexture6 INT,NucSurfTexture7 INT,NucSurfTexture8 INT,NucSurfTexture9 INT,NucSurfTexture10 INT,NucSurfTexture11 INT,NucSurfTexture12 INT,NucSurfTexture13 INT,NucSurfTexture14 INT,NucSurfTexture15 INT,NucSurfTexture16 INT,NucSurfTexture17 INT,NucSurfTexture18 INT,NucSurfTexture19 INT,NucSurfTexture20 INT,NucSpectrumDenoised1 INT,NucSpectrumDenoised2 INT,NucSpectrumDenoised3 INT,NucSpectrumDenoised4 INT,NucSpectrumDenoised5 INT,NucSpectrumDenoised6 INT,NucSpectrumDenoised7 INT,NucSpectrumDenoised8 INT,NucSpectrumDenoised9 INT,NucSpectrumDenoised10 INT,NucSpectrumDenoised11 INT,NucSpectrumDenoised12 INT,NucSpectrumDenoised13 INT,NucSpectrumDenoised14 INT,NucSpectrumDenoised15 INT,NucSpectrumDenoised16 INT,NucSpectrumDenoised17 INT,NucSpectrumDenoised18 INT,NucSpectrumDenoised19 INT,NucSpectrumDenoised20 INT,NucVolume INT,CellSurfaceArea INT,CellSpectrumMax1 INT,CellSpectrumMax2 INT,CellSpectrumMax3 INT,CellSpectrumMax4 INT,CellSpectrumMax5 INT,CellSpectrumMax6 INT,CellSpectrumMax7 INT,CellSpectrumMax8 INT,CellSpectrumMax9 INT,CellSpectrumMax10 INT,CellSpectrumMax11 INT,CellSpectrumMax12 INT,CellSpectrumMax13 INT,CellSpectrumMax14 INT,CellSpectrumMax15 INT,CellSpectrumMax16 INT,CellSpectrumMax17 INT,CellSpectrumMax18 INT,CellSpectrumMax19 INT,CellSpectrumMax20 INT,CellSpectrumMean1 INT,CellSpectrumMean2 INT,CellSpectrumMean3 INT,CellSpectrumMean4 INT,CellSpectrumMean5 INT,CellSpectrumMean6 INT,CellSpectrumMean7 INT,CellSpectrumMean8 INT,CellSpectrumMean9 INT,CellSpectrumMean10 INT,CellSpectrumMean11 INT,CellSpectrumMean12 INT,CellSpectrumMean13 INT,CellSpectrumMean14 INT,CellSpectrumMean15 INT,CellSpectrumMean16 INT,CellSpectrumMean17 INT,CellSpectrumMean18 INT,CellSpectrumMean19 INT,CellSpectrumMean20 INT,CellSpectrumVar1 INT,CellSpectrumVar2 INT,CellSpectrumVar3 INT,CellSpectrumVar4 INT,CellSpectrumVar5 INT,CellSpectrumVar6 INT,CellSpectrumVar7 INT,CellSpectrumVar8 INT,CellSpectrumVar9 INT,CellSpectrumVar10 INT,CellSpectrumVar11 INT,CellSpectrumVar12 INT,CellSpectrumVar13 INT,CellSpectrumVar14 INT,CellSpectrumVar15 INT,CellSpectrumVar16 INT,CellSpectrumVar17 INT,CellSpectrumVar18 INT,CellSpectrumVar19 INT,CellSpectrumVar20 INT,CellVolume INT,CellSpectrumDenoised1 INT,CellSpectrumDenoised2 INT,CellSpectrumDenoised3 INT,CellSpectrumDenoised4 INT,CellSpectrumDenoised5 INT,CellSpectrumDenoised6 INT,CellSpectrumDenoised7 INT,CellSpectrumDenoised8 INT,CellSpectrumDenoised9 INT,CellSpectrumDenoised10 INT,CellSpectrumDenoised11 INT,CellSpectrumDenoised12 INT,CellSpectrumDenoised13 INT,CellSpectrumDenoised14 INT,CellSpectrumDenoised15 INT,CellSpectrumDenoised16 INT,CellSpectrumDenoised17 INT,CellSpectrumDenoised18 INT,CellSpectrumDenoised19 INT,CellSpectrumDenoised20 INT,CellSurfaceSpreadRoughness INT,CellSurfacePsuedoEccentricity INT,CellSurfaceDistanceToCentroid INT,CellSurfaceSTDDistanceToCentroid INT,CellSurfIntensitySTD INT,CellNumberGaussians INT,CellSTDLabels INT,CellRatioGaussians INT,CellSTDIntensities INT,CellKurtIntensities INT,CellSkewIntensities INT,CellNumberVoxelsHigh INT,CellSurfTexture1 INT,CellSurfTexture2 INT,CellSurfTexture3 INT,CellSurfTexture4 INT,CellSurfTexture5 INT,CellSurfTexture6 INT,CellSurfTexture7 INT,CellSurfTexture8 INT,CellSurfTexture9 INT,CellSurfTexture10 INT,CellSurfTexture11 INT,CellSurfTexture12 INT,CellSurfTexture13 INT,CellSurfTexture14 INT,CellSurfTexture15 INT,CellSurfTexture16 INT,CellSurfTexture17 INT,CellSurfTexture18 INT,CellSurfTexture19 INT,CellSurfTexture20 INT,AxialQuality1 INT,AxialQuality2 INT,AxialQuality3 INT,AxialQuality4 INT,AxialQuality5 INT,AxialQuality6 INT,AxialQuality7 INT,AxialQuality8 INT,AxialQuality9 INT,AxialQuality10 INT,AxialQuality11 INT,AxialQuality12 INT,AxialQuality13 INT,AxialQuality14 INT,AxialQuality15 INT,AxialQuality16 INT,AxialQuality17 INT,AxialQuality18 INT,AxialQuality19 INT,AxialQuality20 INT,AxialQuality21 INT,AxialQuality22 INT,AxialQuality23 INT,AxialQuality24 INT,AxialQuality25 INT,AxialQuality26 INT,AxialQuality27 INT,AxialQuality28 INT,AxialQuality29 INT,AxialQuality30 INT,AxialQuality31 INT,AxialQuality32 INT,AxialQuality33 INT,AxialQuality34 INT,AxialQuality35 INT,AxialQuality36 INT,AxialQuality37 INT,AxialQuality38 INT,AxialQuality39 INT,AxialQuality40 INT,AxialQuality41 INT,AxialQuality42 INT,AxialQuality43 INT,AxialQuality44 INT,AxialQuality45 INT,AxialQuality46 INT,AxialQuality47 INT,AxialQuality48 INT,AxialQuality49 INT,AxialQuality50 INT,AxialQuality51 INT,AxialQuality52 INT,AxialQuality53 INT,AxialQuality54 INT,AxialQuality55 INT,AxialQuality56 INT,AxialQuality57 INT,AxialQuality58 INT,AxialQuality59 INT,AxialQuality60 INT,AxialQuality61 INT,AxialQuality62 INT,AxialQuality63 INT,AxialQuality64 INT,AxialQuality65 INT,AxialQuality66 INT,AxialQuality67 INT,AxialQuality68 INT,AxialQuality69 INT,AxialQuality70 INT,AxialQuality71 INT,AxialQuality72 INT,AxialQuality73 INT,AxialQuality74 INT,AxialQuality75 INT,AxialQuality76 INT,AxialQuality77 INT,AxialQuality78 INT,AxialQuality79 INT,AxialQuality80 INT,AxialQuality81 INT,AxialQuality82 INT,AxialQuality83 INT,AxialQuality84 INT,AxialQuality85 INT,AxialQuality86 INT,AxialQuality87 INT,AxialQuality88 INT,AxialQuality89 INT,AxialQuality90 INT,AxialQuality91 INT,AxialQuality92 INT,AxialQuality93 INT,AxialQuality94 INT,AxialQuality95 INT,AxialQuality96 INT,AxialQuality97 INT,AxialQuality98 INT,AxialQuality99 INT,AxialQuality100 INT
                //
                //);";
                //                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                //                command.ExecuteNonQuery();
                //                m_dbConnection.Close();
            }
            else
            {

                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Data");
                SQLiteConnection.CreateFile(SQLFile);

                string datasource = @"Data Source='" + SQLFile + "';Version=3;";

                m_dbConnection = new SQLiteConnection(datasource);
                m_dbConnection.Open();

                //                string sql =
                //    @"PRAGMA main.page_size = 4096;
                //PRAGMA main.cache_size=10000;
                //PRAGMA main.locking_mode=EXCLUSIVE;
                //PRAGMA main.synchronous=NORMAL;
                //PRAGMA main.journal_mode=WAL;
                //PRAGMA main.cache_size=5000;
                //";

                //                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                //                command.ExecuteNonQuery();

                //                string sql = @"create table CellQuality (datasetName varchar(20) UNIQUE, EvaulatorName varchar(20), cellType varchar(60), cellGood int, reconGood int, reconSucceeded int, noisy int, 
                //                                rings int, interesting int,goodstain int, clipping int, interferingobject int, comments varchar(120));";
                string s = "YYYY-MM-DD HH:MM:SS.SSS";
                System.Diagnostics.Debug.Print(s.Length.ToString());
                string sql = @"create table CellQuality (
datasetName varchar(20) UNIQUE,
EvaluatorName varchar(20), 
cellType varchar(60), 
datasetDate VARCHAR(23),
reconDate VARCHAR(23),
evalDate VARCHAR(23),
cellGood int, 
reconGood int, 
reconSucceeded int, 
noisy int, 
registrationGood int, 
focusGood int,
rings int, 
interesting int,
goodstain int, 
clipping int, 
maskCorrect int,
cell_Repeat int,
interferingobject int, 
comments varchar(120),
background varchar(120),
error varchar(254),
stacktrace varchar(254),
secondsInCurvature float,
secondsInBackground float,
secondsInRegistration float,
secondsInNoisy float,
secondsInAlign1 float,
secondsInMirror float,
secondsInAlign2 float,
secondsInFBP1 float,
secondsInAlign3 float,
secondsInFBP2 float,
secondsInMovie float,
secondsInTikh float,
secondsInWholeRecon float);";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
            }
        }

        public static void SaveDataset(Dataset data)
        {
            string sql = @"INSERT OR REPLACE INTO CellQuality  
              (datasetName, EvaluatorName,cellType,
              cellGood,reconGood,reconSucceeded,
              noisy,rings,interesting,
              goodstain,clipping,focusGood,
              registrationGood,interferingobject,reconDate, 
              background, datasetDate, evalDate, maskCorrect,maskCorrect10,maskCorrect40, cell_Repeat,
              comments) VALUES (";
            sql += ("'" + data.DatasetName + "',");
            if (DataStore.User!="" && ( data.Evaluator.ToLower()=="themachine" || data.Evaluator.ToLower()=="undone") )
                sql += ("'" + DataStore.User + "',");
            else 
                sql += ("'" + data.Evaluator + "',");
            sql += ("'" + data.CellType + "',");

            sql += (boolToInt(data.Cell_Good) + ",");
            sql += (boolToInt(data.Recon_Good) + ",");
            sql += (boolToInt(data.Recon_Succeeded) + ",");

            sql += (boolToInt(data.Noisy) + ",");
            sql += (boolToInt(data.Rings) + ",");
            sql += (boolToInt(data.Interesting) + ",");

            sql += (boolToInt(data.GoodStain) + ",");
            sql += (boolToInt(data.Clipping) + ",");
            sql += (boolToInt(data.FocusGood) + ",");

            sql += (boolToInt(data.AlignmentGood) + ",");
            sql += (data.InterferingObject + ",");
            sql += ("'" + DateTimeToSQL(data.ReconDate) + "',");

            sql += ("'" + data.BackGround + "',");
            sql += ("'" + DateTimeToSQL(data.CollectionDate) + "',");
            sql += ("'" + DateTimeToSQL(DateTime.Now) + "',");
            sql += (boolToInt(data.MaskCorrect ) + ",");
            sql += (boolToInt(data.MaskCorrect10) + ",");
            sql += (boolToInt(data.MaskCorrect40) + ",");
            sql += (boolToInt(data.Cell_Repeat) + ",");
            sql += ("'" + data.Comments.Replace("'", "").Replace("\"", "") + "');");

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        #region datetime
        public static string DateTimeToSQL(DateTime date)
        {
            return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:00.000", date.Year, date.Month, date.Day, date.Hour, date.Minute);
        }


        public static DateTime SQLToDateTime(string sql)
        {
            string[] parts = sql.Split(new string[] { " ", "-", ":", "." }, StringSplitOptions.RemoveEmptyEntries);
            return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]));
        }

        public static DateTime GetCollectionDate(string name)
        {

            string dirName = "";
            string[] parts = null;
            string Prefix = "";
            string Year = "";
            string month = "";
            string day = "";

            dirName = name;
            parts = dirName.Split('_');
            Prefix = parts[0];
            Year = parts[1].Substring(0, 4);
            month = parts[1].Substring(4, 2);
            day = parts[1].Substring(6, 2);


            string hour = parts[2].Substring(0, 2);
            string minute = parts[2].Substring(2, 2);
            string sec = parts[2].Substring(4, 2);

            return new DateTime(int.Parse(Year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), int.Parse(sec));
        }
        #endregion

        public static string[] GetDirectories(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            List<string> Foldernames = new List<string>();
            reader.Read();
            while (reader.HasRows)
            {
                try
                {
                    Foldernames.Add((string)reader["datasetName"]);
                }
                catch { }
                reader.Read();
            }

            for (int i = 0; i < Foldernames.Count; i++)
            {
                string dirName = Foldernames[i];
                string[] parts = dirName.Split('_');
                string Prefix = parts[0];
                string Year = parts[1].Substring(0, 4);
                string month = parts[1].Substring(4, 2);
                string day = parts[1].Substring(6, 2);

                string dataFolder = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                Foldernames[i] = dataFolder;
            }
            return Foldernames.ToArray();
        }

        public static void LoadDataset(Dataset data)
        {

            string sql = @"Select * from  CellQuality  where (datasetName='" + data.DatasetName + "');";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();


            reader.Read();
            if (reader.HasRows)
            {
                //(datasetName , EvaulatorName , cellType , cellGood , reconGood , reconSucceeded , noisy , 
                //                   rings , interesting ,goodstain , clipping , interferingobject , comments )

                sql += ("'" + data.BackGround + "',");
                sql += ("'" + DateTimeToSQL(data.CollectionDate) + "',");
                sql += ("'" + DateTimeToSQL(DateTime.Now) + "',");

                try
                {
                    data.CollectionDate = SQLToDateTime((string)reader["datasetDate"]);
                }
                catch { }
                try
                {
                    data.ReconDate = SQLToDateTime((string)reader["reconDate"]);
                }
                catch { }
                try
                {
                    data.EvaluationDate = SQLToDateTime((string)reader["evalDate"]);
                }
                catch { }

                try
                {
                    data.BackGround = (string)reader["background"];
                }
                catch { }


                try
                {
                    data.CellType = (string)reader["cellType"];
                }
                catch { }
                try
                {
                    data.Cell_Good = (bool)((int)reader["cellGood"] == 1);
                }
                catch { }
                try
                {
                    data.Recon_Good = (bool)((int)reader["reconGood"] == 1);
                }
                catch { }
                try
                {
                    //data.Recon_Succeeded=(bool)((int)reader["reconSucceeded"]==1);
                    data.Noisy = (bool)((int)reader["noisy"] == 1);
                }
                catch { }
                try
                {
                    data.Cell_Repeat = (bool)((int)reader["cell_Repeat"] == 1);
                }
                catch { }
                try
                {
                    data.MaskCorrect = (bool)((int)reader["maskCorrect"] == 1);
                }
                catch { }
                try
                {
                    data.MaskCorrect10 = (bool)((int)reader["maskCorrect10"] == 1);
                }
                catch { }
                try
                {
                    data.MaskCorrect40 = (bool)((int)reader["maskCorrect40"] == 1);
                }
                catch { }
                try
                {
                    data.Rings = (bool)((int)reader["rings"] == 1);
                }
                catch { }
                try
                {
                    data.Interesting = (bool)((int)reader["interesting"] == 1);
                }
                catch { }
                try
                {
                    data.GoodStain = (bool)((int)reader["goodstain"] == 1);
                }
                catch { }
                try
                {
                    data.Clipping = (bool)((int)reader["clipping"] == 1);
                }
                catch { }
                try
                {
                    data.InterferingObject = (int)reader["interferingobject"];
                }
                catch { }
                try
                {
                    data.Comments = ((string)reader["comments"]);
                }
                catch { }
                try
                {
                    data.AlignmentGood = (bool)((int)reader["registrationGood"] == 1);
                }
                catch { }
                try
                {
                    data.FocusGood = (bool)((int)reader["focusGood"] == 1);
                }
                catch { }
                try
                {
                    data.setEvaluator((string)reader["EvaluatorName"],true);
                }
                catch { }

            }
            else
                data.setEvaluator( "undone");

            System.Diagnostics.Debug.Print(reader.ToString());

            reader.Close();


        }

        public static int boolToInt(bool value)
        {
            if (value)
                return 1;
            else
                return 0;
        }

        public static void Close()
        {

            m_dbConnection.Close();
        }

        /// <summary>
        /// a dictionary that allows an key to be added with the same name as a previous key.
        /// The old key will be replaced
        /// </summary>
        public class ReplaceStringDictionary : Dictionary<string, object>
        {
            private object CriticalSectionLock = new object();
            public void AddSafe(string Key, object Value)
            {
                lock (CriticalSectionLock)
                {
                    if (this.ContainsKey(Key) == false)
                        this.Add(Key, Value);
                    else
                    {
                        this.Remove(Key);
                        this.Add(Key, Value);
                    }
                }
            }
        }



    }
}

