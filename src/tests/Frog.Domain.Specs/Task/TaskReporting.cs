using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    public class TaskReporting : BDD
    {
        ExecTask task;
        TaskReporter reporter;

        protected override void Given()
        {
            reporter = Substitute.For<TaskReporter>();
            task = new ExecTask("ruby", "-e 'exit 0'", reporter, "task_name");
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_report_process_id_right_away()
        {
            reporter.Received().TaskStarted(Arg.Is<int>(x => x > 0));
        }

    }

}