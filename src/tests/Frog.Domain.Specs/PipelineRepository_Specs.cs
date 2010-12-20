using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class PipelineRepository_Specs
    {
        public static PipelineRepository PipelineRepository;
        public static Pipeline Pipeline1 = MockRepository.GenerateMock<Pipeline>();
        public static Pipeline Pipeline2 = MockRepository.GenerateMock<Pipeline>();

        Establish context = () => PipelineRepository = new PipelineRepository(Pipeline1, Pipeline2);
        Because of = () => PipelineRepository.SourceUpdate("water_id", 123456, "path");
        It should_offer_water_to_stream1 = () => Pipeline1.AssertWasCalled(x => x.Process(new SourceDrop("water_id", 123456, "path")));
        It should_offer_water_to_stream2 = () => Pipeline2.AssertWasCalled(x => x.Process(new SourceDrop("water_id", 123456, "path")));
    }
}