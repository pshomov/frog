using System;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Specs.Support
{
    public class FixedTasksDispenser : TaskSource
    {
        readonly ITask[] tasks;

        public FixedTasksDispenser(params ITask[] tasks)
        {
            this.tasks = tasks;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            return new List<ITask>(tasks);
        }
    }
}