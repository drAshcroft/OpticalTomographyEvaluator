using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReconstructCells;
using System.IO;
using System.Data.SQLite;

namespace ASU_Evaluation2
{
    public class Fixes
    {
        public static List<string> CellTypes()
        {
            List<string> CellTypes = new List<string>();
            try
            {
                string[] lines = dates.Split(new string[] { "\n\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

             
                string cellType = "Unknown";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("cct"))
                    {
                    }
                    else if (lines[i].Contains("/"))
                    {

                    }
                    else if (lines[i].Trim() != "")
                    {
                        CellTypes.Add(lines[i].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }

            return CellTypes;

        }
        #region dateCoding
        static string dates = @"

HME1
cct001
5/23/2011
5/24/2011
5/25/2011
9/7/2011
9/8/2011
9/12/2011
9/14/2011
9/15/2011
9/16/2011
9/19/2011
9/20/2011

MDA-MB-231

5/27/2011
5/31/2011
6/1/2011
7/20/2011
7/21/2011
8/8/2011
8/9/2011
8/10/2011
8/12/2011
8/15/2011
8/17/2011
8/18/2011
8/19/2011
8/24/2011
8/25/2011
8/26/2011
8/31/2011

sw48

7/27/2012
7/30/2012
7/31/2012
8/22/2012
8/24/2012
12/12/2012
12/14/2012
1/11/2013
1/16/2013
1/29/2013
1/30/2013
2/27/2013
3/1/2013
3/4/2013


LoVo

8/29/2012
12/12/2012
1/2/2013
1/8/2013
1/9/2013
1/10/2013
1/17/2013
1/18/2013
2/12/2013
2/13/2013
2/14/2013


sw480

9/4/2012
9/5/2012
9/14/2012
9/18/2012
9/19/2012
11/21/2012
1/25/2013
1/28/2013
1/29/2013
3/4/2013
3/6/2013
3/7/2013


EPCDMSO

12/17/2010
1/14/2011
2/22/2011
2/25/2011
3/8/2011
3/9/2011
3/10/2011
3/11/2011
3/14/2011
5/16/2011
5/17/2011
5/18/2011
5/19/2011
12/9/2011
12/13/2011
2/6/2012
2/7/2012
2/13/2012
2/14/2012
2/15/2012
2/17/2012


CPADMSO

12/8/2010
12/9/2010
12/10/2010
12/14/2010
12/15/2010
12/16/2010
12/17/2010


1/7/2011
1/13/2011
1/14/2011
2/21/2012
2/20/2012
2/22/2012
2/17/2012


Flo1

4/1/2013
4/2/2013



FLO1DMSO

12/20/2010
12/21/2010
12/22/2010
12/23/2010
12/28/2010
12/29/2010
1/11/2011
1/12/2011
1/13/2011
1/18/2011
2/29/2012
2/27/2012


EPCvstat

1/4/2011
2/17/2011
3/1/2011
4/19/2011
4/21/2011
8/3/2011
8/4/2011
8/8/2011
3/5/2012
3/4/2012
3/2/2012
3/6/2012
3/7/2012


CPAvstat

1/3/2011
2/18/2011
2/22/2011
4/21/2011
4/22/2011
4/24/2011
4/25/2011
4/26/2011
4/27/2011
4/29/2011
7/25/2011
7/26/2011
7/28/2011
3/9/2012
3/14/2012
3/16/2012
3/17/2012
3/18/2012
3/19/2012
3/8/2013


Flo1vstat

1/6/2011
1/6/2011
1/7/2011
1/24/2011
1/25/2011
2/22/2011
2/23/2011
2/24/2011
2/25/2011
4/14/2011
4/15/2011
5/3/2011
5/4/2011
5/6/2011
5/9/2011
5/10/2011
3/9/2012
3/10/2012
3/11/2012
3/12/2012
3/13/2012

Tera1

3/27/2013
3/29/2013
4/1/2013


HCT116

3/22/2013


AAC1

4/2/2013
4/3/2013
4/5/2013
4/8/2013


CPAvstat
cct001
3/11/2013

EPC2sorted
cct001
8/3/2012
8/6/2012
8/7/2012
8/8/2012
8/10/2012
11/19/2012
11/20/2012

CPAsorted
cct001
2/1/2013
2/4/2013
2/5/2013
2/5/2013

CPDsorted
cct001
1/2/2013


T4-2 clusters fuelgen
cct001
3/19/2013

t4-2 clusters ROCK
ct001
12/20/2012
3/18/2013


100 micron Beads
cct001
5/23/2012


6 micron beads
cct001
10/26/2012
10/29/2012


HCT116 3b--
cct001
3/30/2012
4/2/2012
4/3/2012



hct116 dko
cct001
4/8/2013
4/10/2013
4/12/2013
5/9/2013
5/14/2013


Primary squamous
cct001
5/23/2011
9/21/2011
9/22/2011

Primary fibroblasts
cct001
9/22/2011
9/23/2011
5/10/2012


Flo1sorted
cct001
8/10/2012
8/14/2012
9/10/2012
9/11/2012
9/12/2012
11/21/2012
11/26/2012
11/27/2012


HPNE
cct001
3/11/2013
3/13/2013


MCF10a clusters fuelgen
cct001
11/13/2012
11/14/2012
2/14/2013
2/15/2013
2/18/2013
2/20/2013
2/22/2013

s1 clusters fuelgen
cct001


Primary BE
cct001
10/5/2011
10/7/2011
10/10/2011
5/9/2012
5/11/2012
8/28/2012
8/29/2012
4/15/2013
4/17/2013
4/22/2013
4/24/2013
4/25/2013
4/26/2013
4/29/2013
5/8/2013
5/15/2013
5/20/2013
5/22/2013
5/23/2013
5/25/2013

Primary EA
cct001
5/30/2013
5/31/2013
6/1/2013
6/3/2013
6/4/2013
6/5/2013


Primary Br normal
cct001
6/10/2013



Primary Br Tumor
cct001
6/11/2013
6/12/2013
6/19/2013
6/22/2013
6/25/2013


CPA
cct001
11/7/2011
11/10/2011
11/14/2011
11/15/2011
11/16/2011
11/17/2011
11/21/2011
11/22/2011
11/23/2011
11/28/2011
11/30/2011
12/1/2011
12/2/2011
12/5/2011
12/6/2011
12/27/2012
12/28/2012
2/8/2013
2/11/2013
2/12/2013

cct002
12/6/10 - 12/9/10


EPC2
cct001
12/20/2011
12/21/2011
12/22/2011
12/23/2011
12/28/2011
12/29/2011
12/30/2011
1/3/2012
12/17/2012
12/18/2012
2/6/2013
2/7/2013

cct004
12/6/10 - 12/9/10



Flo1
cct001
12/8/2010
1/20/2011
1/24/2011
1/12/2012
1/11/2012
1/10/2012
1/9/2012
1/6/2012
1/5/2012
1/4/2012
12/18/2012
12/19/2012


cct004
12/9/10 - 12/14/10



CPD
cct001
3/1/2011
3/1/2011
3/2/2011
3/3/2011
1/20/2012
1/23/2012
1/24/2012
1/25/2012
1/30/2012
1/31/2012
2/1/2012
3/7/2012
3/21/2012
3/22/2012
12/19/2012
1/3/2013
1/4/2013
1/7/2013
1/8/2013


AAC1-SB10
cct001
1/25/2011
3/30/2011
3/31/2011
4/1/2011
4/4/2011
4/5/2011
6/1/2011
10/19/2011
10/21/2011
10/24/2011
3/28/2012


AAC1
cct001
1/10/2011
1/28/2011
2/2/2011
3/15/2011
5/19/2011
5/20/2011
6/3/2011
6/6/2011
6/8/2011
6/9/2011
6/13/2011
6/14/2011
9/28/2011
9/29/2011
9/30/2011
10/5/2011
10/24/2011
10/26/2011
3/26/2012
3/27/2012

RKO
cct001
2/15/2011
2/16/2011
2/17/2011
3/7/2011
3/16/2011
3/17/2011
3/21/2011
3/22/2011
3/23/2011
3/24/2011
3/29/2011
3/30/2011
4/4/2012


HCT116
cct001
6/17/2011
6/20/2011
6/24/2011
6/27/2011
6/28/2011
6/29/2011
7/6/2011
7/8/2011
7/14/2011
7/15/2011
7/18/2011

Tera2
cct001
5/9/2012
6/26/2012
6/27/2012
6/28/2012
6/29/2012
7/2/2012
7/3/2012
7/5/2012
7/6/2012
7/9/2012
7/10/2012
7/11/2012
8/21/2012
8/22/2012


Tera1
cct001
5/9/2012
7/31/2012
8/1/2012
8/14/2012
8/15/2012
8/20/2012


Panc1
cct001
5/9/2012
5/14/2012
5/15/2012
5/16/2012
5/17/2012
9/28/2012
10/1/2012
10/2/2012
12/3/2012
12/4/2012


Capan1
cct001
9/21/2012
9/22/2012
9/24/2012
9/25/2012
9/26/2012
10/22/2012
10/23/2012
10/24/2012
12/4/2012
12/5/2012
12/7/2012
";


        #endregion
        public static void SaveCellTypes()
        {
            try
            {
                string[] lines = dates.Split(new string[] { "\n\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                string machineType = "cct001";
                string cellType = "Unknown";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("cct"))
                        machineType = lines[i].Trim();
                    else if (lines[i].Contains("/"))
                    {
                        string sql = "";
                        if (lines[i].Contains("-"))
                        {
                            string[] parts = lines[i].Split(new string[] { "-", " ", "/" }, StringSplitOptions.RemoveEmptyEntries);
                            //12/6/10 - 12/9/10
                            string year = parts[2];
                            if (year.Length == 2) year = "20" + year;

                            DateTime startTime = new DateTime(int.Parse(year), int.Parse(parts[0]), int.Parse(parts[1]), 0, 0, 0);
                            DateTime endTime = new DateTime(int.Parse(year), int.Parse(parts[3]), int.Parse(parts[4]), 23, 59, 59);
                            string sTime = DataStore.DateTimeToSQL(startTime);
                            string eTime = DataStore.DateTimeToSQL(endTime);
                            sql = "UPDATE CellQuality SET celltype='" + cellType + "' WHERE datasetDate>='" + sTime + "' AND datasetDate<='" + eTime + "'  AND datasetName like '" + machineType + "%';";
                        }
                        else
                        {
                            string[] parts = lines[i].Split(new string[] { "-", " ", "/" }, StringSplitOptions.RemoveEmptyEntries);
                            //12/6/10 - 12/9/10
                            string year = parts[2];
                            if (year.Length == 2) year = "20" + year;

                            try
                            {
                                DateTime startTime = new DateTime(int.Parse(year), int.Parse(parts[0]), int.Parse(parts[1]), 0, 0, 0);
                                DateTime endTime = new DateTime(int.Parse(year), int.Parse(parts[0]), int.Parse(parts[1]), 23, 59, 59);

                                string sTime = DataStore.DateTimeToSQL(startTime);
                                string eTime = DataStore.DateTimeToSQL(endTime);
                                sql = "UPDATE CellQuality SET celltype='" + cellType + "' WHERE datasetDate>='" + sTime + "' AND datasetDate<='" + eTime + "'  AND datasetName like '" + machineType + "%';";
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.Print(ex.Message);

                            }
                        }



                        try
                        {
                            SQLiteCommand command2 = new SQLiteCommand(sql, m_dbConnection);
                            command2.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {

                            System.Diagnostics.Debug.Print(ex.Message);
                        }

                    }
                    else if (lines[i].Trim() != "")
                        cellType = lines[i].Trim();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);

            }
        }

        private static SQLiteConnection m_dbConnection;
        public static void CreateBetterDatabase(string SQLFile)
        {

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
vgCellGood int,
vgReconGood int,
fbpCellGood int,
fbpReconGood int,
predCellGood int,
predReconGood int,
predNoisy int,
predRings int,
predFocus int,
predInteresting int,
predClipping int,
predGoodStain int,
predRegistrationGood int,
reconSucceeded int, 
noisy int, 
registrationGood int, 
focusGood int,
rings int, 
interesting int,
goodstain int, 
clipping int, 
maskCorrect int,
maskCorrect10 int,
maskCorrect40 int,
cell_Repeat int,
interferingobject int, 
comments varchar(120),
Vcomments varchar(120),
FBPcomments varchar(120),
background varchar(120),
boxtoosmall int,
cellSize int,
colorCamera int,
fluorCamera int,
interpolation int,
backgroundstolen int,
reconVersion varchar(10),
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
secondsInWholeRecon float,
maxClipping int,
minClipping int,
maxIntensity float,
aveIntensity float
);";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
            }
        }


        public static void AddClipped(string dir)
        {
            string[] files = Directory.GetFiles(dir, "*.csv", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(file));

                int[] cl = CellLocation.MaxY(CellLocation.Open(file));
                if (cl[0] < -20 || cl[1] > 830)
                {
                    string sql = "UPDATE CellQuality SET maxClipping=" + cl[0] + ",minClipping=" + cl[1] + " WHERE datasetName='" + dirName + "' ;";

                    SQLiteCommand command2 = new SQLiteCommand(sql, m_dbConnection);
                    command2.ExecuteNonQuery();
                }
            }
        }

        public static void WriteNewData()
        {
            DataStore.OpenDatabase(DataStore.DataFolder + @"\Eval2.sqlite");
            using (StreamReader sr = new StreamReader(@"c:\temp\sql.txt"))
            {
                string file = sr.ReadToEnd();
                string[] lines = file.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string sql in lines)
                {
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                }
            }
            DataStore.Close();
        }


