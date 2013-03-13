using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.TaskSources;
using Frog.Support;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.DotNet
{
    public class MSBuildDetector : TaskSource
    {
        public MSBuildDetector(TaskFileFinder taskFileFinder, OS os)
        {
            this.taskFileFinder = taskFileFinder;
            this.os = os;
        }

        public IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var allSolutionFiles = taskFileFinder.FindFiles(projectFolder);
            var name = "Build solution";
            var comand = os == OS.Windows ? "{0}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe".format(Environment.GetEnvironmentVariable("SYSTEMROOT")) : "xbuild";
            if (allSolutionFiles.Count == 1) return As.List<TaskDescription>(new ShellTaskDescription() { Arguments = allSolutionFiles[0], Command = comand, Name = name});
            if (allSolutionFiles.Count > 0)
            {
                var rootFolderSolutions =
                    allSolutionFiles.Where(slnPath => slnPath.IndexOf(Path.DirectorySeparatorChar) == -1).ToList();
                if (rootFolderSolutions.Count == 0) return new List<TaskDescription>();
                var rootBuildSlnIdx =
                    rootFolderSolutions.FindIndex(
                        s => s.Equals("build.sln", StringComparison.InvariantCultureIgnoreCase));
                if (rootBuildSlnIdx > -1)
                    return As.List<TaskDescription>(new ShellTaskDescription(){Arguments = rootFolderSolutions[rootBuildSlnIdx], Command = comand, Name = name});
                if (rootFolderSolutions.Count > 1) return new List<TaskDescription>();
                return
                    As.List(new ShellTaskDescription()
                        {
                            Command =
                                comand,
                            Arguments = rootFolderSolutions[0],
                            Name =
                                name
                        });
            }

            return new List<TaskDescription>();
        }

        readonly TaskFileFinder taskFileFinder;
        private readonly OS os;
    }
}