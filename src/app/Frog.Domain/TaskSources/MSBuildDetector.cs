using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.CustomTasks;
using Frog.Support;

namespace Frog.Domain.TaskSources
{
    public class MSBuildDetector : TaskSource
    {
        readonly FileFinder fileFinder;

        public MSBuildDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var allSolutionFiles = fileFinder.FindAllSolutionFiles(projectFolder);
            if (allSolutionFiles.Count == 1) return As.List<ITask>(new MSBuildTask(allSolutionFiles[0]));
            if (allSolutionFiles.Count > 0)
            {
                var rootFolderSolutions =
                    allSolutionFiles.Where(slnPath => slnPath.IndexOf(Path.DirectorySeparatorChar) == -1).ToList();
                var rootBuildSlnIdx =
                    rootFolderSolutions.FindIndex(
                        s => s.Equals("build.sln", StringComparison.InvariantCultureIgnoreCase));
                if (rootBuildSlnIdx > -1)
                    return As.List<ITask>(new MSBuildTask(rootFolderSolutions[rootBuildSlnIdx]));
                if (rootFolderSolutions.Count > 1) return new List<ITask>();
                return As.List<ITask>(new MSBuildTask(rootFolderSolutions[0]));
            }

            return new List<ITask>();
        }
    }
}