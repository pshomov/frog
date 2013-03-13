using System;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
{
    [TestFixture]
    public class CheckkingForUpdatesTwiceInARowWhileAgentIsNotRunning : BDD
    {
        SystemDriver system;

        protected override void Given()
        {
            var repoUrl = Guid.NewGuid().ToString();
            system = new SystemDriver(runRevisionChecker : false);
            system.SourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo{Revision = "14"});
            system.RegisterNewProject(repoUrl);
            system.CheckProjectsForUpdates();
            make_sure_one_CHECK_FOR_UPDATE_message_is_sent();
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_only_once()
        {
            var prober = new PollingProber(1000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckRevision>())
                            ));
            Assert.False(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              Two.Commands<CheckRevision>())
                            ));
        }

        private void make_sure_one_CHECK_FOR_UPDATE_message_is_sent()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(msgs => msgs, A.Command<CheckRevision>())
                            ));
        }
    }
}