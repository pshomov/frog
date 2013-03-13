using System.Collections.Generic;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection.CompoundTask
{
    [TestFixture]
    public class CompoundTask_CollectsTasksUntilSourceSaysStop : BDD
    {
        CompoundTaskSource compoundTaskSource;
        TaskSource task1;
        TaskSource task2;

        protected override void Given()
        {
            task1 = Substitute.For<TaskSource>();
            bool stop;
            task1.Detect(Arg.Any<string>(), out stop).Returns(info =>
                {
                    info[1] = true;
                    return new List<TaskDescription>();
                });
            task2 = Substitute.For<TaskSource>();
            compoundTaskSource = new CompoundTaskSource(task1, task2);
        }

        protected override void When()
        {
            bool shouldStop;
            compoundTaskSource.Detect("", out shouldStop);
        }

        [Test]
        public void should_have_detected_task_from_both_sources()
        {
            bool shouldStop = true;
            task1.Received().Detect("", out shouldStop);
            task2.DidNotReceive().Detect("", out shouldStop);
        }
    }
}