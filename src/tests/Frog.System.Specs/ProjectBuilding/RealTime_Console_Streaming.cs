using System;
using System.Linq;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SaaS.Engine;
using xray;
using BuildStarted = Frog.Domain.BuildStarted;
using CheckoutInfo = Frog.Domain.CheckoutInfo;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class RealTime_Console_Streaming : BDD
    {
        private const string RepoUrl = "123";
        private SystemDriver system;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "12" });
            sourceRepoDriver.GetSourceRevision("12", Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "comment", Revision = "12"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            bool shouldStop;
            testSystem.TasksSource.Detect(Arg.Any<string>(), out shouldStop).Returns(
                As.List(
                    (TaskDescription)
                    new FakeTaskDescription(TerminalOutput1,
                                            TerminalOutput2),
                    (TaskDescription)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }

        [Test]
        public void should_send_TERMINAL_UPDATE_messages()
        {
            var prober = new PollingProber(10000, 100);
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
            Guid buildId = Guid.Empty;
            prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                             .Has(x => x, An.Event<BuildStarted>(ev =>
                                                               {
                                                                   buildId = ev.BuildId;
                                                                   terminalId1 = ev.Status.Tasks[0].TerminalId;
                                                                   terminalId2 = ev.Status.Tasks[1].TerminalId;
                                                                   return true;
                                                               })));
                        
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<BuildId, SaaS.Client.Projections.Frog.Projects.Build>(new BuildId(buildId)))
                                         .Has(statuses => statuses,
                                              A.Check<SaaS.Client.Projections.Frog.Projects.Build>(
                                                  arg => arg.GetTerminalOutput(new TerminalId(terminalId1)).Match(TerminalOutput1)))
                            ));
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<BuildId, SaaS.Client.Projections.Frog.Projects.Build>(new BuildId(buildId)))
                                         .Has(statuses => statuses,
                                              A.Check<SaaS.Client.Projections.Frog.Projects.Build>(
                                                  arg => arg.GetTerminalOutput(new TerminalId(terminalId2)).Match(TerminalOutput3 + ".*\n.*" +
                                                      TerminalOutput4)))
                            ));
        }

        private const string TerminalOutput1 = "Terminal output 1";
        private const string TerminalOutput2 = "Terminal output 2";
        private const string TerminalOutput3 = "Terminal output 3";
        private const string TerminalOutput4 = "Terminal output 4";
    }
}