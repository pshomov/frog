using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Task
{
    [TestFixture]
    public class TaskReporting : BDD
    {
        ExecTask task;
        int taskStarted;
        int pid;

        protected override void Given()
        {
            task = new ExecTask("ruby", "-e 'exit 0'", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            taskStarted = 0;
            task.OnTaskStarted += pid =>
                                      {
                                          taskStarted++;
                                          this.pid = pid;
                                      }; 
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_report_process_id_right_away()
        {
            Assert.That(pid, Is.GreaterThan(0));
        }

        [Test]
        public void should_trigger_event_only_once()
        {
            Assert.That(taskStarted, Is.EqualTo(1));
        }

    }

}