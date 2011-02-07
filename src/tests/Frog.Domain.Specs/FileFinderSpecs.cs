using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class NUnitFileFinderSpecs : BDD
    {
        FileFinder fileFinder;
        PathFinder pathFinder;

        public override void Given()
        {
            pathFinder = Substitute.For<PathFinder>("");
            fileFinder = new DefaultFileFinder(pathFinder);
        }

        public override void When()
        {
            fileFinder.FindAllNUnitAssemblies();
        }

        [Test]
        public void should_search_for_files_that_end_in_word_Test()
        {
            pathFinder.Received().apply(Arg.Any<Action<string>>(),
                                        Arg.Is<string>(
                                            s => s.Equals("*.Test.dll", StringComparison.InvariantCultureIgnoreCase)));
        }
    }

    [TestFixture]
    public class MSSolutionsFileFinderSpecs : BDD
    {
        FileFinder fileFinder;
        PathFinder pathFinder;

        public override void Given()
        {
            pathFinder = Substitute.For<PathFinder>("");
            fileFinder = new DefaultFileFinder(pathFinder);
        }

        public override void When()
        {
            fileFinder.FindAllSolutionFiles();
        }

        [Test]
        public void should_search_for_all_files_with_extension_SLN()
        {
            pathFinder.Received().apply(Arg.Any<Action<string>>(),
                                        Arg.Is<string>(
                                            s => s.Equals("*.sln", StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}