using SimpleCQRS;

namespace Frog.Domain
{
    public class RepositoryRegistered : Event
    {
        public string RepoUrl;
    }
}