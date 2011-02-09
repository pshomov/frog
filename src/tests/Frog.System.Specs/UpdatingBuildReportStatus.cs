using System.Threading;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Domain.UI;
using NUnit.Framework;
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
            pipeline = new PipelineOfTasks(system.Bus, new FixedTasksDispenser(new ExecTask(@"ruby", @"-e 'exit 2'")));
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
            Assert.True(prober.check(Take.Snapshot(() => system.CurrentReport).
                                         Has(x => x.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineStarted))
                            ));
            Assert.True(prober.check(Take.Snapshot(() => system.CurrentReport).
                                         Has(x => x.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineComplete))
                            ));
        }

        [Test]
        public void should_have_build_result_in_build_complete_event()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.CurrentReport).
                                         Has(x => x.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineComplete))
                                         .
                                         Has(x => x.Completion,
                                             Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Error))
                            ));
        }

        protected override void GivenCleanup()
        {
            system.ResetSystem();
        }
    }
}