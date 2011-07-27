using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    public abstract class RepositoryTrackerSpecsBase : BDD
    {
        protected RepositoryTracker.RepositoryTracker repositoryTracker;
        protected IBus bus;

        protected override void Given()
        {
            bus = Substitute.For<IBus>();
            repositoryTracker = new RepositoryTracker.RepositoryTracker(bus, new InMemoryProjectsRepository());
        }
    }

    [TestFixture]
    public class RegisterRepositoryChecksItForUpdates : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.Handle(new RegisterRepository {Repo = "http://fle"});
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates => 
                updates.RepoUrl == "http://fle" && updates.Revision == ""));
        }

    }

    [TestFixture]
    public class RegisterSameRepoTwiceNoDuplication : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
        }

        [Test]
        public void should_send_only_one_check_for_updates_command()
        {
            repositoryTracker.CheckForUpdates();
            Assert.That(bus.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Send").Count(), Is.EqualTo(1));
        }

    }

    [TestFixture]
    public class ChecksForUpdatesAllRegisteredRepositories : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle1" });
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle2" });
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle3" });
        }

        [Test]
        public void should_send_only_one_check_for_updates_command()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates =>
                updates.RepoUrl == "http://fle1" && updates.Revision == ""));
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates =>
                updates.RepoUrl == "http://fle2" && updates.Revision == ""));
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates =>
                updates.RepoUrl == "http://fle3" && updates.Revision == ""));
        }

    }

    [TestFixture]
    public class RepositoryListensForBuildUpdatesSpecs : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.JoinTheMessageParty();
        }

        [Test]
        public void should_register_repository()
        {
            bus.Received().RegisterHandler(Arg.Any<Action<UpdateFound>>(), Arg.Any<string>());
            bus.Received().RegisterHandler(Arg.Any<Action<RegisterRepository>>(), Arg.Any<string>());
        }

    }

    [TestFixture]
    public class RepositoryUpdateLastRevisionNumber : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
        }

        protected override void When()
        {
            repositoryTracker.Handle(new UpdateFound{RepoUrl = "http://fle", Revision = "12"});
        }

        [Test]
        public void should_send_new_revision_as_latest_known_revision_number_on_subsecuen_checks_for_updates()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates => updates.RepoUrl == "http://fle" && updates.Revision == "12"));
        }
    }

    [TestFixture]
    public class RepositoryUpdaterChecksForUpdateTwiceInARow : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.CheckForUpdates();
            bus.ClearReceivedCalls();
        }

        protected override void When()
        {
            repositoryTracker.CheckForUpdates();
        }

        [Test]
        public void should_not_send_a_new_command_to_check_for_updates_since_it_already_has_asked_before()
        {
            bus.DidNotReceive().Send(Arg.Any<CheckForUpdates>());
        }
    }

    [TestFixture]
    public class RepositoryUpdaterChecksForUpdateThenGetsResponseAndChecksAgain : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.CheckForUpdates();
            repositoryTracker.Handle(new UpdateFound {RepoUrl = "http://fle", Revision = "123"});
            bus.ClearReceivedCalls();
        }

        protected override void When()
        {
            repositoryTracker.CheckForUpdates();
        }

        [Test]
        public void should_send_a_new_command_to_check_for_updates()
        {
            bus.Received().Send(Arg.Any<CheckForUpdates>());
        }
    }
}