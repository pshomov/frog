using System.Collections.Generic;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class NUnitTaskDetectorAllAssembliesSpecs : BDD
    {
        TaskFileFinder _projectTaskFileRepo;
        NUnitTaskDetector nunitTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            _projectTaskFileRepo = Substitute.For<TaskFileFinder>();
            nunitTaskDetecttor = new NUnitTaskDetector(_projectTaskFileRepo);
            _projectTaskFileRepo.FindFiles("basefolder").Returns(As.List(Os.DirChars("l1\\l2\\l3\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l4\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l3\\flo.tests.csproj")));
        }

        protected override void When()
        {
            items = nunitTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_have_as_many_nunt_tasks_as_detected_non_conflicting_assemblies()
        {
            Assert.That(items.Count, Is.EqualTo(3));
        }

        [Test]
        public void should_have_project_path_as_arg0_of_the_task()
        {
            Assert.That((items[0] as NUnitTask).Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\fle.test.dll")));
            Assert.That((items[1] as NUnitTask).Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l4\\bin\\Debug\\fle.test.dll")));
            Assert.That((items[2] as NUnitTask).Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\flo.tests.dll")));
        }
    }
}