using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentHandlesTerminalUpdates : AgentSpecsBase
    {
        protected override void When()
        {
            Agent.Handle(new Build{RepoUrl = "http://fle", Revision = "2"});
        }

        [Test]
        public void should_receive_terinal_updates_events()
        {
            Bus.Received().Publish(
                Arg.Is<TerminalUpdate>(
                    update =>
                    update.BuildId == "http://fle" && update.Content == "content" && update.TaskIndex == 1 &&
                    update.ContentSequenceIndex == 1));
        }
    }
}