        public static void CopyData()
        {
            string datasource = @"Data Source='" + DataStore.DataFolder + @"\Eval_View3.sqlite" + "';Version=3;";

            var m_dbConnection2 = new SQLiteConnection(datasource);
            m_dbConnection2.Open();

            string sql = @"Select * from  CellQuality;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection2);
            SQLiteDataReader reader = command.ExecuteReader();

            using (StreamWriter outfile = new StreamWriter(@"c:\temp\sql.txt", false))
            {
                reader.Read();
                int i = 0;
                while (reader.HasRows)
                {
                    bool noSave = false;
                  
                    i++;
                    Dictionary<string, string> newValues = new Dictionary<string, string>();


                    for (int k = 0; k < reader.FieldCount; k++)
                    {
                        try
                        {
                            string val =  reader[k].ToString().Trim();
                            if (val =="")
                                newValues.Add((string)reader.GetName(k),"0");
                            else 
                                newValues.Add((string)reader.GetName(k),val);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Print(ex.Message);
                        }
                    }

                    
                    // datasetNameEvaluatorNameevalDatecomments
                    string valueNames = "", valueValues = "", sets = "";
                    foreach (KeyValuePair<string, string> kvp in newValues)
                    {
                        if (("datasetNameEvaluatorNameevalDatecommentsbackgroundcellType").Contains(kvp.Key) || kvp.Key.ToLower().Contains("date") || kvp.Key.ToLower().Contains("comment"))
                        {
                            sets += kvp.Key + "='" + kvp.Value + "',";
                            valueNames += kvp.Key + ",";
                            valueValues += "'" + kvp.Value + "',";
                        }
                        else
                        {
                            sets += kvp.Key + "=" + kvp.Value + ",";
                            valueNames += kvp.Key + ",";
                            valueValues += "" + kvp.Value + ",";
                        }
                    }
                    if (noSave == false)
                    {
                        sets = sets.Substring(0, sets.Length - 1);
                        valueNames = valueNames.Substring(0, valueNames.Length - 1);
                        valueValues = valueValues.Substring(0, valueValues.Length - 1);

                     
                        string sql2 = "INSERT OR REPLACE INTO CellQuality (cell_Repeat,maskCorrect," + valueNames + ") VALUES (0,1," + valueValues + ")";

                     

                        try
                        {
                            SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                            command2.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Print(ex.Message);
                        }
                    }

                
                    reader.Read();
                }
            }
            reader.Close();
            m_dbConnection2.Close();

        }


