using System;
using System.IO;
using Frog.Domain.Integration;
using Frog.Domain.Integration.TaskSources.BuildSystems;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using Frog.Domain.Integration.TaskSources.BuildSystems.DotNet;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class NUnitFileFinderSpecs : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            _taskFileFinder = new NUnitTaskFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles("basefolder");
        }

        [Test]
        public void should_search_for_files_that_end_in_word_Test()
        {
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                                       Arg.Is<string>(
                                                           s =>
                                                           s.Equals("*.Test.csproj",
                                                                    StringComparison.InvariantCultureIgnoreCase)),
                                                       "basefolder");
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                                       Arg.Is<string>(
                                                           s =>
                                                           s.Equals("*.Tests.csproj",
                                                                    StringComparison.InvariantCultureIgnoreCase)),
                                                       "basefolder");
        }
    }

    [TestFixture]
    public class MSSolutionsFileFinderSpecs : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            _taskFileFinder = new SolutionTaskFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles("basefolder");
        }

        [Test]
        public void should_search_for_all_files_with_extension_SLN()
        {
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                                       Arg.Is<string>(
                                                           s =>
                                                           s.Equals("*.sln", StringComparison.InvariantCultureIgnoreCase)),
                                                       "basefolder");
        }
    }

    [TestFixture]
    public class CustomFileFindSpec : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            _taskFileFinder = new CustomFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles("basefolder");
        }

        [Test]
        public void should_search_for_file_RUNZ_DOT_ME_at_the_root_of_the_tree()
        {
            pathFinder.Received().FindFilesAtTheBase(Arg.Any<Action<string>>(),
                                                     Arg.Is<string>(
                                                         s =>
                                                         s.Equals("runz.me",
                                                                  StringComparison.InvariantCultureIgnoreCase)),
                                                     "basefolder");
        }
    }

}