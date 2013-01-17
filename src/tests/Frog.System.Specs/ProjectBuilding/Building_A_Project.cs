using System;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Building_A_Project : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;
        private Guid newGuid;
        private Guid taskGuid;

        protected override void Given()
        {
            taskGuid = Guid.NewGuid();
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "Fle", Revision = "123"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (Task)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, newGuid);
        }

        [Test]
        public void should_announce_the_project_has_been_checked_out()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<ProjectCheckedOut>(
                                                  ev =>
                                                  ev.BuildId == newGuid && ev.RepoUrl == RepoUrl &&
                                                  ev.SequenceId == 0
                                                  ))));
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
                                                  ev.Status.Tasks.Count == 1 && ev.SequenceId == 1 && ev.Status.Tasks[0].TerminalId != Guid.Empty
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
                                                  ev.TaskStatus == TaskInfo.TaskStatus.Started && ev.SequenceId == 2
                                                  ))));
        }

        [Test]
        public void should_update_the_terminal_outout_for_the_task()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x, An.Event<BuildUpdated>(
                                             ev =>
                                                 {
                                                     if (ev.TaskIndex == 0 && ev.BuildId == newGuid &&
                                                         ev.TaskStatus == TaskInfo.TaskStatus.Started)
                                                     {
                                                         taskGuid = ev.TerminalId;
                                                         return true;
                                                     }
                                                     return false;
                                                 }))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == newGuid &&
                                                  ev.TaskIndex == 0 &&
                                                  ev.TerminalId == taskGuid &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(TerminalOutput3) &&
                                                  ev.SequenceId == 0
                                                  ))
                            ));
        }

        [Test]
        public void should_mark_the_task_as_finished_and_successful()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x, An.Event<BuildUpdated>(
                                             ev =>
                                             {
                                                 if (ev.TaskIndex == 0 && ev.BuildId == newGuid &&
                                                     ev.TaskStatus == TaskInfo.TaskStatus.Started)
                                                 {
                                                     taskGuid = ev.TerminalId;
                                                     return true;
                                                 }
                                                 return false;
                                             }))
                                         .Has(x => x,
                                              An.Event<BuildUpdated>(
                                                  ev =>
                                                  ev.BuildId == newGuid &&
                                                  ev.TerminalId == taskGuid &&
                                                  ev.TaskStatus == TaskInfo.TaskStatus.FinishedSuccess && 
                                                  ev.TaskIndex == 0 &&
                                                  ev.SequenceId == 3
                                                  ))));
        }
        [Test]
        public void should_mark_the_build_as_finished_and_successful()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<BuildEnded>(
                                                  ev =>
                                                  ev.BuildId == newGuid &&
                                                  ev.TotalStatus == BuildTotalEndStatus.Success && 
                                                  ev.SequenceId == 4
                                                  ))));
        }

        [Test]
        public void should_have_the_build_as_the_current_one_in_the_ui()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetProjectStatusView())
                                         .Has(x => x, A.Check<ProjectView>(view => view.GetCurrentBuild(RepoUrl) == newGuid))));
        }

        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";
    }
}