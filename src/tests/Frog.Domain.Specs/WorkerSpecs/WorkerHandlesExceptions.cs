using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerHandlesExceptions : WorkerSpecsBase
    {
        int eventCount;

        protected override void Given()
        {
            base.Given();
            eventCount = 0;

            SourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "qwe" });
            WorkingAreaGovernor.AllocateWorkingArea().Returns("1", "2");

            Pipeline.When(
                worker => worker.Process(Arg.Is<SourceDrop>(drop => drop.SourceDropLocation == "1"))).Do(
                    info => { throw new ApplicationException(); });
            Pipeline.When(
                worker => worker.Process(Arg.Is<SourceDrop>(drop => drop.SourceDropLocation == "2"))).Do(
                    callInfo =>
                        {
                            var terminalId = Guid.NewGuid();
                            Pipeline.OnBuildStarted +=
                                Raise.Event<BuildStartedDelegate>(new PipelineStatus());
                            Pipeline.OnBuildUpdated +=
                                Raise.Event<Action<int, Guid, TaskInfo.TaskStatus>>(0, terminalId, TaskInfo.TaskStatus.Started);
                            Pipeline.OnBuildEnded +=
                                Raise.Event<Action<BuildTotalEndStatus>>(BuildTotalEndStatus.Success);
                            Pipeline.OnTerminalUpdate +=
                                Raise.Event<Action<TerminalUpdateInfo>>(new TerminalUpdateInfo(0, "", 0, terminalId));
                        });

            Worker = new Worker(Pipeline, WorkingAreaGovernor);
            Worker.OnBuildStarted += delegate { eventCount++; };
            Worker.OnBuildEnded += delegate { eventCount++; };
            Worker.OnBuildUpdated += delegate { eventCount++; };
            Worker.OnTerminalUpdates += delegate { eventCount++; };

            try
            {
                Worker.ExecutePipelineForRevision(SourceRepoDriver, "");
            }
            catch (ApplicationException)
            {
            }
        }

        protected override void When()
        {
            Worker.ExecutePipelineForRevision(SourceRepoDriver, "");
        }

        [Test]
        public void should_have_reised_the_event_subscribers_ony_for_the_current_check()
        {
            Assert.That(eventCount, Is.EqualTo(4));
        }
    }
}