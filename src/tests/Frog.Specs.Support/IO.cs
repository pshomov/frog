using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Frog.Specs.Support
{
    public class IO
    {
        public static string GetTemporaryDirectory()
        {
            string temp_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp_directory);
            return temp_directory;
        }
    }
}
