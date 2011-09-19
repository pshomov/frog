using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            View.Handle(new BuildEnded(BuildMessage.BuildId, BuildTotalEndStatus.Success));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Overall,
                        Is.EqualTo(BuildTotalStatus.BuildEndedSuccess));
        }
    }
}