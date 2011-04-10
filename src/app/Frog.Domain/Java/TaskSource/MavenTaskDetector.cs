using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Support;

namespace Frog.Domain.Java.TaskSource
{
    public class MavenTaskDetector : TaskSources.TaskSource
    {
        readonly FileFinder fileFinder;

        public MavenTaskDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var rakeFile = fileFinder.FindRakeFile(projectFolder);
            var bundlerFile = fileFinder.FindBundlerFile(projectFolder);
            var tasks = new List<ITask>();
            if (rakeFile.Exists(s => s == "Rakefile"))
            {
                if (bundlerFile) tasks.Add(new BundlerTask());
                tasks.Add(new RakeTask());
            }
            return tasks;
        }
    }
}