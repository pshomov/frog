using System;
using System.IO;
using System.Threading;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using NUnit.Framework;
using SimpleCQRS;
using xray;
using It = NHamcrest.Core;

namespace Frog.System.Specs
{
    [TestFixture]
    public class UpdatingBuildReportStatus : BDD
    {
        Valve valve;
        PipelineOfTasks pipeline;
        BuildStatus report;

        public override void Given()
        {
            TestSystem.CleanTestSystem();
            report = new BuildStatus();
            var statusView = new PipelineStatusView(report);
            TestSystem.theBus.RegisterHandler<BuildStarted>(statusView.Handle);
            TestSystem.theBus.RegisterHandler<BuildEnded>(statusView.Handle);

            pipeline = new PipelineOfTasks(TestSystem.theBus, new ExecTask(@"ruby", @"-e 'exit 2'"));
            valve = new Valve(TestSystem.driver, pipeline, TestSystem.area);
        }

        public override void When()
        {
            ThreadPool.QueueUserWorkItem(state => valve.Check());
        }

        [Test]
        public void should_receive_build_started_event_followed_by_build_complete()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => report.Current).
                Has(x => x, It.Is.EqualTo(BuildStatus.Status.Started))
                ));
            Assert.True(prober.check(Take.Snapshot(() => report.Current).
                Has(x => x, It.Is.EqualTo(BuildStatus.Status.Complete))
                ));
        }

        [Test]
        public void should_have_build_result_in_build_complete_event()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => report).
                Has(x => x.Current, It.Is.EqualTo(BuildStatus.Status.Complete)).
                Has(x => x.Completion, It.Is.EqualTo(BuildStatus.TaskExit.Error))
                ));
        }

        public override void Cleanup()
        {
            TestSystem.CleanTestSystem();
        }
    }

    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>
    {
        readonly BuildStatus report;

        public PipelineStatusView(BuildStatus report)
        {
            this.report = report;
        }

        public void Handle(BuildStarted message)
        {
            report.Current = BuildStatus.Status.Started;
        }

        public void Handle(BuildEnded message)
        {
            report.Current = BuildStatus.Status.Complete;
            switch (message.Status)
            {
                case BuildEnded.BuildStatus.Fail:
                    report.Completion = BuildStatus.TaskExit.Fail;
                    break;
                case BuildEnded.BuildStatus.Error:
                    report.Completion = BuildStatus.TaskExit.Error;
                    break;
                case BuildEnded.BuildStatus.Success:
                    report.Completion = BuildStatus.TaskExit.Success;
                    break;
                default:
                    throw new ArgumentException("Build status argument not handled correctly, please report this error.");
            }
        }
    }

    public class BuildStatus
    {
        public enum Status {Started, NotStarted, Complete}
        public enum TaskExit {Dugh, Success, Error, Fail}
        public Status Current { get; set; }
        public TaskExit Completion { get; set; }
    }
}