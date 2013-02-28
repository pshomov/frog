using System;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain
{
    public abstract class OrderedEvent : Event
    {
        public int SequenceId;

        protected OrderedEvent(int sequenceId)
        {
            SequenceId = sequenceId;
        }
    }

    public abstract class BuildEvent : OrderedEvent
    {
        public Guid BuildId { get; set; }

        protected BuildEvent() : this(Guid.Empty, -1)
        {
        }

        protected BuildEvent(Guid buildId, int sequenceId) : base(sequenceId)
        {
            BuildId = buildId;
        }
    }

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


    public class BuildStarted : BuildEvent
    {
        public string RepoUrl { get; set; }
        public PipelineStatus Status { get; set; }

        public BuildStarted()
        {
        }

        public BuildStarted(Guid buildId, PipelineStatus status, string repoUrl, int sequenceId)
            : base(buildId, sequenceId)
        {
            Status = status;
            RepoUrl = repoUrl;
        }
    }

    public class BuildUpdated : BuildEvent
    {
        public int TaskIndex { get; set; }

        public TaskInfo.TaskStatus TaskStatus { get; set; }

        public Guid TerminalId { get;  set; }
        public string RepoURL { get;  set; }

        public BuildUpdated()
        {
        }

        public BuildUpdated(Guid buildId, string repoUrl, int taskIndex, TaskInfo.TaskStatus newStatus, int sequenceId, Guid terminalId)
            : base(buildId, sequenceId)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
            TerminalId = terminalId;
            RepoURL = repoUrl;
        }
    }

    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }

        public string RepoURL { get; set; }

        public BuildEnded()
        {
        }

        public BuildEnded(Guid buildId, string repoURL, BuildTotalEndStatus totalStatus, int sequenceId)
            : base(buildId, sequenceId)
        {
            TotalStatus = totalStatus;
            RepoURL = repoURL;
        }
    }

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get; set; }

        public int ContentSequenceIndex { get; set; }
        public int TaskIndex { get; set; }

        public Guid TerminalId { get; set; }

        public string RepoURL { get; set; }

        public TerminalUpdate()
        {
        }

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, Guid buildId, string repoURL, int sequenceId,
                              Guid terminalId)
            : base(buildId, sequenceId)
        {
            Content = content;
            TaskIndex = taskIndex;
            ContentSequenceIndex = contentSequenceIndex;
            TerminalId = terminalId;
            RepoURL = repoURL;
        }
    }


    public class Agent : Handles<Build>
    {
        public Agent(IBus theBus, Worker worker, SourceRepoDriverFactory repoDriverFactory)
        {
            this.theBus = theBus;
            this.worker = worker;
            this.repoDriverFactory = repoDriverFactory;
        }

        public void Handle(Build message)
        {
            NextEventId = 0;
            Action<BuildTotalEndStatus> onBuildEnded =
                started => theBus.Publish(new BuildEnded(message.Id, message.RepoUrl, started, NextEventId));
            BuildStartedDelegate onBuildStarted =
                started =>
                theBus.Publish(new BuildStarted(buildId: message.Id, status: started, repoUrl: message.RepoUrl,
                                                sequenceId: NextEventId));
            Action<int, Guid, TaskInfo.TaskStatus> onBuildUpdated =
                (i, terminalId, status) =>
                theBus.Publish(new BuildUpdated(buildId: message.Id, repoUrl:message.RepoUrl, taskIndex: i, newStatus: status,
                                                sequenceId: NextEventId, terminalId: terminalId));
            Action<TerminalUpdateInfo> onTerminalUpdates = info =>
                                                           theBus.Publish(new TerminalUpdate(content: info.Content,
                                                                                             repoURL: message.RepoUrl,
                                                                                             taskIndex: info.TaskIndex,
                                                                                             contentSequenceIndex:
                                                                                                 info.
                                                                                                 ContentSequenceIndex,
                                                                                             buildId: message.Id,
                                                                                             sequenceId:
                                                                                                 info.
                                                                                                 ContentSequenceIndex,
                                                                                             terminalId: info.TerminalId));
            ProjectCheckedOutDelegate onProjectCheckedOut =
                info =>
                theBus.Publish(new ProjectCheckedOut(buildId: message.Id, sequenceId: NextEventId)
                                   {CheckoutInfo = info, RepoUrl = message.RepoUrl});

            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            worker.OnProjectCheckedOut += onProjectCheckedOut;
            try
            {
                worker.ExecutePipelineForRevision(repoDriverFactory(message.RepoUrl), message.Revision.Revision);
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

        public void JoinTheParty()
        {
            theBus.RegisterHandler<Build>(Handle, "Agent");
        }

        readonly IBus theBus;
        readonly Worker worker;
        readonly SourceRepoDriverFactory repoDriverFactory;

        int nextEventId;

        private int NextEventId
        {
            get { return nextEventId++; }
            set { nextEventId = value; }
        }
    }
}