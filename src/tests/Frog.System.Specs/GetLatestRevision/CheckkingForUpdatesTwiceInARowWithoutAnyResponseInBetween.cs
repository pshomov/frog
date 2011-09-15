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
    public class CheckkingForUpdatesTwiceInARowWhileAgentIsNotRunning : BDD
    {
        SystemDriver system;

        protected override void Given()
        {
            var repoUrl = Guid.NewGuid().ToString();
            system = new SystemDriver(runAgent : false);
            system.SourceRepoDriver.GetLatestRevision().Returns("14");
            system.RegisterNewProject(repoUrl);
            system.CheckProjectsForUpdates();
            make_sure_one_CHECK_FOR_UPDATE_message_is_sent();
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_only_once()
        {
            var prober = new PollingProber(1000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<Build>())
                            ));
            Assert.False(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              Two.Commands<Build>())
                            ));
        }

        private void make_sure_one_CHECK_FOR_UPDATE_message_is_sent()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(msgs => msgs, A.Command<Build>())
                            ));
        }
    }
}