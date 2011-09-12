using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Make
{
    public class MakeFileFinder : TaskFileFinder
    {
        private readonly PathFinder pathFinder;

        public MakeFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var rakeFile = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesAtTheBase(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "MAKEFILE", baseFolder);
            return rakeFile;
        }
    }
}