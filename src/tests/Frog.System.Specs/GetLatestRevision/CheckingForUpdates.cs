using System.Linq;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.RevisionChecker;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
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
        public void should_check_for_new_revision_and_request_a_build_of_it()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckRevision>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123"))
                                         .Has(x => x,
                                              An.Event<UpdateFound>(
                                                  found => found.RepoUrl == "http://123" && found.Revision == "12"))
                                         .Has(x => x,
                                              A.Command<Build>(
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
                                              A.Command<CheckRevision>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123"))
                            ));
            Assert.False(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                          .Has(x => x.Skip(messageCheckpoint).ToList(),
                                               A.Command<Build>(
                                                   ev =>
                                                   ev.RepoUrl == "http://123"))
                             ));
        }
    }
}