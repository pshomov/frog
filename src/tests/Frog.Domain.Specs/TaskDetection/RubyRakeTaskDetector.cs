using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class RubyRakeTaskDetector : MSBuildTaskDetectorSpecsBase
    {
        RakeTaskDetector taskDetector;
        IList<ITask> tasks;

        protected override void Given()
        {
            ProjectFileRepo.FindRakeFile("base").Returns(As.List("Rakefile"));
            taskDetector = new RakeTaskDetector(ProjectFileRepo);
        }

        protected override void When()
        {
            tasks = taskDetector.Detect("base");
        }

        [Test]
        public void should_generate_a_rake_task()
        {
            Assert.That(tasks.Count, Is.EqualTo(1));
            Assert.That(tasks[0].GetType(), Is.EqualTo(typeof(RakeTask)));
        }
    }
}