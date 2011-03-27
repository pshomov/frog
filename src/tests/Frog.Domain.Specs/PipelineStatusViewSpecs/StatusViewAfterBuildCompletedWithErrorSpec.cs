using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            View.Handle(new BuildEnded("http://fle", BuildTotalStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildStatuses["http://fle"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.PipelineCompletedFailure));
        }
    }
}