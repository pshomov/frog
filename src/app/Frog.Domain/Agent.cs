using System;
using System.Text.RegularExpressions;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildEvent : Event
    {
        public string RepoUrl { get;  set; }

        public BuildEvent(){}

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
        public PipelineStatus Status { get; set; }

        public BuildStarted()
        {
        }

        public BuildStarted(string repoUrl, PipelineStatus status) : base(repoUrl)
        {
            Status = status;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public int TaskIndex { get;  set; }

        public TaskInfo.TaskStatus TaskStatus { get; set; }

        public BuildUpdated(string repoUrl, int taskIndex, TaskInfo.TaskStatus newStatus) : base(repoUrl)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
        }

        public BuildUpdated(){}
    }

    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }

        public BuildEnded(string repoUrl, BuildTotalEndStatus totalStatus) : base(repoUrl)
        {
            TotalStatus = totalStatus;
        }

        public BuildEnded(){}
    }

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get;  set; }

        public int TaskIndex { get;  set; }

        public int ContentSequenceIndex { get;  set; }

        public TerminalUpdate()
        {
        }

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
        private readonly SourceRepoDriverFactory repoDriverFactory;

        public Agent(IBus theBus, Worker worker, SourceRepoDriverFactory repoDriverFactory)
        {
            this.theBus = theBus;
            this.worker = worker;
            this.repoDriverFactory = repoDriverFactory;
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<CheckForUpdates>(Handle, "Agent");
        }

        public void Handle(CheckForUpdates message)
        {
            Action<string> onUpdateFound =
                s => theBus.Publish(new UpdateFound {RepoUrl = message.RepoUrl, Revision = s});
            Action<BuildTotalEndStatus> onBuildEnded = started => theBus.Publish(new BuildEnded(message.RepoUrl, started));
            BuildStartedDelegate onBuildStarted =
                started => theBus.Publish(new BuildStarted(status: started, repoUrl: message.RepoUrl));
            Action<int, TaskInfo.TaskStatus> onBuildUpdated =
                (i, status) => theBus.Publish(new BuildUpdated(repoUrl: message.RepoUrl, taskIndex: i, newStatus: status));
            Action<TerminalUpdateInfo> onTerminalUpdates = info => 
                                                         theBus.Publish(new TerminalUpdate(repoUrl: message.RepoUrl,
                                                                                           content: info.Content, taskIndex: info.TaskIndex,
                                                                                           contentSequenceIndex: info.ContentSequenceIndex));
            Action onCheckForUpdateFailed = () => theBus.Publish(new CheckForUpdateFailed(){repoUrl = message.RepoUrl});

            worker.OnUpdateFound += onUpdateFound;
            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            worker.OnCheckForUpdateFailed += onCheckForUpdateFailed;
            try
            {
                worker.CheckForUpdatesAndKickOffPipeline(repoDriverFactory(message.RepoUrl), message.Revision);
            }
            finally
            {
                worker.OnBuildEnded -= onBuildEnded;
                worker.OnBuildStarted -= onBuildStarted;
                worker.OnBuildUpdated -= onBuildUpdated;
                worker.OnUpdateFound -= onUpdateFound;
                worker.OnTerminalUpdates -= onTerminalUpdates;
                worker.OnCheckForUpdateFailed -= onCheckForUpdateFailed;
            }
        }
    }
}