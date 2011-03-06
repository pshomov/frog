using NSubstitute;

namespace Frog.Domain.Specs.ValveSpecs
{
    public abstract class ValveWhenUpdateSpecsBase : BDD
    {
        protected SourceRepoDriver sourceRepoDriver;
        protected Domain.Pipeline pipeline;
        protected WorkingArea workingArea;
        protected Domain.Valve valve;

        public override void Given()
        {
            sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            pipeline = Substitute.For<Domain.Pipeline>();
            workingArea = Substitute.For<WorkingArea>();
        }
    }
}