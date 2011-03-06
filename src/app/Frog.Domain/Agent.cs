using SimpleCQRS;

namespace Frog.Domain
{
    public class Agent : Handles<CheckForUpdates>
    {
        readonly FakeBus theBus;
        readonly IValve valve;

        public Agent(FakeBus theBus, IValve valve)
        {
            this.theBus = theBus;
            this.valve = valve;
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<CheckForUpdates>(Handle);
        }

        public void Handle(CheckForUpdates message)
        {
            valve.Check(message.RepoUrl, message.Revision);
        }
    }
}