using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Support;

namespace Frog.Domain.TaskSources
{
    public class RakeTaskDetector : TaskSource
    {
        readonly FileFinder fileFinder;

        public RakeTaskDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var rakeFile = fileFinder.FindRakeFile(projectFolder);
            if (rakeFile.Exists(s => s == "Rakefile")) return As.List((ITask) new RakeTask());
            return new List<ITask>();
        }
    }
}