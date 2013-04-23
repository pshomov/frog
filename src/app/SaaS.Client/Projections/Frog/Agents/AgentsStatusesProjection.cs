using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Frog.Agents
{

    [DataContract]
    public sealed class AgentStatus
    {
        [DataMember(Order = 1)]
        public AgentId Id { get; set; }
        [DataMember(Order = 2)]
        public List<string> Capabilities { get; set; }

        public AgentStatus()
        {
            Capabilities = new List<string>();
            Id = new AgentId(Guid.Empty);
        }
    }

    [DataContract]
    public sealed class AgentsStatus
    {
        [DataMember(Order = 1)]
        public List<AgentStatus> Available { get; set; }
        [DataMember(Order = 2)]
        public List<AgentStatus> Busy { get; set; }
        [DataMember(Order = 3)]
        public Dictionary<BuildId, AgentId> BuildId2AgentId { get; set; }


        public AgentsStatus()
        {
            Available = new List<AgentStatus>();
            Busy = new List<AgentStatus>();
            BuildId2AgentId = new Dictionary<BuildId, AgentId>();
        }

    }

    public class AgentsStatusesProjection
    {
        readonly IDocumentWriter<unit, AgentsStatus> writer;

        public AgentsStatusesProjection(IDocumentWriter<unit, AgentsStatus> writer)
        {
            this.writer = writer;
            writer.AddOrUpdate(unit.it, () => new AgentsStatus(), status => { });
        }

        public void When(AgentJoined e)
        {
            writer.AddOrUpdate(unit.it, () =>
                {
                    var agents_status = new AgentsStatus();
                    agents_status.Available.Add(new AgentStatus()
                        {
                            Id = e.Id,
                            Capabilities = new List<string>(e.Capabilities)
                        });
                    return agents_status;
                }, status => status.Available.Add(new AgentStatus()
                    {
                        Id = e.Id,
                        Capabilities = new List<string>(e.Capabilities)
                    }));
        }

        public void When(BuildStarted e)
        {
            writer.UpdateOrThrow(unit.it,status =>
                {
                    var agent_status = status.Available.First(agentStatus => agentStatus.Id == e.AgentId);
                    status.Busy.Add(agent_status);
                    status.Available.Remove(agent_status);
                    status.BuildId2AgentId[e.Id] = e.AgentId;
                });
        }

        public void When(BuildEnded e)
        {
            writer.UpdateOrThrow(unit.it, status =>
                {
                    var agent_status = status.Busy.First(agentStatus => agentStatus.Id == status.BuildId2AgentId[e.Id]);
                    status.Available.Add(agent_status);
                    status.Busy.Remove(agent_status);
                });
        }
    }
}