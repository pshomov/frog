using Frog.Domain.Integration.UI;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "http://";
            HandleABuild(BuildTotalEndStatus.Error);
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildView.GetBuildStatus(BuildMessage.BuildId).Overall,
                        Is.EqualTo(BuildTotalStatus.BuildEndedError));
        }
    }
}