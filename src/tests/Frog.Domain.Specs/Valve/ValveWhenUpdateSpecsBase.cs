using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public abstract class ValveWhenUpdateSpecsBase : BDD
    {
        protected SourceRepoDriver sourceRepoDriver;
        protected Pipeline pipeline;
        protected WorkingArea workingArea;
        protected Valve valve;

        public override void Given()
        {
            sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            pipeline = MockRepository.GenerateMock<Pipeline>();
            workingArea = MockRepository.GenerateMock<WorkingArea>();
        }
    }
}