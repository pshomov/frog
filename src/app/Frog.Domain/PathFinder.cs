using System;
using System.IO;

namespace Frog.Domain.Specs.PathGoble
{
    public class PathFinder
    {
        readonly string start;
        readonly string pattern;

        public PathFinder(string start, string pattern)
        {
            this.start = start;
            this.pattern = pattern;
        }

        public void apply(Action<string> action)
        {
            Diver(start,  action); 
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