using System;
using System.Collections.Generic;
using System.Linq;

namespace Frog.Domain
{
    public class SourceDrop
    {
        public string SourceDropLocation
        {
            get { return sourceDropLocation; }
        }

        public SourceDrop(string sourceDropLocation)
        {
            this.sourceDropLocation = sourceDropLocation;
        }

        readonly string sourceDropLocation;
    }

    public delegate void ProjectCheckedOutDelegate(CheckoutInfo info);

    public delegate void BuildStartedDelegate(PipelineStatus status);

    public interface Pipeline
    {
        event Action<BuildTotalEndStatus> OnBuildEnded;
        event BuildStartedDelegate OnBuildStarted;
        event Action<int, Guid, TaskInfo.TaskStatus> OnBuildUpdated;
        event Action<TerminalUpdateInfo> OnTerminalUpdate;
        void Process(SourceDrop sourceDrop);
    }

    public class TerminalUpdateInfo
    {
        public string Content { get; private set; }
        public int ContentSequenceIndex { get; private set; }
        public int TaskIndex { get; private set; }
        public Guid TerminalId { get; private set; }

        public TerminalUpdateInfo(int contentSequenceIndex, string content, int taskIndex, Guid terminalId)
        {
            ContentSequenceIndex = contentSequenceIndex;
            Content = content;
            TaskIndex = taskIndex;
            TerminalId = terminalId;
        }
    }

    public class PipelineStatus
    {
        public List<TaskInfo> Tasks = new List<TaskInfo>();
        public override string ToString()
        {
            return string.Format("Pipeline:\r\n{0}", Tasks.Select(info => info.ToString()).Aggregate((info, taskInfo) => info + "\r\n"+taskInfo ));
        }
    }

    public class TaskInfo
    {
        public enum TaskStatus
        {
            NotStarted,
            Started,
            FinishedSuccess,
            FinishedError
        }

        public string Name;
        public TaskStatus Status { get; set; }
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

        public override string ToString()
        {
            return string.Format("Name: {0}, Status {1}, Id {2}", Name, Status, TerminalId);
        }
    }

    public enum BuildTotalEndStatus
    {
        Error,
        Success
    }
}