using System.Collections.Generic;
using Frog.Domain.BuildSystems.Make;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class MakeTaskDetectorSpecs : BDD
    {
        private TaskFileFinder projectTaskFileRepo;
        private MakeTaskDetector makeTaskDetector;
        private IList<Domain.Task> items;

        protected override void Given()
        {
            projectTaskFileRepo = Substitute.For<TaskFileFinder>();
            makeTaskDetector = new MakeTaskDetector(projectTaskFileRepo);
            projectTaskFileRepo.FindFiles("basefolder").Returns(As.List(Os.DirChars("makefile")));
        }

        protected override void When()
        {
            items = makeTaskDetector.Detect("basefolder");
        }

        [Test]
        public void should_have_only_one_task()
        {
            Assert.That(items.Count, Is.EqualTo(1));
        }

        [Test]
        public void should_be_a_make_task()
        {
            Assert.That((items[0] as MakeTask), Is.Not.Null);
        }
    }
}