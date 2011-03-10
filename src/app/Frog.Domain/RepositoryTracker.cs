using System;
using System.Collections.Generic;
using SimpleCQRS;

namespace Frog.Domain
{
    public class CheckForUpdates : Event
    {
        public string RepoUrl;
        public string Revision;
    }

    public class RepositoryTracker
    {
        public class RepositoryInfo
        {
            public string Url;
            public string LastBuiltRevision;
        }
        readonly IEventPublisher eventPublisher;
        List<RepositoryInfo> trackedRepos;

        public RepositoryTracker(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher;
            trackedRepos = new List<RepositoryInfo>();
        }

        public void Track(string repoUrl)
        {
            trackedRepos.Add(new RepositoryInfo{Url = repoUrl, LastBuiltRevision = ""});
        }

        public void CheckForUpdates()
        {
            trackedRepos.ForEach(s => eventPublisher.Publish(new CheckForUpdates{RepoUrl = s.Url, Revision = s.LastBuiltRevision}));
        }
    }
}