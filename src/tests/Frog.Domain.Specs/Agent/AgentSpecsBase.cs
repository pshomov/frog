using System;
using Frog.Specs.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.Domain.Specs.Agent
{
    public abstract class AgentSpecsBase : BDD
    {
        protected IBus bus;
        protected Domain.Agent agent;
        protected Worker worker;

        protected override void Given()
        {
            bus = Substitute.For<IBus>();
            worker = Substitute.For<Worker>(null, null);
            worker.When(
                iValve => iValve.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Any<string>())).Do(
                    info =>
                        {
                            worker.OnUpdateFound += Raise.Event<Action<string>>("new_rev");
                            worker.OnBuildStarted +=
                                Raise.Event<Action<PipelineStatus>>(new PipelineStatus(Guid.NewGuid()));
                            worker.OnBuildUpdated +=
                                Raise.Event<Action<PipelineStatus>>(new PipelineStatus(Guid.NewGuid()));
                            worker.OnBuildEnded += Raise.Event<Action<BuildTotalStatus>>(BuildTotalStatus.Success);
                        });
            agent = new Domain.Agent(bus, worker);
            agent.JoinTheParty();
        }
    }
}