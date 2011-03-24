using Frog.Specs.Support;
using NUnit.Framework;
using Frog.Support;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    public class TaskCurrentDirectory : BDD
    {
        ExecTask _task;
        ExecTaskResult taskResult;
        private string _arguments;

        protected override void Given()
        {
            string _app = "adasdasd";
            if (Os.IsWindows) {_app = @"cmd.exe"; _arguments=@"/c if %CD%==c:\temp exit /b 41";}
            if (Os.IsUnix) {_app = "/bin/bash"; _arguments = @"-c ""test `pwd` == '/usr/bin' && (echo 'matches'; exit 41)""";}
            _task = new ExecTask(_app, _arguments, "task_name");
        }

        protected override void When()
        {
            if (Os.IsWindows) taskResult = _task.Perform(new SourceDrop(@"c:\temp"));
            if (Os.IsUnix) taskResult = _task.Perform(new SourceDrop(@"/usr/bin"));
        }

        [Test]
        public void task_should_have_current_directory_set_to_the_working_area()
        {
            Assert.That(taskResult.ExitCode == 41);
        }

    }
}