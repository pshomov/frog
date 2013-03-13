using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Task
{
    public class TasksGetKilledIfTheyHaveNoActivity : BDD
    {
        private OSExecuatableTask task;
        private IProcessWrapper processWrapper;

        protected override void Given()
        {
            processWrapper = Substitute.For<IProcessWrapper>();
            processWrapper.ProcessTreeCPUUsageId.Returns("");
            task = new OSExecuatableTask("fle", "flo", "name",
                                (s, s1, arg3) => processWrapper, periodLengthMs: 1000, quotaNrPeriods: 10);
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_kill_task()
        {
            processWrapper.Received().Dispose();
        }

        [Test]
        public void should_not_execute_all_quota_periods()
        {
            processWrapper.Received(1).WaitForProcess(Arg.Any<int>());
        }
    }
}