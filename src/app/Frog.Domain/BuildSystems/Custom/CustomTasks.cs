using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.TaskSources;
using Newtonsoft.Json;

namespace Frog.Domain.BuildSystems.Custom
{
    public class CustomTasks : TaskSource
    {
        readonly TaskFileFinder taskFileFinder;

        public CustomTasks(TaskFileFinder taskFileFinder)
        {
            this.taskFileFinder = taskFileFinder;
        }

        public IList<Task> Detect(string projectFolder, Func<string, string> getContent)
        {
            var configPrototype = new {pipeline = new[] {new {name = "stage", tasks = new[] {"task 1", "task2"}}}};
            var foundFiles = taskFileFinder.FindFiles(projectFolder);
            var parsedConfig = JsonConvert.DeserializeAnonymousType(getContent(foundFiles.Single()), configPrototype);
            return parsedConfig.pipeline.SelectMany(arg => arg.tasks.Select(s => (Task)new ShellTask {args = s, cmd = ""})).ToList();
        }
    }
}