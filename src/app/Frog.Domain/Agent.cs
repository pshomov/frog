using System;
using System.Text.RegularExpressions;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain
{
    public class BuildEvent : Event
    {
        public Guid BuildId { get;  set; }

        public BuildEvent(){}

        public BuildEvent(Guid repoUrl)
        {
//            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
//            if (privateRepo.IsMatch(repoUrl))
//            {
//                var b = privateRepo.Match(repoUrl).Groups;
//                BuildId = b[1].Value + b[4].Value;
//            }
//            else
//                BuildId = repoUrl;
            BuildId = repoUrl;
        }
    }

    public class BuildStarted : BuildEvent
    {
        public PipelineStatus Status { get; set; }

        public BuildStarted(Guid repoUrl, PipelineStatus status) : base(repoUrl)
        {
            Status = status;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public int TaskIndex { get;  set; }

        public TaskInfo.TaskStatus TaskStatus { get; set; }

        public BuildUpdated(Guid repoUrl, int taskIndex, TaskInfo.TaskStatus newStatus) : base(repoUrl)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
        }
    }

    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }

        public BuildEnded(Guid repoUrl, BuildTotalEndStatus totalStatus) : base(repoUrl)
        {
            TotalStatus = totalStatus;
        }
    }

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get;  set; }

        public int TaskIndex { get;  set; }

        public int ContentSequenceIndex { get;  set; }

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, Guid repoUrl)
            : base(repoUrl)
        {
            Content = content;
            TaskIndex = taskIndex;
            ContentSequenceIndex = contentSequenceIndex;
        }
    }


    public class Agent : Handles<Build>
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
            theBus.RegisterHandler<Build>(Handle, "Agent");
        }

        public void Handle(Build message)
        {
            Action<BuildTotalEndStatus> onBuildEnded = started => theBus.Publish(new BuildEnded(message.Id, started));
            BuildStartedDelegate onBuildStarted =
                started => theBus.Publish(new BuildStarted(status: started, repoUrl: message.Id));
            Action<int, TaskInfo.TaskStatus> onBuildUpdated =
                (i, status) => theBus.Publish(new BuildUpdated(repoUrl: message.Id, taskIndex: i, newStatus: status));
            Action<TerminalUpdateInfo> onTerminalUpdates = info => 
                                                         theBus.Publish(new TerminalUpdate(repoUrl: message.Id,
                                                                                           content: info.Content, taskIndex: info.TaskIndex,
                                                                                           contentSequenceIndex: info.ContentSequenceIndex));

            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            try
            {
                worker.CheckForUpdatesAndKickOffPipeline(repoDriverFactory(message.RepoUrl), message.Revision);
            }
            finally
            {
                worker.OnBuildEnded -= onBuildEnded;
                worker.OnBuildStarted -= onBuildStarted;
                worker.OnBuildUpdated -= onBuildUpdated;
                worker.OnTerminalUpdates -= onTerminalUpdates;
            }
        }
    }
}