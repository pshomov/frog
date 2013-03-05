using System.Collections.Generic;
using System.Linq;
using Frog.Domain.BuildSystems.Solution;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class NUnitTaskDetectorAllAssembliesSpecs : BDD
    {
        TaskFileFinder projectTaskFileRepo;
        NUnitTaskDetector nunitTaskDetecttor;
        IEnumerable<Task> items;

        protected override void Given()
        {
            projectTaskFileRepo = Substitute.For<TaskFileFinder>();
            nunitTaskDetecttor = new NUnitTaskDetector(projectTaskFileRepo);
            projectTaskFileRepo.FindFiles("basefolder").Returns(As.List(Os.DirChars("l1\\l2\\l3\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l4\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l3\\flo.tests.csproj")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = nunitTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_have_as_many_nunt_tasks_as_detected_non_conflicting_assemblies()
        {
            Assert.That(items.Count(), Is.EqualTo(3));
        }

        [Test]
        public void should_have_nunit_launched_with_parameter_the_test_assembly()
        {
            Assert.That(((ShellTask) items.First()).Arguments, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\fle.test.dll")));
            Assert.That(((ShellTask) items.ElementAt(1)).Arguments, Is.EqualTo(Os.DirChars("l1\\l2\\l4\\bin\\Debug\\fle.test.dll")));
            Assert.That(((ShellTask) items.ElementAt(2)).Arguments, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\flo.tests.dll")));
        }

        [Test]
        public void should_have_nunit_as_command()
        {
            Assert.That(((ShellTask) items.First()).Command, Is.EqualTo("nunit"));
            Assert.That(((ShellTask) items.ElementAt(1)).Command, Is.EqualTo("nunit"));
            Assert.That(((ShellTask) items.ElementAt(2)).Command, Is.EqualTo("nunit"));
        }
    }
}