        public static void UpgradeData()
        {
            string datasource = @"Data Source='" + DataStore.DataFolder + @"\EvalOOO.sqlite" + "';Version=3;";

            var m_dbConnection2 = new SQLiteConnection(datasource);
            m_dbConnection2.Open();

            string sql = @"Select * from  CellQuality;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection2);
            SQLiteDataReader reader = command.ExecuteReader();

            using (StreamWriter outfile = new StreamWriter(@"c:\temp\sql.txt", false))
            {
                reader.Read();
                int i = 0;
                while (reader.HasRows)
                {
                    bool noSave = false;
                    System.Diagnostics.Debug.Print("read" + i);
                    if (i == 3233)
                        System.Diagnostics.Debug.Print("");
                    i++;
                    Dictionary<string, string> newValues = new Dictionary<string, string>();

                    newValues.Add("datasetName", (string)reader["datasetName"]);
                    newValues.Add("EvaluatorName", (string)reader["EvaulatorName"]);

                    if ((string)reader["EvaulatorName"] == "Old") noSave = true;

                    string date = (string)reader["evalDate"];
                    string[] parts = date.Split(new string[] { "/", ":", " " }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime d = DateTime.Now;

                    if (parts.Length == 3)
                        noSave = true;
                    else
                    {
                        int y = int.Parse(parts[2].Substring(0, 4));
                        int m = int.Parse(parts[0]);
                        int dd = int.Parse(parts[1]);
                        int hh = int.Parse(parts[2].Substring(4, parts[2].Length - 4));
                        int min = int.Parse(parts[3]);
                        int sec = 0;
                        d = new DateTime(y, m, dd, hh, min, sec);
                    }

                    newValues.Add("evalDate", DataStore.DateTimeToSQL(d));
                    newValues.Add("cellGood", DataStore.boolToInt((bool)((int)reader["cellGood"] == 1)).ToString());
                    newValues.Add("reconGood", DataStore.boolToInt((bool)((int)reader["reconGood"] == 1)).ToString()); ;
                    newValues.Add("noisy", DataStore.boolToInt((bool)((int)reader["noisy"] == 1)).ToString()); ;
                    newValues.Add("rings", DataStore.boolToInt((bool)((int)reader["rings"] == 1)).ToString()); ;
                    newValues.Add("interesting", DataStore.boolToInt((bool)((int)reader["interesting"] == 1)).ToString()); ;
                    newValues.Add("goodstain", DataStore.boolToInt((bool)((int)reader["goodstain"] == 1)).ToString()); ;
                  //  newValues.Add("clipping", DataStore.boolToInt((bool)((int)reader["clipping"] == 1)).ToString()); ;
                    newValues.Add("interferingobject", ((int)reader["interferingobject"]).ToString()); ;
                    try
                    {
                        newValues.Add("comments", (string)reader["comments"]);
                    }
                    catch { }
                    try
                    {
                        newValues.Add("registrationGood", DataStore.boolToInt((bool)((int)reader["registrationGood"] == 1)).ToString()); ;
                    }
                    catch
                    {
                        newValues.Add("registrationGood", "1");
                    }
                    try
                    {
                        newValues.Add("focusGood", DataStore.boolToInt((bool)((int)reader["focusGood"] == 1)).ToString()); ;
                    }
                    catch
                    {
                        newValues.Add("focusGood", "1");
                    }
                    // datasetNameEvaluatorNameevalDatecomments
                    string valueNames = "", valueValues = "", sets = "";
                    foreach (KeyValuePair<string, string> kvp in newValues)
                    {
                        if (("datasetNameEvaluatorNameevalDatecomments").Contains(kvp.Key))
                        {
                            sets += kvp.Key + "='" + kvp.Value + "',";
                            valueNames += kvp.Key + ",";
                            valueValues += "'" + kvp.Value + "',";
                        }
                        else
                        {
                            sets += kvp.Key + "=" + kvp.Value + ",";
                            valueNames += kvp.Key + ",";
                            valueValues += "" + kvp.Value + ",";
                        }
                    }
                    if (noSave == false)
                    {
                        sets = sets.Substring(0, sets.Length - 1);
                        valueNames = valueNames.Substring(0, valueNames.Length - 1);
                        valueValues = valueValues.Substring(0, valueValues.Length - 1);

                        sql = "UPDATE CellQuality SET " + sets + " WHERE datasetName='" + (string)reader["datasetName"] + "';";

                        string sql2 = "INSERT OR REPLACE INTO CellQuality (" + valueNames + ") VALUES (" + valueValues + ")";

                        System.Diagnostics.Debug.Print(sql);


                        try
                        {
                            //SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                            //command2.ExecuteNonQuery();
                            SQLiteCommand command2 = new SQLiteCommand(sql, m_dbConnection);
                            command2.ExecuteNonQuery();
                        }
                        catch
                        {
                            SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                            command2.ExecuteNonQuery();
                        }
                    }

                    //sql = "INSERT OR REPLACE INTO CellQuality  (datasetName, EvaluatorName,cellType,cellGood,reconGood,reconSucceeded,noisy,rings,interesting,goodstain,clipping,focusGood,registrationGood,interferingobject,evalDate,comments) VALUES (";
                    //sql += ("'" + data.DatasetName + "',");
                    //sql += ("'" + data.Evaluator + "',");
                    //sql += ("'" + data.CellType + "',");
                    //sql += (boolToInt(data.Cell_Good) + ",");
                    //sql += (boolToInt(data.Recon_Good) + ",");
                    //sql += (boolToInt(data.Recon_Succeeded) + ",");
                    //sql += (boolToInt(data.Noisy) + ",");
                    //sql += (boolToInt(data.Rings) + ",");
                    //sql += (boolToInt(data.Interesting) + ",");
                    //sql += (boolToInt(data.GoodStain) + ",");
                    //sql += (boolToInt(data.Clipping) + ",");
                    //sql += (boolToInt(data.FocusGood) + ",");
                    //sql += (boolToInt(data.AlignmentGood) + ",");
                    //sql += (data.InterferingObject + ",");

                    //string value = DateTime.Now.ToString();
                    //string[] parts5 = value.Split(new string[] { " ", ":", "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                    //parts5[0] = ("00" + parts5[0]);
                    //parts5[0] = parts5[0].Substring(parts5[0].Length - 2);

                    //parts5[1] = ("00" + parts5[1]);
                    //parts5[1] = parts5[1].Substring(parts5[1].Length - 2);

                    //parts5[3] = ("00" + parts5[3]);
                    //parts5[3] = parts5[3].Substring(parts5[3].Length - 2);

                    //parts5[4] = ("00" + parts5[4]);
                    //parts5[4] = parts5[4].Substring(parts5[4].Length - 2);

                    //parts5[5] = ("00" + parts5[5]);
                    //parts5[5] = parts5[5].Substring(parts5[5].Length - 2);
                    //value = parts5[2] + parts5[0] + parts5[1] + parts5[3] + parts5[4] + parts5[5];


                    //sql += (value + ",");
                    //sql += ("'" + data.Comments.Replace("'", "").Replace("\"", "") + "');");

                    //sql = sql.Replace("\n", " ");
                    //System.Diagnostics.Debug.Print(sql);

                    //if (!noSave)
                    //    outfile.WriteLine(sql);
                    reader.Read();
                }
            }
            reader.Close();
            m_dbConnection2.Close();
          
        }

        public static void UpgradeDataML()
        {
            string datasource = @"Data Source='" + DataStore.DataFolder + @"\Eval_NewData.sqlite" + "';Version=3;";

            var m_dbConnection2 = new SQLiteConnection(datasource);
            m_dbConnection2.Open();

            string sql = @"Select * from  reconQuality;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection2);
            SQLiteDataReader reader = command.ExecuteReader();

            using (StreamWriter outfile = new StreamWriter(@"c:\temp\sql.txt", false))
            {
                reader.Read();
                int i = 0;
                while (reader.HasRows)
                {

                    DateTime d = DateTime.Now;

                    string valueNames = "";
                    string valueValues = "";
                    for (int j = 0; j < reader.FieldCount; j++)
                    {
                        string vName = reader.GetName(j);
                        string vValue = reader[j].ToString();
                        if (vName.Trim() == "datasetName")
                            vValue = "'" + vValue + "'";
                        if (vName.Trim() == "paramDate")
                            vValue = "'2013-06-19 12:00:00.000'";

                        valueNames += vName + ",";
                        valueValues += vValue + ",";
                    }

                    valueNames = valueNames.Substring(0, valueNames.Length - 1);
                    valueValues = valueValues.Substring(0, valueValues.Length - 1);


                    string sql2 = "INSERT OR REPLACE INTO reconQuality (" + valueNames + ") VALUES (" + valueValues + ")";

                    System.Diagnostics.Debug.Print(sql2);



                    SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                    command2.ExecuteNonQuery();


                    reader.Read();
                }
            }
            reader.Close();
            m_dbConnection2.Close();
            m_dbConnection.Close();
        }

        public static void RecoverAllData(string topFolder)
        {
            string[] dir = Directory.GetDirectories(topFolder);
            List<string> dirs = new List<string>(dir);
            dirs.Sort();
            dirs.Reverse();

            foreach (string subdir in dirs)
            {
                if (subdir.Contains("cct") == true)
                {
                    try
                    {
                        string[] months = Directory.GetDirectories(subdir);

                        foreach (string month in months)
                        {
                            try
                            {
                                string[] days = Directory.GetDirectories(month);
                                foreach (string day in days)
                                {
                                    try
                                    {
                                        string[] datasets = Directory.GetDirectories(day);
                                        foreach (string dataset in datasets)
                                        {
                                            RecoverOldData(dataset);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
        }

        private static string fixDate(string dateS)
        {
            string[] parts = dateS.Split(new string[] { "/", " ", "-", ":", "." }, StringSplitOptions.RemoveEmptyEntries);
            DateTime date = new DateTime(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]));

            string nDate = string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:00.000", date.Year, date.Month, date.Day, date.Hour, date.Minute);
            return nDate;
        }

        private static void RecoverOldData(string foldername)
        {

            ASU_Evaluation2.DataStore.ReplaceStringDictionary Values = RecoverData(foldername);
            if (Values != null)
            {
                /*
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
interferingobject int, 
comments varchar(120),
Vcomments varchar(120),
FBPcomments varchar(120),
background varchar(120),
boxtoosmall int,
vgCellGood int,
vgReconGood int,
fbpCellGood int,
fbpReconGood int,
cellSize int,
colorCamera int,
fluorCamera int,
interpolation int,
backgroundstolen int,
reconVersion VARCHAR(10)
                 */
                string sql = @"INSERT OR REPLACE INTO CellQuality  
              (
datasetName , 
EvaluatorName , 
cellType , 
datasetDate ,
reconDate ,
evalDate ,
cellGood , 
reconGood  , 
reconSucceeded  , 
noisy  , 
registrationGood  , 
focusGood  ,
rings  , 
interesting  ,
goodstain  , 
clipping  , 
interferingobject  , 
comments ,
Vcomments ,
FBPcomments ,
background ,
boxtoosmall  ,
vgCellGood  ,
vgReconGood  ,
fbpCellGood  ,
fbpReconGood  ,
cellSize  ,
colorCamera  ,
fluorCamera  ,
interpolation  ,
backgroundstolen  ,
reconVersion 
) VALUES (";
                //foreach (KeyValuePair<string, object> kvp in Values)
                //    System.Diagnostics.Debug.Print(kvp.Key);

                try
                {
                    sql += ("'" + Path.GetFileName(foldername) + "',");
                    sql += ("'" + "Old" + "',");
                    sql += ("'" + "Unknown" + "',");
                    sql += ("'" + DataStore.DateTimeToSQL(DataStore.GetCollectionDate(Path.GetFileName(foldername))) + "',");
                    if (Values.ContainsKey("Run_Time"))
                        sql += ("'" + ((string)Values["Run_Time"]) + "',");
                    else
                        sql += ("'" + fixDate((string)Values["run time"]) + "',");
                    /*evalDate ,*/
                    sql += ("'" + Values["evaldate"].ToString() + "',");



                    try
                    {
                        sql += (DataStore.boolToInt((bool)Values["Good_Cell"]) + ",");
                    }
                    catch
                    {
                        sql += "-1,";
                    }

                    if (Values.ContainsKey("Recon_Quality"))
                        sql += (DataStore.boolToInt((bool)Values["Recon_Quality"]) + ",");
                    else if (Values.ContainsKey("ReconQuality"))
                        sql += (DataStore.boolToInt(((string)Values["ReconQuality"]).ToLower() == "true") + ",");
                    else
                        sql += "-1,";

                    sql += (DataStore.boolToInt((bool)Values["ReconSucceeded"]) + ",");
                    sql += (DataStore.boolToInt((bool)Values["Noise"]) + ",");
                    sql += (DataStore.boolToInt((bool)Values["Registration_Quality"]) + ",");

                    if (Values.ContainsKey("Focus_Quality") == true)
                        sql += (DataStore.boolToInt((bool)Values["Focus_Quality"]) + ",");
                    else if (Values.ContainsKey("Vfocusvalue"))
                        sql += (DataStore.boolToInt(((string)Values["Vfocusvalue"]).ToLower() == "true") + ",");
                    else
                        sql += "-1,";


                    sql += (DataStore.boolToInt((bool)Values["Rings"]) + ",");
                    sql += (DataStore.boolToInt((bool)Values["Interesting"]) + ",");

                    sql += (DataStore.boolToInt((bool)Values["Cell_Staining"]) + ",");



                    sql += (DataStore.boolToInt((bool)Values["TooClose"]) + ",");



                    if (Values.ContainsKey("Interfering_Object") == true)
                        sql += (DataStore.boolToInt((bool)Values["Interfering_Object"]) + ",");
                    else if (Values.ContainsKey("VInterferingObject"))
                        sql += (DataStore.boolToInt(((string)Values["VInterferingObject"]).ToLower() == "true") + ",");
                    else
                        sql += "-1,";


                    sql += ("'" + ((string)Values["Comments"]).Replace("'", "").Replace("\"", "") + "',");
                    sql += ("'" + ((string)Values["VComments"]).Replace("'", "").Replace("\"", "") + "',");
                    sql += ("'" + ((string)Values["Comments"]).Replace("'", "").Replace("\"", "") + "',");




                    if (Values.ContainsKey("Background") == true)
                        sql += ("'" + (string)Values["Background"] + "',");
                    else if (Values.ContainsKey("background"))
                        sql += ("'" + (string)Values["background"] + "',");
                    else
                        sql += "' ',";

                    //boxtoosmall  ,vgCellGood  , vgReconGood  ,
                    sql += "0,0,0,";
                    sql += (DataStore.boolToInt((bool)Values["Good_Cell"]) + ",");
                    if (Values.ContainsKey("Recon_Quality"))
                        sql += (DataStore.boolToInt((bool)Values["Recon_Quality"]) + ",");
                    else if (Values.ContainsKey("ReconQuality"))
                        sql += (DataStore.boolToInt(((string)Values["ReconQuality"]).ToLower() == "true") + ",");
                    else
                        sql += "-1,";
                    sql += (Values["cellsize"].ToString() + ",");

                    sql += (DataStore.boolToInt(((string)Values["iscolor"]).ToLower() == "true") + ",");
                    sql += (DataStore.boolToInt(((string)Values["isfluor"]).ToLower() == "true") + ",");


                    sql += "0,";
                    sql += (DataStore.boolToInt(((string)Values["backgroundstolen"]).ToLower() == "true") + ",");
                    sql += ("'" + Values["version"].ToString() + "');");


                    // System.Diagnostics.Debug.Print(sql);
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    System.Diagnostics.Debug.Print(ex.Message);
                }
            }
        }

        private static ASU_Evaluation2.DataStore.ReplaceStringDictionary RecoverData(string FilePath)
        {
            ASU_Evaluation2.DataStore.ReplaceStringDictionary Values = new ASU_Evaluation2.DataStore.ReplaceStringDictionary();

            Values.Add("ReconSucceeded", false);
            Values.Add("Noise", false);
            Values.Add("Registration_Quality", false);
            Values.Add("Rings", false);
            Values.Add("Interesting", false);

            Values.Add("Cell_Staining", false);



            Values.Add("TooClose", false);
            Values.Add("Comments", " ");
            Values.Add("VComments", " ");
            Values.Add("FBPComments", " ");


            Values.Add("Good_Cell", false);
            Values.Add("cellsize", false);

            Values.Add("isColor", "false");
            Values.Add("isFluor", "false");

            Values.AddSafe("evaldate", "2012-12-31 12:30:00.000");
            Values.Add("backgroundstolen", "false");
            Values.Add("version", "0.5");

            string DataPath;
            if (FilePath.ToLower().Contains("data") == false)
                DataPath = FilePath + "\\data\\";
            else
                DataPath = FilePath + "\\";
            String line;
            string[] Parts;
            Values.AddSafe("ReconQuality", "-");
            try
            {
                if (File.Exists(DataPath + "UserCommentsNew.txt") == true)
                {

                    System.IO.FileInfo file1 = new System.IO.FileInfo(DataPath + "UserCommentsNew.txt");
                    Values.AddSafe("evaldate", DataStore.DateTimeToSQL(file1.LastWriteTime));
                    line = "";
                    using (StreamReader sr = new StreamReader(DataPath + "UserCommentsNew.txt"))
                    {
                        line = sr.ReadToEnd();
                    }


                    Parts = line.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < Parts.Length; i++)
                    {


                        string[] KeyValuePair = Parts[i].Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);


                        if (KeyValuePair.Length > 1)
                        {
                            string key = KeyValuePair[0].Trim();
                            string value = KeyValuePair[1].Trim();

                            if (key == "ReconSucceeded")
                            {
                                Values.AddSafe("ReconSucceeded", "False" != value);
                            }
                            else if (key == "Run_Time")
                            {
                                try
                                {
                                    string[] parts5 = value.Split(new string[] { " ", ":", "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                                    parts5[0] = ("00" + parts5[0]);
                                    parts5[0] = parts5[0].Substring(parts5[0].Length - 2);

                                    parts5[1] = ("00" + parts5[1]);
                                    parts5[1] = parts5[1].Substring(parts5[1].Length - 2);

                                    parts5[3] = ("00" + parts5[3]);
                                    parts5[3] = parts5[3].Substring(parts5[3].Length - 2);

                                    parts5[4] = ("00" + parts5[4]);
                                    parts5[4] = parts5[4].Substring(parts5[4].Length - 2);

                                    parts5[5] = ("00" + parts5[5]);
                                    parts5[5] = parts5[5].Substring(parts5[5].Length - 2);
                                    value = parts5[2] + "-" + parts5[0] + "-" + parts5[1] + " " + parts5[3] + ":" + parts5[4] + ":" + parts5[5] + ".000";
                                    Values.AddSafe("Run_Time", value);
                                }
                                catch
                                {
                                    Values.AddSafe("Run_Time", "2012-12-30 3:35.000");
                                }

                            }
                            else if (key == "Registration_Quality")
                            {
                                Values.AddSafe("Registration_Quality", "Bad" != value);
                            }
                            else if (key == "Cell_Staining")
                            {
                                Values.AddSafe("Cell_Staining", "Bad" != value);
                            }
                            else if (key == "Focus_Quality")
                            {
                                Values.AddSafe("Focus_Quality", "Bad" != value);
                            }
                            else if (key == "Background")
                            {
                                Values.AddSafe("Background", value);
                            }
                            else if (key == "Interfering_Object")
                            {
                                Values.AddSafe("Interfering_Object", "Yes" == value);
                            }
                            else if (key == "Good_Cell")
                            {
                                Values.AddSafe("Good_Cell", "Yes" == value);
                            }
                            else if (key == "TooClose")
                            {
                                Values.AddSafe("TooClose", "Yes" == value);
                            }
                            else if (key == "Interesting")
                            {
                                Values.AddSafe("Interesting", "Yes" == value);
                            }
                            else if (key == "Recon_Quality")
                            {
                                Values.AddSafe("Recon_Quality", ("Bad" != value));
                            }
                            else if (key == "Noise")
                            {
                                Values.AddSafe("Noise", "Yes" == value);
                            }
                            else if (key == "Rings")
                            {
                                Values.AddSafe("Rings", "Yes" == value);
                            }
                            else if (key == "Comments")
                            {
                                Values.AddSafe("Comments", value);
                            }

                        }
                    }

                }
            }
            catch (Exception Exception)
            {
                System.Diagnostics.Debug.Print(Exception.Message);
            }

            try
            {

                using (StreamReader sr = new StreamReader(DataPath + "comments.txt"))
                {
                    line = sr.ReadToEnd();
                }

                line = line.Replace("\r", "").Replace("\n", "").ToLower();
                Parts = line.Split(new string[] { "<", "/>", ">" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < Parts.Length; i += 2)
                {
                    try
                    {
                        Values.AddSafe(Parts[i], Parts[i + 1]);
                    }
                    catch { }
                }

                Values.AddSafe("CNoise", "No");
                Values.AddSafe("CRings", "No");

                string outOfImageRange = "-";
                try
                {
                    outOfImageRange = (string)Values["outofimagerange"];
                    if (outOfImageRange == "True")
                        outOfImageRange = "Yes";
                    else
                        outOfImageRange = "No";
                }
                catch { }
                Values.AddSafe("CTooClose", outOfImageRange);

                string NumberOfCells = "";
                try
                {
                    NumberOfCells = (string)Values["numberofcells"];
                }
                catch { }
                Values.AddSafe("Cnumberofcells", NumberOfCells);

                string CenteringQuality = "-";
                try
                {
                    CenteringQuality = (string)Values["center quality"];

                    double dCenterQuality = double.Parse(CenteringQuality);


                    if (line.Contains("bad centering") == true)
                        CenteringQuality = "Bad";
                    else if (dCenterQuality > 4)
                        CenteringQuality = "Questionable";
                    else
                        CenteringQuality = "OK";
                }
                catch
                {
                    CenteringQuality = "-";
                }
                Values.AddSafe("Ccenteringquality", CenteringQuality);

                string CenteringQuality2 = "";
                try
                {
                    CenteringQuality2 = (string)Values["centeringqualityactual"];
                }
                catch { }
                Values.AddSafe("Ccenteringquality2", CenteringQuality2);


                string CellStainingAverage = "";
                Values.AddSafe("Ccell staining quality", "");
                try
                {
                    CellStainingAverage = (string)Values["cell staining average"];

                    if (double.Parse(CellStainingAverage) > 10000)
                    {
                        Values.AddSafe("Ccell staining quality", "OK");
                    }
                    // else
                    // {
                    //     Values.AddSafe("Ccell staining quality", "Bad");

                    // }
                }
                catch { }
                Values.AddSafe("Ccell staining average", CellStainingAverage);


                string ReconAverage = "";
                try
                {
                    ReconAverage = (string)Values["eval factor"];
                    ReconAverage += (", " + (string)Values["eval f4"]);


                    ReconAverage = (string)Values["reconvsstack"];
                    //  ReconAverage = (string)Values["cylinder boundry"];
                    //  ReconAverage = Math.Round(100 * double.Parse(ReconAverage) / double.Parse(CellStainingAverage)).ToString();
                }
                catch { }
                Values.AddSafe("Crecon staining", ReconAverage);


                string ReconSucceeded = "False";
                try
                {
                    ReconSucceeded = (string)Values["recon"];
                }
                catch { }

                if (ReconSucceeded == "False")
                {
                    if (File.Exists(DataPath + "ProjectionObject.cct") == true || File.Exists(DataPath + "ProjectionObject.tif") == true)
                    {
                        ReconSucceeded = "False but succeeded";
                    }
                    else
                        ReconSucceeded = "False";
                }
                Values.AddSafe("Crecon", ReconSucceeded);


                string FocusValue = "-";
                string FocusVar = "";
                try
                {
                    if (line.Contains("bad focus") == true)
                    {
                        FocusValue = "Bad";
                    }
                    else if (line.Contains("questionable Focus"))
                        FocusValue = "Questionable";
                    else
                    {
                        FocusValue = ((string)Values["focusvalue"]);
                        FocusVar = ((string)Values["focusvar"]);


                        double dFocusValue = double.Parse(FocusValue);
                        double dFocusVar = Math.Round(100 * double.Parse((string)Values["focusvaluesd"]) / dFocusValue, 1);
                        if (dFocusVar > 18)
                            FocusValue = "Bad";
                        else if (dFocusVar > 9)
                            FocusValue = "Questionable";
                        else
                            FocusValue = "OK";

                        if (dFocusValue < 4 && FocusValue != "Bad")
                            FocusValue = "Questionable";
                    }

                }
                catch
                {
                    FocusValue = "-";

                }


                Values.AddSafe("Cfocusvalue", FocusValue);
                Values.AddSafe("Cfocusvar", FocusVar);

                string reconGood = "-";
                try
                {
                    string d = (string)Values["reconvsstack"];
                    double dd = double.Parse(d);
                    if (dd > .8)
                        reconGood = "Good";
                    else
                        reconGood = "Bad";
                }
                catch { }
                Values.AddSafe("CReconQuality", reconGood);
                Values.AddSafe("CRecon_Quality", reconGood);

                string BackgroundSubtraction = "No Info";
                try
                {
                    if (Values.ContainsKey("backgroundsubtraction"))
                        BackgroundSubtraction = (String)Values["backgroundsubtraction"];
                    else if (Values.ContainsKey("background"))
                        BackgroundSubtraction = (String)Values["background"];

                    if (Values.ContainsKey("backgroundremovalmethod"))
                        BackgroundSubtraction = (String)Values["backgroundremovalmethod"];

                }
                catch { }
                Values.AddSafe("Cbackgroundsubtraction", BackgroundSubtraction);

            }
            catch { }

            try
            {
                Values.AddSafe("VInterferingObject", "");


                string pPath = FilePath.Replace("\"", "").Replace("'", "");

                if (pPath.EndsWith("\\") == false)
                    pPath += "\\";

                string dirName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(Path.GetDirectoryName(pPath)));
                string[] parts = dirName.Split('_');
                string Prefix = parts[0];
                string Year = parts[1].Substring(0, 4);
                string month = parts[1].Substring(4, 2);
                string day = parts[1].Substring(6, 2);


                DataPath = Path.Combine("w:\\", Prefix + "\\" + Year + month + "\\" + day + "\\" + dirName);
                Values.AddSafe("VInterferingObject", "-");
                DataPath = DataPath + "\\MotionControlEvaluation.xml";
                Values.AddSafe("VInterferingObject", 0);
                Values.AddSafe("VComments", "");
                Values.AddSafe("VInterferingObject", 0);
                Values.AddSafe("Vfocusvalue", "good");
                if (File.Exists(DataPath))
                {
                    /*
                     <entry key="ContrastReversal"/>
<entry key="VerticalMotionCorrectionInBoundaries"/>
<entry key="Comments">noise</entry>
<entry key="NoiseUncorrectedRegionInterference">Yes</entry>
<entry key="ReconstructionChecksum">-11457786796.159</entry>
<entry key="DateCompleted">11-Mar-2013 16:32:04</entry>
<entry key="AirBubbles"/>
<entry key="InterferingObject">No</entry>
<entry key="BackgroundDebris"/>
<entry key="AxialMotionCorrectionInBoundaries"/>
<entry key="DataSetID">cct001_20130311_090555</entry>
<entry key="ObjectOutOfFocus">No</entry>
<entry key="ObjectMovesOutOfFrame">No</entry>
<entry key="TwistingOrBlurring"/>
<entry key="LabelingUserID">shelland</entry>
                     */
                    line = "";
                    using (StreamReader sr = new StreamReader(DataPath))
                    {
                        line = sr.ReadToEnd();
                    }
                    string[] parts2 = line.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < parts2.Length; i++)
                    {
                        if (parts2[i].Contains("Comments"))
                        {
                            string[] parts3 = parts2[i].Split(new string[] { "</", "\">" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts3.Length > 1)
                            {
                                string Comments = parts3[1];
                                Values.AddSafe("VComments", Comments);
                            }
                        }
                        if (parts2[i].Contains("InterferingObject"))
                        {
                            string[] parts3 = parts2[i].Split(new string[] { "</", "\">" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts3.Length > 1)
                            {
                                string Comments = parts3[1];
                                Values.AddSafe("VInterferingObject", Comments);
                            }
                        }

                        if (parts2[i].Contains("BackgroundDebris"))
                        {
                            string[] parts3 = parts2[i].Split(new string[] { "</", "\">" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts3.Length > 1)
                            {
                                string Comments = parts3[1];
                                Values.AddSafe("VInterferingObject", Comments);
                            }
                        }

                        if (parts2[i].Contains("ObjectOutOfFocus"))
                        {
                            string[] parts3 = parts2[i].Split(new string[] { "</", "\">" }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts3.Length > 1)
                            {
                                string Comments = parts3[1];
                                string cur = ((string)Values["focusvalue"]);
                                if (cur != "OK")
                                {
                                    if (Comments == "Yes")
                                        Values.AddSafe("Vfocusvalue", "Bad");
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("");
            }

            return Values;


        }
    }
}
