using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryDocument
    {
        public bool CheckForUpdateRequested;
        public string projecturl;
        public string revision;
    }

    public interface IProjectsRepository
    {
        IEnumerable<RepositoryDocument> AllProjects { get; }
        void ProjectCheckComplete(string repoUrl);
        void ProjectCheckInProgress(string repoUrl);
        void RemoveProject(string repoUrl);
        void TrackRepository(string repoUrl);
        void UpdateLastKnownRevision(string repoUrl, string revision);
        void WipeBucket();
    }
}