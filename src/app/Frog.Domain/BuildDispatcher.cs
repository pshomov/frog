using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildDispatcher : Handles<BuildRequest>
    {
        private readonly IBus theBus;
        private readonly BuildAgentStatus buildstatuses;

        public BuildDispatcher(IBus theBus, BuildAgentStatus buildstatuses)
        {
            this.theBus = theBus;
            this.buildstatuses = buildstatuses;
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<BuildRequest>(Handle, "BuildDispatcher");
        }

        public void Handle(BuildRequest message)
        {
            if (message.CapabilitiesNeeded.Length > 0)
            {
                var agent_info =
                    buildstatuses.AvailableAgents.FirstOrDefault(
                        agent => message.CapabilitiesNeeded.All(agent.Capabilities.Contains));
                if (agent_info != null)
                    theBus.SendDirect(new Build {Id = message.Id, RepoUrl = message.RepoUrl, Revision = message.Revision},
                                agent_info.Id);
            }
            else
            {
                var agent_info =
                    buildstatuses.AvailableAgents.FirstOrDefault();
                if (agent_info != null)
                    theBus.SendDirect(new Build { Id = message.Id, RepoUrl = message.RepoUrl, Revision = message.Revision},
                                agent_info.Id);
            }
        }
    }

    public class AgentInfo
    {
        public string Id;
        public List<string> Capabilities;
    }

    public interface BuildAgentStatus
    {
        IEnumerable<AgentInfo> AvailableAgents { get; }
    }

    public class BuildRequest : Command
    {
        public string RepoUrl { get; set; }

        public RevisionInfo Revision { get; set; }

        public string[] CapabilitiesNeeded { get; set; }

        public Guid Id { get; set; }
    }
}
