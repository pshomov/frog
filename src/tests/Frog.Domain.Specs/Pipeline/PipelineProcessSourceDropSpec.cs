using System;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs.Pipeline
{
    public abstract class PipelineProcessSourceDropSpecBase : BDD
    {
        protected Domain.Pipeline pipeline;
        protected ExecTask task1;
        protected ExecTask task2;
        protected TaskSource taskSource;
        protected IExecTaskGenerator execTaskGenerator;
        protected MSBuildTaskDescriptions srcTask1;
        protected IEventPublisher bus;
        protected Action<BuildStarted> pipelineOnBuildStarted;
        protected Action<BuildEnded> pipelineOnOnBuildEnded;
        protected Action<BuildUpdated> pipelineOnBuildUpdated;

        public override void Given()
        {
            bus = Substitute.For<IEventPublisher>();
            taskSource = Substitute.For<TaskSource>();
            execTaskGenerator = Substitute.For<IExecTaskGenerator>();
            pipeline = new PipelineOfTasks(taskSource, execTaskGenerator);
            pipelineOnBuildStarted = Substitute.For<Action<BuildStarted>>();
            pipelineOnOnBuildEnded = Substitute.For<Action<BuildEnded>>();
            pipelineOnBuildUpdated = Substitute.For<Action<BuildUpdated>>();
            pipeline.OnBuildStarted += pipelineOnBuildStarted;
            pipeline.OnBuildUpdated += pipelineOnBuildUpdated;
            pipeline.OnBuildEnded += pipelineOnOnBuildEnded;
        }
    }

    [TestFixture]
    public class PipelineProcessSourceDropSpec : PipelineProcessSourceDropSpecBase
    {
        public override void Given()
        {
            base.Given();
            srcTask1 = new MSBuildTaskDescriptions("");
            taskSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(srcTask1));
            task1 = Substitute.For<ExecTask>("", "", "");
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Failure, 4));
            task2 = Substitute.For<ExecTask>("", "", "");
            task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(task1, task2));
        }

        public override void When()
        {
            pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_get_all_tasks()
        {
            taskSource.Received().Detect("");
        }

        [Test]
        public void should_ask_task_generator_for_task_descriptions()
        {
            execTaskGenerator.Received().GimeTasks(srcTask1);
        }

        [Test]
        public void should_broadcast_build_started_with_two_non_stared_tasks()
        {
            pipelineOnBuildStarted.Received().Invoke(Arg.Is<BuildStarted>(
                    started =>
                    started.Status.tasks.Count == 2 &&
                    started.Status.tasks[0].Status == TasksInfo.TaskStatus.NotStarted &&
                    started.Status.tasks[1].Status == TasksInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_update_build_status_when_task_starts()
        {
            pipelineOnBuildUpdated.Received().Invoke(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[0].Status == TasksInfo.TaskStatus.Started &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_update_build_status_when_task_finishes()
        {
            pipelineOnBuildUpdated.Received().Invoke(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[0].Status == TasksInfo.TaskStatus.FinishedError &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.NotStarted));
        }

        [Test]
        public void should_publish_build_ended_with_error()
        {
            pipelineOnOnBuildEnded.Received().Invoke(Arg.Is<BuildEnded>(
                started =>
                started.Status == BuildEnded.BuildStatus.Error));
        }

        [Test]
        public void should_not_start_second_task_at_all()
        {
            pipelineOnBuildUpdated.DidNotReceive().Invoke(Arg.Is<BuildUpdated>(
                started =>
                started.Status.tasks.Count == 2 &&
                started.Status.tasks[1].Status == TasksInfo.TaskStatus.Started));
        }

        [Test]
        public void should_not_call_second_task()
        {
            task1.Received().Perform(Arg.Any<SourceDrop>());
            task2.DidNotReceive().Perform(Arg.Any<SourceDrop>());
        }

    }
}