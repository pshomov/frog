using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerKicksPipelineOffWhenSourceUpdates : WorkerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            SourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "2344" });
            WorkingAreaGovernor.AllocateWorkingArea().Returns("dugh");
            Worker = new Worker(Pipeline, WorkingAreaGovernor);
        }

        protected override void When()
        {
            Worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver: SourceRepoDriver, revision: "123");
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            SourceRepoDriver.Received().GetSourceRevision("123", "dugh");
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            Pipeline.Received().Process(Arg.Is<SourceDrop>(obj => obj.SourceDropLocation == "dugh"));
        }

        [Test]
        public void should_clean_working_area()
        {
            WorkingAreaGovernor.Received().DeallocateWorkingArea("dugh");
        }
    }
}