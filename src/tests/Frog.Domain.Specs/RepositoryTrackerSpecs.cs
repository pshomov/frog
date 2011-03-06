using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class RepositoryTrackerSpecs : BDD
    {
        RepositoryTracker repositoryTracker;
        IEventPublisher bus;

        public override void Given()
        {
            bus = Substitute.For<IEventPublisher>();
            repositoryTracker = new RepositoryTracker(bus);
        }

        public override void When()
        {
            repositoryTracker.Track("http://fle");
        }

        [Test]
        public void should_register_repository()
        {
            repositoryTracker.CheckForUpdates();
            bus.Received().Publish(Arg.Is<CheckForUpdates>(updates => updates.RepoUrl == "http://fle"));
        }

    }
}