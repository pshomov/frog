namespace Frog.Domain
{
    public delegate SourceRepoDriver SourceRepoDriverFactory(string repositoryURL);

    public struct RevisionInfo
    {
        public string Revision;
    }

    public class CheckoutInfo
    {
        public string Comment;
        public string Revision;
    }

    public interface SourceRepoDriver
    {
        RevisionInfo GetLatestRevision();
        CheckoutInfo GetSourceRevision(string revision, string checkoutPath);
    }
}