using System;
using SimpleCQRS;

namespace Frog.Domain.RevisionChecker
{
    public class RevisionChecker
    {
        private readonly IBus bus;
        private readonly SourceRepoDriverFactory sourceRepoDriver;

        public RevisionChecker(IBus bus, SourceRepoDriverFactory sourceRepoDriver)
        {
            this.bus = bus;
            this.sourceRepoDriver = sourceRepoDriver;
        }

        public void Handle(CheckRevision checkRevision)
        {
            var driver = sourceRepoDriver(checkRevision.RepoUrl);
            try
            {
                string latestRevision = driver.GetLatestRevision();
                bus.Publish(new UpdateFound { RepoUrl = checkRevision.RepoUrl, Revision = latestRevision });
            }
            catch (Exception e)
            {
                bus.Publish(new CheckForUpdateFailed {repoUrl = checkRevision.RepoUrl});
            }
        }
    }
}