﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frog.Domain.UI
{
    public class BuildStatus
    {
        public void BuildStarted(IEnumerable<TaskInfo> taskInfos)
        {
            tasks = new List<TaskState>(from taskInfo in taskInfos select new TaskState(taskInfo.Name));
        }

        List<TaskState> tasks;
        public BuildTotalStatus Overall { get; private set; }

        public IEnumerable<TaskState> Tasks
        {
            get { return tasks; }
        }

        public void BuildUpdated(int taskIndex, TaskInfo.TaskStatus status)
        {
            tasks[taskIndex].UpdateTaskStatus(status);
        }

        public void BuildEnded(BuildTotalEndStatus overall)
        {
            Overall = overall  == BuildTotalEndStatus.Success ? BuildTotalStatus.BuildEndedSuccess : BuildTotalStatus.BuildEndedError;
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
        public string Name { get; private set; }
        readonly TerminalOutput terminalOutput;
        TaskInfo.TaskStatus status;

        public string TerminalOutput
        {
            get { return terminalOutput.Combined; }
        }

        public void AddTerminalOutput(int sequence, string content)
        {
            terminalOutput.Add(sequence, content);
        }

        public TaskInfo.TaskStatus Status
        {
            get { return status; }
        }

        public void UpdateTaskStatus(TaskInfo.TaskStatus status)
        {
            this.status = status;
        }

        public TaskState(string name)
        {
            Name = name;
            terminalOutput = new TerminalOutput();
            status = TaskInfo.TaskStatus.NotStarted;
        }
    }
    public class TerminalOutput
    {
        readonly List<string> contentPieces = new List<string>();
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

        public string Combined
        {
            get
            {
                lock (contentPieces)
                {
                    var buffer = new StringBuilder();
                    foreach (var contentPiece in contentPieces)
                    {
                        if (contentPiece == null) break;
                        buffer.Append(contentPiece);
                    }
                    return buffer.ToString();
                }
            }
        }
    }
}