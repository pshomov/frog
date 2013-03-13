using SimpleCQRS;

namespace Frog.Domain.RepositoryTracker
{
    public class RepositoryRegistered : Event
    {
        public string RepoUrl;
    }
}