using System;
using System.Collections.Generic;
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

    public interface TaskDispencer
    {
        List<ExecTask> GimeTasks();
    }

    public class FixedTasksDispencer : TaskDispencer
    {
        readonly ExecTask[] tasks;

        public FixedTasksDispencer(params ExecTask[] tasks)
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
        readonly TaskDispencer tasksDispencer;

        public PipelineOfTasks(IEventPublisher eventPublisher, TaskDispencer tasksDispencer)
        {
            this.eventPublisher = eventPublisher;
            this.tasksDispencer = tasksDispencer;
        }

        public PipelineOfTasks(TaskDispencer taskDispencer) : this((IEventPublisher) new FakeBus(), taskDispencer)
        {
        }

        public void Process(SourceDrop sourceDrop)
        {
            eventPublisher.Publish(new BuildStarted());
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            if (
                tasksDispencer.GimeTasks().Exists(
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