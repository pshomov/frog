using System;
using System.Collections.Generic;
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
        readonly List<RepositoryInfo> trackedRepos;

        public RepositoryTracker(IBus bus)
        {
            this.bus = bus;
            trackedRepos = new List<RepositoryInfo>();
        }

        public void Track(string repoUrl)
        {
            trackedRepos.Add(new RepositoryInfo{Url = repoUrl, LastBuiltRevision = ""});
        }

        public void CheckForUpdates()
        {
            trackedRepos.ForEach(s => bus.Publish(new CheckForUpdates{RepoUrl = s.Url, Revision = s.LastBuiltRevision}));
        }

        public void StartListeningForBuildUpdates()
        {
            bus.RegisterHandler<UpdateFound>(Handle);
        }

        public void Handle(UpdateFound message)
        {
            var repo = trackedRepos.Single(repositoryInfo => repositoryInfo.Url == message.RepoUrl);
            repo.LastBuiltRevision = message.Revision;
        }
    }
}