using System;
using System.Collections.Generic;
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
    public class BuildingProject : BDD
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
        public void should_announce_the_build_has_started()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<BuildStarted>(
                                                  ev =>
                                                  ev.BuildId == newGuid && ev.RepoUrl == RepoUrl &&
                                                  ev.Status.Tasks.Count == 1
                                                  ))));
        }

        [Test]
        public void should_update_build_status()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<BuildUpdated>(
                                                  ev =>
                                                  ev.BuildId == newGuid && ev.TaskIndex == 0 &&
                                                  ev.TaskStatus == TaskInfo.TaskStatus.Started
                                                  ))));
        }

        [Test]
        public void should_update_the_terminal_outout_for_the_task()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == newGuid && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(TerminalOutput3)))));
        }

        [Test]
        public void should_mark_the_build_as_finished_and_successful()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<BuildEnded>(
                                                  ev =>
                                                  ev.BuildId == newGuid && ev.TotalStatus == BuildTotalEndStatus.Success))));
        }

        [Test]
        public void should_have_the_build_as_the_current_one_in_the_ui()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetCurrentBuilds())
                                         .Has(x => x, A.Check<Dictionary<string, Guid>>(guids => guids[RepoUrl] == newGuid))));
        }

        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";
    }
}