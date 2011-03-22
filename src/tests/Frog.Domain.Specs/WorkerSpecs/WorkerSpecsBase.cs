using NSubstitute;

namespace Frog.Domain.Specs.ValveSpecs
{
    public abstract class WorkerSpecsBase : BDD
    {
        protected SourceRepoDriver sourceRepoDriver;
        protected Domain.Pipeline pipeline;
        protected WorkingArea workingArea;
        protected Worker worker;

        public override void Given()
        {
            sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            pipeline = Substitute.For<Domain.Pipeline>();
            workingArea = Substitute.For<WorkingArea>();
        }
    }
}