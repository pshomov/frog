using System;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Task
{
    public class TasksCPUQuotaMeasurementOfExitedProcessDoesNotCauseException : BDD
    {
        private ExecTask task;
        private IProcessWrapper processWrapper;

        protected override void Given()
        {
            processWrapper = Substitute.For<IProcessWrapper>();
            processWrapper.ProcessTreeCPUUsageId.Returns(info => {throw new InvalidOperationException("process is gone");});
            task = new ExecTask("fle", "flo", "name",
                                (s, s1, arg3) => processWrapper, periodLengthMs: 1000, quotaNrPeriods: 3);
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_exec_task_without_exception()
        {
        }
    }
}