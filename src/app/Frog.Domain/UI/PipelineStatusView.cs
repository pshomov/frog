using System.Collections.Concurrent;
using System.Linq;
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
            buildStatus.BuildStarted(message.Status.Tasks);
        }

        public void Handle(BuildUpdated message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].BuildUpdated(message.TaskIndex, message.TaskStatus);
        }

        public void Handle(BuildEnded message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].BuildEnded(message.TotalStatus);
        }

        public void Handle(TerminalUpdate message)
        {
            EnsureReportExistsForRepo(message.RepoUrl);
            report[message.RepoUrl].Tasks.ElementAt(message.TaskIndex).AddTerminalOutput(message.ContentSequenceIndex,
                                                                                         message.Content);
        }

        void EnsureReportExistsForRepo(string repoUrl)
        {
            report.TryAdd(repoUrl, new BuildStatus());
        }
    }
}