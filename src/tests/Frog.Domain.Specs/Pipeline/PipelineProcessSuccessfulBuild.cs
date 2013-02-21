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
            bool shouldStop;
            TaskSource.Detect(Arg.Any<string>(), out shouldStop).Returns(As.List<Task>(SrcTask1));

            Task1 = Substitute.For<IExecTask>();
            Task1.When(task => task.Perform(Arg.Any<SourceDrop>()))
                .Do(info => Task1.OnTerminalOutputUpdate += Raise.Event<Action<string>>("task1"));
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            
            Task2 = Substitute.For<IExecTask>();
            Task2.When(task => task.Perform(Arg.Any<SourceDrop>()))
                .Do(info => Task2.OnTerminalOutputUpdate += Raise.Event<Action<string>>("task2"));
            Task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));

            ExecTaskGenerator.GimeTasks(Arg.Any<Task>()).Returns(As.List(Task1, Task2));

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
        public void should_have_the_same_terminalId_when_task_finshes()
        {
            PipelineOnBuildUpdated.Received().Invoke(1, guidTask1, TaskInfo.TaskStatus.FinishedSuccess);
            PipelineOnBuildUpdated.Received().Invoke(0, guidTask0, TaskInfo.TaskStatus.FinishedSuccess);
        }

        [Test]
        public void should_have_the_same_terminalId_when_task_terminal_update()
        {
            PipelineOnTerminalUpdate.Received().Invoke(Arg.Is<TerminalUpdateInfo>(info => info.TaskIndex == 0 && info.TerminalId == guidTask0));
            PipelineOnTerminalUpdate.Received().Invoke(Arg.Is<TerminalUpdateInfo>(info => info.TaskIndex == 1 && info.TerminalId == guidTask1));
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
            PipelineOnBuildStarted.Invoke(Arg.Do<PipelineStatus>(status => {
                                                                               guidTask0 = status.Tasks[0].TerminalId;
                                                                               guidTask1 = status.Tasks[1].TerminalId;
            }));
        }
    }
}