using System.Text.RegularExpressions;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>, Handles<ProjectCheckedOut>
    {
        private readonly ProjectView projectView;

        public PipelineStatusView(ProjectView projectView)
        {
            this.projectView = projectView;
        }

        public void Handle(BuildStarted message)
        {
            var buildStatus = projectView.GetBuildStatus(message.BuildId);
            buildStatus.BuildStarted(message.Status.Tasks);
        }

        public void Handle(BuildUpdated message)
        {
            projectView.GetBuildStatus(message.BuildId).BuildUpdated(message.TaskIndex, message.TaskStatus);
        }

        public void Handle(BuildEnded message)
        {
            projectView.GetBuildStatus(message.BuildId).BuildEnded(message.TotalStatus);
        }

        public void Handle(TerminalUpdate message)
        {
            projectView.GetBuildStatus(message.BuildId).Tasks[message.TaskIndex].AddTerminalOutput(message.ContentSequenceIndex,
                                                                                         message.Content);
        }

        public void Handle(ProjectCheckedOut message)
        {
            string repoUrl = message.RepoUrl;
            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
            if (privateRepo.IsMatch(repoUrl))
            {
                var b = privateRepo.Match(repoUrl).Groups;
                repoUrl = b[1].Value + b[4].Value;
            }
            projectView.SetCurrentBuild(repoUrl, message.BuildId, message.CheckoutInfo.Comment, message.CheckoutInfo.Revision);
        }
    }
}