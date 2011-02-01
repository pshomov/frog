using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Frog.Specs.Support
{
    public static class OSHelpers
    {
        public static void ClearAttributes(string currentDir)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    ClearAttributes(dir);
                    File.SetAttributes(dir,FileAttributes.Directory);
                }
                string[] files = Directory.GetFiles(currentDir);
                foreach (string file in files)
                    File.SetAttributes(file, FileAttributes.Normal);
            }
        }
    }
}
