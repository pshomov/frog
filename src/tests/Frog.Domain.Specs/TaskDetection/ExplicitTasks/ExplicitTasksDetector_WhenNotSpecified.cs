using System.Collections.Generic;
using System.Linq;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection.ExplicitTasks
{
    [TestFixture]
    public class ExplicitTasksDetector_WhenNotSpecified : TaskDetectorSpecsBase
    {
        TaskSource customTasks;
        IEnumerable<TaskDescription> tasks;
        bool shouldStop;

        protected override void Given()
        {
            customTasks = new CustomTasksDetector(taskFileFinder, null);
            taskFileFinder.FindFiles("basefolder").Returns(new List<string>());
        }

        protected override void When()
        {
            tasks = customTasks.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_detect_0_tasks()
        {
            Assert.That(tasks.Count(), Is.EqualTo(0));            
        }

        [Test]
        public void should_allow_further_task_detection()
        {
            Assert.That(shouldStop, Is.False);            
        }
    }
}
