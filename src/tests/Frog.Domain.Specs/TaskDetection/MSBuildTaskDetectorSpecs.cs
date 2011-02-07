using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class MSBuildTaskDetectorSpecs : MSBuildTaskDetectorSpecsBase
    {
        MSBuildDetector msbuildTaskDetecttor;
        IList<MSBuildTaskDescriptions> items;

        public override void Given()
        {
            msbuildTaskDetecttor = new MSBuildDetector(projectFileRepo);
            projectFileRepo.FindAllSolutionFiles().Returns(Underware.As.List("a1.sln", "a1\\a2.sln",
                                                                             "a2\\asdas\\asd\\b.sln"));
        }

        public override void When()
        {
            items = msbuildTaskDetecttor.Detect();
        }

        [Test]
        public void should_prefer_root_file_sln_over_any_other_down_the_hierarchy()
        {
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items[0].solutionFile, Is.EqualTo("a1.sln"));
        }
    }

    public struct MSBuildTaskDescriptions
    {
        public readonly string solutionFile;

        public MSBuildTaskDescriptions(string solutionFile)
        {
            this.solutionFile = solutionFile;
        }
    }

    public class MSBuildDetector
    {
        readonly FileFinder fileFinder;

        public MSBuildDetector(FileFinder fileFinder)
        {
            this.fileFinder = fileFinder;
        }

        public IList<MSBuildTaskDescriptions> Detect()
        {
            var allSolutionFiles = fileFinder.FindAllSolutionFiles();
            var rootFolderSolutions =
                allSolutionFiles.Where(slnPath => slnPath.IndexOf(Path.DirectorySeparatorChar) == -1).ToList();
            return Underware.As.List(new MSBuildTaskDescriptions(rootFolderSolutions[0]));
        }
    }
}