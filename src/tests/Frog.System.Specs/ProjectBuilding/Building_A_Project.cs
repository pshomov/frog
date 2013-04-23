using System;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using xray;
using BuildEnded = Frog.Domain.BuildEnded;
using BuildStarted = Frog.Domain.BuildStarted;
using BuildTotalEndStatus = Frog.Domain.BuildTotalEndStatus;
using BuildUpdated = Frog.Domain.BuildUpdated;
using CheckoutInfo = Frog.Domain.CheckoutInfo;
using ProjectCheckedOut = Frog.Domain.ProjectCheckedOut;
using TaskInfo = Frog.Domain.TaskInfo;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Building_A_Project : SystemBDD
    {
        private const string RepoUrl = "123";
        private Guid buildId;
        private Guid taskId;

        protected override void Given()
        {
            base.Given();
            taskId = Guid.NewGuid();
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "Fle", Revision = "123"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            testSystem
                .WithProjections()
                .WithRepositoryTracker()
                .WithRevisionChecker(url => sourceRepoDriver)
                .AddAgent(url => sourceRepoDriver, workingAreaGoverner, new string[] {});
            bool shouldStop;
            testSystem.TasksSource.Detect(Arg.Any<string>(), out shouldStop).Returns(
                As.List(
                    (TaskDescription)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            buildId = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, buildId);
        }

        [Test]
        public void should_announce_the_project_has_been_checked_out()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<ProjectCheckedOut>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.RepoUrl == RepoUrl &&
                                                  ev.SequenceId == 0
                                                  ))));
        }

        [Test]
        public void should_announce_the_build_has_started()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<BuildStarted>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.RepoUrl == RepoUrl &&
                                                  ev.Status.Tasks.Count == 1 && ev.SequenceId == 1 && ev.Status.Tasks[0].TerminalId != Guid.Empty
                                                  ))));
        }

        [Test]
        public void should_update_build_status()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<BuildUpdated>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.TaskIndex == 0 &&
                                                  ev.TaskStatus == TaskInfo.TaskStatus.Started && ev.SequenceId == 2
                                                  ))));
        }

        [Test]
        public void should_update_the_terminal_outout_for_the_task()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<BuildUpdated>(
                                             ev =>
                                                 {
                                                     if (ev.TaskIndex == 0 && ev.BuildId == buildId &&
                                                         ev.TaskStatus == TaskInfo.TaskStatus.Started)
                                                     {
                                                         taskId = ev.TerminalId;
                                                         return true;
                                                     }
                                                     return false;
                                                 }))
                                         .Has(An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == buildId &&
                                                  ev.TaskIndex == 0 &&
                                                  ev.TerminalId == taskId &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(TerminalOutput3) &&
                                                  ev.SequenceId == 0
                                                  ))
                            ));
        }

        [Test]
        public void should_mark_the_task_as_finished_and_successful()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<BuildUpdated>(
                                             ev =>
                                             {
                                                 if (ev.TaskIndex == 0 && ev.BuildId == buildId &&
                                                     ev.TaskStatus == TaskInfo.TaskStatus.Started)
                                                 {
                                                     taskId = ev.TerminalId;
                                                     return true;
                                                 }
                                                 return false;
                                             }))
                                         .Has(An.Event<BuildUpdated>(
                                                  ev =>
                                                  ev.BuildId == buildId &&
                                                  ev.TerminalId == taskId &&
                                                  ev.TaskStatus == TaskInfo.TaskStatus.FinishedSuccess && 
                                                  ev.TaskIndex == 0 &&
                                                  ev.SequenceId == 3
                                                  ))));
        }
        [Test]
        public void should_mark_the_build_as_finished_and_successful()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<BuildEnded>(
                                                  ev =>
                                                  ev.BuildId == buildId &&
                                                  ev.TotalStatus == BuildTotalEndStatus.Success && 
                                                  ev.SequenceId == 4
                                                  ))));
        }

        [Test]
        public void should_have_the_build_as_the_current_one_in_the_ui()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(A.Check<ProjectHistory>(view => view.Current.buildId == new BuildId(buildId)))));
        }

        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";
    }
}