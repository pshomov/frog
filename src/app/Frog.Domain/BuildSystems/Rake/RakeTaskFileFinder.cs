using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Rake
{
    public class RakeTaskFileFinder : TaskFileFinder
    {
        public RakeTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "GEMFILE",
                                            baseFolder);
            return testtasks.Count > 0;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var rakeFile = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesAtTheBase(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE",
                                          baseFolder);
            if (rakeFile.Count == 0)
                pathFinder.FindFilesAtTheBase(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE.RB",
                                              baseFolder);
            return rakeFile;
        }

        readonly PathFinder pathFinder;
    }
}