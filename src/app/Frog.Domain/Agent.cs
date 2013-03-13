using System;
using SimpleCQRS;

namespace Frog.Domain
{
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