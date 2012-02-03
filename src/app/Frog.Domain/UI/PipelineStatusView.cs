using System;
using System.Text.RegularExpressions;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>, Handles<ProjectCheckedOut>
    {
//        private readonly ProjectView projectView;
        private IStoreEvents eventStore;

        public PipelineStatusView(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }


        public void Handle(BuildStarted message)
        {
            using (
                var eventStream = eventStore.OpenStream(message.BuildId, int.MinValue, int.MaxValue))
            {
                eventStream.Add(new EventMessage(){Body = message});
                eventStream.CommitChanges(Guid.NewGuid());
            }
//            projectView.SetBuildStarted(message.BuildId, message.Status.Tasks);
        }

        public void Handle(BuildUpdated message)
        {
             using (
                var eventStream = eventStore.OpenStream(message.BuildId, int.MinValue, int.MaxValue))
            {
                eventStream.Add(new EventMessage(){Body = message});
                eventStream.CommitChanges(Guid.NewGuid());
            }
//           projectView.BuildUpdated(message.BuildId,message.TaskIndex, message.TaskStatus);
        }

        public void Handle(BuildEnded message)
        {
            using (
                var eventStream = eventStore.OpenStream(message.BuildId, int.MinValue, int.MaxValue))
            {
                eventStream.Add(new EventMessage() { Body = message });
                eventStream.CommitChanges(Guid.NewGuid());
            }
            //            projectView.BuildEnded(message.BuildId, message.TotalStatus);
        }

        public void Handle(TerminalUpdate message)
        {
            using (
                var eventStream = eventStore.OpenStream(message.BuildId, int.MinValue, int.MaxValue))
            {
                eventStream.Add(new EventMessage() { Body = message });
                eventStream.CommitChanges(Guid.NewGuid());
            }
            //            projectView.AppendTerminalOutput(message.BuildId, message.TaskIndex, message.ContentSequenceIndex, message.Content);
        }

        public void Handle(ProjectCheckedOut message)
        {
            //
            string repoUrl = message.RepoUrl;
            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
            if (privateRepo.IsMatch(repoUrl))
            {
                var b = privateRepo.Match(repoUrl).Groups;
                repoUrl = b[1].Value + b[4].Value;
            }
            using (
                var eventStream = eventStore.CreateStream(message.BuildId))
            {
                eventStream.Add(new EventMessage() { Body = message });
                eventStream.CommitChanges(Guid.NewGuid());
            }
            using (
                var eventStream = eventStore.OpenStream(EventBasedProjectView.KeyGenerator(repoUrl), Int32.MinValue, Int32.MaxValue))
            {
                eventStream.Add(new EventMessage() { Body = message });
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return;
//            projectView.SetCurrentBuild(repoUrl, message.BuildId, message.CheckoutInfo.Comment, message.CheckoutInfo.Revision);
        }
    }
}