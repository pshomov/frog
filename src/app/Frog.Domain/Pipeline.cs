using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain.ExecTasks;
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

    public delegate void BuildStartedDelegate(PipelineStatus status);

    public interface Pipeline
    {
        void Process(SourceDrop sourceDrop);
        event BuildStartedDelegate OnBuildStarted;
        event Action<int, TaskInfo.TaskStatus> OnBuildUpdated;
        event Action<BuildTotalEndStatus> OnBuildEnded;
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
        public PipelineStatus()
        {
        }

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
        public List<TaskInfo> Tasks;
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

    public enum BuildTotalEndStatus
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
            List<IExecTask> execTasks = GenerateTasks(sourceDrop);
            RunTasks(sourceDrop, execTasks);
        }

        public event BuildStartedDelegate OnBuildStarted = status => {};
        public event Action<int, TaskInfo.TaskStatus> OnBuildUpdated = (i,status) => {};
        public event Action<BuildTotalEndStatus> OnBuildEnded = status => {};
        public event Action<TerminalUpdateInfo> OnTerminalUpdate = info => {};

        void RunTasks(SourceDrop sourceDrop, List<IExecTask> execTasks)
        {
            ExecTaskResult.Status execTaskStatus = ExecTaskResult.Status.Success;
            PipelineStatus status = GeneratePipelineStatus(execTasks);
            OnBuildStarted(status);

            for (int i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                OnBuildUpdated(i, TaskInfo.TaskStatus.Started);
                int sequneceIndex = 0;
                Action<string> execTaskOnOnTerminalOutputUpdate = s => OnTerminalUpdate(new TerminalUpdateInfo(sequneceIndex++, s, i));
                execTask.OnTerminalOutputUpdate += execTaskOnOnTerminalOutputUpdate;
                try
                {
                    execTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                }
                catch(Exception e)
                {
                    execTaskStatus = ExecTaskResult.Status.Error;
                    execTaskOnOnTerminalOutputUpdate("Runz>> Exception running the task:" + e);
                }
                execTask.OnTerminalOutputUpdate -= execTaskOnOnTerminalOutputUpdate;
                var taskStatus = execTaskStatus == ExecTaskResult.Status.Error
                                             ? TaskInfo.TaskStatus.FinishedError
                                             : TaskInfo.TaskStatus.FinishedSuccess;
                OnBuildUpdated(i, taskStatus);
                if (execTaskStatus != ExecTaskResult.Status.Success) break;
            }

            OnBuildEnded(execTaskStatus == ExecTaskResult.Status.Error
                             ? BuildTotalEndStatus.Error
                             : BuildTotalEndStatus.Success);
        }

        List<IExecTask> GenerateTasks(SourceDrop sourceDrop)
        {
            var execTasks = new List<IExecTask>();
            foreach (var task in tasksSource.Detect(sourceDrop.SourceDropLocation))
            {
                execTasks.AddRange(execTaskGenerator.GimeTasks(task));
            }
            return execTasks;
        }

        PipelineStatus GeneratePipelineStatus(List<IExecTask> execTasks)
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