using System;
using Frog.Domain.UI;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : BDD
    {
        PipelineStatusView view;
        PipelineStatusView.BuildStatus buildStatus;
        PipelineStatus pipelineStatus;

        public override void Given()
        {
            buildStatus = new PipelineStatusView.BuildStatus();
            view = new PipelineStatusView(buildStatus);
        }

        public override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.NotStarted}
                                         }
                                 };
            view.Handle(new BuildStarted
                            {
                                Status =
                                    new PipelineStatus(pipelineStatus)
                            });
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineStarted));
        }

        [Test]
        public void should_set_status_as_as_in_message()
        {
            Assert.True(new CompareObjects().Compare(buildStatus.PipelineStatus, pipelineStatus));
        }
    }

    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : BDD
    {
        PipelineStatusView view;
        PipelineStatusView.BuildStatus buildStatus;

        public override void Given()
        {
            buildStatus = new PipelineStatusView.BuildStatus();
            view = new PipelineStatusView(buildStatus);
        }

        public override void When()
        {
            view.Handle(new BuildEnded(BuildEnded.BuildStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineCompletedFailure));
        }

    }
    [TestFixture]
    public class StatusViewAfterBuildCompletedSuccessfullySpec : BDD
    {
        PipelineStatusView view;
        PipelineStatusView.BuildStatus buildStatus;

        public override void Given()
        {
            buildStatus = new PipelineStatusView.BuildStatus();
            view = new PipelineStatusView(buildStatus);
        }

        public override void When()
        {
            view.Handle(new BuildEnded(BuildEnded.BuildStatus.Success));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineCompletedSuccess));
        }

    }

    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : BDD
    {
        PipelineStatusView view;
        PipelineStatusView.BuildStatus buildStatus;
        PipelineStatus pipelineStatus;

        public override void Given()
        {
            buildStatus = new PipelineStatusView.BuildStatus();
            view = new PipelineStatusView(buildStatus);
        }

        public override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
            {
                tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.Started}
                                         }
            };
            view.Handle(new BuildUpdated()
            {
                Status =
                    new PipelineStatus(pipelineStatus)
            });

        }

        [Test]
        public void should_set_status_to_TASK_STARTED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineStarted));
        }

        [Test]
        public void should_set_task_name_to_empty()
        {
            Assert.True(new CompareObjects().Compare(buildStatus.PipelineStatus, pipelineStatus));
        }

    }

}