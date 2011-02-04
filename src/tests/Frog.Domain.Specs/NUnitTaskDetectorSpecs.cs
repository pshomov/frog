using System;
using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class NUnitTaskDetectorSpecs : BDD
    {
        FileFinder projectFileRepo;
        NUnitTaskDetctor nunitTaskDetecttor;
        IList<NUnitTask> items;

        public override void Given()
        {
            projectFileRepo = Substitute.For<FileFinder>();
            nunitTaskDetecttor = new NUnitTaskDetctor(projectFileRepo);
            projectFileRepo.FindAllNUnitAssemblies().Returns(Underware.As.ListOf("a1"));
        }

        public override void When()
        {
            items = nunitTaskDetecttor.Detect();
        }

        [Test]
        public void should_look_for_all_nunit_assemblies()
        {
            projectFileRepo.Received().FindAllNUnitAssemblies();
        }

        [Test]
        public void should_have_as_many_nunt_tasks_as_detected_assemblies()
        {
            Assert.That(items.Count, Is.EqualTo(1));
        }

        [Test]
        public void should_have_assembly_path_as_arg0_of_the_task()
        {
            Assert.That(items[0].Assembly, Is.EqualTo("a1"));
        }

    }
}