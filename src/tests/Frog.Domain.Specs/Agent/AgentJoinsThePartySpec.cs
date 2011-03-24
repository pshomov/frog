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
            agent.JoinTheParty();
        }

        [Test]
        public void should_listen_for_CHECK_FOR_UPDATES_message()
        {
            bus.Received().RegisterHandler(Arg.Any<Action<CheckForUpdates>>());
        }
    }
}