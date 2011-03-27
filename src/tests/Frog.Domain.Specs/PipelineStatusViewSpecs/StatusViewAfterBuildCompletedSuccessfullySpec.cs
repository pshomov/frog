using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            View.Handle(new BuildEnded("http://flo", BuildTotalStatus.Success));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildStatuses["http://flo"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.PipelineCompletedSuccess));
        }
    }
}