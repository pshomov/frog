using System.Collections.Generic;
using System.Linq;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.FrogSystemTest
{
    public class TestTaskDetector : TaskSource
    {
        readonly TaskFileFinder _taskFileFinder;

        public TestTaskDetector(TaskFileFinder _taskFileFinder)
        {
            this._taskFileFinder = _taskFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var testTaskFiles = _taskFileFinder.FindFiles(projectFolder);
            return testTaskFiles.Select(s => (ITask) new TestTaskDescription(s)).ToList();
        }
    }

    public class TestTaskDescription : ITask
    {
        public readonly string path;

        public TestTaskDescription(string path)
        {
            this.path = path;
        }
    }
}