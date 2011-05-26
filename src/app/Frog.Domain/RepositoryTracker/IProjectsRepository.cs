using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryDocument
    {
        public string projecturl;
        public string revision;
    }

    public interface IProjectsRepository
    {
        void TrackRepository(string repoUrl);
        IEnumerable<RepositoryDocument> AllProjects { get; }
        void UpdateLastKnownRevision(string repoUrl, string revision);
    }
}