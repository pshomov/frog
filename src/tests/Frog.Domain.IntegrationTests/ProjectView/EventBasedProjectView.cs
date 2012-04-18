using EventStore;
using EventStore.Serialization;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture, Ignore]
    public class EventBasedProjectView : ProjectViewSpecs
    {
        protected override Integration.UI.ProjectView GetProjectView()
        {
            var eventStore = WireupEventStore();
            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();
            return new Integration.UI.EventBasedProjectView(eventStore);
        }

        private IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .Build();
        }
    }
}