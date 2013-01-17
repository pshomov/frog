using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frog.Domain.Integration.UI
{
    public class BuildStatus
    {
        public List<TaskState> tasks = new List<TaskState>();
        public BuildTotalStatus Overall { get; set; }

        public IList<TaskState> Tasks
        {
            get { return new List<TaskState>(tasks); }
        }

        public void BuildEnded(BuildTotalEndStatus overall)
        {
            Overall = overall == BuildTotalEndStatus.Success
                          ? BuildTotalStatus.BuildEndedSuccess
                          : BuildTotalStatus.BuildEndedError;
        }

        public void BuildStarted(IEnumerable<TaskInfo> taskInfos)
        {
            tasks =
                new List<TaskState>(from taskInfo in taskInfos select new TaskState(taskInfo.Name, taskInfo.TerminalId));
        }

        public void BuildUpdated(int taskIndex, TaskInfo.TaskStatus status)
        {
            tasks[taskIndex].UpdateTaskStatus(status);
        }
    }

    public enum BuildTotalStatus
    {
        BuildStarted,
        BuildEndedError,
        BuildEndedSuccess
    }

    public class TaskState
    {
        public TaskInfo.TaskStatus status;
        public string Name { get; set; }

        public TaskInfo.TaskStatus Status
        {
            get { return status; }
        }

        public Guid TerminalId { get; private set; }

        public TaskState() : this("Unnamed, should not see this", Guid.Empty)
        {
        }

        public TaskState(string name, Guid terminalId)
        {
            TerminalId = terminalId;
            Name = name;
            status = TaskInfo.TaskStatus.NotStarted;
        }

        public void UpdateTaskStatus(TaskInfo.TaskStatus status)
        {
            this.status = status;
        }
    }

    public class TerminalOutput
    {
        public List<string> contentPieces = new List<string>();

        public void Add(int sequnceIndex, string content)
        {
            lock (contentPieces)
            {
                if (contentPieces.Count <= sequnceIndex)
                {
                    int itemsToAllocate = sequnceIndex - contentPieces.Count + 1;
                    for (var i = 0; i < itemsToAllocate; i++)
                        contentPieces.Add(null);
                }
                contentPieces[sequnceIndex] = content;
            }
        }

        public Info GetContent(int sinceIndex)
        {
            lock (contentPieces)
            {
                var buffer = new StringBuilder();
                var lastChunkIndex = sinceIndex;
                for (int index = sinceIndex; index < contentPieces.Count; index++)
                {
                    var contentPiece = contentPieces[index];
                    if (contentPiece == null) break;
                    lastChunkIndex = index + 1;
                    buffer.Append(contentPiece);
                }
                return new Info {Content = buffer.ToString(), LastChunkIndex = lastChunkIndex};
            }
        }

        public struct Info
        {
            public string Content;
            public int LastChunkIndex;
        }
    }
}