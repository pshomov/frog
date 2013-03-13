using System;

namespace Frog.Domain
{
    public class BuildStarted : BuildEvent
    {
        public string RepoUrl { get; set; }
        public PipelineStatus Status { get; set; }

        public BuildStarted()
        {
        }

        public BuildStarted(Guid buildId, PipelineStatus status, string repoUrl, int sequenceId)
            : base(buildId, sequenceId)
        {
            Status = status;
            RepoUrl = repoUrl;
        }
    }
}