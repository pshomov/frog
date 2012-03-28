using EventStore;
using EventStore.Serialization;

namespace Frog.Specs.Support
{
    public static class StoreFactory
    {
        public static IStoreEvents WireupEventStore()
        {
            var eventStore = Wireup.Init()
                .UsingInMemoryPersistence()
                .InitializeStorageEngine()
                .Build();

            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();

            return eventStore;
        }
    }
}
