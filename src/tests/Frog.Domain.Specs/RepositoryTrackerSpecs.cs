using System;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    public abstract class RepositoryTrackerSpecsBase : BDD
    {
        protected RepositoryTracker repositoryTracker;
        protected IBus bus;

        public override void Given()
        {
            bus = Substitute.For<IBus>();
            repositoryTracker = new RepositoryTracker(bus);
        }
    }

    [TestFixture]
    public class RepositoryTrackerSpecs : RepositoryTrackerSpecsBase
    {
        public override void When()
        {
            repositoryTracker.Track("http://fle");
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Publish(Arg.Is<CheckForUpdates>(updates => 
                updates.RepoUrl == "http://fle" && updates.Revision == ""));
        }

    }

    [TestFixture]
    public class RepositoryListensForBuildUpdatesSpecs : RepositoryTrackerSpecsBase
    {
        public override void When()
        {
            repositoryTracker.StartListeningForBuildUpdates();
        }

        [Test]
        public void should_register_repository()
        {
            bus.Received().RegisterHandler(Arg.Any<Action<UpdateFound>>());
        }

    }
    [TestFixture]
    public class RepositoryUpdatesLastRevisionInfoSpecs : RepositoryTrackerSpecsBase
    {
        public override void Given()
        {
            base.Given();
            repositoryTracker.Track("http://fle");
        }

        public override void When()
        {
            repositoryTracker.Handle(new UpdateFound{RepoUrl = "http://fle", Revision = "12"});
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Publish(Arg.Is<CheckForUpdates>(updates => updates.RepoUrl == "http://fle" && updates.Revision == "12"));
        }

    }
}