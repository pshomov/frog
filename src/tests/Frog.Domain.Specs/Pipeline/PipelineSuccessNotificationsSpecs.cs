using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
using Arg = NSubstitute.Arg;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineSuccessNotificationsSpecs : BDD
    {
        readonly ExecTask task1 = Substitute.For<ExecTask>("", "");
        readonly ExecTask task2 = Substitute.For<ExecTask>("", "");
        PipelineOfTasks pipeline;

        readonly IEventPublisher bus = Substitute.For<IEventPublisher>();

        public override void Given()
        {
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            pipeline = new PipelineOfTasks(bus, new FixedTasksDispenser(task1, task2));
        }

        public override void When()
        {
            pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_send_pipeline_started_message()
        {
            task1.Received().Perform(Arg.Any<SourceDrop>());
            bus.Received().Publish(Arg.Any<BuildStarted>());
        }

        [Test]
        public void should_start_second_task_too()
        {
            task2.Received().Perform(Arg.Any<SourceDrop>());
        }

        [Test]
        public void should_send_pipeline_ended_message_with_status_success()
        {
            bus.Received().Publish(Arg.Is<BuildEnded>(obj => obj.Status == BuildEnded.BuildStatus.Success));
        }

    }
}