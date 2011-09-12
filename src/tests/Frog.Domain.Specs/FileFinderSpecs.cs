using System;
using System.IO;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
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
    public class RakeFileFinderSpecs : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            pathFinder.When(pf => pf.FindFilesAtTheBase(Arg.Any<Action<string>>(), "RAKEFILE", Arg.Any<string>())).Do(
                info =>
                    {
                        var cb = (Action<string>)info.Args()[0];
                        var base_folder = (string)info.Args()[2];
                        cb(Support.Os.DirChars(string.Format("{0}\\Rakefile", base_folder)));
                    });
            _taskFileFinder = new RakeTaskFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles(Directory.GetCurrentDirectory());
        }

        [Test]
        public void should_search_for_file_RAKEFILE_at_the_root_of_the_tree()
        {
            pathFinder.Received().FindFilesAtTheBase(Arg.Any<Action<string>>(),
                                                     Arg.Is<string>(
                                                         s =>
                                                         s.Equals("RAKEFILE",
                                                                  StringComparison.InvariantCultureIgnoreCase)),
                                                     Arg.Any<string>());
        }

        [Test]
        public void should_not_look_for_alternative_filename()
        {
            pathFinder.DidNotReceive().FindFilesAtTheBase(Arg.Any<Action<string>>(), Arg.Is<string>(
                                                         s =>
                                                         s.Equals("RAKEFILE.RB",
                                                                  StringComparison.InvariantCultureIgnoreCase)),Arg.Any<string>());
        }

    }

    [TestFixture]
    public class RakeFileFinderAlternativeFilenameSpecs : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            _taskFileFinder = new RakeTaskFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles("basefolder");
        }

        [Test]
        public void should_search_for_file_RAKEFILE_at_the_root_of_the_tree()
        {
            pathFinder.Received().FindFilesAtTheBase(Arg.Any<Action<string>>(),
                                                     Arg.Is<string>(
                                                         s =>
                                                         s.Equals("RAKEFILE.RB",
                                                                  StringComparison.InvariantCultureIgnoreCase)),
                                                     "basefolder");
        }
    }

    [TestFixture]
    public class BundlerFileFindSpec : BDD
    {
        private TaskFileFinder _taskFileFinder;
        private PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            _taskFileFinder = new BundlerFileFinder(pathFinder);
        }

        protected override void When()
        {
            _taskFileFinder.FindFiles("basefolder");
        }

        [Test]
        public void should_search_for_file_RAKEFILE_at_the_root_of_the_tree()
        {
            pathFinder.Received().FindFilesAtTheBase(Arg.Any<Action<string>>(),
                                                     Arg.Is<string>(
                                                         s =>
                                                         s.Equals("GEMFILE",
                                                                  StringComparison.InvariantCultureIgnoreCase)),
                                                     "basefolder");
        }
    }
}