using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class PipelineRepositorySourceUpdate
    {
        public static PipelineRepository PipelineRepository;
        public static Pipeline Pipeline1 = MockRepository.GenerateMock<Pipeline>();
        private static SourceDrop _srcDrop = new SourceDrop("water_id", 123456, "path");
        public static Pipeline Pipeline2 = MockRepository.GenerateMock<Pipeline>();

        Establish context = () => PipelineRepository = new PipelineRepository(Pipeline1, Pipeline2);
        Because of = () => PipelineRepository.SourceUpdate(_srcDrop);
        It should_offer_water_to_stream1 = () => Pipeline1.AssertWasCalled(x => x.Process(_srcDrop));
        It should_offer_water_to_stream2 = () => Pipeline2.AssertWasCalled(x => x.Process(_srcDrop));
    }
}