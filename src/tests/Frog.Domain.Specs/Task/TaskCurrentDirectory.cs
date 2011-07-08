using System;
using System.IO;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using NUnit.Framework;
using Frog.Support;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    public class TaskCurrentDirectory : BDD
    {
        IExecTask _task;
        ExecTaskResult taskResult;
        private DirectoryInfo parent;

        protected override void Given()
        {
            parent = Directory.GetParent(Directory.GetCurrentDirectory());
            var _app = "ruby"; 
            var arguments=string.Format(@"-e 'puts Dir.pwd; if (Dir.pwd.tr(""/"", ""\\"") == ""{0}"") then puts ""got it""; exit 41; end '", parent.FullName.Replace("\\", "\\\\"));
            _task = new ExecTask(_app, arguments, "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        }

        protected override void When()
        {
            taskResult = _task.Perform(new SourceDrop(parent.FullName));
        }

        [Test]
        public void task_should_have_current_directory_set_to_the_working_area()
        {
            Assert.That(taskResult.ExitCode == 41);
        }

    }
}