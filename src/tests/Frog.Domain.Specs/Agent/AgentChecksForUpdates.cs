using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentChecksForUpdates : AgentSpecsBase
    {
        protected override void When()
        {
            Agent.Handle(new CheckForUpdates {RepoUrl = "http://fle", Revision = "2"});
        }

        [Test]
        public void should_check_for_updates()
        {
            Worker.Received().CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), "2");
        }

        [Test]
        public void should_publish_UPDATE_FOUND_event_when_there_is_a_new_revision()
        {
            Bus.Received().Publish(
                Arg.Is<UpdateFound>(found => found.Revision == "new_rev" && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildStarted_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskStatus == TaskInfo.TaskStatus.Started && found.TaskIndex  == 0 && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.RepoUrl == "http://fle"));
        }
    }
}