using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Make
{
    public class MakeTaskDetector : TaskSource
    {
        private readonly TaskFileFinder taskFileFinder;

        public MakeTaskDetector(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var result = new List<ITask>();
            List<string> foundFiles = taskFileFinder.FindFiles(projectFolder);
            if (foundFiles.Count == 1) result.Add(new MakeTask());
            return result;
        }
    }
}