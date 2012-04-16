using System;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class TerminalOutputEventHandler : Handles<TerminalUpdate>
    {
        public TerminalOutputEventHandler(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        public void Handle(TerminalUpdate message)
        {
            using (var eventStream = GetEventStream(message.TerminalId))
            {
                eventStream.Add(new EventMessage {Body = message});
                eventStream.CommitChanges(Guid.NewGuid());
            }
        }

        readonly IStoreEvents eventStore;

        IEventStream GetEventStream(Guid id)
        {
            return eventStore.OpenStream(id, 0, Int32.MaxValue);
        }
    }
}