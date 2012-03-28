using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewListOfBuilds : StatusViewCurrentBuildPublicRepoBase
    {
        private Guid oldGuid;

        protected override void When()
        {
            RepoUrl = "http://psh:pass@github.com/p1/p2";
            HandleABuild(BuildTotalEndStatus.Success);
            oldGuid = NewGuid;
            HandleABuild(BuildTotalEndStatus.Success);
        }

        [Test]
        public void should_have_as_many_builds_in_the_list_as_they_were()
        {
            Assert.That(ProjectView.GetListOfBuilds("http://github.com/p1/p2").Count, Is.EqualTo(2));
        }

        [Test]
        public void should_have_builds_in_the_order_as_they_got_executed()
        {
            Assert.That(ProjectView.GetListOfBuilds("http://github.com/p1/p2")[0].BuildId, Is.EqualTo(oldGuid));
            Assert.That(ProjectView.GetListOfBuilds("http://github.com/p1/p2")[1].BuildId, Is.EqualTo(NewGuid));
        }
    }
}