using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.TaskSources;
using Newtonsoft.Json;

namespace Frog.Domain.BuildSystems.Custom
{
    public class CustomTasksDetector : TaskSource
    {
        readonly TaskFileFinder taskFileFinder;
        readonly Func<string, string> getContent;

        public CustomTasksDetector(TaskFileFinder taskFileFinder, Func<string, string> getContent)
        {
            this.taskFileFinder = taskFileFinder;
            this.getContent = getContent;
        }

        public IEnumerable<Task> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var configPrototype = new { pipeline = new[] { new { stage = "stage name", tasks = new[] { "task 1", "task2" } } } };
            var foundFiles = taskFileFinder.FindFiles(projectFolder);
            if (foundFiles.Count == 0) return new List<Task>();
            shouldStop = true;
            var parsedConfig = JsonConvert.DeserializeAnonymousType(getContent(Path.Combine(projectFolder, foundFiles.Single())), configPrototype);
            return parsedConfig.pipeline.SelectMany(arg => arg.tasks.Select(s => new ShellTask {Arguments = s, Name = s}));
        }
    }
}