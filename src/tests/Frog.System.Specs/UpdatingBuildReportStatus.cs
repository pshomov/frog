using System.Collections.Generic;
using System.Threading;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskDetection;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NHamcrest;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
using xray;
using Has = NHamcrest.Core.Has;
using Is = NHamcrest.Core.Is;

namespace Frog.System.Specs
{
    [TestFixture]
    public class UpdatingBuildReportStatus : BDD
    {
        Valve valve;
        PipelineOfTasks pipeline;
        SystemDriver system;
        RepositoryDriver repo;

        public override void Given()
        {
            system = SystemDriver.GetCleanSystem();
            repo = RepositoryDriver.GetNewRepository();
            var execTaskGenerator = Substitute.For<IExecTaskGenerator>();
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(
                As.List(new ExecTask(@"ruby", @"-e 'exit 2'", "task_name")));
            pipeline = new PipelineOfTasks(system.Bus, new FixedTasksDispenser(new MSBuildTaskDescriptions("asdasd")),
                                           execTaskGenerator);
            system.MonitorRepository(repo.Url);
            valve = new Valve(system.Git, pipeline, system.WorkingArea);
        }

        public override void When()
        {
            ThreadPool.QueueUserWorkItem(state => valve.Check());
        }

        [Test]
        public void should_receive_build_started_event_followed_by_build_complete()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              new CustomMatcher<List<Event>>("find it",
                                                                             list =>
                                                                             list.FindIndex(
                                                                                 @event =>
                                                                                 @event.GetType() ==
                                                                                 typeof (BuildStarted) &&
                                                                                 (((BuildStarted) @event).Status.tasks[0
                                                                                      ].Status ==
                                                                                  TasksInfo.TaskStatus.NotStarted))
                                                                                   > -1))
                                         .Has(x => x,
                                              new CustomMatcher<List<Event>>("find it",
                                                                             list =>
                                                                             list.FindIndex(
                                                                                 @event =>
                                                                                 @event.GetType() ==
                                                                                 typeof(BuildUpdated) &&
                                                                                 (((BuildUpdated)@event).Status.tasks[0
                                                                                      ].Status ==
                                                                                  TasksInfo.TaskStatus.Started))
                                                                                   > -1))
                                         .Has(x => x,
                                              new CustomMatcher<List<Event>>("find it",
                                                                             list =>
                                                                             list.FindIndex(
                                                                                 @event =>
                                                                                 @event.GetType() ==
                                                                                 typeof(BuildUpdated) &&
                                                                                 (((BuildUpdated)@event).Status.tasks[0
                                                                                      ].Status ==
                                                                                  TasksInfo.TaskStatus.FinishedError))
                                                                                   > -1))
                                         .Has(x => x, Has.Item(Is.InstanceOf(typeof(BuildEnded))))
                            ));
        }

        protected override void GivenCleanup()
        {
            system.ResetSystem();
        }
    }
}