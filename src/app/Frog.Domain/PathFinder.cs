using System;
using System.IO;

namespace Frog.Domain
{
    public class PathFinder
    {
        public virtual void FindFilesAtTheBase(Action<string> action, string pattern, string baseFolder)
        {
            string[] files = Directory.GetFiles(baseFolder, pattern);
            foreach (string file in files)
                action(file);
        }

        public virtual void FindFilesRecursively(Action<string> action, string pattern, string baseFolder)
        {
            this.pattern = pattern;
            Diver(baseFolder, action);
        }

        string pattern;

        void Diver(string currentDir, Action<string> action)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    Diver(dir, action);
                }
                string[] files = Directory.GetFiles(currentDir, pattern);
                foreach (string file in files)
                    action(file);
            }
        }
    }
}