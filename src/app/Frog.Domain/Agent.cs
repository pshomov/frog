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

        public BuildEvent(Guid buildId)
        {
//            var privateRepo = new Regex(@"^(http://)(\w+):(\w+)@(github.com.*)$");
//            if (privateRepo.IsMatch(repoUrl))
//            {
//                var b = privateRepo.Match(repoUrl).Groups;
//                BuildId = b[1].Value + b[4].Value;
//            }
//            else
//                BuildId = repoUrl;
            BuildId = buildId;
        }
    }

    public class ProjectCheckedOut : BuildEvent
    {
        public string RepoUrl;
        public CheckoutInfo CheckoutInfo;
    }


    public class BuildStarted : BuildEvent
    {
        public PipelineStatus Status { get; set; }
        public string RepoUrl { get; set; }

        public BuildStarted(){}

        public BuildStarted(Guid buildId, PipelineStatus status, string repoUrl) : base(buildId)
        {
            Status = status;
            RepoUrl = repoUrl;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public int TaskIndex { get;  set; }

        public TaskInfo.TaskStatus TaskStatus { get; set; }
		
		public BuildUpdated(){}
		
        public BuildUpdated(Guid buildId, int taskIndex, TaskInfo.TaskStatus newStatus) : base(buildId)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
        }
    }

    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }
		
		public BuildEnded(){}
		
        public BuildEnded(Guid buildId, BuildTotalEndStatus totalStatus) : base(buildId)
        {
            TotalStatus = totalStatus;
        }
    }

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get;  set; }

        public int TaskIndex { get;  set; }

        public int ContentSequenceIndex { get;  set; }
		
		public TerminalUpdate(){}

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, Guid buildId)
            : base(buildId)
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
                started => theBus.Publish(new BuildStarted(buildId: message.Id, status: started, repoUrl: message.RepoUrl));
            Action<int, TaskInfo.TaskStatus> onBuildUpdated =
                (i, status) => theBus.Publish(new BuildUpdated(buildId: message.Id, taskIndex: i, newStatus: status));
            Action<TerminalUpdateInfo> onTerminalUpdates = info => 
                                                         theBus.Publish(new TerminalUpdate(buildId: message.Id,
                                                                                           content: info.Content, taskIndex: info.TaskIndex,
                                                                                           contentSequenceIndex: info.ContentSequenceIndex));
            ProjectCheckedOutDelegate onProjectCheckedOut =
                info => theBus.Publish(new ProjectCheckedOut{BuildId = message.Id, CheckoutInfo = info, RepoUrl = message.RepoUrl});

            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            worker.OnProjectCheckedOut += onProjectCheckedOut;
            try
            {
                worker.CheckForUpdatesAndKickOffPipeline(repoDriverFactory(message.RepoUrl), message.Revision.Revision);
            }
            finally
            {
                worker.OnBuildEnded -= onBuildEnded;
                worker.OnBuildStarted -= onBuildStarted;
                worker.OnBuildUpdated -= onBuildUpdated;
                worker.OnTerminalUpdates -= onTerminalUpdates;
                worker.OnProjectCheckedOut -= onProjectCheckedOut;
            }
        }
    }
}