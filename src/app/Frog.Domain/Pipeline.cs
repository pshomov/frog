using System.Collections.Generic;
using System.Threading;
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

    public interface TaskDispenser
    {
        List<ExecTask> GimeTasks();
    }

    public class FixedTasksDispenser : TaskDispenser
    {
        readonly ExecTask[] tasks;

        public FixedTasksDispenser(params ExecTask[] tasks)
        {
            this.tasks = tasks;
        }

        public List<ExecTask> GimeTasks()
        {
            return new List<ExecTask>(tasks);
        }
    }

    public class BuildStarted : Event
    {
    }

    public class TaskStarted : Event
    {
    }

    public class TaskFinished : Event
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
        class DummyBus : IEventPublisher
        {
            public void Publish<T>(T @event) where T : Event
            {
            }
        }

        readonly IEventPublisher eventPublisher;
        readonly TaskDispenser tasksDispenser;

        public PipelineOfTasks(IEventPublisher eventPublisher, TaskDispenser tasksDispenser)
        {
            this.eventPublisher = eventPublisher;
            this.tasksDispenser = tasksDispenser;
        }

        public PipelineOfTasks(TaskDispenser taskDispenser) : this(new DummyBus(), taskDispenser)
        {
        }

        public void Process(SourceDrop sourceDrop)
        {
            eventPublisher.Publish(new BuildStarted());
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            foreach (var task in tasksDispenser.GimeTasks())
            {
                eventPublisher.Publish(new TaskStarted());
                lastTaskStatus = task.Perform(sourceDrop).ExecStatus;
                eventPublisher.Publish(new TaskFinished());
                if (lastTaskStatus != ExecTaskResult.Status.Success) break;
            }

            eventPublisher.Publish(lastTaskStatus == ExecTaskResult.Status.Error
                                       ? new BuildEnded(BuildEnded.BuildStatus.Error)
                                       : new BuildEnded(BuildEnded.BuildStatus.Success));
        }
    }

}