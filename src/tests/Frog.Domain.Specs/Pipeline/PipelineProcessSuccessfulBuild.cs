using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.CustomTasks;
using Frog.Domain.ExecTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineProcessSuccessfulBuild : PipelineProcessSpecBase
    {
        protected override void Given()
        {
            base.Given();
            SrcTask1 = new MSBuildTask("");
            TaskSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(SrcTask1));
            Task1 = Substitute.For<IExecTask>();
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            Task2 = Substitute.For<IExecTask>();
            Task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            ExecTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(Task1, Task2));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_update_build_status_when_first_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_update_build_status_when_second_task_starts()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(1), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_update_build_status_when_second_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(1), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_publish_build_ended_with_success()
        {
            PipelineOnBuildEnded.Received().Invoke(Arg.Is<BuildTotalEndStatus>(
                started =>
                started == BuildTotalEndStatus.Success));
        }

        [Test]
        public void should_call_both_tasks()
        {
            Task1.Received().Perform(Arg.Any<SourceDrop>());
            Task2.Received().Perform(Arg.Any<SourceDrop>());
        }
    }
}