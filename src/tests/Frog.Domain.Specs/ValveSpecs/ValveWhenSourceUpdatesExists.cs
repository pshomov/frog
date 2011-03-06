using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.ValveSpecs
{
    [TestFixture]
    public class ValveWhenSourceUpdatesExists : ValveWhenUpdateSpecsBase
    {
        SourceDrop sourceDrop;

        public override void Given()
        {
            base.Given();
            sourceRepoDriver.CheckForUpdates().Returns(true);
            sourceDrop = new SourceDrop("");
            sourceRepoDriver.GetLatestSourceDrop("").Returns(sourceDrop);
            workingArea.AllocateWorkingArea().Returns("");
            valve = new Domain.Valve(sourceRepoDriver, pipeline, workingArea);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_ask_repository_to_do_initial_checkout()
        {
            sourceRepoDriver.Received().CheckForUpdates();
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            sourceRepoDriver.Received().GetLatestSourceDrop("");
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            pipeline.Received().Process(sourceDrop);
        }

    }
}