using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Rake
{
    public class BundlerFileFinder : TaskFileFinder
    {
        public BundlerFileFinder(PathFinder pathFinder)
        {
            _pathFinder = pathFinder;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            _pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "GEMFILE",
                                             baseFolder);
            return testtasks.Count > 0;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var rakeFile = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            _pathFinder.FindFilesAtTheBase(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "GEMFILE",
                                           baseFolder);
            return rakeFile;
        }

        readonly PathFinder _pathFinder;
    }
}