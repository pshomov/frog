using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class MSBuildTaskDetectorSpecs : MSBuildTaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<ITask> items;

        public override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(projectFileRepo);
            projectFileRepo.FindAllSolutionFiles().Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a1\\a2.sln"),
                                                                             Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        public override void When()
        {
            items = msbuildTaskDetecttor.Detect();
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That((items[0] as MSBuildTaskDescriptions).solutionFile, Is.EqualTo("a1.sln"));
        }
    }

    [TestFixture]
    public class MSBuildTaskDetectorMultipleSolutionsAtTheRootSpecs : MSBuildTaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<ITask> items;

        public override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(projectFileRepo);
            projectFileRepo.FindAllSolutionFiles().Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        public override void When()
        {
            items = msbuildTaskDetecttor.Detect();
        }

        [Test]
        public void should_not_build_anything_if_it_looks_confusing()
        {
            Assert.That(items.Count, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class MSBuildTaskDetectorMultipleSolutionsAtTheRootWithBuildOneSpecs : MSBuildTaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<ITask> items;

        public override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(projectFileRepo);
            projectFileRepo.FindAllSolutionFiles().Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars("Build.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        public override void When()
        {
            items = msbuildTaskDetecttor.Detect();
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That((items[0] as MSBuildTaskDescriptions).solutionFile, Is.EqualTo("Build.sln"));
        }
    }

    [TestFixture]
    public class NoSolutionsAtAllSpecs : MSBuildTaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<ITask> items;

        public override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(projectFileRepo);
            projectFileRepo.FindAllSolutionFiles().Returns(new List<string>());
        }

        public override void When()
        {
            items = msbuildTaskDetecttor.Detect();
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count, Is.EqualTo(0));
        }
    }
}