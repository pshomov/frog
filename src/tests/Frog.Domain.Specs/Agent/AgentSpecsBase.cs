using System;
using Frog.Specs.Support;
using Frog.Support;
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
        protected Guid TerminalId;

        protected override void Given()
        {
            Bus = Substitute.For<IBus>();
            Repo = Substitute.For<SourceRepoDriver>();
            Worker = Substitute.For<Worker>(null, null);
            Worker.When(
                iValve => iValve.ExecutePipelineForRevision(Arg.Any<SourceRepoDriver>(), Arg.Any<string>())).Do(
                    info =>
                        {
                            Worker.OnProjectCheckedOut +=
                                Raise.Event<Pipeline.ProjectCheckedOutDelegate>(new CheckoutInfo {Comment = "committed", Revision = "2"});
                            TerminalId = Guid.NewGuid();
                            Worker.OnBuildStarted +=
                                Raise.Event<Pipeline.BuildStartedDelegate>(new PipelineStatus(){Tasks = As.List(new TaskInfo("name", TerminalId))});
                            Worker.OnTerminalUpdates += Raise.Event<Action<TerminalUpdateInfo>>(new TerminalUpdateInfo(contentSequenceIndex: 0, content: "content", taskIndex: 1, terminalId: TerminalId));
                            Worker.OnBuildUpdated +=
                                Raise.Event<Action<int, Guid, TaskInfo.TaskStatus>>(0, TerminalId, TaskInfo.TaskStatus.Started);
                            Worker.OnBuildEnded += Raise.Event<Action<BuildTotalEndStatus>>(BuildTotalEndStatus.Success);
                        });
        }
    }
}