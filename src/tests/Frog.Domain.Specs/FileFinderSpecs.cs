using System;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class NUnitFileFinderSpecs : BDD
    {
        FileFinder fileFinder;
        PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            fileFinder = new DefaultFileFinder(pathFinder);
        }

        protected override void When()
        {
            fileFinder.FindAllNUnitAssemblies("basefolder");
        }

        [Test]
        public void should_search_for_files_that_end_in_word_Test()
        {
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                        Arg.Is<string>(
                                            s => s.Equals("*.Test.csproj", StringComparison.InvariantCultureIgnoreCase)), "basefolder");
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                        Arg.Is<string>(
                                            s => s.Equals("*.Tests.csproj", StringComparison.InvariantCultureIgnoreCase)), "basefolder");
        }
    }

    [TestFixture]
    public class MSSolutionsFileFinderSpecs : BDD
    {
        FileFinder fileFinder;
        PathFinder pathFinder;

        protected override void Given()
        {
            pathFinder = Substitute.For<PathFinder>();
            fileFinder = new DefaultFileFinder(pathFinder);
        }

        protected override void When()
        {
            fileFinder.FindAllSolutionFiles("basefolder");
        }

        [Test]
        public void should_search_for_all_files_with_extension_SLN()
        {
            pathFinder.Received().FindFilesRecursively(Arg.Any<Action<string>>(),
                                        Arg.Is<string>(
                                            s => s.Equals("*.sln", StringComparison.InvariantCultureIgnoreCase)), "basefolder");
        }
    }
}