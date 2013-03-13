using System.Collections.Generic;

namespace Frog.Domain
{
    public class RepositoryDocument
    {
        public bool CheckForUpdateRequested;
        public string projecturl;
        public string revision;
    }

    public interface ProjectsRepository
    {
        IEnumerable<RepositoryDocument> AllProjects { get; }
        void ProjectCheckComplete(string repoUrl);
        void ProjectCheckInProgress(string repoUrl);
        void RemoveProject(string repoUrl);
        void TrackRepository(string repoUrl);
        void UpdateLastKnownRevision(string repoUrl, string revision);
    }
}