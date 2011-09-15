using SimpleCQRS;

namespace Frog.Domain.RevisionChecker
{
    public class CheckRevision : Command
    {
        public string RepoUrl;
    }
}