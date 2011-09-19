﻿using System;
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
        protected ConcurrentDictionary<Guid, BuildStatus> BuildStatuses;
        protected BuildStarted BuildMessage;
        private ConcurrentDictionary<string, Guid> currentBuilds;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuilds = new ConcurrentDictionary<string, Guid>();
            View = new PipelineStatusView(BuildStatuses, currentBuilds);

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
            Assert.That(currentBuilds[RepoUrl], Is.EqualTo(BuildMessage.BuildId));
        }

    }
    class StatusViewCurrentBuildPrivateRepo : BDD
    {
        private const string PrivateRepoUrl = "http://psh:pass@github.com/p1/p2";
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<Guid, BuildStatus> BuildStatuses;
        protected BuildStarted BuildMessage;
        private ConcurrentDictionary<string, Guid> currentBuilds;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuilds = new ConcurrentDictionary<string, Guid>();
            View = new PipelineStatusView(BuildStatuses, currentBuilds);

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
            Assert.That(currentBuilds["http://github.com/p1/p2"], Is.EqualTo(BuildMessage.BuildId));
        }

    }
}