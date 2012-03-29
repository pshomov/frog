using System;
using System.Linq;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class BuildStatusAfterBuildStarted : BDD
    {
        BuildStatus buildStatus;

        protected override void Given()
        {
            buildStatus = new BuildStatus();
        }

        protected override void When()
        {
            buildStatus.BuildStarted(As.List(new TaskInfo("task_name", new Guid()), new TaskInfo("dd", new Guid())));
        }

        [Test]
        public void should_have_build_tasks_prepared()
        {
            Assert.That(buildStatus.Tasks.Count(), Is.EqualTo(2));
            Assert.That(buildStatus.Tasks[0].Name, Is.EqualTo("task_name"));
            Assert.That(buildStatus.Tasks[1].Name, Is.EqualTo("dd"));
        }

        [Test]
        public void should_set_the_task_output_to_empty()
        {
            Assert.That(buildStatus.Tasks[0].GetTerminalOutput().Content, Is.EqualTo(""));
            Assert.That(buildStatus.Tasks[1].GetTerminalOutput().Content, Is.EqualTo(""));
        }

        [Test]
        public void should_set_all_tasks_status_to_not_started()
        {
            Assert.That(buildStatus.Tasks[0].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
            Assert.That(buildStatus.Tasks[1].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }
    }

    [TestFixture]
    public class BuildStatusAfterBuildUpdated : BDD
    {
        BuildStatus buildStatus;

        protected override void Given()
        {
            buildStatus = new BuildStatus();
            buildStatus.BuildStarted(As.List(new TaskInfo("task_name", new Guid()), new TaskInfo("dd", new Guid())));
        }

        protected override void When()
        {
            buildStatus.BuildUpdated(1, TaskInfo.TaskStatus.Started);
        }

        [Test]
        public void should_keep_the_tasks_count()
        {
            Assert.That(buildStatus.Tasks.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_update_the_task_status_specified_by_the_index()
        {
            Assert.That(buildStatus.Tasks[1].Status, Is.EqualTo(TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_not_update_status_for_all_other_tasks()
        {
            Assert.That(buildStatus.Tasks[0].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }
    }


    [TestFixture]
    public class BuildStatusAfterBuildEndedSuccessfully : BDD
    {
        BuildStatus buildStatus;

        protected override void Given()
        {
            buildStatus = new BuildStatus();
            buildStatus.BuildStarted(As.List(new TaskInfo("task_name", new Guid()), new TaskInfo("dd", new Guid())));
        }

        protected override void When()
        {
            buildStatus.BuildEnded(BuildTotalEndStatus.Success);
        }

        [Test]
        public void should_keep_the_tasks_count()
        {
            Assert.That(buildStatus.Tasks.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_not_update_tasks_statuses()
        {
            Assert.That(buildStatus.Tasks[1].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
            Assert.That(buildStatus.Tasks[0].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_set_total_status_to_specified_value()
        {
            Assert.That(buildStatus.Overall, Is.EqualTo(BuildTotalStatus.BuildEndedSuccess));
        }

    }

    [TestFixture]
    public class BuildStatusAfterBuildEndedFailure : BDD
    {
        BuildStatus buildStatus;

        protected override void Given()
        {
            buildStatus = new BuildStatus();
            buildStatus.BuildStarted(As.List(new TaskInfo("task_name", new Guid()), new TaskInfo("dd", new Guid())));
        }

        protected override void When()
        {
            buildStatus.BuildEnded(BuildTotalEndStatus.Error);
        }

        [Test]
        public void should_keep_the_tasks_count()
        {
            Assert.That(buildStatus.Tasks.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_not_update_tasks_statuses()
        {
            Assert.That(buildStatus.Tasks[1].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
            Assert.That(buildStatus.Tasks[0].Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_set_total_status_to_specified_value()
        {
            Assert.That(buildStatus.Overall, Is.EqualTo(BuildTotalStatus.BuildEndedError));
        }

    }
}