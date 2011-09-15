using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentHandlesException : AgentSpecsBase
    {
        private const int NumberOfExpectedEvents = 6;

        protected override void Given()
        {
            base.Given();
            var WorkerWithEvents = Worker;
            Worker = Substitute.For<Worker>(null,null);
            Worker.When(
                worker => worker.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Is("2"))).Do(
                    info => { throw new ApplicationException(); });
            Worker.When(
                worker => worker.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Is("3"))).Do(
                    callInfo => WorkerWithEvents.CheckForUpdatesAndKickOffPipeline(null,null));
            Agent = new Domain.Agent(Bus, Worker, url => null);
            try
            {
                Agent.Handle(new Build("asda", "2"));
            } catch(ApplicationException)
            {
            }
        }

        protected override void When()
        {
            Agent.Handle(new Build("asda", "3"));
        }

        [Test]
        public void should_unsubscribe_from_events_even_when_worker_throws_exception()
        {
            Assert.That(Bus.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Publish").Count(), Is.EqualTo(NumberOfExpectedEvents));
        }
    }
}