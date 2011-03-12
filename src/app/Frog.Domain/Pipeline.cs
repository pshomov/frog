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
        event Action<PipelineStatus> OnBuildStarted;
        event Action<PipelineStatus> OnBuildUpdated;
        event Action<BuildTotalStatus> OnBuildEnded;
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

    public enum BuildTotalStatus
    {
        Error,
        Success
    }

    public class PipelineOfTasks : Pipeline
    {
        readonly TaskSource tasksSource;
        readonly IExecTaskGenerator execTaskGenerator;

        public PipelineOfTasks(TaskSource tasksSource, IExecTaskGenerator execTaskGenerator)
        {
            this.tasksSource = tasksSource;
            this.execTaskGenerator = execTaskGenerator;
        }

        public void Process(SourceDrop sourceDrop)
        {
            List<ExecTask> execTasks = GenerateTasks(sourceDrop);
            RunTasks(sourceDrop, execTasks);
        }

        public event Action<PipelineStatus> OnBuildStarted;
        public event Action<PipelineStatus> OnBuildUpdated;
        public event Action<BuildTotalStatus> OnBuildEnded;

        void RunTasks(SourceDrop sourceDrop, List<ExecTask> execTasks)
        {
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            PipelineStatus status = GeneratePipelineStatus(execTasks);
            OnBuildStarted(new PipelineStatus(status));

            for (int i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                status.tasks[i].Status = TasksInfo.TaskStatus.Started;
                OnBuildUpdated(new PipelineStatus(status));
                lastTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                status.tasks[i].Status = lastTaskStatus == ExecTaskResult.Status.Error ? TasksInfo.TaskStatus.FinishedError : TasksInfo.TaskStatus.FinishedSuccess;
                OnBuildUpdated(new PipelineStatus(status));
                if (lastTaskStatus != ExecTaskResult.Status.Success) break;
            }

            OnBuildEnded(lastTaskStatus == ExecTaskResult.Status.Error
                                       ? BuildTotalStatus.Error
                                       : BuildTotalStatus.Success);
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