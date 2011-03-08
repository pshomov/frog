using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
using Arg = NSubstitute.Arg;

namespace Frog.Domain.Specs.ValveSpecs
{
    [TestFixture]
    public class ValveSourceUpdates : ValveWhenUpdateSpecsBase
    {
        IEventPublisher eventPublisher;

        public override void Given()
        {
            base.Given();
            sourceRepoDriver.GetLatestRevision().Returns("2344");
            workingArea.AllocateWorkingArea().Returns("dugh");
            eventPublisher = Substitute.For<IEventPublisher>();
            valve = new Valve(pipeline, workingArea, eventPublisher);
        }

        public override void When()
        {
            valve.Check(repoUrl:sourceRepoDriver, revision:"123");
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
            pipeline.Received().Process(Arg.Is<SourceDrop>(obj => obj.SourceDropLocation == "dugh" ));
        }

        [Test]
        public void should_send_message_that_build_has_started()
        {
            eventPublisher.Received().Publish(Arg.Is<UpdateFound>(found => found.Revision == "2344"));
        }


    }
}