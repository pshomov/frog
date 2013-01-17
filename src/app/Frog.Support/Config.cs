using System;
using System.Linq;
using System.Reflection;
using EventStore;
using EventStore.Serialization;
using MongoDB.Bson.Serialization;
using SimpleCQRS;

namespace Frog.Support
{
    public class Config
    {
        public static IStoreEvents WireupEventStore()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(Event)));
            foreach (var t in types)
                BsonClassMap.LookupClassMap(t);
            return Wireup.Init()
//                .LogToOutputWindow()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .Build();
        }

    }
}
