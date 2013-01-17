using System;
using System.Linq;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.Integration.UI;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class RealTime_Console_Streaming : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "12" });
            sourceRepoDriver.GetSourceRevision("12", Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "comment", Revision = "12"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (Task)
                    new FakeTaskDescription(TerminalOutput1,
                                            TerminalOutput2),
                    (Task)
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
            var terminalId1 = Guid.Empty;
            var terminalId2 = Guid.Empty;
            prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                             .Has(x => x, An.Event<BuildStarted>(ev =>
                                                               {
                                                                   terminalId1 = ev.Status.Tasks[0].TerminalId;
                                                                   terminalId2 = ev.Status.Tasks[1].TerminalId;
                                                                   return true;
                                                               })));
                        
            Assert.True(prober.check(Take.Snapshot(() => system.GetTerminalStatusView())
                                         .Has(statuses => statuses,
                                              A.Check<ViewForTerminalOutput>(
                                                  arg =>
                                                  arg.GetTerminalOutput(terminalId1).Match(
                                                      TerminalOutput1 + ".*\n.*" +
                                                      TerminalOutput2)))
                            ));
            Assert.True(prober.check(Take.Snapshot(() => system.GetTerminalStatusView())
                                         .Has(statuses => statuses,
                                              A.Check<ViewForTerminalOutput>(
                                                  arg =>
                                                  arg.GetTerminalOutput(terminalId2).Match(
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