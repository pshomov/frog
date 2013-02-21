using System;
using System.Collections.Generic;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Rake
{
    public class RubyTaskDetector : TaskSource
    {
        public RubyTaskDetector(TaskFileFinder rakeTaskFileFinder, TaskFileFinder bundlerFileFinder)
        {
            this.rakeTaskFileFinder = rakeTaskFileFinder;
            this.bundlerFileFinder = bundlerFileFinder;
        }

        public IList<Task> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var rakeFile = rakeTaskFileFinder.FindFiles(projectFolder);
            var bundlerFile = bundlerFileFinder.FindFiles(projectFolder);
            var tasks = new List<Task>();
            if (
                rakeFile.Exists(
                    s =>
                    s.Equals("Rakefile", StringComparison.InvariantCultureIgnoreCase) ||
                    s.Equals("Rakefile.rb", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (bundlerFile.Count > 0) tasks.Add(new BundlerTask());
                tasks.Add(new RakeTask());
            }
            return tasks;
        }

        readonly TaskFileFinder rakeTaskFileFinder;
        readonly TaskFileFinder bundlerFileFinder;
    }
}