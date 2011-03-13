using System;
using System.Collections.Concurrent;
using System.Linq;
using SimpleCQRS;

namespace Frog.Domain
{
    public class CheckForUpdates : Event
    {
        public string RepoUrl;
        public string Revision;
    }

    public class RepositoryTracker : Handles<UpdateFound>
    {
        public class RepositoryInfo
        {
            public RepositoryInfo()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; private set; }
            public string Url;
            public string LastBuiltRevision;
        }

        readonly IBus bus;
        readonly ConcurrentDictionary<string, RepositoryInfo> trackedRepos;

        public RepositoryTracker(IBus bus)
        {
            this.bus = bus;
            trackedRepos = new ConcurrentDictionary<string, RepositoryInfo>();
        }

        public void Track(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl, new RepositoryInfo {Url = repoUrl, LastBuiltRevision = ""});
        }

        public void CheckForUpdates()
        {
            trackedRepos.ToList().ForEach(
                s => bus.Publish(new CheckForUpdates {RepoUrl = s.Value.Url, Revision = s.Value.LastBuiltRevision}));
        }

        public void StartListeningForBuildUpdates()
        {
            bus.RegisterHandler<UpdateFound>(Handle);
        }

        public void Handle(UpdateFound message)
        {
            trackedRepos[message.RepoUrl].LastBuiltRevision = message.Revision;
        }
    }
}