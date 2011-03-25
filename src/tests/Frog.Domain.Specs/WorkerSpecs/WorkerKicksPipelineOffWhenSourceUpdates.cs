using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerKicksPipelineOffWhenSourceUpdates : WorkerSpecsBase
    {
        bool updateFound;
        string newRevision;

        protected override void Given()
        {
            base.Given();
            SourceRepoDriver.GetLatestRevision().Returns("2344");
            WorkingAreaGovernor.AllocateWorkingArea().Returns("dugh");
            Worker = new Worker(Pipeline, WorkingAreaGovernor);
            Worker.OnUpdateFound += s =>
                                        {
                                            updateFound = true;
                                            newRevision = s;
                                        };
        }

        protected override void When()
        {
            Worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver: SourceRepoDriver, revision: "123");
        }

        [Test]
        public void should_ask_repository_for_latest_rev()
        {
            SourceRepoDriver.Received().GetLatestRevision();
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            SourceRepoDriver.Received().GetSourceRevision("2344", "dugh");
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            Pipeline.Received().Process(Arg.Is<SourceDrop>(obj => obj.SourceDropLocation == "dugh"));
        }

        [Test]
        public void should_send_message_that_build_has_started()
        {
            Assert.That(updateFound, Is.True);
            Assert.That(newRevision, Is.EqualTo("2344"));
        }

        [Test]
        public void should_clean_working_area()
        {
            WorkingAreaGovernor.Received().DeallocateWorkingArea("dugh");
        }
    }
}