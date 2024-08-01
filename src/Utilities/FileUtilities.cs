using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilities
{
    public class FileUtilities
    {
        static public void DoCopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                try
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, true);
                }
                catch { }
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                DoCopyFolder(folder, dest);
            }
        }
    }
}
