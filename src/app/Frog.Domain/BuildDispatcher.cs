using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildDispatcher : Handles<BuildRequest>
    {
        private IBus theBus;

        public BuildDispatcher(IBus theBus)
        {
            this.theBus = theBus;
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<BuildRequest>(Handle, "BuildDispatcher");
        }

        public void Handle(BuildRequest message)
        {
            theBus.Send(new Build{Id = message.Id, RepoUrl = message.RepoUrl, Revision = message.Revision});
        }
    }

    public class BuildRequest : Command
    {
        public string RepoUrl { get; set; }

        public RevisionInfo Revision { get; set; }

        public string[] CapabilitiesNeeded { get; set; }

        public Guid Id { get; set; }
    }

}
