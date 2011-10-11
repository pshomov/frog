using System;
using System.IO;
using System.Web.Script.Serialization;
using SimpleCQRS;

namespace Frog.Domain.EventsArchiver
{
    public class EventsArchiver : Handles<Message>
    {
        private readonly IBus bus;
        readonly JavaScriptSerializer jsonBridge = new JavaScriptSerializer();

        public EventsArchiver(IBus bus)
        {
            this.bus = bus;
        }

        public void Handle(Message message)
        {
            File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "runz_events.log"), jsonBridge.Serialize(message));
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler<Message>(this.Handle, "all_messages");
        }
    }
}
