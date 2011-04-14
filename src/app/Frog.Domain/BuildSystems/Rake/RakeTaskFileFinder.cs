using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Rake
{
    public class RakeTaskFileFinder : TaskFileFinder
    {
        readonly PathFinder pathFinder;

        public RakeTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var rakeFile = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE", baseFolder);
            return rakeFile;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "GEMFILE", baseFolder);
            return testtasks.Count > 0;
        }
    }
}