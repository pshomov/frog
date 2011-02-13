using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskDetection;
using Frog.Specs.Support;
using Frog.Support;
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

            var task1 = Substitute.For<ExecTask>(null, null, null);
            var task2 = Substitute.For<ExecTask>(null, null, null);

            var execTaskGenerator = Substitute.For<IExecTaskGenerator>();
            execTaskGenerator.Received().GimeTasks(Arg.Any<ITask>()).Returns(As.List(task1, task2));

            pipeline = new PipelineOfTasks(system.Bus,
                                           new FixedTasksDispenser(new MSBuildTaskDescriptions("sdsd")), execTaskGenerator);
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