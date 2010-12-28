using System;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ValveSpec : BDD
    {
        SourceRepoDriver _sourceRepoDriver;
        Pipeline pipeline;
        Valve valve;

        public override void Given()
        {
            pipeline = MockRepository.GenerateMock<Pipeline>();
            _sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            valve = new Valve(_sourceRepoDriver, pipeline);
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

    public class Valve
    {
        readonly SourceRepoDriver _sourceRepoDriver;
        readonly Pipeline _pipeline;

        public Valve(SourceRepoDriver sourceRepoDriver, Pipeline pipeline)
        {
            _sourceRepoDriver = sourceRepoDriver;
            _pipeline = pipeline;
        }

        public void Check()
        {
            _sourceRepoDriver.CheckForUpdates();
            _pipeline.Process(null);
        }
    }
}