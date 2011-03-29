using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>
    {
        readonly ConcurrentDictionary<string, BuildStatus> report;

        public PipelineStatusView(ConcurrentDictionary<string, BuildStatus> report)
        {
            this.report = report;
        }

        public void Handle(BuildStarted message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            var buildStatus = report[message.RepoUrl];
            buildStatus.Current = BuildStatus.OverallStatus.PipelineStarted;
            buildStatus.TaskState = new List<TaskState>();
            foreach (var taskInfo in message.Status.Tasks)
            {
                buildStatus.TaskState.Add(new TaskState(taskInfo));
            }
        }

        public void Handle(BuildUpdated message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].UpdateTaskStatus(message.Status.Tasks);
        }

        public void Handle(BuildEnded message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].Current = message.TotalStatus == BuildTotalStatus.Success
                                                  ? BuildStatus.OverallStatus.PipelineCompletedSuccess
                                                  : BuildStatus.OverallStatus.PipelineCompletedFailure;
        }

        public void Handle(TerminalUpdate message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].TaskState[message.TaskIndex].terminalOutput.Add(message.ContentSequenceIndex, message.Content);
        }

        void EnsureReportExistsForRepo(string repoUrl)
        {
            report.TryAdd(repoUrl, new BuildStatus());
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
                        for (var i = 0; i < itemsToAllocate; i++ ) 
                            contentPieces.Add(null);
                    }
                    contentPieces[sequnceIndex] = content;
                }
            }

            public string Combined
            {
                get {
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

        public class TaskState
        {
            TaskInfo status;
            public readonly TerminalOutput terminalOutput;

            public TaskState(TaskInfo status)
            {
                this.status = status;
                terminalOutput = new TerminalOutput();
            }

            public TaskInfo Status
            {
                get { return status; }
                set { status = value; }
            }
        }

        public class BuildStatus
        {
            public BuildStatus()
            {
                Current = OverallStatus.NotStarted;
                TaskState = new List<TaskState>();
            }

            public IList<TaskState> TaskState;

            public void UpdateTaskStatus(IList<TaskInfo> tasksStatus)
            {
                for (int i = 0; i < tasksStatus.Count; i++)
                {
                    TaskState[i].Status = tasksStatus[i];
                }
            }

            public enum OverallStatus
            {
                PipelineStarted,
                NotStarted,
                PipelineCompletedFailure,
                PipelineCompletedSuccess
            }

            public OverallStatus Current { get; set; }
        }
    }
}