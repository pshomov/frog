using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentChecksForUpdates : AgentSpecsBase
    {
        private Build buildMessage;

        protected override void When()
        {
            buildMessage = new Build{RepoUrl = "http://fle", Revision = "2"};
            Agent.Handle(buildMessage);
        }

        [Test]
        public void should_check_for_updates()
        {
            Worker.Received().CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), "2");
        }

        [Test]
        public void should_publish_BuildStarted_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.BuildId == buildMessage.Id));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskStatus == TaskInfo.TaskStatus.Started && found.TaskIndex == 0 && found.BuildId == buildMessage.Id));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.BuildId == buildMessage.Id));
        }
    }
}