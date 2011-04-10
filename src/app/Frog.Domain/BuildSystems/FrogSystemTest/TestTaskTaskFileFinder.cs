using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.FrogSystemTest
{
    public class TestTaskTaskFileFinder : TaskFileFinder
    {
        readonly PathFinder pathFinder;

        public TestTaskTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.testtask", baseFolder);
            return testtasks;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }
    }
}