using System;
using System.Text.RegularExpressions;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusEventHandler : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>, Handles<ProjectCheckedOut>
    {
        private readonly IStoreEvents eventStore;

        public PipelineStatusEventHandler(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        IEventStream GetEventStream(Guid id)
        {
            return eventStore.OpenStream(id, Int32.MinValue, Int32.MaxValue);
        }

        public void Handle(BuildStarted message)
        {
            StoreEvent(message);
        }

        public void Handle(BuildUpdated message)
        {
            StoreEvent(message);
        }

        public void Handle(BuildEnded message)
        {
            StoreEvent(message);
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
        }

        private void StoreEvent(BuildEvent message)
        {
            using (var eventStream = GetEventStream(message.BuildId))
            {
                eventStream.Add(new EventMessage() {Body = message});
                eventStream.CommitChanges(Guid.NewGuid());
            }
        }
    }
}