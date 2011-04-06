using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain
{
    public interface FileFinder
    {
        List<string> FindAllNUnitAssemblies(string baseFolder);
        List<string> FindAllSolutionFiles(string baseFolder);
        List<string> FindAllTestTaskFiles(string baseFolder);
        List<string> FindRakeFile(string baseFolder);
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
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TEST.CSPROJ", baseFolder);
            pathFinder.FindFilesRecursively(s => dlls.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.TESTS.CSPROJ", baseFolder);
            return dlls;
        }

        public List<string> FindAllSolutionFiles(string baseFolder)
        {
            var slns = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => slns.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.sln", baseFolder);
            return slns;
        }

        public List<string> FindAllTestTaskFiles(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.testtask", baseFolder);
            return testtasks;
        }

        public List<string> FindRakeFile(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "*.testtask", baseFolder);
            return testtasks;
        }
    }
}