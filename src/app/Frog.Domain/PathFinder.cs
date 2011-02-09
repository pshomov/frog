using System;
using System.IO;

namespace Frog.Domain
{
    public class PathFinder
    {
        private string pattern;

        public virtual void apply(Action<string> action, string pattern, string baseFolder)
        {
            this.pattern = pattern;
            Diver(baseFolder, action); 
        }

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