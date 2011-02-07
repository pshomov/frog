using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SimpleCQRS;
using Arg = NSubstitute.Arg;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class PipelineFailsNotificationsSpecs : BDD
    {
        readonly ExecTask task1 = Substitute.For<ExecTask>("", "");
        readonly ExecTask task2 = Substitute.For<ExecTask>("", "");
        PipelineOfTasks pipeline;

        readonly IEventPublisher bus = Substitute.For<IEventPublisher>();

        public override void Given()
        {
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Failure, -1));
            pipeline = new PipelineOfTasks(bus, new FixedTasksDispencer(task1, task2));
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
        public void should_not_start_second_task()
        {
            task2.DidNotReceive().Perform(Arg.Any<SourceDrop>());
        }

        [Test]
        public void should_have_build_finished_message_with_failure_result()
        {
            bus.Received().Publish(Arg.Is<BuildEnded>(obj => obj.Status == BuildEnded.BuildStatus.Error));
        }

    }
}