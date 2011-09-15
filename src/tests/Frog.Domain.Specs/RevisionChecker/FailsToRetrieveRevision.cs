using System;
using Frog.Domain.RevisionChecker;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs.RevisionChecker
{
    class FailsToRetrieveRevision : BDD
    {
        private Domain.RevisionChecker.RevisionChecker rc;
        private SourceRepoDriver sr;
        private IBus bus;

        protected override void Given()
        {
            sr = Substitute.For<SourceRepoDriver>();
            sr.When(driver => driver.GetLatestRevision()).Do(info => {throw new TimeoutException();});
            bus = Substitute.For<IBus>();
            rc = new Domain.RevisionChecker.RevisionChecker(bus, url => sr);
        }

        protected override void When()
        {
            rc.Handle(new CheckRevision {RepoUrl = "http://fle"});
        }

        [Test]
        public void should_send_event_with_revision_found()
        {
            bus.Received().Publish(Arg.Is<CheckForUpdateFailed>(found => found.repoUrl == "http://fle"));
        }
    }
}
