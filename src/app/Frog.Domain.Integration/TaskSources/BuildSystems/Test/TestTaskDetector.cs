using System.Collections.Generic;
using System.Linq;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.Test
{
    public class TestTaskDetector : TaskSource
    {
        public TestTaskDetector(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var testTaskFiles = taskFileFinder.FindFiles(projectFolder);
            return testTaskFiles.Select(s => new TestTaskDescription(s));
        }

        readonly TaskFileFinder taskFileFinder;
    }

}