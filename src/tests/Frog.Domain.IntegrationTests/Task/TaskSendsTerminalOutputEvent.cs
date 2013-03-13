using System.Threading;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Task
{
    [TestFixture]
    public class TaskSendsTerminalOutputEventWhenStdOutputChange : BDD
    {
        string stdOutput = "";
        ExecTask task;

        protected override void Given()
        {
            task = new OSExecuatableTask("ruby", "-e ' STDOUT.sync = true; puts \"output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            task.OnTerminalOutputUpdate += s => stdOutput += s;
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_receive_the_terminal_output()
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(stdOutput, Is.StringContaining("S>output")));
        }
    }

    [TestFixture]
    public class TaskSendsTerminalOutputEventWhenErrOutputChange : BDD
    {
        string errOutput = "";
        ExecTask task;

        protected override void Given()
        {
            task = new OSExecuatableTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            task.OnTerminalOutputUpdate += s => errOutput += s;
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_receive_the_terminal_output()
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(errOutput, Is.StringContaining("E>error output")));
        }
    }

    [TestFixture]
    public class TaskSendErrorCodeToErrorOutputEventWhenProgramNotFound : BDD
    {
        string errOutput = "";
        ExecTask task;

        protected override void Given()
        {
            task = new OSExecuatableTask("flipflof", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            task.OnTerminalOutputUpdate += s => errOutput += s;
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_receive_error_message_on_terminal_output()
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(errOutput, Is.StringContaining(string.Format("E> Task has exited with error"))));
        }
    }

    [TestFixture]
    public class TaskTerminalOutputDoesNotNeedSubscribersToWork : BDD
    {
        ExecTask task;

        protected override void Given()
        {
            task = new OSExecuatableTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_perform_the_behaviour_under_test_without_exception()
        {
            Thread.Sleep(500);
            // Giving it some time so may be the exception will happen on some other thread
        }
    }
}