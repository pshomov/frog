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
    public class CheckingForUpdates : BDD
    {
        SystemDriver system;

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
        public void should_send_CHECK_FOR_UPDATES_message()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123" && ev.Revision == ""))
                            ));
        }

        [Test]
        public void should_send_UPDATE_FOUND_message_and_update_the_last_build_revision()
        {
            var prober = new PollingProber(5000, 100);
            system.CheckProjectsForUpdates();
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == "http://123" && ev.Revision == "12"))
                            ));
        }
    }
}