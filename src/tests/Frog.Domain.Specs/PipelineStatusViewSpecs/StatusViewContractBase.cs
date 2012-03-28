using System;
using System.Collections.Generic;
using Frog.Domain.UI;
using Frog.Specs.Support;

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

}