using System.IO;

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
                    File.SetAttributes(dir, FileAttributes.Directory);
                }
                string[] files = Directory.GetFiles(currentDir);
                foreach (string file in files)
                    File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        public static void NukeDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                ClearAttributes(directory);
                Directory.Delete(directory,true);
            }
        }

        public static string GetMeAWorkingFolder()
        {
            var changeset = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }

    }
}