using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class InMemoryProjectsRepository : ProjectsRepository, ProjectsRepositoryTestSupport
    {
        public IEnumerable<RepositoryDocument> AllProjects
        {
            get { return trackedRepos.Values; }
        }

        public InMemoryProjectsRepository()
        {
            trackedRepos = new ConcurrentDictionary<string, RepositoryDocument>();
        }

        public void ProjectCheckComplete(string repoUrl)
        {
            trackedRepos[repoUrl].CheckForUpdateRequested = false;
        }

        public void ProjectCheckInProgress(string repoUrl)
        {
            trackedRepos[repoUrl].CheckForUpdateRequested = true;
        }

        public void RemoveProject(string repoUrl)
        {
            RepositoryDocument value;
            trackedRepos.TryRemove(repoUrl, out value);
        }

        public void TrackRepository(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl,
                                new RepositoryDocument
                                    {projecturl = repoUrl, revision = "", CheckForUpdateRequested = false});
        }

        public void UpdateLastKnownRevision(string repoUrl, string revision)
        {
            trackedRepos[repoUrl].revision = revision;
            trackedRepos[repoUrl].CheckForUpdateRequested = false;
        }

        public void WipeBucket()
        {
            trackedRepos.Clear();
        }

        readonly ConcurrentDictionary<string, RepositoryDocument> trackedRepos;
    }
}