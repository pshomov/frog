using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.ExecTasks;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frog.Domain.Specs.Task
{
    class ObservesProcessForHangingBehaviour : BDD
    {
        private ExecTask task;
        private IProcessWrapper processWrapper;

        protected override void Given()
        {
            processWrapper = MockRepository.GenerateMock<IProcessWrapper>();
            processWrapper.Expect(wrapper => wrapper.TotalProcessorTime).Return(TimeSpan.FromMinutes(1.0));
            processWrapper.Expect(wrapper => wrapper.TotalProcessorTime).Return(TimeSpan.FromMinutes(2.0));
            processWrapper.Expect(wrapper => wrapper.TotalProcessorTime).Return(TimeSpan.FromMinutes(3.0));
            task = new ExecTask("fle", "flo", "name",
                                (s, s1, arg3) => processWrapper, period:1000);
        }

        protected override void When()
        {
            task.Perform(new SourceDrop(""));
        }

        [Test]
        public void should_wait_for_process_some_time()
        {
            processWrapper.AssertWasCalled(wrapper => wrapper.WaitForProcess(1000));
        }

        [Test]
        public void should_kill_process_after_timeout()
        {
            processWrapper.AssertWasCalled(wrapper => wrapper.WaitForProcess(Arg<int>.Is.Anything), options => options.Repeat.Times(3));
            processWrapper.AssertWasCalled(wrapper => wrapper.Kill());
        }

    }
}
