using System;
using Frog.Domain.CustomTasks;
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
            SrcTask1 = new MSBuildTask("");
            TaskSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(SrcTask1));
            Task2 = Substitute.For<IExecTask>();
            Task2.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(info => { throw new Exception("ufff"); });
            ExecTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(As.List(Task2));
        }

        protected override void When()
        {
            Pipeline.Process(new SourceDrop(""));
        }

        [Test]
        public void should_announce_the_task_with_error_status()
        {
            PipelineOnBuildUpdated.Received().Invoke(0, TaskInfo.TaskStatus.FinishedError);
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