using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : BDD
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
            view.Handle(new BuildStarted());
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineStarted));
        }

        [Test]
        public void should_set_task_name_to_empty()
        {
            Assert.That(buildStatus.TaskName, Is.EqualTo(""));
        }

        [Test]
        public void should_set_exit_code_to_minus_one()
        {
            Assert.That(buildStatus.Completion, Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Dugh));
        }

    }

    [TestFixture]
    public class StatusViewAfterBuildCompletedSpec : BDD
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
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.PipelineComplete));
        }

        [Test]
        public void should_set_task_name_to_empty()
        {
            Assert.That(buildStatus.TaskName, Is.EqualTo(""));
        }

        [Test]
        public void should_set_exit_code_to_minus_one()
        {
            Assert.That(buildStatus.Completion, Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Error));
        }

    }

    [TestFixture]
    public class StatusViewAfterTaskStartedSpec : BDD
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
            view.Handle(new TaskStarted("fle"));
        }

        [Test]
        public void should_set_status_to_TASK_STARTED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.TaskStarted));
        }

        [Test]
        public void should_set_task_name_to_empty()
        {
            Assert.That(buildStatus.TaskName, Is.EqualTo("fle"));
        }

        [Test]
        public void should_set_exit_code_to_minus_one()
        {
            Assert.That(buildStatus.Completion, Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Dugh));
        }

    }

    [TestFixture]
    public class StatusViewAfterTaskCompletedSpec : BDD
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
            view.Handle(new TaskFinished("fle", ExecTaskResult.Status.Error));
        }

        [Test]
        public void should_set_status_to_TASK_ENDED()
        {
            Assert.That(buildStatus.Current, Is.EqualTo(PipelineStatusView.BuildStatus.Status.TaskEnded));
        }

        [Test]
        public void should_set_task_name_to_empty()
        {
            Assert.That(buildStatus.TaskName, Is.EqualTo("fle"));
        }

        [Test]
        public void should_set_exit_code_to_minus_one()
        {
            Assert.That(buildStatus.Completion, Is.EqualTo(PipelineStatusView.BuildStatus.TaskExit.Error));
        }

    }
}