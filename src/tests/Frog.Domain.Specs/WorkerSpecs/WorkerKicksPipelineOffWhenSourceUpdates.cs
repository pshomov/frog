using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerKicksPipelineOffWhenSourceUpdates : WorkerSpecsBase
    {
        private CheckoutInfo checkoutInfo;

        protected override void Given()
        {
            base.Given();
            SourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "2344" });
            SourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "comment"});
            WorkingAreaGovernor.AllocateWorkingArea().Returns("dugh");
            Worker = new Worker(Pipeline, WorkingAreaGovernor);
            Worker.OnProjectCheckedOut += info => checkoutInfo = info;
        }

        protected override void When()
        {
            Worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver: SourceRepoDriver, revision: "123");
        }

        [Test]
        public void should_provide_source_checkout_info()
        {
            Assert.That(checkoutInfo.Comment, Is.EqualTo("comment"));
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