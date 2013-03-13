namespace Frog.Domain
{
    public class CheckRevision : SimpleCQRS.Command
    {
        public string RepoUrl;
    }
}