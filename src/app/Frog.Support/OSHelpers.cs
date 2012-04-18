using System;
using System.IO;

namespace Frog.Support
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
    
        public static int RiakPort()
        {
            return Int32.Parse(
                Environment.GetEnvironmentVariable(
                    "RUNZ_RIAK_PORT") ?? "8087");
        }

        public static string RiakHost()
        {
            return Environment.GetEnvironmentVariable(
                "RUNZ_RIAK_HOST") ?? "localhost";
        }

        public static string TerminalViewConnection()
        {
            return "mongodb://p:p@ds031607.mongolab.com:31607/terminal_view";
            return Environment.GetEnvironmentVariable(
                "RUNZ_TERMINAL_VIEW") ?? "localhost";
        }

        public static string RabbitHost()
        {
            return Environment.GetEnvironmentVariable("RUNZ_RABBITMQ_SERVER") ?? "localhost";
        }
    }
}