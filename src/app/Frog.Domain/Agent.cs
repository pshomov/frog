using System;
using System.Text.RegularExpressions;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildEvent : Event
    {
        public string RepoUrl { get; private set; }

        public BuildEvent(string repoUrl)
        {
            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
            if (privateRepo.IsMatch(repoUrl))
            {
                var b = privateRepo.Match(repoUrl).Groups;
                RepoUrl = b[1].Value + b[4].Value;
            } else
                RepoUrl = repoUrl;
        }
    }

    public class BuildStarted : BuildEvent
    {
        public PipelineStatus Status;

        public BuildStarted(string repoUrl, PipelineStatus status) : base(repoUrl)
        {
            Status = status;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public PipelineStatus Status;

        public BuildUpdated(string repoUrl, PipelineStatus status) : base(repoUrl)
        {
            Status = status;
        }
    }

    public class BuildEnded : BuildEvent
    {
        public readonly BuildTotalStatus TotalStatus;

        public BuildEnded(string repoUrl, BuildTotalStatus totalStatus) : base(repoUrl)
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
            Action<BuildTotalStatus> onBuildEnded = started => theBus.Publish(new BuildEnded(message.RepoUrl, started));
            Action<PipelineStatus> onBuildStarted = started => theBus.Publish(new BuildStarted(status : started, repoUrl : message.RepoUrl));
            Action<PipelineStatus> onBuildUpdated = started => theBus.Publish(new BuildUpdated(status : started, repoUrl : message.RepoUrl));
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