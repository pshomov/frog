using System;
using EventStore;

namespace Frog.Domain.Integration.UI
{
    public class EventBasedTerminalOutputStore : TerminalOutputStore
    {
        readonly IStoreEvents event_store;

        public EventBasedTerminalOutputStore(IStoreEvents eventStore)
        {
            event_store = eventStore;
        }

        public void RegisterTerminalOutput(TerminalUpdate message)
        {
            using (var eventStream = GetEventStream(message.TerminalId))
            {
                eventStream.Add(new EventMessage {Body = message});
                eventStream.CommitChanges(Guid.NewGuid());
            }
        }
        IEventStream GetEventStream(Guid id)
        {
            return event_store.OpenStream(id, 0, Int32.MaxValue);
        }
    }
}