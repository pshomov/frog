using System;
using System.Collections.Generic;

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

    public delegate void ProjectCheckedOutDelegate(CheckoutInfo info);
    public delegate void BuildStartedDelegate(PipelineStatus status);

    public interface Pipeline
    {
        void Process(SourceDrop sourceDrop);
        event BuildStartedDelegate OnBuildStarted;
        event Action<int, Guid, TaskInfo.TaskStatus> OnBuildUpdated;
        event Action<BuildTotalEndStatus> OnBuildEnded;
        event Action<TerminalUpdateInfo> OnTerminalUpdate;
    }

    public class TerminalUpdateInfo
    {
        public TerminalUpdateInfo(int contentSequenceIndex, string content, int taskIndex, Guid terminalId)
        {
            ContentSequenceIndex = contentSequenceIndex;
            Content = content;
            TaskIndex = taskIndex;
            TerminalId = terminalId;
        }

        public string Content { get; private set; }
        public int ContentSequenceIndex { get; private set; }
        public int TaskIndex { get; private set; }
        public Guid TerminalId { get; private set; }
    }

    public class PipelineStatus
    {
        public List<TaskInfo> Tasks = new List<TaskInfo>();
    }

    public class TaskInfo
    {
        public Guid TerminalId { get; set; }

        public TaskInfo()
        {
        }

        public TaskInfo(string name, Guid terminalId)
        {
            TerminalId = terminalId;
            Name = name;
            Status = TaskStatus.NotStarted;
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
}