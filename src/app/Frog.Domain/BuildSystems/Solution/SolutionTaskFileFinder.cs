﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.BuildSystems.Solution
{
    public class SolutionTaskFileFinder : TaskFileFinder

    {
        readonly PathFinder pathFinder;

        public SolutionTaskFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            var slns = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => slns.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.sln", baseFolder);
            return slns;
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }
    }
}