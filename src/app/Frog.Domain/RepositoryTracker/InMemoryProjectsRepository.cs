using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class InMemoryProjectsRepository : IProjectsRepository
    {
        private readonly ConcurrentDictionary<string, RepositoryDocument> trackedRepos;

        public InMemoryProjectsRepository()
        {
            trackedRepos = new ConcurrentDictionary<string, RepositoryDocument>();
        }

        public void TrackRepository(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl, new RepositoryDocument { projecturl = repoUrl, revision = "", CheckForUpdateRequested = false});
        }

        public IEnumerable<RepositoryDocument> AllProjects { get { return trackedRepos.Values; } }
        public void UpdateLastKnownRevision(string repoUrl, string revision)
        {
            trackedRepos[repoUrl].revision = revision;
            trackedRepos[repoUrl].CheckForUpdateRequested = false;
        }
    }
}