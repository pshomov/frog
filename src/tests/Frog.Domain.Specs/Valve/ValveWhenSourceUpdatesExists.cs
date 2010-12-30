using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ValveWhenSourceUpdatesExists : ValveWhenUpdateSpecsBase
    {
        SourceDrop sourceDrop;

        public override void Given()
        {
            base.Given();
            sourceRepoDriver.Expect(driver => driver.CheckForUpdates()).Return(true);
            sourceDrop = new SourceDrop("");
            sourceRepoDriver.Expect(driver => driver.GetLatestSourceDrop("")).Return(sourceDrop);
            workingArea.Stub(area => area.AllocateWorkingArea()).Return("");
            valve = new Valve(sourceRepoDriver, pipeline, workingArea);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_ask_repository_to_do_initial_checkout()
        {
            sourceRepoDriver.AssertWasCalled(driver => driver.CheckForUpdates());
        }

        [Test]
        public void should_ask_repository_for_a_source_drop()
        {
            sourceRepoDriver.AssertWasCalled(driver => driver.GetLatestSourceDrop(""));
        }

        [Test]
        public void should_tell_pipeline_to_start_rolin()
        {
            pipeline.AssertWasCalled(pipeline1 => pipeline1.Process(sourceDrop));
        }

    }
}