using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerWhenNoUpdates : WorkerSpecsBase
    {
        private string revisions;

        protected override void Given()
        {
            base.Given();
            SourceRepoDriver.GetLatestRevision().Returns("2344");
            WorkingAreaGovernor.AllocateWorkingArea().Returns("dugh");
            Worker = new Worker(Pipeline, WorkingAreaGovernor);
            Worker.OnUpdateFound += s => { revisions = s; };
            revisions = "";
        }

        protected override void When()
        {
            Worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver:SourceRepoDriver, revision:"2344");
        }

        [Test]
        public void should_ask_repository_for_latest_rev()
        {
            SourceRepoDriver.Received().GetLatestRevision();
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            SourceRepoDriver.DidNotReceive().GetSourceRevision(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            Pipeline.DidNotReceive().Process(Arg.Any<SourceDrop>());
        }

        [Test]
        public void should_not_clean_working_area()
        {
            WorkingAreaGovernor.DidNotReceive().DeallocateWorkingArea(Arg.Any<string>());
        }

        [Test]
        public void should_send_retrieved_revision()
        {
            Assert.That(revisions, Is.EqualTo("2344"));
        }

    }
}