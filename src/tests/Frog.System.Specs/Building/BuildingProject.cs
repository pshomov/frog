using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.CustomTasks;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.Building
{
    [TestFixture]
    class BuildingProject : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;
        private Guid newGuid;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (ITask)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, "123", newGuid);
        }

        [Test]
        public void should_have_aggregate_id_in_all_messages_related_to_the_build()
        {
//            var prober = new PollingProber(5000, 100);
//            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
//                                         .Has(x => x,
//                                              An.Event<TerminalUpdate>(
//                                                  ev =>
//                                                  ev.BuildId && ev.TaskIndex == 0 &&
//                                                  ev.ContentSequenceIndex == 0 &&
//                                                  ev.Content.Contains(TerminalOutput3)))
        }

        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";

    }
}
