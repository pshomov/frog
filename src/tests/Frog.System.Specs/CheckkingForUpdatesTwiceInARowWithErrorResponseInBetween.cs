using System.Threading;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NUnit.Framework;
using SimpleCQRS;
using xray;

namespace Frog.System.Specs
{
    internal class AgentFailsToCheckForUpdate : Handles<CheckForUpdates>
    {
        private readonly IBus theBus;

        public AgentFailsToCheckForUpdate(IBus theBus)
        {
            this.theBus = theBus;
        }

        public void Handle(CheckForUpdates message)
        {
            theBus.Publish(new CheckForUpdateFailed{repoUrl = message.RepoUrl});
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<CheckForUpdates>(Handle, "Agent");
        }
    }

    internal class SystemWithFaultingAgent : TestSystem
    {
        protected override void SetupAgent()
        {
            new AgentFailsToCheckForUpdate(TheBus).JoinTheParty();
        }
    }

    [TestFixture]
    public class CheckkingForUpdatesTwiceInARowWithErrorResponseInBetween : BDD
    {
        SystemDriver<SystemWithFaultingAgent> system;
        RepositoryDriver repo;
        private PollingProber prober;

        protected override void Given()
        {
            repo = RepositoryDriver.GetNewRepository();
            system = SystemDriver<SystemWithFaultingAgent>.GetCleanSystem();
            system.RegisterNewProject(repo.Url);
            system.CheckProjectsForUpdates();
            prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                             .Has(msgs => msgs, An.Event<CheckForUpdateFailed>())
                             .Has(msgs => msgs, A.Command<CheckForUpdates>())
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
                                              Two.Commands<CheckForUpdates>())
                            ));
        }
    }
}