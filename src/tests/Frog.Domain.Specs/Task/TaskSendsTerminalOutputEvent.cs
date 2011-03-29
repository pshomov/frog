using System;
using System.Threading;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskSendsTerminalOutputEventWhenStdOutputChange : BDD
    {
        string stdOutput = "";
        ExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDOUT.sync = true; puts \"output\";'", "name");
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
                () => Assert.That(stdOutput, Is.EqualTo(string.Format("S>output{0}", Environment.NewLine))));
        }
    }

    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskSendsTerminalOutputEventWhenErrOutputChange : BDD
    {
        string errOutput = "";
        ExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name");
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
                () => Assert.That(errOutput, Is.EqualTo(string.Format("E>error output{0}", Environment.NewLine))));
        }
    }

    [Ignore]
    [TestFixture]
    [Category(TestTypes.Integration)]
    public class TaskTerminalOutputDoesNotNeedSubscribersToWork : BDD
    {
        ExecTask task;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e ' STDERR.sync = true; $stderr.puts \"error output\";'", "name");
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_perform_the_behaviour_under_test_without_exception()
        {
            Thread.Sleep(500);
            // The exception right now happens on a different thread and that's why it does not trip this test but the goal is to have it do that
        }
    }
}