using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class SQLTools
    {
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
    }
}
