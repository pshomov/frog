using System;
using Frog.Domain.Specs;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class AgentJoinsThePartySpec : BDD
    {
        FakeBus bus;
        Agent agent;

        public override void Given()
        {
            bus = Substitute.For<FakeBus>();
            var valve = Substitute.For<IValve>();
            agent = new Agent(bus, valve);
        }

        public override void When()
        {
            agent.JoinTheParty();
        }

        [Test]
        public void should_listen_for_CHECK_FOR_UPDATES_message()
        {
            bus.Received().RegisterHandler(Arg.Any<Action<CheckForUpdates>>());
        }

    }

    [TestFixture]
    public class AgentGetsToCheckForUpdates : BDD
    {
        FakeBus bus;
        Agent agent;
        IValve valve;

        public override void Given()
        {
            bus = Substitute.For<FakeBus>();
            valve = Substitute.For<IValve>();
            agent = new Agent(bus, valve);
            agent.JoinTheParty();
        }

        public override void When()
        {
            agent.Handle(new CheckForUpdates{RepoUrl = "http://fle", Revision = "2"});
        }

        [Test]
        public void should_listen_for_CHECK_FOR_UPDATES_message()
        {
            valve.Received().Check("http://fle", "2");
        }

    }
}