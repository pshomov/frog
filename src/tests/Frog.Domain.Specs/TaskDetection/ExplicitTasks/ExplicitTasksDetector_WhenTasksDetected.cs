using System.Collections.Generic;
using System.Linq;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection.ExplicitTasks
{
    [TestFixture]
    public class ExplicitTasksDetector_WhenTasksDetected : TaskDetectorSpecsBase
    {
        TaskSource customTasks;
        IEnumerable<TaskDescription> tasks;
        bool shouldStop;

        protected override void Given()
        {
            customTasks = new CustomTasksDetector(taskFileFinder, s => @"{ pipeline: [{ 
    stage : ""Build"",
   tasks : [""task 1"", ""task 2""]
},
{
    stage : ""Deploy to staging"",
    tasks : [""task 3"", ""task 4""]
}]}
");
            taskFileFinder.FindFiles("basefolder").Returns(As.List("runz.me"));
        }

        protected override void When()
        {
            tasks = customTasks.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_detect_4_tasks()
        {
            Assert.That(tasks.Count(), Is.EqualTo(4));            
        }

        [Test]
        public void should_detect_tasks_in_correct_sequence_tasks()
        {
            Assert.That(((ShellTaskDescription)tasks.First()).Arguments, Is.EqualTo("task 1"));            
            Assert.That(((ShellTaskDescription)tasks.ElementAt(1)).Arguments, Is.EqualTo("task 2"));            
            Assert.That(((ShellTaskDescription)tasks.ElementAt(2)).Arguments, Is.EqualTo("task 3"));            
            Assert.That(((ShellTaskDescription)tasks.ElementAt(3)).Arguments, Is.EqualTo("task 4"));            
        }

        [Test]
        public void should_stop_further_task_detection()
        {
            Assert.That(shouldStop, Is.True);            
        }
    }
}
