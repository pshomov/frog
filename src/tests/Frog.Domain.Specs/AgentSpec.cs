using System;
using System.Linq;
using System.Threading;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    public abstract class AgentGetsToCheckForUpdatesBase : BDD
    {
        protected IBus bus;
        protected Agent agent;
        protected Worker worker;

        protected override void Given()
        {
            bus = Substitute.For<IBus>();
            worker = Substitute.For<Worker>(null, null);
            worker.When(
                iValve => iValve.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Any<string>())).Do(
                    info =>
                        {
                            worker.OnUpdateFound += Raise.Event<Action<string>>("new_rev");
                            worker.OnBuildStarted +=
                                Raise.Event<Action<PipelineStatus>>(new PipelineStatus(Guid.NewGuid()));
                            worker.OnBuildUpdated +=
                                Raise.Event<Action<PipelineStatus>>(new PipelineStatus(Guid.NewGuid()));
                            worker.OnBuildEnded += Raise.Event<Action<BuildTotalStatus>>(BuildTotalStatus.Success);
                        });
            agent = new Agent(bus, worker);
            agent.JoinTheParty();
        }
    }

    [TestFixture]
    public class AgentJoinsThePartySpec : AgentGetsToCheckForUpdatesBase
    {
        protected override void When()
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
    public class AgentGetsToCheckForUpdates : AgentGetsToCheckForUpdatesBase
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
                Arg.Is<BuildEnded>(found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://fle"));
        }

    }
    [TestFixture]
    public class AgentGetsToCheckForUpdatesWithTwoPeojects : AgentGetsToCheckForUpdatesBase
    {
        protected override void When()
        {
            agent.Handle(new CheckForUpdates {RepoUrl = "http://fle", Revision = "2"});
            agent.Handle(new CheckForUpdates {RepoUrl = "http://flo", Revision = "2"});
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
            bus.Received().Publish(
                Arg.Is<UpdateFound>(found => found.Revision == "new_rev" && found.RepoUrl == "http://flo"));
            Console.WriteLine(bus.ReceivedCalls());
            Assert.That(bus.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Publish").ToList().Count, Is.EqualTo(8));
        }

        [Test]
        public void should_publish_BuildStarted_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://fle"));
            bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.RepoUrl == "http://flo"));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.Status != null && found.RepoUrl == "http://fle"));
            bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.Status != null && found.RepoUrl == "http://flo"));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            bus.Received().Publish(
                Arg.Is<BuildEnded>(found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://fle"));
            bus.Received().Publish(
                Arg.Is<BuildEnded>(found => found.TotalStatus == BuildTotalStatus.Success && found.RepoUrl == "http://flo"));
        }

    }
    
}