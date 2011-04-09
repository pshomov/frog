using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class AloneRakeTaskDetection : TaskDetectorSpecsBase
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
            Assert.That(tasks[0].GetType(), Is.EqualTo(typeof (RakeTask)));
        }
    }

    [TestFixture]
    public class RakeAndBundlerComboTasksDetection : TaskDetectorSpecsBase
    {
        RakeTaskDetector taskDetector;
        IList<ITask> tasks;

        protected override void Given()
        {
            ProjectFileRepo.FindRakeFile("base").Returns(As.List("Rakefile"));
            ProjectFileRepo.FindBundlerFile("base").Returns(true);
            taskDetector = new RakeTaskDetector(ProjectFileRepo);
        }

        protected override void When()
        {
            tasks = taskDetector.Detect("base");
        }

        [Test]
        public void should_generate_two_tasks()
        {
            Assert.That(tasks.Count, Is.EqualTo(2));
        }

        [Test]
        public void should_have_first_the_bundler_task()
        {
            Assert.That(tasks[0].GetType(), Is.EqualTo(typeof (BundlerTask)));
        }

        [Test]
        public void should_have_second_the_rake_task()
        {
            Assert.That(tasks[1].GetType(), Is.EqualTo(typeof (RakeTask)));
        }
    }

    [TestFixture]
    public class AloneBundlerTaskGetsIgnored : TaskDetectorSpecsBase
    {
        RakeTaskDetector taskDetector;
        IList<ITask> tasks;

        protected override void Given()
        {
            ProjectFileRepo.FindRakeFile("base").Returns(new List<string>());
            ProjectFileRepo.FindBundlerFile("base").Returns(true);
            taskDetector = new RakeTaskDetector(ProjectFileRepo);
        }

        protected override void When()
        {
            tasks = taskDetector.Detect("base");
        }

        [Test]
        public void should_generate_two_tasks()
        {
            Assert.That(tasks.Count, Is.EqualTo(0));
        }
    }
}