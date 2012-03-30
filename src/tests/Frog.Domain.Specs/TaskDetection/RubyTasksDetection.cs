using System.Collections.Generic;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    public class RakeTaskDetectorAlternativeFilenameSpecs : TaskDetectorSpecsBase
    {
        protected TaskFileFinder bundlerFileFinder;
        protected RubyTaskDetector taskDetector;
        protected IList<Domain.Task> tasks;

        protected override void Given()
        {
            taskFileFinder.FindFiles("base").Returns(As.List("Rakefile.rb"));
            bundlerFileFinder = Substitute.For<TaskFileFinder>();
            bundlerFileFinder.FindFiles("base").Returns(Empty.ListOf("String"));
            taskDetector = new RubyTaskDetector(taskFileFinder, bundlerFileFinder);
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
    public abstract class RakeTaskDetectorSpecsBase : TaskDetectorSpecsBase
    {
        protected TaskFileFinder bundlerFileFinder;
        protected RubyTaskDetector taskDetector;
        protected IList<Domain.Task> tasks;

        protected override void Given()
        {
            taskFileFinder.FindFiles("base").Returns(As.List("Rakefile"));
            bundlerFileFinder = Substitute.For<TaskFileFinder>();
            bundlerFileFinder.FindFiles("base").Returns(Empty.ListOf("String"));
            taskDetector = new RubyTaskDetector(taskFileFinder, bundlerFileFinder);
        }

        protected override void When()
        {
            tasks = taskDetector.Detect("base");
        }
    }

    
    [TestFixture]
    public class AloneRakeTaskDetection : RakeTaskDetectorSpecsBase
    {

        [Test]
        public void should_generate_a_rake_task()
        {
            Assert.That(tasks.Count, Is.EqualTo(1));
            Assert.That(tasks[0].GetType(), Is.EqualTo(typeof (RakeTask)));
        }
    }


    [TestFixture]
    public class RakeAndBundlerComboTasksDetection : RakeTaskDetectorSpecsBase
    { 
        protected override void Given()
        {
            taskFileFinder.FindFiles("base").Returns(As.List("Rakefile"));
            bundlerFileFinder = Substitute.For<TaskFileFinder>();
            bundlerFileFinder.FindFiles("base").Returns(As.List("Gemfile"));
            taskDetector = new RubyTaskDetector(taskFileFinder, bundlerFileFinder);
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
    public class AloneBundlerTaskGetsIgnored : RakeTaskDetectorSpecsBase
    {
        protected override void Given()
        {
            taskFileFinder.FindFiles("base").Returns(new List<string>());
            taskDetector = new RubyTaskDetector(taskFileFinder, taskFileFinder);
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
