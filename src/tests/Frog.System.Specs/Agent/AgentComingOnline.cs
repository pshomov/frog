using Frog.Domain;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class AgentComingOnline : BDD
    {
        private SystemDriver system;
        private TestSystem testSystem;

        protected override void Given()
        {
            testSystem = new TestSystem();
            system = new SystemDriver(testSystem);
        }

        protected override void When()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            testSystem.SetupAgent(url => sourceRepoDriver, workingAreaGoverner, "tag1", "tag2");
        }

        [Test]
        public void should_announce_the_project_has_been_checked_out()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<AgentJoined>(
                                                  ))));
        }

        [Test]
        public void should_provide_agent_capabilites()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<AgentJoined>(
                                                  ev => Lists.AreEqual(ev.Capabilities, As.List("tag1", "tag2"))
                                                  ))));
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }
    }
}