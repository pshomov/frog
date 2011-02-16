using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain
{
    public interface FileFinder
    {
        List<string> FindAllNUnitAssemblies(string baseFolder);
        List<string> FindAllSolutionFiles(string baseFolder);
    }

    public class DefaultFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public DefaultFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindAllNUnitAssemblies(string baseFolder)
        {
            var dlls = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.apply(s => dlls.Add(s.Remove(0, baseFolderFull.Length+1)), "*.TEST.CSPROJ", baseFolder);
            return dlls;
        }

        public List<string> FindAllSolutionFiles(string baseFolder)
        {
            var slns = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.apply(s => slns.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.sln", baseFolder);
            return slns;
        }
    }
}