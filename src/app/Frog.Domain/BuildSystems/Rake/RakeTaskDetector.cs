using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Rake
{
    public class RakeTaskDetector : TaskSource
    {
        readonly TaskFileFinder _rakeTaskFileFinder;
        private readonly TaskFileFinder _bundlerFileFinder;

        public RakeTaskDetector(TaskFileFinder _rakeTaskFileFinder, TaskFileFinder bundlerFileFinder)
        {
            this._rakeTaskFileFinder = _rakeTaskFileFinder;
            _bundlerFileFinder = bundlerFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var rakeFile = _rakeTaskFileFinder.FindFiles(projectFolder);
            var bundlerFile = _bundlerFileFinder.FindFiles(projectFolder);
            var tasks = new List<ITask>();
            if (rakeFile.Exists(s => s == "Rakefile"))
            {
                if (bundlerFile.Count>0) tasks.Add(new BundlerTask());
                tasks.Add(new RakeTask());
            }
            return tasks;
        }
    }
}