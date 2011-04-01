using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain.TaskSources;

namespace Frog.Domain
{
    public class SourceDrop
    {
        readonly string sourceDropLocation;

        public SourceDrop(string sourceDropLocation)
        {
            this.sourceDropLocation = sourceDropLocation;
        }

        public string SourceDropLocation
        {
            get { return sourceDropLocation; }
        }
    }

    public interface Pipeline
    {
        void Process(SourceDrop sourceDrop);
        event Action<PipelineStatus> OnBuildStarted;
        event Action<PipelineStatus> OnBuildUpdated;
        event Action<BuildTotalStatus> OnBuildEnded;
        event Action<TerminalUpdateInfo> OnTerminalUpdate;
    }

    public class TerminalUpdateInfo
    {
        public TerminalUpdateInfo(int contentSequenceIndex, string content, int taskIndex)
        {
            ContentSequenceIndex = contentSequenceIndex;
            Content = content;
            TaskIndex = taskIndex;
        }

        public string Content { get; private set; }
        public int ContentSequenceIndex { get; private set; }
        public int TaskIndex { get; private set; }
    }

    public class PipelineStatus
    {
        public PipelineStatus(Guid id)
        {
            Tasks = new List<TaskInfo>();
            PipelineId = id;
        }

        public PipelineStatus(PipelineStatus pipelineStatus)
        {
            PipelineId = pipelineStatus.PipelineId;
            Tasks = new List<TaskInfo>();

            pipelineStatus.Tasks.ToList().ForEach(info => Tasks.Add(new TaskInfo(info)));
        }

        public Guid PipelineId;
        public IList<TaskInfo> Tasks;
    }

    public class TaskInfo
    {
        public TaskInfo()
        {
        }

        public TaskInfo(string name)
        {
            Name = name;
            Status = TaskStatus.NotStarted;
        }

        public TaskInfo(TaskInfo taskInfo)
        {
            Name = taskInfo.Name;
            Status = taskInfo.Status;
        }

        public enum TaskStatus
        {
            NotStarted,
            Started,
            FinishedSuccess,
            FinishedError
        }

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

        public event Action<PipelineStatus> OnBuildStarted = status => {};
        public event Action<PipelineStatus> OnBuildUpdated = status => {};
        public event Action<BuildTotalStatus> OnBuildEnded = status => {};
        public event Action<TerminalUpdateInfo> OnTerminalUpdate = info => {};

        void RunTasks(SourceDrop sourceDrop, List<ExecTask> execTasks)
        {
            ExecTaskResult.Status lastTaskStatus = ExecTaskResult.Status.Success;
            PipelineStatus status = GeneratePipelineStatus(execTasks);
            OnBuildStarted(new PipelineStatus(status));

            for (int i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                status.Tasks[i].Status = TaskInfo.TaskStatus.Started;
                OnBuildUpdated(new PipelineStatus(status));
                int sequneceIndex = 0;
                Action<string> execTaskOnOnTerminalOutputUpdate = s => OnTerminalUpdate(new TerminalUpdateInfo(sequneceIndex++, s, i));
                execTask.OnTerminalOutputUpdate += execTaskOnOnTerminalOutputUpdate;
                lastTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                execTask.OnTerminalOutputUpdate -= execTaskOnOnTerminalOutputUpdate;
                status.Tasks[i].Status = lastTaskStatus == ExecTaskResult.Status.Error
                                             ? TaskInfo.TaskStatus.FinishedError
                                             : TaskInfo.TaskStatus.FinishedSuccess;
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
                pipelineStatus.Tasks.Add(new TaskInfo
                                             {Name = execTask.Name, Status = TaskInfo.TaskStatus.NotStarted});
            }
            return pipelineStatus;
        }
    }
}