using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Utilities
{
    public static class StringExtensions
    {
        public static string[] SortNumberedFiles(this string[] UnsortedFilenames)
        {



            Regex objNaturalPattern = new Regex(@"((\b[0-9]+)?\.)?[0-9]+\b");

            //find the minimum value so it will got back in correctly
            int MinFileN = int.MaxValue;
            int BadFiles = 0;
            SortedList<int, string> FilenameList = new SortedList<int, string>();
            for (int i = 0; i < UnsortedFilenames.Length; i++)
            {
                MatchCollection mc = objNaturalPattern.Matches(Path.GetFileNameWithoutExtension(UnsortedFilenames[i]));
                if (mc.Count != 0)
                {
                    try
                    {
                        string Filenumber = mc[0].Value;
                        int iFileNumber = 0;
                        int.TryParse(Filenumber, out iFileNumber);
                        if (iFileNumber < MinFileN) MinFileN = iFileNumber;
                        FilenameList.Add(iFileNumber, UnsortedFilenames[i]);
                    }
                    catch { }
                }
                else
                    BadFiles++;
            }


            if (FilenameList.Count == 0)
            {
                objNaturalPattern = new Regex(@"((\b[0-9]+)?\.)?[0-9]+p\b");

                //find the minimum value so it will got back in correctly
                 MinFileN = int.MaxValue;
                 BadFiles = 0;
                FilenameList = new SortedList<int, string>();
                for (int i = 0; i < UnsortedFilenames.Length; i++)
                {
                    MatchCollection mc = objNaturalPattern.Matches(Path.GetFileNameWithoutExtension(UnsortedFilenames[i]));
                    if (mc.Count != 0)
                    {
                        try
                        {
                            string Filenumber = mc[0].Value;
                            Filenumber = Filenumber.Substring(0, Filenumber.Length - 1);
                            int iFileNumber = 0;
                            int.TryParse(Filenumber, out iFileNumber);
                            if (iFileNumber < MinFileN) MinFileN = iFileNumber;
                            FilenameList.Add(iFileNumber+1, UnsortedFilenames[i]);
                        }
                        catch { }
                    }
                    else
                        BadFiles++;
                }

                objNaturalPattern = new Regex(@"((\b[0-9]+)?\.)?[0-9]+n\b");

                //find the minimum value so it will got back in correctly
                MinFileN = int.MaxValue;
                BadFiles = 0;
               
                for (int i = 0; i < UnsortedFilenames.Length; i++)
                {
                    MatchCollection mc = objNaturalPattern.Matches(Path.GetFileNameWithoutExtension(UnsortedFilenames[i]));
                    if (mc.Count != 0)
                    {
                        try
                        {
                            string Filenumber = mc[0].Value;
                            Filenumber = Filenumber.Substring(0, Filenumber.Length - 1);
                            int iFileNumber = 0;
                            int.TryParse(Filenumber, out iFileNumber);
                            if (iFileNumber < MinFileN) MinFileN = iFileNumber;
                            FilenameList.Add(iFileNumber*-1-1, UnsortedFilenames[i]);
                        }
                        catch { }
                    }
                    else
                        BadFiles++;
                }

                objNaturalPattern = new Regex(@"((\b[0-9]+)?\.)?[0-9]+m\b");

                //find the minimum value so it will got back in correctly
                MinFileN = int.MaxValue;
                BadFiles = 0;
                
                for (int i = 0; i < UnsortedFilenames.Length; i++)
                {
                    MatchCollection mc = objNaturalPattern.Matches(Path.GetFileNameWithoutExtension(UnsortedFilenames[i]));
                    if (mc.Count != 0)
                    {
                        try
                        {
                            string Filenumber = mc[0].Value;
                            Filenumber = Filenumber.Substring(0, Filenumber.Length - 1);
                            int iFileNumber = 0;
                            int.TryParse(Filenumber, out iFileNumber);
                            if (iFileNumber < MinFileN) MinFileN = iFileNumber;
                            FilenameList.Add(0, UnsortedFilenames[i]);
                        }
                        catch { }
                    }
                    else
                        BadFiles++;
                }



            }


            string[] Filenames = new string[FilenameList.Count];
            int ii = 0;
            foreach (string Filename in FilenameList.Values)
            {
                Filenames[ii] = Filename;
                ii++;
            }

            //  string[] Filenames = new string[UnsortedFilenames.Length-BadFiles ];
            /*  for (int i = 0; i < UnsortedFilenames.Length; i++)
              {
                  MatchCollection mc = objNaturalPattern.Matches(Path.GetFileNameWithoutExtension(UnsortedFilenames[i]));
                  if (mc.Count != 0)
                  {
                      string Filenumber = mc[0].Value;
                      int iFileNumber = 0;
                      int.TryParse(Filenumber, out iFileNumber);
                      Filenames[iFileNumber - MinFileN] = UnsortedFilenames[i];
                  }
              }*/
            return Filenames;

        }


        public static string FormatException(Exception ex)
        {
            string[] parts = ex.StackTrace.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string message;
            if (parts.Length >1)
                 message = ex.Message.Replace("\n", " ") + " --- " + parts[0] + "---" + parts[1];
            else 
                 message = ex.Message.Replace("\n", " ") + " --- " +ex.StackTrace;
            message = message.Replace("<", " ");
            message = message.Replace(">", " ");
            message = message.Replace(",", " ");
            return message;
        }
    }
}
