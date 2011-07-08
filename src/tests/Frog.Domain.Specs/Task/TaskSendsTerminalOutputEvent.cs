using System;
using System.Threading;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskSendsTerminalOutputEventWhenStdOutputChange : BDD
    {
        string stdOutput = "";
        IExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDOUT.sync = true; puts \"output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
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
                () => Assert.That(stdOutput, Is.StringContaining(string.Format("S>output{0}", Environment.NewLine))));
        }
    }

    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskSendsTerminalOutputEventWhenErrOutputChange : BDD
    {
        string errOutput = "";
        IExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
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
                () => Assert.That(errOutput, Is.StringContaining(string.Format("E>error output{0}", Environment.NewLine))));
        }
    }

    [TestFixture]
    public class TaskSendErrorCodeToErrorOutputEventWhenProgramNotFound : BDD
    {
        string errOutput = "";
        IExecTask task;

        protected override void Given()
        {
            task = new ExecTask("flipflof", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
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
                () => Assert.That(errOutput, Is.StringContaining(string.Format("E>Process exited with error"))));
        }
    }

    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskTerminalOutputDoesNotNeedSubscribersToWork : BDD
    {
        IExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
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