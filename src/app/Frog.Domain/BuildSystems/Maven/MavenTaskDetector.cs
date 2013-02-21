using System;
using System.Collections.Generic;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.Maven
{
    public class MavenTaskDetector : TaskSource
    {
        public MavenTaskDetector(TaskFileFinder _taskFileFinder)
        {
            this._taskFileFinder = _taskFileFinder;
        }

        public IList<Task> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var tasks = new List<Task>();
            return tasks;
        }

        readonly TaskFileFinder _taskFileFinder;
    }
}