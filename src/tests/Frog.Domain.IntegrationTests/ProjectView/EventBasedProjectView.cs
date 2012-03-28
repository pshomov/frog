using EventStore;
using EventStore.Serialization;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture, Ignore]
    public class EventBasedProjectView : ProjectViewSpecs
    {
        protected override UI.ProjectView GetProjectView()
        {
            var eventStore = WireupEventStore();
            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();
            return new UI.EventBasedProjectView(eventStore);
        }

        private IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                .LogToOutputWindow()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .Build();
        }
    }
}