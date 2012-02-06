using EventStore;
using EventStore.Serialization;

namespace Frog.Specs.Support
{
    public static class StoreFactory
    {
        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                
                .UsingMongoPersistence("EventStore",new DocumentObjectSerializer() )
                .InitializeStorageEngine()
                .Build();
        }
    }
}
