using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.Domain
{
    public class OrderedEvent : Event
    {
        public int SequenceId;

        protected OrderedEvent(int sequenceId)
        {
            SequenceId = sequenceId;
        }
    }

    public class BuildEvent : OrderedEvent
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

        public Guid TerminalId { get; private set; }

        public BuildUpdated()
        {
        }

        public BuildUpdated(Guid buildId, int taskIndex, TaskInfo.TaskStatus newStatus, int sequenceId, Guid terminalId)
            : base(buildId, sequenceId)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
            TerminalId = terminalId;
        }
    }

    public class BuildEnded : BuildEvent
    {
        public BuildTotalEndStatus TotalStatus { get; set; }

        public BuildEnded()
        {
        }

        public BuildEnded(Guid buildId, BuildTotalEndStatus totalStatus, int sequenceId)
            : base(buildId, sequenceId)
        {
            TotalStatus = totalStatus;
        }
    }

    public class TerminalUpdate : BuildEvent
    {
        public string Content { get; set; }

        public int ContentSequenceIndex { get; set; }
        public int TaskIndex { get; set; }

        public Guid TerminalId { get; set; }

        public TerminalUpdate()
        {
        }

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, Guid buildId, int sequenceId,
                              Guid terminalId)
            : base(buildId, sequenceId)
        {
            Content = content;
            TaskIndex = taskIndex;
            ContentSequenceIndex = contentSequenceIndex;
            TerminalId = terminalId;
        }
    }


    public class Agent : Handles<Build>
    {
        int BatchPeriod = 3000;

        public Agent(IBus theBus, Worker worker, SourceRepoDriverFactory repoDriverFactory, int batchPeriod)
        {
            this.theBus = theBus;
            this.worker = worker;
            this.repoDriverFactory = repoDriverFactory;
            this.BatchPeriod = batchPeriod;
        }

        public void Handle(Build message)
        {
            var terminalOutputsBatch = new ConcurrentQueue<BuildEvent>(); 
            NextEventId = 0;
            Action<BuildTotalEndStatus> onBuildEnded =
                started => terminalOutputsBatch.Enqueue(new BuildEnded(message.Id, started, NextEventId));
            BuildStartedDelegate onBuildStarted =
                started =>
                terminalOutputsBatch.Enqueue(new BuildStarted(buildId: message.Id, status: started, repoUrl: message.RepoUrl,
                                                sequenceId: NextEventId));
            Action<int, Guid, TaskInfo.TaskStatus> onBuildUpdated =
                (i, terminalId, status) =>
                terminalOutputsBatch.Enqueue(new BuildUpdated(buildId: message.Id, taskIndex: i, newStatus: status,
                                                sequenceId: NextEventId, terminalId: terminalId));
            Action<TerminalUpdateInfo> onTerminalUpdates = info => 
                                                           terminalOutputsBatch.Enqueue(new TerminalUpdate(content: info.Content,
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
                terminalOutputsBatch.Enqueue(new ProjectCheckedOut(buildId: message.Id, sequenceId: NextEventId)
                                   {CheckoutInfo = info, RepoUrl = message.RepoUrl});

            worker.OnTerminalUpdates += onTerminalUpdates;
            worker.OnBuildStarted += onBuildStarted;
            worker.OnBuildUpdated += onBuildUpdated;
            worker.OnBuildEnded += onBuildEnded;
            worker.OnProjectCheckedOut += onProjectCheckedOut;
            var t = new Timer(state => PublishPendingEventBatch(terminalOutputsBatch), null, Timeout.Infinite, BatchPeriod);
            try
            {
                t.Change(0, BatchPeriod);
                worker.ExecutePipelineForRevision(repoDriverFactory(message.RepoUrl), message.Revision.Revision);
            }
            finally
            {
                worker.OnBuildEnded -= onBuildEnded;
                worker.OnBuildStarted -= onBuildStarted;
                worker.OnBuildUpdated -= onBuildUpdated;
                worker.OnTerminalUpdates -= onTerminalUpdates;
                worker.OnProjectCheckedOut -= onProjectCheckedOut;
                var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                t.Dispose(eventWaitHandle);
                eventWaitHandle.WaitOne();
                PublishPendingEventBatch(terminalOutputsBatch);
            }
        }

        void PublishPendingEventBatch(ConcurrentQueue<BuildEvent> terminalOutputsBatch)
        {
            BuildEvent newItem;
            var itemsToSend = new List<BuildEvent>();
            while (terminalOutputsBatch.TryDequeue(out newItem)) itemsToSend.Add(newItem);
            theBus.Publish(itemsToSend.ToArray());
        }

        public void JoinTheParty()
        {
            theBus.RegisterHandler<Build>(Handle, "Agent");
        }

        readonly IBus theBus;
        readonly Worker worker;
        readonly SourceRepoDriverFactory repoDriverFactory;

        int nextEventId;

        protected int NextEventId
        {
            get { return nextEventId++; }
            private set { nextEventId = value; }
        }
    }
}