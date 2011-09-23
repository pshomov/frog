using System.Linq;
using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentSpecsWithTwoPeojects : AgentSpecsBase
    {
        private Build buildMessage1;
        private Build buildMessage2;

        protected override void When()
        {
            buildMessage1 = new Build { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "2" } };
            Agent.Handle(buildMessage1);
            buildMessage2 = new Build { RepoUrl = "http://flo", Revision = new RevisionInfo { Revision = "2" } };
            Agent.Handle(buildMessage2);
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
                Arg.Is<BuildStarted>(found => found.Status != null && found.BuildId == buildMessage1.Id));
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.BuildId == buildMessage2.Id));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskIndex == 0 && found.TaskStatus == TaskInfo.TaskStatus.Started && found.BuildId == buildMessage1.Id));
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskIndex == 0 && found.TaskStatus == TaskInfo.TaskStatus.Started && found.BuildId == buildMessage2.Id));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.BuildId == buildMessage1.Id));
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.BuildId == buildMessage2.Id));
        }
    }
}