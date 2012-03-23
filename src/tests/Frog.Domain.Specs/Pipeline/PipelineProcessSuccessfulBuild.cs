using System;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.ExecTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineProcessSuccessfulBuild : PipelineProcessSpecBase
    {
        private Guid guidTask1;
        private Guid guidTask0;

        protected override void Given()
        {
            base.Given();
            SrcTask1 = new MSBuildTask("");
            TaskSource.Detect(Arg.Any<string>()).Returns(As.List<Domain.Task>(SrcTask1));
            Task1 = Substitute.For<IExecTask>();
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            Task2 = Substitute.For<IExecTask>();
            Task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            ExecTaskGenerator.GimeTasks(Arg.Any<Domain.Task>()).Returns(As.List(Task1, Task2));

            SaveTheTerminalIdsForTasks();
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_update_build_status_when_first_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(0), Arg.Any<Guid>(), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_update_build_status_when_second_task_starts()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(1), Arg.Any<Guid>(), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.Started));
        }

        [Test]
        public void should_update_build_status_when_second_task_finishes()
        {
            PipelineOnBuildUpdated.Received().Invoke(Arg.Is(1), Arg.Any<Guid>(), Arg.Is<TaskInfo.TaskStatus>(
                status =>
                status == TaskInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_have_the_same_guid_when_task_starts_and_finshes()
        {
            PipelineOnBuildUpdated.Received().Invoke(1, guidTask1, TaskInfo.TaskStatus.FinishedSuccess);
            PipelineOnBuildUpdated.Received().Invoke(0, guidTask0, TaskInfo.TaskStatus.FinishedSuccess);
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

        private void SaveTheTerminalIdsForTasks()
        {
            PipelineOnBuildUpdated.Invoke(1, Arg.Do<Guid>(guid => { guidTask1 = guid; }), TaskInfo.TaskStatus.Started);
            PipelineOnBuildUpdated.Invoke(0, Arg.Do<Guid>(guid => { guidTask0 = guid; }), TaskInfo.TaskStatus.Started);
        }
    }
}