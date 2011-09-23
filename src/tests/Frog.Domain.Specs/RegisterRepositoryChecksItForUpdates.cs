using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.RevisionChecker;
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
            bus.Received().Send(Arg.Is<CheckRevision>(updates => 
                updates.RepoUrl == "http://fle"));
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
            bus.Received().Send(Arg.Is<CheckRevision>(updates =>
                updates.RepoUrl == "http://fle1"));
            bus.Received().Send(Arg.Is<CheckRevision>(updates =>
                updates.RepoUrl == "http://fle2"));
            bus.Received().Send(Arg.Is<CheckRevision>(updates =>
                updates.RepoUrl == "http://fle3"));
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
            bus.DidNotReceive().Send(Arg.Any<Build>());
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
            repositoryTracker.Handle(new UpdateFound { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "123" } });
            bus.ClearReceivedCalls();
        }

        protected override void When()
        {
            repositoryTracker.CheckForUpdates();
        }

        [Test]
        public void should_send_a_new_command_to_check_for_updates()
        {
            bus.Received().Send(Arg.Any<CheckRevision>());
        }
    }

    [TestFixture]
    public class RepositoryUpdaterChecksForUpdateThenGetsFailureResponseAndChecksAgain : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.CheckForUpdates();
            repositoryTracker.Handle(new CheckForUpdateFailed{repoUrl = "http://fle"});
            bus.ClearReceivedCalls();
        }

        protected override void When()
        {
            repositoryTracker.CheckForUpdates();
        }

        [Test]
        public void should_send_a_new_command_to_check_for_updates()
        {
            bus.Received().Send(Arg.Any<CheckRevision>());
        }
    }

    [TestFixture]
    public class RepositoryChecksForUpdateAndFindsNewRevision : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.CheckForUpdates();
        }

        protected override void When()
        {
            repositoryTracker.Handle(new UpdateFound { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "789" } });
        }

        [Test]
        public void should_send_a_command_to_build_the_project()
        {
            bus.Received().Send(Arg.Is<Build>(project => project.RepoUrl == "http://fle" && project.Revision.Revision == "789"));
        }
    }
    [TestFixture]
    public class RepositoryChecksForUpdateButNoNewRevision : RepositoryTrackerSpecsBase
    {
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Handle(new RegisterRepository { Repo = "http://fle" });
            repositoryTracker.CheckForUpdates();
            repositoryTracker.Handle(new UpdateFound { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "789" } });
            repositoryTracker.CheckForUpdates();
            bus.ClearReceivedCalls();
        }

        protected override void When()
        {
            repositoryTracker.Handle(new UpdateFound { RepoUrl = "http://fle", Revision = new RevisionInfo{Revision = "789"}});
        }

        [Test]
        public void should_send_a_command_to_build_the_project()
        {
            bus.DidNotReceive().Send(Arg.Any<Build>());
        }
    }
}