using System;
using System.Collections.Generic;
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
        List<ExecTask> GimeTasks(string projectFolder);
    }

    public class BuildStarted : Event
    {
        public PipelineStatus Status;
    }

    public class BuildUpdated : Event
    {
        public PipelineStatus Status;
    }

    public class TaskStarted : Event
    {
        public TaskStarted(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class TaskFinished : Event
    {
        readonly string taskName;
        readonly ExecTaskResult.Status status;

        public TaskFinished(string taskName, ExecTaskResult.Status status)
        {
            this.taskName = taskName;
            this.status = status;
        }

        public ExecTaskResult.Status Status
        {
            get { return status; }
        }

        public string TaskName
        {
            get { return taskName; }
        }
    }

    public class PipelineStatus
    {
        public PipelineStatus(Guid id)
        {
            tasks = new List<TasksInfo>();
            PipelineId = id;
        }

        public PipelineStatus(PipelineStatus pipelineStatus)
        {
            PipelineId = pipelineStatus.PipelineId;
            tasks = new List<TasksInfo>();
            pipelineStatus.tasks.ForEach(info => tasks.Add(new TasksInfo(info)));
        }

        public Guid PipelineId;
        public List<TasksInfo> tasks;
    }

    public class TasksInfo
    {
        public TasksInfo()
        {
        }

        public TasksInfo(TasksInfo tasksInfo)
        {
            Name = tasksInfo.Name;
            Status = tasksInfo.Status;
        }

        public enum TaskStatus {NotStarted, Started, FinishedSuccess, FinishedError}
        public string Name;
        public TaskStatus Status { get; set; }
    }

    public class BuildEnded : Event
    {
        public enum BuildStatus
        {
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
        readonly TaskSource tasksSource;
        readonly IExecTaskGenerator execTaskGenerator;

        public PipelineOfTasks(IEventPublisher eventPublisher, TaskSource tasksSource, IExecTaskGenerator execTaskGenerator)
        {
            this.eventPublisher = eventPublisher;
            this.tasksSource = tasksSource;
            this.execTaskGenerator = execTaskGenerator;
        }

        public PipelineOfTasks(TaskSource taskDispenser, IExecTaskGenerator execTaskGenerator) : this(new DummyBus(), taskDispenser, execTaskGenerator)
        {
        }

        public void Process(SourceDrop sourceDrop)
        {
            List<ExecTask> execTasks = GenerateTasks(sourceDrop);
            RunTasks(sourceDrop, execTasks);
        }

        void RunTasks(SourceDrop sourceDrop, List<ExecTask> execTasks)
        {
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            PipelineStatus status = GeneratePipelineStatus(execTasks);
            eventPublisher.Publish(new BuildStarted {Status = new PipelineStatus(status)});

            for (int i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                status.tasks[i].Status = TasksInfo.TaskStatus.Started;
                eventPublisher.Publish(new BuildUpdated {Status = new PipelineStatus(status)});
                lastTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                status.tasks[i].Status = lastTaskStatus == ExecTaskResult.Status.Error ? TasksInfo.TaskStatus.FinishedError : TasksInfo.TaskStatus.FinishedSuccess;
                eventPublisher.Publish(new BuildUpdated { Status = new PipelineStatus(status) });
                if (lastTaskStatus != ExecTaskResult.Status.Success) break;
            }

            eventPublisher.Publish(lastTaskStatus == ExecTaskResult.Status.Error
                                       ? new BuildEnded(BuildEnded.BuildStatus.Error)
                                       : new BuildEnded(BuildEnded.BuildStatus.Success));
        }

        List<ExecTask> GenerateTasks(SourceDrop sourceDrop)
        {
            var execTasks = new List<ExecTask>();
            foreach (var task in tasksSource.Detect(sourceDrop.SourceDropLocation))
            {
                execTasks.AddRange(execTaskGenerator.GimeTasks(task));
            }
            return execTasks;
        }

        PipelineStatus GeneratePipelineStatus(List<ExecTask> execTasks)
        {
            var pipelineStatus = new PipelineStatus(Guid.NewGuid());
            foreach (var execTask in execTasks)
            {
                pipelineStatus.tasks.Add(new TasksInfo
                                             {Name = execTask.Name, Status = TasksInfo.TaskStatus.NotStarted});
            }
            return pipelineStatus;
        }
    }

}