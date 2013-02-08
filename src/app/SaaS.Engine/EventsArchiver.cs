using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using Newtonsoft.Json;
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
        }

        public void JoinTheParty()
        {
            bus.RegisterHandler(Handle, "all_messages");
        }

        readonly IBus bus;
        readonly EventStore store;
        Type[] eventsToTranslate = new[] { typeof(RepositoryRegistered), typeof(Frog.Domain.BuildStarted), typeof(Frog.Domain.BuildEnded), typeof(Frog.Domain.BuildUpdated), typeof(Frog.Domain.TerminalUpdate) };

        void Handle(string message, string exchange)
        {
            var types = eventsToTranslate.Where(type => type.Name.Equals(exchange, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (types.Count == 1)
            {
                var targetType = types.First();
                var msg = JsonConvert.DeserializeObject(message, targetType);
                if (targetType == typeof (Frog.Domain.BuildStarted))
                {
                    var ev = (Frog.Domain.BuildStarted) msg;
                    var conv = new BuildStarted(new BuildId(ev.BuildId), ev.RepoUrl,
                                                new PipelineStatus(
                                                    ev.Status.Tasks.Select(
                                                        info =>
                                                        new TaskInfo(new TerminalId(info.TerminalId), info.Name,
                                                                     (TaskStatus) info.Status)).ToArray()));
                    store.AppendEventsToStream(conv.Id, ev.SequenceId-1, new[]{conv});

                } else
                if (targetType == typeof (RepositoryRegistered))
                {
                    var ev = (RepositoryRegistered) msg;
                    var conv = new ProjectRegistered(new ProjectId(ev.RepoUrl), ev.RepoUrl );
                    store.AppendEventsToStream(conv.Id, -1, new[]{conv});
                }
//                store.AppendEventsToStream();

            }
//            store.AppendEventsToStream();
        }
    }
}