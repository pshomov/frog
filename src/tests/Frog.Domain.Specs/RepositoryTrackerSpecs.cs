using System;
using System.Linq;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    public abstract class RepositoryTrackerSpecsBase : BDD
    {
        protected RepositoryTracker repositoryTracker;
        protected IBus bus;

        protected override void Given()
        {
            bus = Substitute.For<IBus>();
            repositoryTracker = new RepositoryTracker(bus);
        }
    }

    [TestFixture]
    public class RepositoryTrackerSpecs : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.Track("http://fle");
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
    public class RepositoryTrackerTracksProjectOnlyOnceSpecs : RepositoryTrackerSpecsBase
    {
        protected override void When()
        {
            repositoryTracker.Track("http://fle");
            repositoryTracker.Track("http://fle");
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            Assert.That(bus.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Send").Count(), Is.EqualTo(1));
        }

    }

    [TestFixture]
    public class RepositoryListensForBuildUpdatesSpecs : RepositoryTrackerSpecsBase
    {
        protected override void When()
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
        protected override void Given()
        {
            base.Given();
            repositoryTracker.Track("http://fle");
        }

        protected override void When()
        {
            repositoryTracker.Handle(new UpdateFound{RepoUrl = "http://fle", Revision = "12"});
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Send(Arg.Is<CheckForUpdates>(updates => updates.RepoUrl == "http://fle" && updates.Revision == "12"));
        }
    }
}