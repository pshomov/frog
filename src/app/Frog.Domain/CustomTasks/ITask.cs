using System;

namespace Frog.Domain.CustomTasks
{
    public interface ITask
    {
        string Name { get; }
    }

    public class TaskBase : ITask
    {
        public string Name { get; set; }
    }
}