using System.Linq;
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
    public class CheckingForUpdates : BDD
    {
        private SystemDriver system;

        protected override void Given()
        {
            system = new SystemDriver();
            system.SourceRepoDriver.GetLatestRevision().Returns("12");
            system.RegisterNewProject("http://123");
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATES_and_receive_event_UPDATE_FOUND_and_send_BUILDPROJECT()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123"))
                                         .Has(x => x,
                                              An.Event<UpdateFound>(
                                                  found => found.RepoUrl == "http://123" && found.Revision == "12"))
                                         .Has(x => x,
                                              A.Command<BuildProject>(
                                                  found => found.RepoUrl == "http://123" && found.Revision == "12"))
                            ));
        }

        [Test]
        public void should_send_UPDATE_FOUND_message_and_update_the_last_build_revision()
        {
            var prober = new PollingProber(3000, 100);
            var messageCheckpoint = system.GetEventsSnapshot().Count;
            system.CheckProjectsForUpdates();
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x.Skip(messageCheckpoint).ToList(),
                                              A.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123"))
                            ));
            Assert.False(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                          .Has(x => x.Skip(messageCheckpoint).ToList(),
                                               A.Command<BuildProject>(
                                                   ev =>
                                                   ev.RepoUrl == "http://123"))
                             ));
        }
    }
}