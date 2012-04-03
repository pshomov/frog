using System;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class TerminalOutputEventHandler : Handles<TerminalUpdate>
    {
        readonly IStoreEvents eventStore;

        public TerminalOutputEventHandler(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        IEventStream GetEventStream(Guid id)
        {
            return eventStore.OpenStream(id, Int32.MinValue, Int32.MaxValue);
        }

        public void Handle(TerminalUpdate message)
        {
            using(var eventStream = GetEventStream(message.TerminalId))
            {
                eventStream.Add(new EventMessage { Body = message });
                eventStream.CommitChanges(Guid.NewGuid());
            }
        }
    }
}