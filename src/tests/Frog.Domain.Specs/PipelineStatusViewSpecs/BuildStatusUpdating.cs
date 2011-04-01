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
        BuildStatuz buildStatuz;

        protected override void Given()
        {
            buildStatuz = new BuildStatuz();
        }

        protected override void When()
        {
            buildStatuz.BuildStarted(As.List(new TaskInfo("task_name"), new TaskInfo("dd")));
        }

        [Test]
        public void should_have_build_tasks_prepared()
        {
            Assert.That(buildStatuz.Tasks.Count(), Is.EqualTo(2));
            Assert.That(buildStatuz.Tasks.ElementAt(0).Name, Is.EqualTo("task_name"));
            Assert.That(buildStatuz.Tasks.ElementAt(1).Name, Is.EqualTo("dd"));
        }

        [Test]
        public void should_set_the_task_output_to_empty()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(0).TerminalOutput, Is.EqualTo(""));
            Assert.That(buildStatuz.Tasks.ElementAt(1).TerminalOutput, Is.EqualTo(""));
        }

        [Test]
        public void should_set_all_tasks_status_to_not_started()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(0).Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
            Assert.That(buildStatuz.Tasks.ElementAt(1).Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }
    }

    [TestFixture]
    public class BuildStatusAfterBuildUpdated : BDD
    {
        BuildStatuz buildStatuz;

        protected override void Given()
        {
            buildStatuz = new BuildStatuz();
            buildStatuz.BuildStarted(As.List(new TaskInfo("task_name"), new TaskInfo("dd")));
        }

        protected override void When()
        {
            buildStatuz.BuildUpdated(1, TaskInfo.TaskStatus.Started);
        }

        [Test]
        public void should_keep_the_tasks_count()
        {
            Assert.That(buildStatuz.Tasks.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_update_the_task_status_specified_by_the_index()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(1).Status, Is.EqualTo(TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_not_update_status_for_all_other_tasks()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(0).Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }
    }


    [TestFixture]
    public class BuildStatusAfterBuildEnded : BDD
    {
        BuildStatuz buildStatuz;

        protected override void Given()
        {
            buildStatuz = new BuildStatuz();
            buildStatuz.BuildStarted(As.List(new TaskInfo("task_name"), new TaskInfo("dd")));
        }

        protected override void When()
        {
            buildStatuz.BuildEnded(BuildTotalStatus.Error);
        }

        [Test]
        public void should_keep_the_tasks_count()
        {
            Assert.That(buildStatuz.Tasks.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_update_the_task_status_specified_by_the_index()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(1).Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_not_update_status_for_all_other_tasks()
        {
            Assert.That(buildStatuz.Tasks.ElementAt(0).Status, Is.EqualTo(TaskInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_set_total_status_to_specified_value()
        {
            Assert.That(buildStatuz.Overall, Is.EqualTo(BuildTotalStatus.Error));
        }

    }
}