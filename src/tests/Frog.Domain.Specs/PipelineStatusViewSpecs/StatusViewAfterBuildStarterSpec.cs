using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "somerepo";
            HandleBuildStarted(DefaultTask);
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Overall,
                        Is.EqualTo(BuildTotalStatus.BuildStarted));
        }
    }
}