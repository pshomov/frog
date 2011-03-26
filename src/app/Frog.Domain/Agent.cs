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
            }
            else
                RepoUrl = repoUrl;
        }
    }

    public class BuildStarted : BuildEvent
    {
        public readonly PipelineStatus Status;

        public BuildStarted(string repoUrl, PipelineStatus status) : base(repoUrl)
        {
            Status = status;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public readonly PipelineStatus Status;

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

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get; private set; }

        public int TaskIndex { get; private set; }

        public int ContentSequenceIndex { get; private set; }

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, string repoUrl)
            : base(repoUrl)
        {
            Content = content;
            TaskIndex = taskIndex;
            ContentSequenceIndex = contentSequenceIndex;
        }
    }


    public class Agent : Handles<CheckForUpdates>
    {
        readonly IBus theBus;
        readonly Worker worker;

        public Agent(IBus theBus, Worker worker)
        {
            this.theBus = theBus;
            this.worker = worker;
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<CheckForUpdates>(Handle);
        }

        public void Handle(CheckForUpdates message)
        {
            Action<string> onUpdateFound =
                s => theBus.Publish(new UpdateFound {RepoUrl = message.RepoUrl, Revision = s});
            Action<BuildTotalStatus> onBuildEnded = started => theBus.Publish(new BuildEnded(message.RepoUrl, started));
            Action<PipelineStatus> onBuildStarted =
                started => theBus.Publish(new BuildStarted(status: started, repoUrl: message.RepoUrl));
            Action<PipelineStatus> onBuildUpdated =
                started => theBus.Publish(new BuildUpdated(status: started, repoUrl: message.RepoUrl));
            worker.OnUpdateFound += onUpdateFound;
            Action<TerminalUpdateInfo> onTerminalUpdates = info => 
                                                         theBus.Publish(new TerminalUpdate(repoUrl: message.RepoUrl,
                                                                                           content: info.Content, taskIndex: info.TaskIndex,
                                                                                           contentSequenceIndex: info.ContentSequenceIndex));
            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            worker.CheckForUpdatesAndKickOffPipeline(new GitDriver(message.RepoUrl), message.Revision);
            worker.OnBuildEnded -= onBuildEnded;
            worker.OnBuildStarted -= onBuildStarted;
            worker.OnBuildUpdated -= onBuildUpdated;
            worker.OnUpdateFound -= onUpdateFound;
            worker.OnTerminalUpdates -= onTerminalUpdates;
        }
    }
}