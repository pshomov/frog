using System;
using System.IO;
using SimpleCQRS;

namespace Frog.Domain.EventsArchiver
{
    public class EventsArchiver
    {
        private readonly IBus bus;

        public EventsArchiver(IBus bus)
        {
            this.bus = bus;
        }

        private void Handle(string message, string exchange)
        {
            File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "runz_events.log"), string.Format("{1} - {0}\n", message, exchange));
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler(this.Handle, "all_messages");
        }
    }
}
