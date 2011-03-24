using System;
using System.Collections.Concurrent;
using Frog.Domain.UI;
using Frog.Specs.Support;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected PipelineStatusView view;
        protected PipelineStatus pipelineStatus;
        protected ConcurrentDictionary<string, PipelineStatusView.BuildStatus> buildStatuses;

        protected override void Given()
        {
            buildStatuses = new ConcurrentDictionary<string, PipelineStatusView.BuildStatus>();
            view = new PipelineStatusView(buildStatuses);
        }
    }

    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.NotStarted}
                                         }
                                 };
            view.Handle(new BuildStarted("http://repo", new PipelineStatus(pipelineStatus)));
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(buildStatuses["http://repo"].Current,
                        Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineStarted));
        }

        [Test]
        public void should_set_status_as_as_in_message()
        {
            Assert.True(new CompareObjects().Compare(buildStatuses["http://repo"].PipelineStatus, pipelineStatus));
        }
    }

    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            view.Handle(new BuildEnded("http://fle", BuildTotalStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatuses["http://fle"].Current,
                        Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineCompletedFailure));
        }
    }

    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            view.Handle(new BuildEnded("http://flo", BuildTotalStatus.Success));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatuses["http://flo"].Current,
                        Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineCompletedSuccess));
        }
    }

    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.Started}
                                         }
                                 };
            view.Handle(new BuildUpdated("http://dugh", new PipelineStatus(pipelineStatus)));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(buildStatuses["http://dugh"].Current,
                        Is.EqualTo(PipelineStatusView.BuildStatus.Status.NotStarted));
        }

        [Test]
        public void should_set_task_status_to_the_value_sent_in_the_event()
        {
            Assert.True(new CompareObjects().Compare(buildStatuses["http://dugh"].PipelineStatus, pipelineStatus));
        }
    }
}