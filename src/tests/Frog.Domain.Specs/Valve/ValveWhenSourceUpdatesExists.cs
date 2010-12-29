using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ValveWhenSourceUpdatesExists : BDD
    {
        SourceRepoDriver _sourceRepoDriver;
        Pipeline pipeline;
        Domain.Valve valve;

        public override void Given()
        {
            pipeline = MockRepository.GenerateMock<Pipeline>();
            _sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            _sourceRepoDriver.Expect(driver => driver.CheckForUpdates()).Return(true);
            valve = new Domain.Valve(_sourceRepoDriver, pipeline);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_ask_repository_to_do_initial_checkout()
        {
            _sourceRepoDriver.AssertWasCalled(driver => driver.CheckForUpdates());
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            pipeline.AssertWasCalled(pipeline1 => pipeline1.Process(Arg<SourceDrop>.Is.Anything));
        }

    }
}