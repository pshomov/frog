using System;
using Frog.Domain.BuildSystems.Solution;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Pipeline
{
    [TestFixture]
    public class PipelineHandlesExecTaskException : PipelineProcessSpecBase
    {
        protected override void Given()
        {
            base.Given();
            bool shouldStop;
            TaskSource.Detect(Arg.Any<string>(), out shouldStop).Returns(As.List<Domain.Task>(new FakeTaskDescription()));
            Task2 = Substitute.For<IExecTask>();
            Task2.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(info => { throw new Exception("ufff"); });
            ExecTaskGenerator.GimeTasks(Arg.Any<FakeTaskDescription>()).Returns(As.List(Task2));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_announce_the_task_with_error_status()
        {
            PipelineOnBuildUpdated.Received().Invoke(0, Arg.Any<Guid>(), TaskInfo.TaskStatus.FinishedError);
        }

        [Test]
        public void should_dump_as_terminal_output_exception_info()
        {
            PipelineOnTerminalUpdate.Received().Invoke(Arg.Is<TerminalUpdateInfo>(info => info.Content.Contains("ufff")));
        }

        [Test]
        public void should_unhook_terminal_event_handler()
        {
            int updates = 0;
            Pipeline.OnTerminalUpdate += info => updates++;
            Pipeline.Process(new SourceDrop(""));
            Assert.That(updates, Is.EqualTo(1));
        }
    }
}