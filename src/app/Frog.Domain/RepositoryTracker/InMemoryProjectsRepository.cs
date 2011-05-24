using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class InMemoryProjectsRepository : IProjectsRepository
    {
        private readonly ConcurrentDictionary<string, RepositoryInfo> trackedRepos;

        public InMemoryProjectsRepository()
        {
            trackedRepos = new ConcurrentDictionary<string, RepositoryInfo>();
        }

        public void TrackRepository(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl, new RepositoryInfo { Url = repoUrl, LastBuiltRevision = "" });
        }

        public RepositoryInfo this[string repoUrl]{
            get { return trackedRepos[repoUrl]; }
        }

        public IEnumerable<RepositoryInfo> AllProjects { get { return trackedRepos.Values; } }
    }
}