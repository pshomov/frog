using System.Linq;
using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentSpecsWithTwoPeojects : AgentSpecsBase
    {
        protected override void When()
        {
            Agent.Handle(new Build(repoUrl: "http://fle", revision: "2"));
            Agent.Handle(new Build(repoUrl: "http://flo", revision: "2"));
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
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://fle"));
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://flo"));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskIndex == 0 && found.TaskStatus == TaskInfo.TaskStatus.Started && found.RepoUrl == "http://fle"));
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskIndex == 0 && found.TaskStatus == TaskInfo.TaskStatus.Started && found.RepoUrl == "http://flo"));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.RepoUrl == "http://fle"));
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.RepoUrl == "http://flo"));
        }
    }
}