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
        readonly IExecTask[] tasks;

        public FixedExecTaskGenerator(params IExecTask[] tasks)
        {
            this.tasks = tasks;
        }

        public List<IExecTask> GimeTasks(ITask task)
        {
            return new List<IExecTask>(tasks);
        }
    }
}
