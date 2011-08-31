using System;
using System.Linq;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using NSubstitute;

namespace Frog.Domain.Specs.Task
{
    public class TasksHaveQuotaOnRunTime : BDD
    {
        private ExecTask task;
        private IProcessWrapper processWrapper;

        protected override void Given()
        {
            processWrapper = Substitute.For<IProcessWrapper>();
            processWrapper.ProcessTreeCPUUsageId.Returns("1weqwesafsd", "2fasdfasdf", "asfdasdfasdfasdfqw");
            task = new ExecTask("fle", "flo", "name",
                                (s, s1, arg3) => processWrapper, periodLengthMs: 1000, quotaNrPeriods: 3);
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_kill_process_after_timeout()
        {
            Assert.That(
                processWrapper.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "WaitForProcess" && call.GetArguments().Count() == 1).Count(),
                Is.EqualTo(3));
            processWrapper.Received().Dispose();
        }

        [Test]
        public void should_wait_for_process_some_time()
        {
            processWrapper.Received().WaitForProcess(1000);
        }
    }
}