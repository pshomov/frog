using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventStore;
using EventStore.Serialization;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public class StoreFactory
    {
        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                .LogToOutputWindow()
                .UsingMongoPersistence("EventStore",new DocumentObjectSerializer() )
                .InitializeStorageEngine()
                .Build();
        }
    }
}
