using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskCurrentDirectory : BDD
    {
        ExecTask _task;
        ExecTaskResult taskResult;
        private string _arguments;

        public override void Given()
        {
            string _app = "adasdasd";
            if (Underware.IsWindows) {_app = @"cmd.exe"; _arguments=@"/c if %CD%==c:\temp exit /b 41";}
            if (Underware.IsUnix) _app = "./currentdir.sh";
            _task = new ExecTask(_app, _arguments);
        }

        public override void When()
        {
            taskResult = _task.Perform(new SourceDrop(@"c:\temp"));
        }

        [Test]
        public void task_should_have_current_directory_set_to_the_working_area()
        {
            Assert.That(taskResult.ExitCode == 41);
        }

    }
}