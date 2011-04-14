using System.Collections.Generic;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.CustomTasks;

namespace Frog.Domain.BuildSystems.Maven.TaskSource
{
    public class MavenTaskDetector : TaskSources.TaskSource
    {
        readonly TaskFileFinder _taskFileFinder;

        public MavenTaskDetector(TaskFileFinder _taskFileFinder)
        {
            this._taskFileFinder = _taskFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var tasks = new List<ITask>();
            return tasks;
        }
    }
}