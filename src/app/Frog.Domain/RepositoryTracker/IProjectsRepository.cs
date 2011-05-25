using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryDocument
    {
        public string Url;
        public string LastBuiltRevision;
    }

    public interface IProjectsRepository
    {
        void TrackRepository(string repoUrl);
        IEnumerable<RepositoryDocument> AllProjects { get; }
        void UpdateLastKnownRevision(string repoUrl, string revision);
    }
}