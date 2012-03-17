using System;
using System.Collections.Generic;
using Frog.Domain.UI;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewContractBase : BDD
    {
        protected string RepoUrl;
        protected PipelineStatusView View;
        protected BuildStarted BuildMessage;
        protected Guid NewGuid;
        protected TaskInfo DefaultTask;
        protected ProjectView ProjectView;

        protected override void GivenCleanup()
        {
            ProjectView.WipeBucket();
        }

        protected override void Given()
        {
            ProjectView = SetupProjectView();
            View = new PipelineStatusView(StoreFactory.WireupEventStore());
        }

        protected abstract ProjectView SetupProjectView();

        protected void HandleABuild(BuildTotalEndStatus buildTotalEndStatus)
        {
            HandleProjectCheckedOut("no comment");
            HandleBuildStarted(DefaultTask);
            HandleBuildEnded(buildTotalEndStatus);
        }

        protected void HandleBuildEnded(BuildTotalEndStatus buildTotalEndStatus)
        {
            View.Handle(new BuildEnded(NewGuid, buildTotalEndStatus, 0));
        }

        protected void HandleBuildStarted(params TaskInfo[] tasks)
        {
            BuildMessage = CreateBuildMessage(NewGuid, RepoUrl, tasks);
            View.Handle(BuildMessage);
        }

        private BuildStarted CreateBuildMessage(Guid buildId, string repoUrl, params TaskInfo[] tasks)
        {
            return new BuildStarted(
                       
                           repoUrl : repoUrl,
                           buildId : buildId,
                           status : new PipelineStatus
                                        {
                                            Tasks = new List<TaskInfo>(tasks)
                                        },sequenceId:0);
        }

        protected void HandleProjectCheckedOut(string comment)
        {
            NewGuid = Guid.NewGuid();
            var checkedOut = new ProjectCheckedOut
                                 (NewGuid,0){ CheckoutInfo = new CheckoutInfo {Comment = comment}, RepoUrl = RepoUrl};
            View.Handle(checkedOut);
        }
    }

    public abstract class StatusViewCurrentBuildPublicRepoBase : StatusViewContractBase
    {

        protected StatusViewCurrentBuildPublicRepoBase()
        {
            DefaultTask = new TaskInfo { Name = "task1", Status = TaskInfo.TaskStatus.NotStarted };
        }

        protected override ProjectView SetupProjectView()
        {
            return new EventBasedProjectView(StoreFactory.WireupEventStore());
        }
    }

    class StatusViewCurrentBuildPublicRepo : StatusViewCurrentBuildPublicRepoBase
    {
        protected PipelineStatus PipelineStatus;

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
