using EventStore;
using EventStore.Serialization;

namespace Frog.Support
{
    public class Config
    {
        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
//                .LogToOutputWindow()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .Build();
        }

    }
}
