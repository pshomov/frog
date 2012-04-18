using System;
using System.Collections.Generic;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewContractBase : BDD
    {
        protected string RepoUrl;
        protected PipelineStatusEventHandler EventHandler;
        protected BuildStarted BuildMessage;
        protected Guid NewGuid;
        protected TaskInfo DefaultTask;
        protected ProjectView ProjectView;
        protected BuildView BuildView;
        ProjectTestSupport project_test_support;
        private int sequnceId;

        int NextSequnceId
        {
            get { return sequnceId++; }
        }

        protected override void GivenCleanup()
        {
            project_test_support.WipeBucket();
        }

        protected override void Given()
        {
            sequnceId = 0;
            ProjectView = SetupProjectView();
            project_test_support = ProjectView as ProjectTestSupport;
            BuildView = ProjectView as BuildView; 
            EventHandler = new PipelineStatusEventHandler(StoreFactory.WireupEventStore());
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
            EventHandler.Handle(new BuildEnded(NewGuid, buildTotalEndStatus, NextSequnceId));
        }

        protected void HandleBuildStarted(params TaskInfo[] tasks)
        {
            BuildMessage = CreateBuildMessage(NewGuid, RepoUrl, tasks);
            EventHandler.Handle(BuildMessage);
        }

        private BuildStarted CreateBuildMessage(Guid buildId, string repoUrl, params TaskInfo[] tasks)
        {
            return new BuildStarted(
                       
                repoUrl : repoUrl,
                buildId : buildId,
                status : new PipelineStatus
                             {
                                 Tasks = new List<TaskInfo>(tasks)
                             },sequenceId:NextSequnceId);
        }

        protected void HandleProjectCheckedOut(string comment)
        {
            NewGuid = Guid.NewGuid();
            var checkedOut = new ProjectCheckedOut
                (NewGuid,0){ CheckoutInfo = new CheckoutInfo {Comment = comment}, RepoUrl = RepoUrl, SequenceId = NextSequnceId};
            EventHandler.Handle(checkedOut);
        }

        protected void HandleBuildUpdated(int taskIndex, TaskInfo.TaskStatus started, Guid terminalId)
        {
            EventHandler.Handle(new BuildUpdated(NewGuid, taskIndex, started, NextSequnceId, terminalId));
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