using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildEvent : Event
    {
        public string RepoUrl;
    }

    public class BuildStarted : BuildEvent
    {
        public PipelineStatus Status;
    }

    public class BuildUpdated : BuildEvent
    {
        public PipelineStatus Status;
    }

    public class BuildEnded : BuildEvent
    {
        public readonly BuildTotalStatus TotalStatus;

        public BuildEnded(BuildTotalStatus totalStatus)
        {
            TotalStatus = totalStatus;
        }
    }

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
            Action<string> onUpdateFound = s => theBus.Publish(new UpdateFound {RepoUrl = message.RepoUrl, Revision = s});
            Action<BuildTotalStatus> onBuildEnded = started => theBus.Publish(new BuildEnded(started) {RepoUrl = message.RepoUrl});
            Action<PipelineStatus> onBuildStarted = started => theBus.Publish(new BuildStarted {Status = started, RepoUrl = message.RepoUrl});
            Action<PipelineStatus> onBuildUpdated = started => theBus.Publish(new BuildUpdated {Status = started, RepoUrl = message.RepoUrl});
            valve.OnUpdateFound += onUpdateFound;
            valve.OnBuildStarted += onBuildStarted;
            valve.OnBuildUpdated += onBuildUpdated;
            valve.OnBuildEnded += onBuildEnded;
            valve.Check(new GitDriver(message.RepoUrl), message.Revision);
            valve.OnBuildEnded -= onBuildEnded;
            valve.OnBuildStarted -= onBuildStarted;
            valve.OnBuildUpdated -= onBuildUpdated;
        }
    }
}