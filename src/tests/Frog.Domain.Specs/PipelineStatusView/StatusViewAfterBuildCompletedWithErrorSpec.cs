using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusView
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            view.Handle(new BuildEnded("http://fle", BuildTotalStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatuses["http://fle"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.PipelineCompletedFailure));
        }
    }
}