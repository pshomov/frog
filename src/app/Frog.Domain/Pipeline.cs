using System.Linq;
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
        readonly ExecTask[] _tasks;

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
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            if (
                _tasks.ToList().Exists(
                    task => (lastTaskStatus = task.Perform(sourceDrop).ExecStatus) != ExecTaskResult.Status.Success))
            {
                if (lastTaskStatus == ExecTaskResult.Status.Error)
                    eventPublisher.Publish(new BuildEnded(BuildEnded.BuildStatus.Error));
            }
            else
            {
                eventPublisher.Publish(new BuildEnded(BuildEnded.BuildStatus.Success));
            }
        }
    }
}