using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frog.Domain.IntegrationTests.Task
{
    public class TasksGetKilledIfTheyHaveNoActivity : BDD
    {
        private ExecTask task;
        private IProcessWrapper processWrapper;

        protected override void Given()
        {
            processWrapper = MockRepository.GenerateMock<IProcessWrapper>();
            processWrapper.Expect(wrapper => wrapper.ProcessTreeCPUUsageId).Return("");
            task = new ExecTask("fle", "flo", "name",
                                (s, s1, arg3) => processWrapper, periodLengthMs: 1000, quotaNrPeriods: 10);
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_kill_task()
        {
            processWrapper.AssertWasCalled(wrapper => wrapper.Dispose());
        }

        [Test]
        public void should_not_execute_all_quota_periods()
        {
            processWrapper.AssertWasCalled(wrapper => wrapper.WaitForProcess(Arg<int>.Is.Anything),
                                           options => options.Repeat.Times(1));
        }
    }
}