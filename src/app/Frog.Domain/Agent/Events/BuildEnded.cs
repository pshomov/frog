using System;
using Frog.Domain.Underware;

namespace Frog.Domain
{
    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }

        public string RepoURL { get; set; }

        public BuildEnded()
        {
        }

        public BuildEnded(Guid buildId, string repoURL, BuildTotalEndStatus totalStatus, int sequenceId)
            : base(buildId, sequenceId)
        {
            TotalStatus = totalStatus;
            RepoURL = repoURL;
        }
    }
}