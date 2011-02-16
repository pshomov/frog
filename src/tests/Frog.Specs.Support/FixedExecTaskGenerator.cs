using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;
using Frog.Domain.CustomTasks;

namespace Frog.Specs.Support
{
    public class FixedExecTaskGenerator : IExecTaskGenerator
    {
        readonly ExecTask[] tasks;

        public FixedExecTaskGenerator(params ExecTask[] tasks)
        {
            this.tasks = tasks;
        }

        public List<ExecTask> GimeTasks(ITask task)
        {
            return new List<ExecTask>(tasks);
        }
    }
}
