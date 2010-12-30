using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Frog.Domain;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ValveWhenNoSourceUpdates : BDD
    {
        SourceRepoDriver sourceRepoDriver;
        Pipeline pipeline;
        Valve valve;

        public override void Given()
        {
            pipeline = MockRepository.GenerateMock<Pipeline>();
            sourceRepoDriver = MockRepository.GenerateMock<SourceRepoDriver>();
            sourceRepoDriver.Expect(driver => driver.CheckForUpdates()).Return(false);
            valve = new Valve(sourceRepoDriver, pipeline, MockRepository.GenerateMock<WorkingArea>());
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_ask_repository_about_updates_in_the_repo()
        {
            sourceRepoDriver.AssertWasCalled(driver => driver.CheckForUpdates());
        }

        [Test]
        public void should_not_tell_pipeline_to_start_rolin()
        {
            pipeline.AssertWasNotCalled(pipeline1 => pipeline1.Process(Arg<SourceDrop>.Is.Anything));
        }

    }
}