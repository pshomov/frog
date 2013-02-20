using System.Collections.Generic;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    [TestFixture]
    public class TaskDetectorSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a1\\a2.sln"),
                                                                             Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        protected override void When()
        {
            items = msbuildTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That((items[0] as MSBuildTask).SolutionFile, Is.EqualTo("a1.sln"));
        }
    }

    [TestFixture]
    public class TaskDetectorMultipleSolutionsAtTheRootSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        protected override void When()
        {
            items = msbuildTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_not_build_anything_if_it_looks_confusing()
        {
            Assert.That(items.Count, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class TaskDetectorMultipleSolutionsAtTheRootWithBuildOneSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars("Build.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        protected override void When()
        {
            items = msbuildTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That((items[0] as MSBuildTask).SolutionFile, Is.EqualTo("Build.sln"));
        }
    }

    [TestFixture]
    public class NoSolutionsAtAllSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(new List<string>());
        }

        protected override void When()
        {
            items = msbuildTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class OneSolutionAnywhereGetsSelectedForBuildSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetector;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetector = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("fle\\flo\\a.sln")));
        }

        protected override void When()
        {
            items = msbuildTaskDetector.Detect("basefolder");
        }

        [Test]
        public void should_always_select_the_only_solution_file()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That((items[0] as MSBuildTask).SolutionFile, Is.EqualTo(Os.DirChars("fle\\flo\\a.sln")));
        }
    }
	
    [TestFixture]
    public class MultipleSplutionsButNoneAtTheRootSpecs : TaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<Domain.Task> items;

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1\\a2.sln"),
                                                                    Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        protected override void When()
        {
            items = msbuildTaskDetecttor.Detect("basefolder");
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count, Is.EqualTo(0));
        }
    }
	
}