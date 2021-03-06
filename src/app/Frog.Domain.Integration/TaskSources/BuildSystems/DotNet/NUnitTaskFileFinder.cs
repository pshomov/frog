using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.DotNet
{
    public class NUnitTaskFileFinder : TaskFileFinder
    {
        public NUnitTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }

        public List<string> FindFiles(string baseFolder)
        {
            var dlls = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TEST.CSPROJ",
                                            baseFolder);
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TESTS.CSPROJ",
                                            baseFolder);
            return dlls;
        }

        readonly PathFinder pathFinder;
    }
}