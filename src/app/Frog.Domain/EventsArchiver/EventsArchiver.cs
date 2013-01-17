using System;
using System.IO;
using SimpleCQRS;

namespace Frog.Domain.EventsArchiver
{
    public class EventsArchiver
    {
        public EventsArchiver(IBus bus)
        {
            this.bus = bus;
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler(Handle, "all_messages");
        }

        readonly IBus bus;

        void Handle(string message, string exchange)
        {
            File.AppendAllText(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "runz_events.log"),
                string.Format("{1} - {0}\n", message, exchange));
        }
    }
}