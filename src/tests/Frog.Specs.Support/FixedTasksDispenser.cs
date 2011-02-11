using System.Collections.Generic;
using Frog.Domain;

namespace Frog.Specs.Support
{
    public class FixedTasksDispenser : TaskDispenser
    {
        readonly ExecTask[] tasks;

        public FixedTasksDispenser(params ExecTask[] tasks)
        {
            this.tasks = tasks;
        }

        public List<ExecTask> GimeTasks(string projectFolder)
        {
            return new List<ExecTask>(tasks);
        }
    }
}