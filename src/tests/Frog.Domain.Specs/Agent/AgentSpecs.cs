using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentSpecs : AgentSpecsBase
    {
        protected override void When()
        {
            agent.Handle(new CheckForUpdates {RepoUrl = "http://fle", Revision = "2"});
        }

        [Test]
        public void should_check_for_updates()
        {
            worker.Received().CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), "2");
        }

        [Test]
        public void should_publish_UPDATE_FOUND_event_when_there_is_a_new_revision()
        {
            bus.Received().Publish(
                Arg.Is<UpdateFound>(found => found.Revision == "new_rev" && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildStarted_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.Status != null && found.RepoUrl == "http://fle"));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://fle"));
        }
    }
}