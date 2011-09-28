using System.Collections.Generic;

namespace Frog.Domain.BuildSystems.Maven
{
    public class MavenTaskDetector : TaskSources.TaskSource
    {
        readonly TaskFileFinder _taskFileFinder;

        public MavenTaskDetector(TaskFileFinder _taskFileFinder)
        {
            this._taskFileFinder = _taskFileFinder;
        }

        public IList<Task> Detect(string projectFolder)
        {
            var tasks = new List<Task>();
            return tasks;
        }
    }
}