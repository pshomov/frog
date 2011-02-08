using System.Threading;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.UI.Web;
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

        public override void Given()
        {
            system = SystemDriver.GetCleanSystem();

            pipeline = new PipelineOfTasks(system.Bus, new FixedTasksDispenser(new ExecTask(@"ruby", @"-e 'exit 2'")));
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

        public override void Cleanup()
        {
            system.ResetSystem();
        }
    }
}