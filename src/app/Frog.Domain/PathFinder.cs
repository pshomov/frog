using System;
using System.IO;

namespace Frog.Domain
{
    public class PathFinder
    {
        readonly string start;
        private string pattern;

        public PathFinder(string start)
        {
            this.start = start;
        }

        public virtual void apply(Action<string> action, string pattern)
        {
            this.pattern = pattern;
            Diver(start, action); 
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