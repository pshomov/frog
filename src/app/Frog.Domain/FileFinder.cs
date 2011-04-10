using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Domain
{
    public interface FileFinder
    {
        List<string> FindAllSolutionFiles(string baseFolder);
        List<string> FindAllTestTaskFiles(string baseFolder);
        List<string> FindRakeFile(string baseFolder);
        List<string> FindFiles(string baseFolder);
        bool FindBundlerFile(string baseFolder);
    }

    public class RakeFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public RakeFileFinder(PathFinder pathFinder)
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
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE", baseFolder);
            return testtasks;
        }

        public List<string> FindFiles(string baseFolder)
        {
            return FindRakeFile(baseFolder);
        }

        public bool FindBundlerFile(string baseFolder)
        {
            var testtasks = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "GEMFILE", baseFolder);
            return testtasks.Count > 0;
        }
    }

    public class TestTaskFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public TestTaskFileFinder(PathFinder pathFinder)
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
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE", baseFolder);
            return testtasks;
        }

        public List<string> FindFiles(string baseFolder)
        {
            return FindAllTestTaskFiles(baseFolder);
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }
    }

    public class SolutionFileFinder : FileFinder

    {
        readonly PathFinder pathFinder;

        public SolutionFileFinder(PathFinder pathFinder)
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
            pathFinder.FindFilesRecursively(s => testtasks.Add(s.Remove(0, baseFolderFull.Length + 1)), "RAKEFILE", baseFolder);
            return testtasks;
        }

        public List<string> FindFiles(string baseFolder)
        {
            return FindAllSolutionFiles(baseFolder);
        }

        public bool FindBundlerFile(string baseFolder)
        {
            throw new NotImplementedException();
        }
    }

    public class NUnitFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public NUnitFileFinder(PathFinder pathFinder)
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
            throw new NotImplementedException();
        }

        public List<string> FindAllTestTaskFiles(string baseFolder)
        {
            throw new NotImplementedException();
        }

        public List<string> FindRakeFile(string baseFolder)
        {
            throw new NotImplementedException();
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