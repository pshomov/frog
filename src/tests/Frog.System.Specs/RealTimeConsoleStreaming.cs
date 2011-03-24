using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs
{
    [TestFixture]
    public class RealTimeConsoleStreaming : BDD
    {
        SystemDriver<TestSystem> system;
        RepositoryDriver repo;

        protected override void Given()
        {
            repo = RepositoryDriver.GetNewRepository();
            system = SystemDriver<TestSystem>.GetCleanSystem();
            system.RegisterNewProject(repo.Url);
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
                                              An.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.Revision == ""))
                            ));
        }

        [Test]
        public void should_send_UPDATE_FOUND_message_and_update_the_last_build_revision()
        {
            var prober = new PollingProber(5000, 100);
            string updateFound = "crazy value";
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<UpdateFound>(
                                                  ev =>
                                                      {
                                                          updateFound = ev.Revision;
                                                          return ev.Revision.Length == 40;
                                                      }))
                            ));
            system.CheckProjectsForUpdates();
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Command<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.Revision == updateFound))
                            ));
        }
    }
}