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
            sourceRepoDriver.GetLatestRevision().Returns("2344");
            workingAreaGoverner.AllocateWorkingArea().Returns("dugh");
            worker = new Worker(pipeline, workingAreaGoverner);
            worker.OnUpdateFound += s =>
                                        {
                                            updateFound = true;
                                            newRevision = s;
                                        };
        }

        protected override void When()
        {
            worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver: sourceRepoDriver, revision: "123");
        }

        [Test]
        public void should_ask_repository_for_latest_rev()
        {
            sourceRepoDriver.Received().GetLatestRevision();
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            sourceRepoDriver.Received().GetSourceRevision("2344", "dugh");
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            pipeline.Received().Process(Arg.Is<SourceDrop>(obj => obj.SourceDropLocation == "dugh"));
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
            workingAreaGoverner.Received().DeallocateWorkingArea("dugh");
        }
    }
}