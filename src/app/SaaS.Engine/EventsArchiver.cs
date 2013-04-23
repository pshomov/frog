using System;
using System.Linq;
using Frog.Domain;
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
            bus.RegisterAll(Handle);
        }

        readonly IBus bus;
        readonly EventStore store;
        readonly Type[] eventsToTranslate = new[] { typeof(Frog.Domain.ProjectCheckedOut), typeof(Frog.Domain.AgentJoined), typeof(RepositoryRegistered), typeof(Frog.Domain.BuildStarted), typeof(Frog.Domain.BuildEnded), typeof(Frog.Domain.BuildUpdated), typeof(TerminalUpdate) };

        void Handle(string message, string exchange)
        {
            var types = eventsToTranslate.Where(type => type.Name.Equals(exchange, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (types.Count == 1)
            {
                var targetType = types.First();
                var msg = JsonConvert.DeserializeObject(message, targetType);
                if (targetType == typeof(Frog.Domain.ProjectCheckedOut))
                {
                    var ev = (Frog.Domain.ProjectCheckedOut)msg;
                    var conv = new ProjectCheckedOut(new BuildId(ev.BuildId), new ProjectId(ev.RepoUrl), ev.RepoUrl, new CheckoutInfo(ev.CheckoutInfo.Comment, ev.CheckoutInfo.Revision));
                    store.AppendEventsToStream(conv.Id, ev.SequenceId, new[] { conv });
                }
                else
                    if (targetType == typeof(Frog.Domain.BuildStarted))
                    {
                        var ev = (Frog.Domain.BuildStarted)msg;
                        var conv = new BuildStarted(new BuildId(ev.BuildId), new ProjectId(ev.RepoUrl),
                                                    new PipelineStatus(
                                                        ev.Status.Tasks.Select(
                                                            info =>
                                                            new TaskInfo(new TerminalId(info.TerminalId), info.Name,
                                                                         (TaskStatus)info.Status)).ToArray()), new AgentId(ev.AgentId));
                        store.AppendEventsToStream(conv.Id, ev.SequenceId, new[] { conv });

                    }
                    else
                        if (targetType == typeof(Frog.Domain.BuildUpdated))
                        {
                            var ev = (Frog.Domain.BuildUpdated)msg;
                            var conv = new BuildUpdated(new BuildId(ev.BuildId), new ProjectId(ev.RepoURL), ev.TaskIndex, (TaskStatus)ev.TaskStatus);
                            store.AppendEventsToStream(conv.Id, ev.SequenceId, new[] { conv });

                        }
                        else
                            if (targetType == typeof(TerminalUpdate))
                            {
                                var ev = (TerminalUpdate)msg;
                                var conv = new TerminalUpdated(new TerminalId(ev.TerminalId), new BuildId(ev.BuildId), new ProjectId(ev.RepoURL), ev.Content, ev.ContentSequenceIndex);
                                store.AppendEventsToStream(conv.Id, ev.SequenceId, new[] { conv });

                            }
                            else
                                if (targetType == typeof(Frog.Domain.BuildEnded))
                                {
                                    var ev = (Frog.Domain.BuildEnded)msg;
                                    var conv = new BuildEnded(new BuildId(ev.BuildId), new ProjectId(ev.RepoURL), (BuildTotalEndStatus)ev.TotalStatus);
                                    store.AppendEventsToStream(conv.Id, ev.SequenceId, new[] { conv });

                                }
                                else
                                    if (targetType == typeof(RepositoryRegistered))
                                    {
                                        var ev = (RepositoryRegistered)msg;
                                        var conv = new ProjectRegistered(new ProjectId(ev.RepoUrl), ev.RepoUrl);
                                        store.AppendEventsToStream(conv.Id, 0, new[] { conv });
                                    }
                                    else
                                        if (targetType == typeof(Frog.Domain.AgentJoined))
                                        {
                                            var ev = (Frog.Domain.AgentJoined)msg;
                                            var conv = new SaaS.Engine.AgentJoined(new AgentId(ev.AgentId), ev.Capabilities.ToArray() );
                                            store.AppendEventsToStream(conv.Id, 0, new[] { conv });
                                        }
            }
        }

        public void LeaveTheParty()
        {
            this.bus.UnRegisterAll();
        }
    }
}