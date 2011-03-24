using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentSpecsWithTwoPeojects : AgentSpecsBase
    {
        protected override void When()
        {
            Agent.Handle(new CheckForUpdates {RepoUrl = "http://fle", Revision = "2"});
            Agent.Handle(new CheckForUpdates {RepoUrl = "http://flo", Revision = "2"});
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
            Bus.Received().Publish(
                Arg.Is<UpdateFound>(found => found.Revision == "new_rev" && found.RepoUrl == "http://flo"));
            Console.WriteLine(Bus.ReceivedCalls());
            Assert.That(Bus.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Publish").ToList().Count,
                        Is.EqualTo(8));
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
                Arg.Is<BuildUpdated>(found => found.Status != null && found.RepoUrl == "http://fle"));
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.Status != null && found.RepoUrl == "http://flo"));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://fle"));
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://flo"));
        }
    }
}