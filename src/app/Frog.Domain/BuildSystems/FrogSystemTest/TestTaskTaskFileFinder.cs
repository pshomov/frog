using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.FrogSystemTest
{
    public class TestTaskTaskFileFinder : TaskFileFinder
    {
        public TestTaskTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.testtask",
                                            baseFolder);
            return testtasks;
        }

        readonly PathFinder pathFinder;
    }
}