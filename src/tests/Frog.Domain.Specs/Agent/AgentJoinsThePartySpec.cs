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
            Agent.JoinTheParty();
        }

        [Test]
        public void should_listen_for_CHECK_FOR_UPDATES_message()
        {
            Bus.Received().RegisterHandler(Arg.Any<Action<CheckForUpdates>>());
        }
    }
}