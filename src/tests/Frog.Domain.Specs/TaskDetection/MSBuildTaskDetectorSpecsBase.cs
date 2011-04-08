using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.TaskDetection
{
    public abstract class MSBuildTaskDetectorSpecsBase : BDD
    {
        protected FileFinder ProjectFileRepo;

        protected override void SetupDependencies()
        {
            ProjectFileRepo = Substitute.For<FileFinder>();
        }
    }
}