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
            report[message.RepoUrl].Current = BuildStatus.Status.PipelineStarted;
            report[message.RepoUrl].PipelineStatus = new PipelineStatus(message.Status);
        }

        public void Handle(BuildUpdated message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].PipelineStatus = new PipelineStatus(message.Status);
        }

        public void Handle(BuildEnded message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].Current = message.TotalStatus == BuildTotalStatus.Success
                                                  ? BuildStatus.Status.PipelineCompletedSuccess
                                                  : BuildStatus.Status.PipelineCompletedFailure;
        }

        void EnsureReportExistsForRepo(string repoUrl)
        {
            report.TryAdd(repoUrl, new BuildStatus());
        }

        public class TerminalOutput
        {
            List<string> contentPieces = new List<string>();
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

        public class BuildStatus
        {
            public BuildStatus()
            {
                Current = Status.NotStarted;
            }

            public PipelineStatus PipelineStatus { get; set; }
            public IList<TerminalOutput> CombinedTerminalOutput { get; set; }

            public enum Status
            {
                PipelineStarted,
                NotStarted,
                PipelineCompletedFailure,
                PipelineCompletedSuccess
            }

            public Status Current { get; set; }
        }
    }
}