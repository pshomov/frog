using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
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
        RepositoryDriver repo;

        public override void Given()
        {
            repo = RepositoryDriver.GetNewRepository();
            system = SystemDriver.GetCleanSystem();
            system.RegisterNewProject(repo.Url);
        }

        public override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATES_message()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<CheckForUpdates>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url))
                            ));
            
        }

        [Test]
        public void should_send_UPDATE_FOUND_message()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<UpdateFound>(
                                                  ev =>
                                                  ev.Revision.Length == 40))
                            ));
            
        }

    }
}