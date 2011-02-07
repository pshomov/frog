using NSubstitute;

namespace Frog.Domain.Specs
{
    public abstract class MSBuildTaskDetectorSpecsBase : BDD
    {
        protected FileFinder projectFileRepo;

        public override void SetupDependencies()
        {
            projectFileRepo = Substitute.For<FileFinder>();
        }
    }
}