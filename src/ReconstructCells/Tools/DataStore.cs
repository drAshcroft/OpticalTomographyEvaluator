﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace ReconstructCells.Tools
{
    class DataStore
    {
        public static string DataFolder;
        public static string User;
        public static string ProcessedDrive;
        public static string StorageDrive;

        public static Random RND = new Random((int)DateTime.Now.Ticks);

        static SQLiteConnection m_dbConnection;


        public static void OpenDatabase(string filename)
        {

            string SQLFile = filename;

            if (File.Exists(SQLFile) == true)
            {
                string datasource = @"Data Source='" + SQLFile + "';Version=3;";

                m_dbConnection = new SQLiteConnection(datasource);
                m_dbConnection.Open();
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
                             datasetName varchar(20) UNIQUE, EvaluatorName varchar(20), cellType varchar(60), datasetDate VARCHAR(23), reconDate VARCHAR(23), evalDate VARCHAR(23),
                             cellGood int, reconGood int, reconSucceeded int, noisy int, registrationGood int, focusGood int,
                             rings int, interesting int,goodstain int, clipping int, interferingobject int, 
                             comments varchar(120),background varchar(120));";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
            }
        }


        public static Queue<Tuple<string, string>> GetBadRecons()
        {
            string sql = @"Select * from  CellQuality  where (reconGood=0);";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            Queue<Tuple<string, string>> Selected = new Queue<Tuple<string, string>>();
            reader.Read();
            if (reader.HasRows)
            {
                string dirName = (string)reader["datasetName"];
                string[] parts = dirName.Split('_');
                string Prefix = parts[0];
                string Year = parts[1].Substring(0, 4);
                string month = parts[1].Substring(4, 2);
                string day = parts[1].Substring(6, 2);

                string OutputPath = Path.Combine(@"z:\ASU_Recon\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                string InputPath;
                if (Prefix.ToLower() == "cct001")
                    InputPath = Path.Combine(@"z:\Raw PP\cct001\Absorption", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                else
                    InputPath = Path.Combine(@"z:\Raw PP", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);

                Selected.Enqueue(new Tuple<string, string>(InputPath, OutputPath));
            }


            return Selected;


        }
    }
}
