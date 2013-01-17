using System;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Task
{
    [TestFixture]
    public class TaskFailsToStartSpec : BDD
    {
        private IExecTask _task;
        private ExecTaskResult _taskResult;

        protected override void Given()
        {
            _task = new ExecTask("ad43wsWasdasd", "", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            _taskResult = _task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_report_task_execution_status()
        {
            Assert.That(!_taskResult.HasExecuted);
        }

        [Test]
        public void should_throw_an_exception_when_trying_to_access_exitcode_value()
        {
            try
            {
                var a = _taskResult.ExitCode;
                Assert.Fail("should have thrown an exception");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Test]
        public void should_have_status_set_to_falure()
        {
            Assert.That(_taskResult.ExecStatus, Is.EqualTo(ExecTaskResult.Status.Error));
        }
    }

    [TestFixture]
    public class TaskStartsButExitsWithNonZeroSpec : BDD
    {
        private IExecTask _task;
        private ExecTaskResult _taskResult;

        protected override void Given()
        {
            _task = new ExecTask("ruby", @"-e 'exit 4'", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            _taskResult = _task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_report_task_execution_status()
        {
            Assert.That(_taskResult.HasExecuted);
        }

        [Test]
        public void should_match_exit_code_value_from_program()
        {
            Assert.That(_taskResult.ExitCode, Is.EqualTo(4));
        }

        [Test]
        public void should_have_status_set_to_error()
        {
            Assert.That(_taskResult.ExecStatus, Is.EqualTo(ExecTaskResult.Status.Error));
        }
    }

    [TestFixture]
    public class TaskStartsAndFinishesWithExitCodeZeroSpec : BDD
    {
        private IExecTask _task;
        private ExecTaskResult _taskResult;

        protected override void Given()
        {
            _task = new ExecTask("ruby", @"-e 'exit 0'", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            _taskResult = _task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_report_task_execution_status()
        {
            Assert.That(_taskResult.HasExecuted);
        }

        [Test]
        public void should_match_exit_code_value_from_program()
        {
            Assert.That(_taskResult.ExitCode, Is.EqualTo(0));
        }

        [Test]
        public void should_have_status_set_to_success()
        {
            Assert.That(_taskResult.ExecStatus, Is.EqualTo(ExecTaskResult.Status.Success));
        }
    }
}