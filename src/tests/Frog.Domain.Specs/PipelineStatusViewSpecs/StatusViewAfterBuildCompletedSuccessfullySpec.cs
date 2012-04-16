using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "http://someurl";
            HandleABuild(BuildTotalEndStatus.Success);
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildView.GetBuildStatus(BuildMessage.BuildId).Overall,
                        Is.EqualTo(BuildTotalStatus.BuildEndedSuccess));
        }
    }
}