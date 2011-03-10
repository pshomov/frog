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
        IBus bus;
        Agent agent;

        public override void Given()
        {
            bus = Substitute.For<IBus>();
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
        IBus bus;
        Agent agent;
        IValve valve;

        public override void Given()
        {
            bus = Substitute.For<IBus>();
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
            valve.Received().Check(Arg.Any<SourceRepoDriver>(), "2");
        }

    }
}