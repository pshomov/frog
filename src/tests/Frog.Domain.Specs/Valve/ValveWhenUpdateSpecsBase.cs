using Rhino.Mocks;

namespace Frog.Domain.Specs.Valve
{
    public abstract class ValveWhenUpdateSpecsBase : BDD
    {
        protected SourceRepoDriver sourceRepoDriver;
        protected Domain.Pipeline pipeline;
        protected WorkingArea workingArea;
        protected Domain.Valve valve;

        public override void Given()
        {
            sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            pipeline = MockRepository.GenerateMock<Domain.Pipeline>();
            workingArea = MockRepository.GenerateMock<WorkingArea>();
        }
    }
}