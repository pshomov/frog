using SimpleCQRS;

namespace Frog.Domain
{
    public class Agent : Handles<CheckForUpdates>
    {
        readonly IBus theBus;
        readonly IValve valve;

        public Agent(IBus theBus, IValve valve)
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
            valve.Check(new GitDriver(message.RepoUrl), message.Revision);
        }
    }
}