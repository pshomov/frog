using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentJoinsThePartySpec : AgentSpecsBase
    {
        private Guid agentId;

        protected override void When()
        {
            agentId = Guid.NewGuid();
            Agent = new Domain.Agent(Bus, Worker, url => Repo, new string[] { }, agentId);
            Agent.JoinTheParty();
        }
         
        [Test]
        public void should_listen_for_direct_BUILD_message()
        {
            Bus.Received().RegisterDirectHandler(Arg.Any<Action<Build>>(), Arg.Is<string>(s => s == agentId.ToString()));
        }

        [Test]
        public void should_publish_agent_availability()
        {
            Bus.Received().Publish(Arg.Any<AgentJoined>());
        }

        [Test]
        public void should_use_same_agent_id_when_registering_for_handling_message_and_announcing_presence()
        {
            Bus.Received().Publish(Arg.Is<AgentJoined>(joined => joined.AgentId == agentId));
        }
    }
}