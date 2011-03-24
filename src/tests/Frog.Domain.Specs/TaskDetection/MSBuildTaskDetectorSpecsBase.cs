using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs
{
    public abstract class MSBuildTaskDetectorSpecsBase : BDD
    {
        protected FileFinder projectFileRepo;

        protected override void SetupDependencies()
        {
            projectFileRepo = Substitute.For<FileFinder>();
        }
    }
}