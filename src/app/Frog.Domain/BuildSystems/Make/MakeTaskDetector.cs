using System;
using System.Collections.Generic;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Make
{
    public class MakeTaskDetector : TaskSource
    {
        public MakeTaskDetector(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IList<Task> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var result = new List<Task>();
            List<string> foundFiles = taskFileFinder.FindFiles(projectFolder);
            if (foundFiles.Count == 1) result.Add(new MakeTask());
            return result;
        }

        readonly TaskFileFinder taskFileFinder;
    }
}