using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain;
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
    public class SystemWithConsoleOutput : TestSystem
    {
        public const string TerminalOutput1 = "Terminal output 1";
        public const string TerminalOutput2 = "Terminal output 2";
        public const string TerminalOutput3 = "Terminal output 3";
        public const string TerminalOutput4 = "Terminal output 4";

        public SystemWithConsoleOutput()
        {
            tasksSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(new MSBuildTask("fle.sln")));
            var tasks = GetExecTasks();
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(tasks);
        }

        IList<IExecTask> GetExecTasks()
        {
            var task1 = Substitute.For<IExecTask>();
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            task1.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info =>
                    {
                        task1.OnTerminalOutputUpdate += Raise.Event<Action<string>>(TerminalOutput1);
                        task1.OnTerminalOutputUpdate += Raise.Event<Action<string>>(TerminalOutput2);
                    });
            var task2 = Substitute.For<IExecTask>();
            task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecutionStatus.Success, 0));
            task2.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info =>
                    {
                        task2.OnTerminalOutputUpdate += Raise.Event<Action<string>>(TerminalOutput3);
                        task2.OnTerminalOutputUpdate += Raise.Event<Action<string>>(TerminalOutput4);
                    });
            return As.List(task1, task2);
        }
    }

    [TestFixture]
    public class RealTimeConsoleStreaming : BDD
    {
        SystemDriver<SystemWithConsoleOutput> system;
        RepositoryDriver repo;

        protected override void Given()
        {
            repo = RepositoryDriver.GetNewRepository();
            system = SystemDriver<SystemWithConsoleOutput>.GetCleanSystem();
            system.RegisterNewProject(repo.Url);
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
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content == SystemWithConsoleOutput.TerminalOutput1))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content == SystemWithConsoleOutput.TerminalOutput2))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content == SystemWithConsoleOutput.TerminalOutput3))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content == SystemWithConsoleOutput.TerminalOutput4))
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
                                                  arg[repo.Url].Tasks.Count() > 0 &&
                                                  arg[repo.Url].Tasks.ElementAt(0).TerminalOutput ==
                                                  SystemWithConsoleOutput.TerminalOutput1 +
                                                  SystemWithConsoleOutput.TerminalOutput2))
                                         .Has(statuses => statuses,
                                              A.Check<Dictionary<string, BuildStatus>>(
                                                  arg =>
                                                  arg[repo.Url].Tasks.Count() > 1 &&
                                                  arg[repo.Url].Tasks.ElementAt(1).TerminalOutput ==
                                                  SystemWithConsoleOutput.TerminalOutput3 +
                                                  SystemWithConsoleOutput.TerminalOutput4)))
                            );
        }
    }
}