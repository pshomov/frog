using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.WorkerSpecs
{
    public abstract class WorkerSpecsBase : BDD
    {
        protected SourceRepoDriver sourceRepoDriver;
        protected Domain.Pipeline pipeline;
        protected WorkingAreaGoverner workingAreaGoverner;
        protected Worker worker;

        protected override void Given()
        {
            sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            pipeline = Substitute.For<Domain.Pipeline>();
            workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
        }
    }
}