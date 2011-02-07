using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Support;

namespace Frog.Domain.TaskDetection
{
    public class MSBuildDetector
    {
        readonly FileFinder fileFinder;

        public MSBuildDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<MSBuildTaskDescriptions> Detect()
        {
            var allSolutionFiles = fileFinder.FindAllSolutionFiles();
            if (allSolutionFiles.Count > 0)
            {
                var rootFolderSolutions =
                    allSolutionFiles.Where(slnPath => slnPath.IndexOf(Path.DirectorySeparatorChar) == -1).ToList();
                var rootBuildSlnIdx =
                    rootFolderSolutions.FindIndex(
                        s => s.Equals("build.sln", StringComparison.InvariantCultureIgnoreCase));
                if (rootBuildSlnIdx > -1)
                    return As.List(new MSBuildTaskDescriptions(rootFolderSolutions[rootBuildSlnIdx]));
                if (rootFolderSolutions.Count > 1) return new List<MSBuildTaskDescriptions>();
                return As.List(new MSBuildTaskDescriptions(rootFolderSolutions[0]));
            }

            return new List<MSBuildTaskDescriptions>();
        }
    }
}