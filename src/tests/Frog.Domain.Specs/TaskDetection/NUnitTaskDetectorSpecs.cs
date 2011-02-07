using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class NUnitTaskDetectorAllAssembliesSpecs : BDD
    {
        FileFinder projectFileRepo;
        NUnitTaskDetctor nunitTaskDetecttor;
        IList<NUnitTask> items;

        public override void Given()
        {
            projectFileRepo = Substitute.For<FileFinder>();
            nunitTaskDetecttor = new NUnitTaskDetctor(projectFileRepo);
            projectFileRepo.FindAllNUnitAssemblies().Returns(As.List(Os.DirChars("l1\\l2\\l3\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l4\\fle.test.csproj"),
                                                                     Os.DirChars("l1\\l2\\l3\\flo.test.csproj")));
        }

        public override void When()
        {
            items = nunitTaskDetecttor.Detect();
        }

        [Test]
        public void should_have_as_many_nunt_tasks_as_detected_non_conflicting_assemblies()
        {
            Assert.That(items.Count, Is.EqualTo(3));
        }

        [Test]
        public void should_have_project_path_as_arg0_of_the_task()
        {
            Assert.That(items[0].Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\fle.test.dll")));
            Assert.That(items[1].Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l4\\bin\\Debug\\fle.test.dll")));
            Assert.That(items[2].Assembly, Is.EqualTo(Os.DirChars("l1\\l2\\l3\\bin\\Debug\\flo.test.dll")));
        }
    }
}