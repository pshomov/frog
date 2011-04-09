using Frog.Domain.CustomTasks;
using Frog.Domain.ExecTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineProcessFaultyBuild : PipelineProcessSpecBase
    {
        protected override void Given()
        {
            base.Given();
            SrcTask1 = new MSBuildTask("");
            TaskSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(SrcTask1));
            Task1 = Substitute.For<IExecTask>();
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Failure, 4));
            Task2 = Substitute.For<IExecTask>();
            Task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            ExecTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(Task1, Task2));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_get_all_tasks()
        {
            TaskSource.Received().Detect("");
        }

        [Test]
        public void should_ask_task_generator_for_task_descriptions()
        {
            ExecTaskGenerator.Received().GimeTasks(SrcTask1);
        }

        [Test]
        public void should_broadcast_build_started_with_two_non_stared_tasks()
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
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_update_build_status_when_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Is<TaskInfo.TaskStatus>(
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
            PipelineOnBuildUpdated.DidNotReceive().Invoke(Arg.Is(1), Arg.Any<TaskInfo.TaskStatus>());
        }

        [Test]
        public void should_not_call_second_task()
        {
            Task1.Received().Perform(Arg.Any<SourceDrop>());
            Task2.DidNotReceive().Perform(Arg.Any<SourceDrop>());
        }
    }
}