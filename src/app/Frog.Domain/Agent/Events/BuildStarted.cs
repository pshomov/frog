using System;
using Frog.Domain.Underware;

namespace Frog.Domain
{
    public class BuildStarted : BuildEvent
    {
        public Guid AgentId { get; set; }
        public string RepoUrl { get; set; }
        public PipelineStatus Status { get; set; }

        public BuildStarted()
        {
        }

        public BuildStarted(Guid buildId, PipelineStatus status, string repoUrl, int sequenceId, Guid agentId)
            : base(buildId, sequenceId)
        {
            Status = status;
            RepoUrl = repoUrl;
            AgentId = agentId;
        }
    }
}