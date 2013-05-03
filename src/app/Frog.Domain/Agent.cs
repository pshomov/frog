using System;
using System.Collections.Generic;
using SimpleCQRS;

namespace Frog.Domain
{
    public class Agent : Handles<Build>
    {
        public Agent(IBus theBus, Worker worker, SourceRepoDriverFactory repoDriverFactory, string[] capabilities, Guid agentId)
        {
            this.theBus = theBus;
            this.worker = worker;
            this.repoDriverFactory = repoDriverFactory;
            this.capabilities = capabilities;
            this.agentId = agentId;
        }

        public void Handle(Build message)
        {
            NextEventId = 0;
            Action<BuildTotalEndStatus> onBuildEnded =
                started => theBus.Publish(new BuildEnded(message.Id, message.RepoUrl, started, NextEventId));
            Pipeline.BuildStartedDelegate onBuildStarted =
                started =>
                theBus.Publish(new BuildStarted(buildId: message.Id, status: started, repoUrl: message.RepoUrl,
                                                sequenceId: NextEventId, agentId : agentId));
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
            Pipeline.ProjectCheckedOutDelegate onProjectCheckedOut =
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
            theBus.Publish(new AgentJoined(){Capabilities = new List<string>(capabilities), AgentId = agentId});
            theBus.RegisterHandler<Build>(Handle, agentId.ToString());
        }

        readonly IBus theBus;
        readonly Worker worker;
        readonly SourceRepoDriverFactory repoDriverFactory;
        private readonly string[] capabilities;

        int nextEventId;
        private Guid agentId;

        private int NextEventId
        {
            get { return nextEventId++; }
            set { nextEventId = value; }
        }

        public void LeaveTheParty()
        {
            theBus.UnRegisterHandler<Build>(Handle, agentId.ToString());
        }
    }

    public class AgentJoined : Event
    {
        public List<string> Capabilities { get; set; }
        public Guid AgentId;
    }

}