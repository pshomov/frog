using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class Streams_Specs
    {
        public static StreamsRepository StreamsRepository;
        public static Stream Stream1 = MockRepository.GenerateMock<Stream>();
        public static Stream Stream2 = MockRepository.GenerateMock<Stream>();

        Establish context = () => StreamsRepository = new StreamsRepository(Stream1, Stream2);
        Because of = () => StreamsRepository.Water("water_id", 123456, "path");
        It should_offer_water_to_stream1 = () => Stream1.AssertWasCalled(x => x.Water("water_id", 123456, "path"));
        It should_offer_water_to_stream2 = () => Stream2.AssertWasCalled(x => x.Water("water_id", 123456, "path"));
    }
}