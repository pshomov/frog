﻿using System;
using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Rake
{
    public class RubyTaskDetector : TaskSource
    {
        readonly TaskFileFinder rakeTaskFileFinder;
        private readonly TaskFileFinder bundlerFileFinder;

        public RubyTaskDetector(TaskFileFinder rakeTaskFileFinder, TaskFileFinder bundlerFileFinder)
        {
            this.rakeTaskFileFinder = rakeTaskFileFinder;
            this.bundlerFileFinder = bundlerFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var rakeFile = rakeTaskFileFinder.FindFiles(projectFolder);
            var bundlerFile = bundlerFileFinder.FindFiles(projectFolder);
            var tasks = new List<ITask>();
            if (rakeFile.Exists(s => s.Equals("Rakefile",StringComparison.InvariantCultureIgnoreCase) || s.Equals("Rakefile.rb", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (bundlerFile.Count>0) tasks.Add(new BundlerTask());
                tasks.Add(new RakeTask());
            }
            return tasks;
        }
    }
}