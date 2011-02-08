using System;
using System.Threading;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.UI.Web;
using NSubstitute;
using NUnit.Framework;

namespace Frog.System.Specs
{
    [TestFixture]
    public class TaskFulfilment : BDD
    {
        Valve valve;
        PipelineOfTasks pipeline;
        SystemDriver system;

        public override void Given()
        {
            system = SystemDriver.GetCleanSystem();

            var task1 = Substitute.For<ExecTask>(null, null);
            var task2 = Substitute.For<ExecTask>(null, null);
            pipeline = new PipelineOfTasks(system.Bus,
                                           new FixedTasksDispenser(task1, task2));
            valve = new Valve(system.Git, pipeline, system.WorkingArea);

            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Failure, 23));
        }

        public override void When()
        {
            ThreadPool.QueueUserWorkItem(state => valve.Check());
        }

        [Test]
        public void should_have_build_result_in_build_complete_event()
        {
            var prober = new EventedProber(3000, system.Bus);   
            Assert.True(prober.check<TaskStarted>());
            makeSureAllProcessesAreDone(prober);
        }

        void makeSureAllProcessesAreDone(EventedProber prober)
        {
            // we do this to make sure the processes that operate on the system have exited so that the cleanup can proceed safely
            Assert.True(prober.check<BuildEnded>());
        }

        public override void Cleanup()
        {
            system.ResetSystem();
        }
    }
}