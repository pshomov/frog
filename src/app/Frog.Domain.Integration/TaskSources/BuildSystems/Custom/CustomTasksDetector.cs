using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.Custom
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

        public IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var config_prototype = new { pipeline = new[] { new { stage = "stage name", tasks = new[] { "task 1", "task2" } } }, start_each_stage = new[]{"task1"} };
            var found_files = taskFileFinder.FindFiles(projectFolder);
            if (found_files.Count == 0) return new List<TaskDescription>();
            shouldStop = true;
            var parsed_config = JsonConvert.DeserializeAnonymousType(getContent(Path.Combine(projectFolder, found_files.Single())), config_prototype);
            var start_tasks = (parsed_config.start_each_stage ?? new string[0]).Select(s => new ShellTaskDescription {Arguments = s, Name = s});
            return parsed_config.pipeline.SelectMany(arg => start_tasks.Concat(arg.tasks.Select(s => new ShellTaskDescription {Arguments = s, Name = s})));
        }
    }
}