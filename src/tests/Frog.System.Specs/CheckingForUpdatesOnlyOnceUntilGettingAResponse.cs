using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NUnit.Framework;
using SimpleCQRS;
using xray;

namespace Frog.System.Specs
{
    internal class Agent : Handles<CheckForUpdates>
    {
        private readonly IBus theBus;

        public Agent(IBus theBus)
        {
            this.theBus = theBus;
        }

        public void Handle(CheckForUpdates message)
        {
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<CheckForUpdates>(Handle, "Agent");
        }
    }

    internal class SystemWithNoAgent : TestSystem
    {
        protected override void SetupAgent()
        {
            new Agent(TheBus).JoinTheParty();
        }
    }
    [TestFixture]
    public class CheckingForUpdatesOnlyOnceUntilgettingAResponse : BDD
    {
        SystemDriver<SystemWithNoAgent> system;
        RepositoryDriver repo;

        protected override void Given()
        {
            repo = RepositoryDriver.GetNewRepository();
            system = SystemDriver<SystemWithNoAgent>.GetCleanSystem();
            system.RegisterNewProject(repo.Url);
            system.CheckProjectsForUpdates();
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_only_once()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              A.Command<CheckForUpdates>())
                            ));
        }
    }
}