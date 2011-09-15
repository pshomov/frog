using System;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs
{
    [TestFixture]
    public class CheckkingForUpdatesTwiceInARowWithErrorResponseInBetween : BDD
    {
        SystemDriver system;
        private PollingProber prober;

        protected override void Given()
        {
            system = new SystemDriver();
            system.SourceRepoDriver.GetLatestRevision().Returns(info =>
            {
                throw new NullReferenceException("fake one");
            });
            system.RegisterNewProject("http://123");
            system.CheckProjectsForUpdates();
            prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(msgs => msgs, An.Event<CheckForUpdateFailed>())
                                         .Has(msgs => msgs, A.Command<Build>())
                            ));
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_twice()
        {
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              Two.Commands<Build>())
                            ));
        }
    }
}