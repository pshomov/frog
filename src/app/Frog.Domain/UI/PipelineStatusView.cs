using System;
using System.Collections.Generic;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>
    {
        readonly Dictionary<string, BuildStatus> report;

        public PipelineStatusView(Dictionary<string, BuildStatus> report)
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
            report[message.RepoUrl].Current = message.TotalStatus == BuildTotalStatus.Success ? BuildStatus.Status.PipelineCompletedSuccess : BuildStatus.Status.PipelineCompletedFailure;
        }

        void EnsureReportExistsForRepo(string repoUrl)
        {
            if (!report.ContainsKey(repoUrl)) report.Add(repoUrl, new BuildStatus());
        }


        public class BuildStatus
        {
            public BuildStatus()
            {
                Current = Status.NotStarted;
            }

            public PipelineStatus PipelineStatus { get; set; }

            public enum Status { PipelineStarted, NotStarted,
                PipelineCompletedFailure,
                PipelineCompletedSuccess
            }
            public Status Current { get; set; }
        }
    }
}