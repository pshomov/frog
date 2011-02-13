using System.Collections.Generic;
using System.Threading;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskDetection;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
using xray;
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
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(new ExecTask(@"ruby", @"-e 'exit 2'", "task_name")));
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
                                         .Has(x => x, NHamcrest.Core.Has.Item(Is.InstanceOf(typeof(BuildStarted))))
                                         .Has(x => x, NHamcrest.Core.Has.Item(Is.InstanceOf(typeof(BuildUpdated))))
                                         .Has(x => x, NHamcrest.Core.Has.Item(Is.InstanceOf(typeof(BuildUpdated))))
                                         .Has(x => x, NHamcrest.Core.Has.Item(Is.InstanceOf(typeof(BuildEnded))))
                            ));
        }

//        [Test]
//        public void should_have_build_result_in_build_complete_event()
//        {
//            var prober = new PollingProber(3000, 100);
//            Assert.True(prober.check(Take.Snapshot(() => system.CurrentReport).
//                                         Has(x => x.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineComplete))
//                                         .
//                                         Has(x => x.Completion,
//                                             Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Error))
//                            ));
//        }

        protected override void GivenCleanup()
        {
            system.ResetSystem();
        }
    }
}