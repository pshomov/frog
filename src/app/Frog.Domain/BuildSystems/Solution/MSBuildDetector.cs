using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.TaskSources;
using Frog.Support;

namespace Frog.Domain.BuildSystems.Solution
{
    public class MSBuildDetector : TaskSource
    {
        readonly TaskFileFinder _taskFileFinder;

        public MSBuildDetector(TaskFileFinder _taskFileFinder)
        {
            this._taskFileFinder = _taskFileFinder;
        }

        public IList<Task> Detect(string projectFolder)
        {
            var allSolutionFiles = _taskFileFinder.FindFiles(projectFolder);
            if (allSolutionFiles.Count == 1) return As.List<Task>(new MSBuildTask(allSolutionFiles[0]));
            if (allSolutionFiles.Count > 0)
            {
                var rootFolderSolutions =
                    allSolutionFiles.Where(slnPath => slnPath.IndexOf(Path.DirectorySeparatorChar) == -1).ToList();
				if (rootFolderSolutions.Count == 0) return new List<Task>();
                var rootBuildSlnIdx =
                    rootFolderSolutions.FindIndex(
                        s => s.Equals("build.sln", StringComparison.InvariantCultureIgnoreCase));
                if (rootBuildSlnIdx > -1)
                    return As.List<Task>(new MSBuildTask(rootFolderSolutions[rootBuildSlnIdx]));
                if (rootFolderSolutions.Count > 1) return new List<Task>();
                return As.List<Task>(new MSBuildTask(rootFolderSolutions[0]));
            }

            return new List<Task>();
        }
    }
}