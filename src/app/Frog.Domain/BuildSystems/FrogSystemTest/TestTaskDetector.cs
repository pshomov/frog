using System.Collections.Generic;
using System.Linq;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain.BuildSystems.FrogSystemTest
{
    public class TestTaskDetector : TaskSource
    {
        readonly TaskFileFinder taskFileFinder;

        public TestTaskDetector(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var testTaskFiles = taskFileFinder.FindFiles(projectFolder);
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

    public class FakeTaskDescription : ITask
    {
        public readonly string[] messages;

        public FakeTaskDescription(params string[] messages)
        {
            this.messages = messages;
        }

    }

}