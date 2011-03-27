using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusView
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            view.Handle(new BuildEnded("http://flo", BuildTotalStatus.Success));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatuses["http://flo"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.PipelineCompletedSuccess));
        }
    }
}