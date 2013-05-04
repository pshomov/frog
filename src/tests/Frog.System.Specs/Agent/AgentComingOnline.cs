using System;
using Frog.Domain;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class AgentComingOnline : SystemBDD
    {
        private Guid agentId;

        protected override void When()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            agentId = Guid.NewGuid();
            testSystem.AddAgent(url => sourceRepoDriver, workingAreaGoverner, agentId, "tag1", "tag2");
        }

        [Test]
        public void should_announce_the_project_has_been_checked_out()
        {
            Assert.True(EventStoreCheck(ES => ES.Has(An.Event<AgentJoined>(joined => joined.AgentId == agentId))));
        }

        [Test]
        public void should_provide_agent_capabilites()
        {
            Assert.True(
                EventStoreCheck(
                    ES => ES.Has(An.Event<AgentJoined>(ev => Lists.AreEqual(ev.Capabilities, As.List("tag1", "tag2"))))));
        }
    }
}