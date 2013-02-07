using System;
using System.Linq;
using System.Web.Script.Serialization;
using Frog.Domain;
using SimpleCQRS;
using EventStore = SaaS.Wires.EventStore;

namespace SaaS.Engine
{
    public class EventsArchiver
    {
        public EventsArchiver(IBus bus, EventStore store)
        {
            this.bus = bus;
            this.store = store;
            jsonSerializer = new JavaScriptSerializer();
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler(Handle, "all_messages");
        }

        readonly IBus bus;
        readonly EventStore store;
        JavaScriptSerializer jsonSerializer;
        Type[] eventsToTranslate = new[] { typeof(Frog.Domain.BuildStarted), typeof(BuildEnded), typeof(BuildUpdated), typeof(TerminalUpdate) };

        void Handle(string message, string exchange)
        {
            var types = eventsToTranslate.Where(type => type.Name.Equals(exchange, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (types.Count == 1)
            {
                var msg = (BuildEvent)jsonSerializer.Deserialize(message, types.First());
//                store.AppendEventsToStream();

            }
//            store.AppendEventsToStream();
        }
    }
}