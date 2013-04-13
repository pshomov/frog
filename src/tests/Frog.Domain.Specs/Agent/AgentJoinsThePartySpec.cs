using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentJoinsThePartySpec : AgentSpecsBase
    {
        protected override void When()
        {
            Agent = new Domain.Agent(Bus, Worker, url => Repo);
            Agent.JoinTheParty();
        }

        [Test]
        public void should_listen_for_CHECK_FOR_UPDATES_message()
        {
            Bus.Received().RegisterHandler(Arg.Any<Action<Build>>(), Arg.Any<string>());
        }

        [Test]
        public void should_publish_agent_availability()
        {
            Bus.Received().Publish(Arg.Any<AgentJoined>());
        }
    }
}