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

        [Test]
        public void should_insert_start_pipeline_in_front_of_each_pipeline()
        {
            var taskFileFinder = NSubstitute.Substitute.For<TaskFileFinder>();
            taskFileFinder.FindFiles(Arg.Any<string>()).Returns(As.List("runz.me"));
            var customTasks = new CustomTasksDetector(taskFileFinder, s => @"{ start_each_stage : [""a""], pipeline: [{ 
    stage : ""Build"",
   tasks : [""task 1""]
}, { 
    stage : ""Build 2"",
   tasks : [""task 2""]
}]}");
            bool stop;
            var tasks = customTasks.Detect("", shouldStop: out stop).ToList();
            Assert.That(tasks[0].Name, Is.EqualTo("a"));
            Assert.That(tasks[1].Name, Is.EqualTo("task 1"));
            Assert.That(tasks[2].Name, Is.EqualTo("a"));
            Assert.That(tasks[3].Name, Is.EqualTo("task 2"));
        }
    }
}
