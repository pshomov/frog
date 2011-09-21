using System;
using Frog.Domain.UI;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewCurrentBuildPublicRepoBase : BDD
    {
        protected string RepoUrl;
        protected PipelineStatusView View;
        protected ProjectView ProjectView;
        protected BuildStarted BuildMessage;
        protected Guid NewGuid;

        protected override void Given()
        {
            ProjectView = new ProjectView();
            View = new PipelineStatusView(ProjectView);
        }

        protected void HandleABuild(BuildTotalEndStatus buildTotalEndStatus)
        {
            HandleBuildStarted();
            HandleBuildEnded(buildTotalEndStatus);
        }

        protected void HandleBuildEnded(BuildTotalEndStatus buildTotalEndStatus)
        {
            View.Handle(new BuildEnded(NewGuid, buildTotalEndStatus));
        }

        protected void HandleBuildStarted()
        {
            NewGuid = Guid.NewGuid();
            BuildMessage = CreateBuildMessage(NewGuid, RepoUrl);
            View.Handle(BuildMessage);
        }

        protected BuildStarted CreateBuildMessage(Guid buildId, string repoUrl)
        {
            return new BuildStarted
                       {
                           RepoUrl = repoUrl,
                           BuildId = buildId,
                           Status = new PipelineStatus()
                                        {
                                            Tasks =
                                                {
                                                    new TaskInfo
                                                        {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                                }
                                        }
                       };
        }
    }

    class StatusViewCurrentBuildPublicRepo : StatusViewCurrentBuildPublicRepoBase
    {
        protected PipelineStatus PipelineStatus;

        protected override void When()
        {
            RepoUrl = "http://lilalo"; 
            HandleABuild(BuildTotalEndStatus.Success);
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(ProjectView.GetCurrentBuild(RepoUrl), Is.EqualTo(BuildMessage.BuildId));
        }

    }

    class StatusViewCurrentBuildPrivateRepo : StatusViewCurrentBuildPublicRepoBase
    {
        protected PipelineStatus PipelineStatus;

        protected override void When()
        {
            RepoUrl = "http://psh:pass@github.com/p1/p2";
            HandleABuild(BuildTotalEndStatus.Success);
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(ProjectView.GetCurrentBuild("http://github.com/p1/p2"), Is.EqualTo(BuildMessage.BuildId));
        }
    }

    class StatusViewListOfBuilds : StatusViewCurrentBuildPublicRepoBase
    {
        protected PipelineStatus PipelineStatus;
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
            Assert.That(ProjectView.GetListOfBuilds("http://github.com/p1/p2")[0], Is.EqualTo(oldGuid));
            Assert.That(ProjectView.GetListOfBuilds("http://github.com/p1/p2")[1], Is.EqualTo(NewGuid));
        }
    }
}
