using System;
using System.Linq;
using Frog.Domain.Specs;

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
        private readonly ExecTask[] _tasks;

        public PipelineOfTasks(params ExecTask[] tasks)
        {
            _tasks = tasks;
        }

        public void Process(SourceDrop sourceDrop)
        {
            _tasks.ToList().Find(task => task.Perform(sourceDrop).ExecStatus != ExecTaskResult.Status.Success);
        }
    }
}