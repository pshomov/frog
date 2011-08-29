using System;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
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
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns("14");
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            system = SystemDriver.GetCleanSystem(() => new TestSystem(workingAreaGoverner, url => sourceRepoDriver, runAgent: false));
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
                                              A.Command<CheckForUpdates>())
                            ));
            Assert.False(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              Two.Commands<CheckForUpdates>())
                            ));
        }

        private void make_sure_one_CHECK_FOR_UPDATE_message_is_sent()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(msgs => msgs, A.Command<CheckForUpdates>())
                            ));
        }
    }
}