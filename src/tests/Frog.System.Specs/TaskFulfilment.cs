using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using xray;
using Is = NHamcrest.Core.Is;

namespace Frog.System.Specs
{
    [TestFixture]
    public class TaskFulfilment : BDD
    {
        Valve valve;
        PipelineOfTasks pipeline;
        SystemDriver system;
        RepositoryDriver repo;

        public override void Given()
        {
            system = SystemDriver.GetCleanSystem();
            repo = RepositoryDriver.GetNewRepository();

            var task1 = Substitute.For<ExecTask>(null, null);
            var task2 = Substitute.For<ExecTask>(null, null);

            pipeline = new PipelineOfTasks(system.Bus,
                                           new FixedTasksDispenser(task1, task2));
            system.MonitorRepository(repo.Url);
            valve = new Valve(system.Git, pipeline, system.WorkingArea);

            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Failure, 23));
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_have_build_result_in_build_complete_event()
        {
            var prober = new PollingProber(3000, 50);
            Assert.True(
                prober.check(
                    Take.Snapshot(() => system.GetEventsSnapshot()).Has(
                        list => list.FindAll(@event => @event.GetType() == typeof (TaskStarted)).Count, Is.EqualTo(1))));
            Assert.True(
                prober.check(
                    Take.Snapshot(() => system.GetEventsSnapshot()).Has(
                        list => list.FindAll(@event => @event.GetType() == typeof (TaskFinished)).Count, Is.EqualTo(1))));
        }

        protected override void GivenCleanup()
        {
            system.ResetSystem();
            repo.Cleanup();
        }
    }
}