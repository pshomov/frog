using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Solution
{
    public class NUnitTaskFileFinder : TaskFileFinder
    {
        readonly PathFinder pathFinder;

        public NUnitTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var dlls = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TEST.CSPROJ", baseFolder);
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TESTS.CSPROJ", baseFolder);
            return dlls;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }
    }
}