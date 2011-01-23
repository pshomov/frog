using System;
using System.Collections.Generic;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskFactorySpecs : BDD
    {
        TaskFactory taskFactoey;
        IList<ExecTask> tasks;
        TaskDetector taskDetector1;

        public override void Given()
        {
            taskDetector1 = Substitute.For<TaskDetector>();
            taskDetector1.Detect().Returns(Underware.As.ListOf(new ExecTask("", "")));
            taskFactoey = new TaskFactory(taskDetector1);
        }

        public override void When()
        {
            tasks = taskFactoey.Generate();
        }

        [Test]
        public void should_attempt_task_generators_in_order_of_priority()
        {
            Assert.That(tasks.Count, Is.EqualTo(1));
        }
    }
}