using SimpleCQRS;

namespace Frog.Domain.RepositoryTracker
{
    public class RegisterRepository : Command
    {
        public string Repo;
    }
}