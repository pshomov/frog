using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.FrogSystemTest
{
    public class TestTaskDetector : TaskSource
    {
        public TestTaskDetector(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IEnumerable<Task> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var testTaskFiles = taskFileFinder.FindFiles(projectFolder);
            return testTaskFiles.Select(s => new TestTask(s));
        }

        readonly TaskFileFinder taskFileFinder;
    }

}