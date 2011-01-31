using System;
using System.Linq;
using Frog.Domain.Specs;
using SimpleCQRS;

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

    public class BuildStarted : Event
    {
    }

    public class BuildEnded : Event
    {
        public enum BuildStatus
        {
            Fail,
            Error,
            Success
        }
        public readonly BuildStatus Status;

        public BuildEnded(BuildStatus status)
        {
            Status = status;
        }
    }

    public class PipelineOfTasks : Pipeline
    {
        class FakeBus : IEventPublisher
        {
            public void Publish<T>(T @event) where T : Event
            {
            }
        }
        readonly IEventPublisher eventPublisher;
        private readonly ExecTask[] _tasks;

        public PipelineOfTasks(IEventPublisher eventPublisher, params ExecTask[] tasks)
        {
            this.eventPublisher = eventPublisher;
            _tasks = tasks;
        }

        public PipelineOfTasks(params ExecTask[] tasks) : this(new FakeBus(), tasks)
        {
        }

        public void Process(SourceDrop sourceDrop)
        {
            eventPublisher.Publish(new BuildStarted());
            if (_tasks.ToList().Exists(task => task.Perform(sourceDrop).ExecStatus != ExecTaskResult.Status.Success))
                eventPublisher.Publish(new BuildEnded(BuildEnded.BuildStatus.Fail));
            else 
                eventPublisher.Publish(new BuildEnded(BuildEnded.BuildStatus.Success));
        }
    }
}