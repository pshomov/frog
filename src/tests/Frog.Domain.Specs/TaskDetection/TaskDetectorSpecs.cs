using System.Collections.Generic;
using System.Linq;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.TaskDetection
{
    public abstract class MsBuildSpecBase : TaskDetectorSpecsBase
    {
        protected MSBuildDetector msbuildTaskDetecttor;
        protected IEnumerable<TaskDescription> items;
    }

    [TestFixture]
    public class TaskDetectorSpecs : MsBuildSpecBase
    {
        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a1\\a2.sln"),
                                                                             Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_have_msbuild_as_command()
        {
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That((items.First() as ShellTaskDescription).Command.Contains("msbuild.exe"), Is.True);
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That((items.First() as ShellTaskDescription).Arguments.Contains("a1.sln"), Is.True);
        }
    }

    [TestFixture]
    public class TaskDetectorUnixSpecs : MsBuildSpecBase
    {
        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Linux);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a1\\a2.sln"),
                                                                             Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_have_msbuild_as_command()
        {
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That((items.First() as ShellTaskDescription).Command.Contains("xbuild"), Is.True);
        }
    }

    [TestFixture]
    public class TaskDetectorMultipleSolutionsAtTheRootSpecs : MsBuildSpecBase
    {
        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_not_build_anything_if_it_looks_confusing()
        {
            Assert.That(items.Count(), Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class TaskDetectorMultipleSolutionsAtTheRootWithBuildOneSpecs : MsBuildSpecBase
    {
        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1.sln"),
                                                                             Os.DirChars("a2.sln"),
                                                                             Os.DirChars("Build.sln"),
                                                                             Os.DirChars(
                                                                                 "a2\\asdas\\asd\\build.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That((items.First() as ShellTaskDescription).Arguments, Is.EqualTo("Build.sln"));
        }
    }

    [TestFixture]
    public class NoSolutionsAtAllSpecs : MsBuildSpecBase
    {

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(new List<string>());
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_always_prefer_the_solution_called_BUILD()
        {
            Assert.That(items.Count(), Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class OneSolutionAnywhereGetsSelectedForBuildSpecs : MsBuildSpecBase
    {

        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("fle\\flo\\a.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_always_select_the_only_solution_file()
        {
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That((items.First() as ShellTaskDescription).Arguments, Is.EqualTo(Os.DirChars("fle\\flo\\a.sln")));
        }
    }
	
    [TestFixture]
    public class MultipleSplutionsButNoneAtTheRootSpecs : MsBuildSpecBase
    {
        protected override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(taskFileFinder, OS.Windows);
            taskFileFinder.FindFiles("basefolder").Returns(As.List(Os.DirChars("a1\\a2.sln"),
                                                                    Os.DirChars("a2\\asdas\\asd\\b.sln")));
        }

        protected override void When()
        {
            bool shouldStop;
            items = msbuildTaskDetecttor.Detect("basefolder", out shouldStop);
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count(), Is.EqualTo(0));
        }
    }
	
}