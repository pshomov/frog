﻿using System.Collections.Generic;
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