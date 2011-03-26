using System;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.CustomTasks;
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
        public SystemWithConsoleOutput()
        {
            tasksSource.Detect(Arg.Any<string>()).Returns(As.List<ITask>(new MSBuildTaskDescriptions("fle.sln")));
            var tasks = GetExecTasks();
            execTaskGenerator.GimeTasks(Arg.Any<ITask>()).Returns(tasks);
        }

        IList<ExecTask> GetExecTasks()
        {
            var task1 = Substitute.For<ExecTask>(null, null, null);
            task1.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            task1.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => task1.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 1"));
            task1.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => task1.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 2"));
            var task2 = Substitute.For<ExecTask>(null, null, null);
            task2.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            task2.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => task2.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 3"));
            task2.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => task2.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 4"));
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
        public void should_send_TERMINAL_UPDATE_message()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content == "Terminal output 1"))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 0 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content == "Terminal output 2"))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 0 &&
                                                  ev.Content == "Terminal output 3"))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev =>
                                                  ev.RepoUrl == repo.Url && ev.TaskIndex == 1 &&
                                                  ev.ContentSequenceIndex == 1 &&
                                                  ev.Content == "Terminal output 4"))
                            ));
        }
    }
}