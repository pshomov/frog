using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskReporting : BDD
    {
        ExecTask task;
        TaskReporter reporter;

        public override void Given()
        {
            reporter = Substitute.For<TaskReporter>();
            task = new ExecTask("ruby", "-e 'exit 0'", reporter);
        }

        public override void When()
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