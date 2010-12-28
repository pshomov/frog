using System;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ValveWhenNoUpdates : BDD
    {
        SourceRepoDriver _sourceRepoDriver;
        Pipeline pipeline;
        Valve valve;

        public override void Given()
        {
            pipeline = MockRepository.GenerateMock<Pipeline>();
            _sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            _sourceRepoDriver.Expect(driver => driver.CheckForUpdates()).Return(false);
            valve = new Valve(_sourceRepoDriver, pipeline);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_ask_repository_about_updates_in_the_repo()
        {
            _sourceRepoDriver.AssertWasCalled(driver => driver.CheckForUpdates());
        }

        [Test]
        public void should_not_tell_pipeline_to_start_rolin()
        {
            pipeline.AssertWasNotCalled(pipeline1 => pipeline1.Process(Arg<SourceDrop>.Is.Anything));
        }

    }
}