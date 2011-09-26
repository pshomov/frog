﻿using System;
using Frog.Specs.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.Domain.Specs.Agent
{
    public abstract class AgentSpecsBase : BDD
    {
        protected IBus Bus;
        protected Domain.Agent Agent;
        protected Worker Worker;
        protected SourceRepoDriver Repo;

        protected override void Given()
        {
            Bus = Substitute.For<IBus>();
            Repo = Substitute.For<SourceRepoDriver>();
            Worker = Substitute.For<Worker>(null, null);
            Worker.When(
                iValve => iValve.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Any<string>())).Do(
                    info =>
                        {
                            Worker.OnProjectCheckedOut +=
                                Raise.Event<ProjectCheckedOutDelegate>(new CheckoutInfo {Comment = "committed", Revision = "2"});
                            Worker.OnBuildStarted +=
                                Raise.Event<BuildStartedDelegate>(new PipelineStatus());
                            Worker.OnTerminalUpdates += Raise.Event<Action<TerminalUpdateInfo>>(new TerminalUpdateInfo(content: "content", taskIndex: 1, contentSequenceIndex: 1));
                            Worker.OnBuildUpdated +=
                                Raise.Event<Action<int, TaskInfo.TaskStatus>>(0, TaskInfo.TaskStatus.Started);
                            Worker.OnBuildEnded += Raise.Event<Action<BuildTotalEndStatus>>(BuildTotalEndStatus.Success);
                        });
            Agent = new Domain.Agent(Bus, Worker, url => Repo);
            Agent.JoinTheParty();
        }
    }
}