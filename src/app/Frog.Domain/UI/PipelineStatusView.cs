using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>, Handles<BuildUpdated>, Handles<ProjectCheckedOut>
    {
        private readonly IStoreEvents eventStore;
        private Dictionary<Guid, IEventStream> streams = new Dictionary<Guid, IEventStream>();
        private int batch;

        public PipelineStatusView(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
            batch = 0;
        }

        IEventStream GetEventStream(Guid id)
        {
            if (streams.ContainsKey(id)) return streams[id];
            return streams[id] = eventStore.OpenStream(id, Int32.MinValue, Int32.MaxValue);
        }

        public void Handle(BuildStarted message)
        {
            var eventStream = GetEventStream(message.BuildId);
            eventStream.Add(new EventMessage(){Body = message});
            eventStream.CommitChanges(Guid.NewGuid());
        }

        public void Handle(BuildUpdated message)
        {
            var eventStream = GetEventStream(message.BuildId);
            eventStream.Add(new EventMessage(){Body = message});
            eventStream.CommitChanges(Guid.NewGuid());
        }

        public void Handle(BuildEnded message)
        {
            var eventStream = GetEventStream(message.BuildId);
            eventStream.Add(new EventMessage(){Body = message});
            eventStream.CommitChanges(Guid.NewGuid());
        }

        public void Handle(TerminalUpdate message)
        {
            var eventStream = GetEventStream(message.BuildId);
            eventStream.Add(new EventMessage(){Body = message});
            eventStream.CommitChanges(Guid.NewGuid());
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
    }
}