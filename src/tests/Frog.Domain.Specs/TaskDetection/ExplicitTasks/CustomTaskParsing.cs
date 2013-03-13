using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.Integration.TaskSources.BuildSystems;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection.ExplicitTasks
{
    [TestFixture]
    public class CustomTaskParsing
    {
        [Test]
        public void should_parse_the_name_of_the_task_when_present()
        {
            var taskFileFinder = NSubstitute.Substitute.For<TaskFileFinder>();
            taskFileFinder.FindFiles(Arg.Any<string>()).Returns(As.List("runz.me"));
            var customTasks = new CustomTasksDetector(taskFileFinder, s => @"{ pipeline: [{ 
    stage : ""Build"",
   tasks : [""task 1"", ""task 2""]
}]}");
            bool stop;
            var tasks = customTasks.Detect("", shouldStop: out stop);
            Assert.That(tasks.First().Name, Is.EqualTo("task 1"));
        }
    }
}
