using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>
    {
        readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuilds;

        public PipelineStatusView(ConcurrentDictionary<Guid, BuildStatus> report, ConcurrentDictionary<string, Guid> currentBuilds)
        {
            this.report = report;
            this.currentBuilds = currentBuilds;
        }

        public void Handle(BuildStarted message)
        {
            string repoUrl = message.RepoUrl;
            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
            if (privateRepo.IsMatch(repoUrl))
            {
                var b = privateRepo.Match(repoUrl).Groups;
                repoUrl = b[1].Value + b[4].Value;
            }
            currentBuilds[repoUrl] = message.BuildId;

            EnsureReportExistsForRepo(message.BuildId);
            var buildStatus = report[message.BuildId];
            buildStatus.BuildStarted(message.Status.Tasks);
        }

        public void Handle(BuildUpdated message)
        {
            EnsureReportExistsForRepo(message.BuildId);
            report[message.BuildId].BuildUpdated(message.TaskIndex, message.TaskStatus);
        }

        public void Handle(BuildEnded message)
        {
            EnsureReportExistsForRepo(message.BuildId);
            report[message.BuildId].BuildEnded(message.TotalStatus);
        }

        public void Handle(TerminalUpdate message)
        {
            EnsureReportExistsForRepo(message.BuildId);
            report[message.BuildId].Tasks[message.TaskIndex].AddTerminalOutput(message.ContentSequenceIndex,
                                                                                         message.Content);
        }

        void EnsureReportExistsForRepo(Guid repoUrl)
        {
            report.TryAdd(repoUrl, new BuildStatus());
        }
    }
}