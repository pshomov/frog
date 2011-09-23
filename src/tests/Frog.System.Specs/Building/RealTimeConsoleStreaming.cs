using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.CustomTasks;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.Building
{
    [TestFixture]
    public class RealTimeConsoleStreaming : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "12" });
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (ITask)
                    new FakeTaskDescription(TerminalOutput1,
                                            TerminalOutput2),
                    (ITask)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_TERMINAL_UPDATE_messages()
        {
            var prober = new PollingProber(5000, 100);
            Guid buildId = Guid.Empty;
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x, A.Command<Build>(ev =>
                                                                           {
                                                                               buildId = ev.Id; return true; }))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(TerminalOutput1)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content.Contains(TerminalOutput2)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(TerminalOutput3)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.BuildId == buildId && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content.Contains(TerminalOutput4)))
                            ));
        }

        [Test]
        public void should_update_view_with_terminal_updates()
        {
            var prober = new PollingProber(5000, 100);
            Guid buildId = Guid.Empty;
            prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                             .Has(x => x, A.Command<Build>(ev =>
                                                               {
                                                                   buildId = ev.Id;
                                                                   return true;
                                                               })));
                        
            Assert.True(prober.check(Take.Snapshot(() => system.GetView())
                                         .Has(statuses => statuses,
                                              A.Check<ProjectView>(
                                                  arg =>
                                                  arg.GetBuildStatus(buildId).Tasks.Count() > 0 &&
                                                  arg.GetBuildStatus(buildId).Tasks[0].GetTerminalOutput().Content.Match(
                                                      TerminalOutput1 + ".*\n.*" +
                                                      TerminalOutput2)))
                            ));
            Assert.True(prober.check(Take.Snapshot(() => system.GetView())
                                         .Has(statuses => statuses,
                                              A.Check<ProjectView>(
                                                  arg =>
                                                  arg.GetBuildStatus(buildId).Tasks.Count() > 1 &&
                                                  arg.GetBuildStatus(buildId).Tasks[1].GetTerminalOutput().Content.Match(
                                                      TerminalOutput3 + ".*\n.*" +
                                                      TerminalOutput4))))
                );
        }

        private const string TerminalOutput1 = "Terminal output 1";
        private const string TerminalOutput2 = "Terminal output 2";
        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";
    }
}