using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentJoinsThePartySpec : AgentSpecsBase
    {
        private string agentId;

        protected override void When()
        {
            Bus.RegisterHandler(Arg.Any<Action<Build>>(), Arg.Do<string>(s => agentId = s));
            Agent = new Domain.Agent(Bus, Worker, url => Repo, new string[] { }, Guid.NewGuid());
            Agent.JoinTheParty();
        }
         
        [Test]
        public void should_listen_for_BUILD_message()
        {
            Bus.Received().RegisterHandler(Arg.Any<Action<Build>>(), Arg.Any<string>());
        }

        [Test]
        public void should_publish_agent_availability()
        {
            Bus.Received().Publish(Arg.Any<AgentJoined>());
        }

        [Test]
        public void should_use_same_agent_id_when_registering_for_handling_message_and_announcing_presence()
        {
            Bus.Received().Publish(Arg.Is<AgentJoined>(joined => joined.AgentId.ToString() == agentId));
        }
    }
}