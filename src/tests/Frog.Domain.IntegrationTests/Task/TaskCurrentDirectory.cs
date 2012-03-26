using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Task
{
    [TestFixture]
    public class TaskCurrentDirectory : BDD
    {
        IExecTask _task;
        ExecTaskResult taskResult;
        private string _arguments;

        protected override void Given()
        {
            string _app = "adasdasd";
            if (Os.IsWindows) {_app = @"cmd.exe"; _arguments=@"/c if %CD%==c:\ exit /b 41";}
            if (Os.IsUnix) {_app = "/bin/bash"; _arguments = @"-c ""test `pwd` == '/usr/bin' && (echo 'matches'; exit 41)""";}
            _task = new ExecTask(_app, _arguments, "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            if (Os.IsWindows) taskResult = _task.Perform(new SourceDrop(@"c:\"));
            if (Os.IsUnix) taskResult = _task.Perform(new SourceDrop(@"/usr/bin"));
        }

        [Test]
        public void task_should_have_current_directory_set_to_the_working_area()
        {
            Assert.That(taskResult.ExitCode == 41);
        }

    }
}