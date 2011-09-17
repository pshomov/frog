using Frog.Domain.RepositoryTracker;
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
            buildMessage = new Build{RepoUrl = "http://fle", Revision = "2"};
            Agent.Handle(buildMessage);
        }

        [Test]
        public void should_receive_terinal_updates_events()
        {
            Bus.Received().Publish(
                Arg.Is<TerminalUpdate>(
                    update =>
                    update.BuildId == buildMessage.Id && update.Content == "content" && update.TaskIndex == 1 &&
                    update.ContentSequenceIndex == 1));
        }
    }
}