using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewCurrentBuildPrivateRepo : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "http://psh:pass@github.com/p1/p2";
            HandleProjectCheckedOut("");
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(ProjectView.GetCurrentBuild("http://github.com/p1/p2"), Is.EqualTo(NewGuid));
        }
    }
}