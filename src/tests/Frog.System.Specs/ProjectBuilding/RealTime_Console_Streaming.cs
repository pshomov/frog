using System;
using Frog.Domain;
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
    public class RealTime_Console_Streaming : SystemBDD
    {
        private const string RepoUrl = "123";

        protected override void Given()
        {
            base.Given();
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns(new RevisionInfo { Revision = "12" });
            sourceRepoDriver.GetSourceRevision("12", Arg.Any<string>()).Returns(new CheckoutInfo(){Comment = "comment", Revision = "12"});

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
                    new FakeTaskDescription(TerminalOutput1,
                                            TerminalOutput2),
                    (TaskDescription)
                    new FakeTaskDescription(TerminalOutput3,
                                            TerminalOutput4)));

            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_TERMINAL_UPDATE_messages()
        {
            Guid buildId = Guid.Empty;
            Assert.True(EventStoreCheck(ES => ES
                                                  .Has(A.Command<Build>(ev =>
                                                      {
                                                          buildId = ev.Id;
                                                          return true;
                                                      }))
                                                  .Has(
                                                      An.Event<TerminalUpdate>(
                                                          ev =>
                                                          ev.BuildId == buildId && ev.TaskIndex == 0 &&
                                                          ev.ContentSequenceIndex == 0 &&
                                                          ev.Content.Contains(TerminalOutput1)))
                                                  .Has(
                                                      An.Event<TerminalUpdate>(
                                                          ev =>
                                                          ev.BuildId == buildId && ev.TaskIndex == 0 &&
                                                          ev.ContentSequenceIndex == 1 &&
                                                          ev.Content.Contains(TerminalOutput2)))
                                                  .Has(
                                                      An.Event<TerminalUpdate>(
                                                          ev =>
                                                          ev.BuildId == buildId && ev.TaskIndex == 1 &&
                                                          ev.ContentSequenceIndex == 0 &&
                                                          ev.Content.Contains(TerminalOutput3)))
                                                  .Has(
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
            var terminalId1 = Guid.Empty;
            var terminalId2 = Guid.Empty;
            Guid buildId = Guid.Empty;
            Assert.True(EventStoreCheck(ES => ES
                             .Has(x => x, An.Event<BuildStarted>(ev =>
                                                               {
                                                                   buildId = ev.BuildId;
                                                                   terminalId1 = ev.Status.Tasks[0].TerminalId;
                                                                   terminalId2 = ev.Status.Tasks[1].TerminalId;
                                                                   return true;
                                                               }))));

            var prober = new PollingProber(5000, 100);
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