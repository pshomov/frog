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
        readonly IEventPublisher eventPublisher;
        List<string> trackedRepos;

        public RepositoryTracker(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher;
            trackedRepos = new List<string>();
        }

        public void Track(string repoUrl)
        {
            trackedRepos.Add(repoUrl);
        }

        public void CheckForUpdates()
        {
            trackedRepos.ForEach(s => eventPublisher.Publish(new CheckForUpdates{RepoUrl = s}));
        }
    }
}