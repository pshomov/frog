using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.CustomTasks;
using Frog.Domain.ExecTasks;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs
{
    public class SystemWithConsoleOutput
    {
        public const string TerminalOutput1 = "Terminal output 1";
        public const string TerminalOutput2 = "Terminal output 2";
        public const string TerminalOutput3 = "Terminal output 3";
        public const string TerminalOutput4 = "Terminal output 4";
    }

    [TestFixture]
    public class RealTimeConsoleStreaming : BDD
    {
        private const string repoUrl = "http://123";
        private SystemDriver system;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns("12");
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.tasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (ITask)
                    new FakeTaskDescription(SystemWithConsoleOutput.TerminalOutput1,
                                            SystemWithConsoleOutput.TerminalOutput2),
                    (ITask)
                    new FakeTaskDescription(SystemWithConsoleOutput.TerminalOutput3,
                                            SystemWithConsoleOutput.TerminalOutput4)));
            system = SystemDriver.GetCleanSystem(() => testSystem);
            system.RegisterNewProject(repoUrl);
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_TERMINAL_UPDATE_messages()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repoUrl && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(SystemWithConsoleOutput.TerminalOutput1)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repoUrl && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content.Contains(SystemWithConsoleOutput.TerminalOutput2)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repoUrl && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content.Contains(SystemWithConsoleOutput.TerminalOutput3)))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repoUrl && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content.Contains(SystemWithConsoleOutput.TerminalOutput4)))
                            ));
        }

        [Test]
        public void should_update_view_with_terminal_updates()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView())
                                         .Has(statuses => statuses,
                                              A.Check<Dictionary<string, BuildStatus>>(
                                                  arg =>
                                                  arg[repoUrl].Tasks.Count() > 0 &&
                                                  arg[repoUrl].Tasks[0].GetTerminalOutput().Content.Match(
                                                      SystemWithConsoleOutput.TerminalOutput1 + ".*\n.*" +
                                                      SystemWithConsoleOutput.TerminalOutput2)))
                            ));
            Assert.True(prober.check(Take.Snapshot(() => system.GetView())
                                         .Has(statuses => statuses,
                                              A.Check<Dictionary<string, BuildStatus>>(
                                                  arg =>
                                                  arg[repoUrl].Tasks.Count() > 1 &&
                                                  arg[repoUrl].Tasks[1].GetTerminalOutput().Content.Match(
                                                      SystemWithConsoleOutput.TerminalOutput3 + ".*\n.*" +
                                                      SystemWithConsoleOutput.TerminalOutput4))))
                );
        }
    }
}