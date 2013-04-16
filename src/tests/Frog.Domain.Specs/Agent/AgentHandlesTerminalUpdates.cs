using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentHandlesTerminalUpdates : AgentSpecsBase
    {
        private Build buildMessage;

        protected override void When()
        {
            Agent = new Domain.Agent(Bus, Worker, url => Repo, new string[] { });
            Agent.JoinTheParty();
            buildMessage = new Build { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "2" } };
            Agent.Handle(buildMessage);
        }

        [Test]
        public void should_receive_terinal_updates_events()
        {
            Bus.Received().Publish(
                Arg.Is<TerminalUpdate>(
                    update =>
                    update.BuildId == buildMessage.Id && update.Content == "content" && update.TaskIndex == 1 && update.SequenceId == 0 && update.TerminalId == TerminalId &&
                    update.ContentSequenceIndex == 0));
        }
    }
}