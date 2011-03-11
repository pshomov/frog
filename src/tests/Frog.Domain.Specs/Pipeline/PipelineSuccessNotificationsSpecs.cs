using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineSuccessNotificationsSpecs : PipelineProcessSourceDropSpecBase
    {
        public override void Given()
        {
            base.Given();
            srcTask1 = new MSBuildTaskDescriptions("");
            taskSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(srcTask1));
            task1 = Substitute.For<ExecTask>("", "", "");
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            task2 = Substitute.For<ExecTask>("", "", "");
            task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(task1, task2));
        }

        public override void When()
        {
            pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_update_build_status_when_first_task_finishes()
        {
            bus.Received().Publish(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[0].Status == TasksInfo.TaskStatus.FinishedSuccess &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_update_build_status_when_second_task_starts()
        {
            bus.Received().Publish(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[0].Status == TasksInfo.TaskStatus.FinishedSuccess &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.Started));
        }

        [Test]
        public void should_update_build_status_when_second_task_finishes()
        {
            bus.Received().Publish(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[0].Status == TasksInfo.TaskStatus.FinishedSuccess &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_publish_build_ended_with_success()
        {
            bus.Received().Publish(Arg.Is<BuildEnded>(
                started =>
                started.Status == BuildEnded.BuildStatus.Success));
        }

        [Test]
        public void should_call_both_tasks()
        {
            task1.Received().Perform(Arg.Any<SourceDrop>());
            task2.Received().Perform(Arg.Any<SourceDrop>());
        }
    }
}