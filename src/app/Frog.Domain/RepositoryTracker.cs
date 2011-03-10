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
        readonly IBus eventPublisher;
        readonly List<RepositoryInfo> trackedRepos;
        List<BuildInfo> builds;


        public RepositoryTracker(IBus eventPublisher)
        {
            this.eventPublisher = eventPublisher;
            trackedRepos = new List<RepositoryInfo>();
            builds = new List<BuildInfo>();
        }

        public void Track(string repoUrl)
        {
            trackedRepos.Add(new RepositoryInfo{Url = repoUrl, LastBuiltRevision = ""});
        }

        public void CheckForUpdates()
        {
            trackedRepos.ForEach(s => eventPublisher.Publish(new CheckForUpdates{RepoUrl = s.Url, Revision = s.LastBuiltRevision}));
        }

        public void StartListeningForBuildUpdates()
        {
            eventPublisher.RegisterHandler<UpdateFound>(Handle);
        }

        public void Handle(UpdateFound message)
        {
            var repo = trackedRepos.Single(repositoryInfo => repositoryInfo.Url == message.RepoUrl);
            repo.LastBuiltRevision = message.Revision;
        }
    }

    public class BuildInfo
    {
        public Guid Id { get; private set; }
        public Guid RepoId { get; private set; }

        public BuildInfo(Guid repoId)
        {
            RepoId = repoId;
            Id = Guid.NewGuid();
        }
    }
}