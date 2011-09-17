using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>
    {
        readonly ConcurrentDictionary<Guid, BuildStatus> report;

        public PipelineStatusView(ConcurrentDictionary<Guid, BuildStatus> report)
        {
            this.report = report;
        }

        public void Handle(BuildStarted message)
        {
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

        public void Handle(Build message)
        {
            throw new NotImplementedException();
        }
    }
}