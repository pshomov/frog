using System;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;
using xray;

namespace Frog.System.Specs
{
    public class SystemWithConsoleOutput : TestSystem
    {
        protected override ExecTask GetExecTask()
        {
            var result = Substitute.For<ExecTask>(null,null,null);
            result.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => result.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 1"));
            result.When(task => task.Perform(Arg.Any<SourceDrop>())).Do(
                info => result.OnTerminalOutputUpdate += Raise.Event<Action<string>>("Terminal output 2"));
            return result;
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
                                                  ev => ev.RepoUrl == repo.Url && ev.TaskIndex == 0 && ev.Content == "Terminal output 1"))
                                         .Has(x => x,
                                              An.Event<TerminalUpdate>(
                                                  ev => ev.RepoUrl == repo.Url && ev.TaskIndex == 0 && ev.Content == "Terminal output 2"))
                            ));
        }
    }
}