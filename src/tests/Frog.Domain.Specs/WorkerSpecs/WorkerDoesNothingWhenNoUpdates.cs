using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerDoesNothingWhenNoUpdates : WorkerSpecsBase
    {
        public override void Given()
        {
            base.Given();
            sourceRepoDriver.GetLatestRevision().Returns("2344");
            workingAreaGoverner.AllocateWorkingArea().Returns("dugh");
            worker = new Worker(pipeline, workingAreaGoverner);
        }

        public override void When()
        {
            worker.CheckForUpdatesAndKickOffPipeline(repo:sourceRepoDriver, revision:"2344");
        }

        [Test]
        public void should_ask_repository_for_latest_rev()
        {
            sourceRepoDriver.Received().GetLatestRevision();
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            sourceRepoDriver.DidNotReceive().GetSourceRevision(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            pipeline.DidNotReceive().Process(Arg.Any<SourceDrop>());
        }

        [Test]
        public void should_not_clean_working_area()
        {
            workingAreaGoverner.DidNotReceive().DeallocateWorkingArea(Arg.Any<string>());
        }
    }
}