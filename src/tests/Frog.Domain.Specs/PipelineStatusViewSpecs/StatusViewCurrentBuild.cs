using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.UI;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewCurrentBuildPublicRepo : BDD
    {
        private const string RepoUrl = "http://lilalo";
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected BuildStarted BuildMessage;
        private ProjectView projectView;

        protected override void Given()
        {
            projectView = new ProjectView();
            View = new PipelineStatusView(projectView);
        }

        protected override void When()
        {
            BuildMessage = new BuildStarted
            {
                RepoUrl = RepoUrl,
                BuildId = Guid.NewGuid(),
                Status = new PipelineStatus(Guid.NewGuid())
                {
                    Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                }
            };
            View.Handle(BuildMessage);
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(projectView.GetCurrentBuild(RepoUrl), Is.EqualTo(BuildMessage.BuildId));
        }

    }

    class StatusViewCurrentBuildPrivateRepo : BDD
    {
        private const string PrivateRepoUrl = "http://psh:pass@github.com/p1/p2";
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected BuildStarted BuildMessage;
        private ProjectView projectView;

        protected override void Given()
        {
            projectView = new ProjectView();
            View = new PipelineStatusView(projectView);
        }

        protected override void When()
        {
            BuildMessage = new BuildStarted
            {
                RepoUrl = PrivateRepoUrl,
                BuildId = Guid.NewGuid(),
                Status = new PipelineStatus(Guid.NewGuid())
                {
                    Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                }
            };
            View.Handle(BuildMessage);
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(projectView.GetCurrentBuild("http://github.com/p1/p2"), Is.EqualTo(BuildMessage.BuildId));
        }
    }
    class StatusViewListOfBuilds : BDD
    {
        private const string PrivateRepoUrl = "http://psh:pass@github.com/p1/p2";
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected BuildStarted BuildMessage;
        private ProjectView projectView;
        private Guid newGuid;
        private Guid oldGuid;

        protected override void Given()
        {
            projectView = new ProjectView();
            View = new PipelineStatusView(projectView);
        }

        protected override void When()
        {
            newGuid = Guid.NewGuid();
            BuildMessage = new BuildStarted
            {
                RepoUrl = PrivateRepoUrl,
                BuildId = newGuid,
                Status = new PipelineStatus(newGuid)
                {
                    Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                }
            };
            View.Handle(BuildMessage);
            View.Handle(new BuildEnded(newGuid, BuildTotalEndStatus.Success));
            oldGuid = newGuid;
            newGuid = Guid.NewGuid();
            BuildMessage = new BuildStarted
            {
                RepoUrl = PrivateRepoUrl,
                BuildId = newGuid,
                Status = new PipelineStatus(newGuid)
                {
                    Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                }
            };
            View.Handle(BuildMessage);
            View.Handle(new BuildEnded(newGuid, BuildTotalEndStatus.Success));
        }

        [Test]
        public void should_have_as_many_builds_in_the_list_as_they_were()
        {
            Assert.That(projectView.GetListOfBuilds("http://github.com/p1/p2").Count, Is.EqualTo(2));
        }

        [Test]
        public void should_have_builds_in_the_order_as_they_got_executed()
        {
            Assert.That(projectView.GetListOfBuilds("http://github.com/p1/p2")[0], Is.EqualTo(oldGuid));
            Assert.That(projectView.GetListOfBuilds("http://github.com/p1/p2")[1], Is.EqualTo(newGuid));
        }
    }
}
