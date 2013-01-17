using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewProjectCheckedOut : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void When()
        {
            RepoUrl = "http://github.com/p1/p2";
            HandleProjectCheckedOut("come comment");
        }

        [Test]
        public void should_have_as_many_builds_in_the_list_as_they_were()
        {
            Assert.That(ProjectView.GetListOfBuilds(RepoUrl)[0].Comment, Is.EqualTo("come comment"));
        }
    }
}