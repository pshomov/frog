using System.Collections.Generic;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryInfo
    {
        public string Url;
        public string LastBuiltRevision;
    }

    public interface IProjectsRepository
    {
        void TrackRepository(string repoUrl);
        RepositoryInfo this[string repoUrl] { get; }
        IEnumerable<RepositoryInfo> AllProjects { get; }
    }
}