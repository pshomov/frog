using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public class RevisionChecker : Handles<CheckRevision>
    {
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
                var latestRevision = driver.GetLatestRevision();
                bus.Publish(new UpdateFound {RepoUrl = checkRevision.RepoUrl, Revision = latestRevision});
            }
            catch (Exception)
            {
                bus.Publish(new CheckForUpdateFailed {RepoUrl = checkRevision.RepoUrl});
            }
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler<CheckRevision>(Handle, "checksForUpdates");
        }

        readonly IBus bus;
        readonly SourceRepoDriverFactory sourceRepoDriver;
    }
}