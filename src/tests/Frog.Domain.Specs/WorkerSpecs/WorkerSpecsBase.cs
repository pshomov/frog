using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.WorkerSpecs
{
    public abstract class WorkerSpecsBase : BDD
    {
        protected SourceRepoDriver SourceRepoDriver;
        protected Domain.Pipeline Pipeline;
        protected WorkingAreaGoverner WorkingAreaGovernor;
        protected Worker Worker;

        protected override void Given()
        {
            SourceRepoDriver = Substitute.For<SourceRepoDriver>();
            Pipeline = Substitute.For<Domain.Pipeline>();
            WorkingAreaGovernor = Substitute.For<WorkingAreaGoverner>();
        }
    }
}