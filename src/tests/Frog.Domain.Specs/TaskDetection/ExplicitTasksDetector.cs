using System.Collections.Generic;
using System.IO;
using System.Text;
using Frog.Domain.BuildSystems.Custom;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class ExplicitTasksDetector : TaskDetectorSpecsBase
    {
        TaskSource customTasks;
        IList<Task> tasks;

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
            bool shouldStop;
            tasks = customTasks.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_detect_4_tasks()
        {
            Assert.That(tasks.Count, Is.EqualTo(4));            
        }

        [Test]
        public void should_detect_tasks_in_correct_sequence_tasks()
        {
            Assert.That(((ShellTask)tasks[0]).args, Is.EqualTo("task 1"));            
            Assert.That(((ShellTask)tasks[1]).args, Is.EqualTo("task 2"));            
            Assert.That(((ShellTask)tasks[2]).args, Is.EqualTo("task 3"));            
            Assert.That(((ShellTask)tasks[3]).args, Is.EqualTo("task 4"));            
        }
    }
}
