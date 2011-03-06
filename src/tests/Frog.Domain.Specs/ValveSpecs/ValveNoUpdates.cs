using NSubstitute;
using NUnit.Framework;
using Arg = NSubstitute.Arg;

namespace Frog.Domain.Specs.ValveSpecs
{
    [TestFixture]
    public class ValveNoUpdates : ValveWhenUpdateSpecsBase
    {
        SourceDrop sourceDrop;

        public override void Given()
        {
            base.Given();
            sourceRepoDriver.GetLatestRevision().Returns("2344");
            sourceDrop = new SourceDrop("");
            sourceRepoDriver.GetLatestSourceDrop("").Returns(sourceDrop);
            workingArea.AllocateWorkingArea().Returns("dugh");
            valve = new Valve(null, pipeline, workingArea);
        }

        public override void When()
        {
            valve.Check(repoUrl:sourceRepoDriver, revision:"2344");
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

    }
}