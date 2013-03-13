using System;

namespace Frog.Domain
{
    public class ProjectCheckedOut : BuildEvent
    {
        public CheckoutInfo CheckoutInfo;
        public string RepoUrl;

        public ProjectCheckedOut()
        {
        }

        public ProjectCheckedOut(Guid buildId, int sequenceId) : base(buildId, sequenceId)
        {
        }
    }
}