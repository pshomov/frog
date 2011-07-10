﻿using System;
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
            processWrapper.TotalProcessorTime.Returns(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(2.0), TimeSpan.FromMinutes(3.0));
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
            processWrapper.Received().WaitForProcess(Arg<int>.Any());
            processWrapper.Received().Kill();
        }

        [Test]
        public void should_wait_for_process_some_time()
        {
            processWrapper.Received().WaitForProcess(1000);
        }
    }
}