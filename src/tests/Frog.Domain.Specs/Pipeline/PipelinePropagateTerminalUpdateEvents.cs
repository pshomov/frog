using System;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.ExecTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelinePropagateTerminalUpdateEvents : PipelineProcessSpecBase
    {
        protected override void Given()
        {
            base.Given();
            bool shouldStop;
            TaskSource.Detect(Arg.Any<string>(), out shouldStop).Returns(As.List<Domain.TaskDescription>(new FakeTaskDescription()));
            Task1 = Substitute.For<ExecTask>();
            Task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 4));
            Task1.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(info =>
                                                                           {
                                                                               Task1.OnTerminalOutputUpdate +=
                                                                                   Raise.Event<Action<string>>("content1");
                                                                           });
            Task1.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(info =>
                                                                           {
                                                                               Task1.OnTerminalOutputUpdate +=
                                                                                   Raise.Event<Action<string>>("content2");
                                                                           });
            ExecTaskGenerator.GimeTasks(Arg.Any<FakeTaskDescription>()).Returns(As.List(Task1));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_generate_event_with_content_contentIndex_and_sequenceIndex1()
        {
            PipelineOnTerminalUpdate.Received().Invoke(Arg.Is<TerminalUpdateInfo>(info => info.Content == "content1" && info.ContentSequenceIndex == 0 && info.TaskIndex == 0));
        }

        [Test]
        public void should_generate_event_with_content_contentIndex_and_sequenceIndex2()
        {
            PipelineOnTerminalUpdate.Received().Invoke(Arg.Is<TerminalUpdateInfo>(info => info.Content == "content2" && info.ContentSequenceIndex == 1 && info.TaskIndex == 0));
        }
    }
}