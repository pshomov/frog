using System.Collections.Generic;
using System.Linq;
using Frog.Domain.CustomTasks;

namespace Frog.Domain.TaskSources
{
    public class TestTaskDetector : TaskSource
    {
        readonly FileFinder fileFinder;

        public TestTaskDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var allSolutionFiles = fileFinder.FindAllTestTaskFiles(projectFolder);
            return allSolutionFiles.Select(s => (ITask) new TestTaskDescription(s)).ToList();
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