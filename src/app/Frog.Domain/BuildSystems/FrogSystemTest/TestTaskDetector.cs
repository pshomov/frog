using System.Collections.Generic;
using System.Linq;
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

        public IList<Task> Detect(string projectFolder)
        {
            var testTaskFiles = taskFileFinder.FindFiles(projectFolder);
            return testTaskFiles.Select(s => (Task) new TestTaskDescription(s)).ToList();
        }
    }

    public class TestTaskDescription : Task
    {
        public readonly string path;

        public TestTaskDescription(string path)
        {
            this.path = path;
        }
    }

    public class FakeTaskDescription : Task
    {
        public readonly string[] messages;

        public FakeTaskDescription(params string[] messages)
        {
            this.messages = messages;
        }

    }

}