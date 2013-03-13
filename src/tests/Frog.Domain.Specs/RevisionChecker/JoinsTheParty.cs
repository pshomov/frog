using System;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs.RevisionChecker
{
    class JoinsTheParty : BDD
    {
        private Domain.RevisionChecker rc;
        private SourceRepoDriver sr;
        private IBus bus;

        protected override void Given()
        {
            sr = Substitute.For<SourceRepoDriver>();
            bus = Substitute.For<IBus>();
            rc = new Domain.RevisionChecker(bus, url => sr);
        }

        protected override void When()
        {
            rc.JoinTheParty();
        }

        [Test]
        public void should_send_event_with_revision_found()
        {
            bus.Received().RegisterHandler(Arg.Any<Action<CheckRevision>>(), Arg.Any<string>());
        }
    }
}
