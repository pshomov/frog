using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryDocument
    {
        public string projecturl;
        public string revision;
        public bool CheckForUpdateRequested;
    }

    public interface IProjectsRepository
    {
        void TrackRepository(string repoUrl);
        IEnumerable<RepositoryDocument> AllProjects { get; }
        void UpdateLastKnownRevision(string repoUrl, string revision);
        void RemoveProject(string repoUrl);
        void ProjectCheckInProgress(string repoUrl);
        void ProjectCheckComplete(string repoUrl);
        void WipeBucket();
    }
}