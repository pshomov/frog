using System;
using System.Linq;

namespace Frog.Domain
{
    public class SourceDrop
    {
        readonly string _sourceDropLocation;

        public SourceDrop(string sourceDropLocation)
        {
            _sourceDropLocation = sourceDropLocation;
        }

        public string SourceDropLocation
        {
            get { return _sourceDropLocation; }
        }
    }

    public interface Pipeline
    {
        void Process(SourceDrop sourceDrop);
    }

    public class PipelineOfTasks : Pipeline
    {
        private readonly Task[] _tasks;

        public PipelineOfTasks(params Task[] tasks)
        {
            _tasks = tasks;
        }

        public void Process(SourceDrop sourceDrop)
        {
            _tasks.ToList().Find(task => task.Perform(sourceDrop).status != TaskResult.Status.Success);
        }
    }
}