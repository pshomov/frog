using System;
using Frog.Domain.ExecTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineProcessFaultyBuild : PipelineProcessSpecBase
    {
        private FakeTaskDescription taskDescription;

        protected override void Given()
        {
            base.Given();
            bool shouldStop;
            taskDescription = new FakeTaskDescription();
            TaskSource.Detect(Arg.Any<string>(), out shouldStop).Returns(As.List<Domain.TaskDescription>(taskDescription));
            Task1 = Substitute.For<ExecutableTask>();
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Failure, 4));
            Task2 = Substitute.For<ExecutableTask>();
            Task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            ExecTaskGenerator.GimeTasks(Arg.Any<FakeTaskDescription>()).Returns(As.List(Task1, Task2));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_get_all_tasks()
        {
            bool shouldStop;
            TaskSource.Received().Detect("", out shouldStop);
        }

        [Test]
        public void should_ask_task_generator_for_task_descriptions()
        {
            ExecTaskGenerator.Received().GimeTasks(taskDescription);
        }

        [Test]
        public void should_broadcast_build_started_with_two_non_started_tasks()
        {
            PipelineOnBuildStarted.Received().Invoke(Arg.Is<PipelineStatus>(
                started =>
                started.Tasks.Count == 2 &&
                started.Tasks[0].Status == TaskInfo.TaskStatus.NotStarted &&
                started.Tasks[1].Status == TaskInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_update_build_status_when_task_starts()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Any<Guid>(), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_update_build_status_when_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Any<Guid>(), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.FinishedError));
        }

        [Test]
        public void should_publish_build_ended_with_error()
        {
            PipelineOnBuildEnded.Received().Invoke(Arg.Is<BuildTotalEndStatus>(
                started =>
                started == BuildTotalEndStatus.Error));
        }

        [Test]
        public void should_not_start_second_task_at_all()
        {
            PipelineOnBuildUpdated.DidNotReceive().Invoke(Arg.Is(1), Arg.Any<Guid>(), Arg.Any<TaskInfo.TaskStatus>());
        }

        [Test]
        public void should_not_call_second_task()
        {
            Task1.Received().Perform(Arg.Any<SourceDrop>());
            Task2.DidNotReceive().Perform(Arg.Any<SourceDrop>());
        }
    }
}