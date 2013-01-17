using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{

    class StatusViewCurrentBuildPublicRepo : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "http://lilalo";
            HandleProjectCheckedOut("");
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(ProjectView.GetCurrentBuild(RepoUrl), Is.EqualTo(NewGuid));
        }

    }
}
