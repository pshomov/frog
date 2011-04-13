using System.Collections.Concurrent;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.Domain
{
    public abstract class SystemBase
    {
        protected readonly IBus TheBus;
        readonly WorkingAreaGoverner areaGoverner;
        public ConcurrentDictionary<string, BuildStatus> report { get; private set; }
        public RepositoryTracker repositoryTracker { get; private set; }
        Agent agent;
        Worker worker;

        protected SystemBase()
        {
            TheBus = SetupBus();

            areaGoverner = SetupWorkingAreaGovernor();
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            SetupAgent();

            SetupView();
        }

        protected virtual IBus SetupBus()
        {
            return new FakeBus();
        }

        protected void SetupWorker(PipelineOfTasks pipeline)
        {
            worker = new Worker(pipeline, areaGoverner);
        }

        protected void SetupAgent()
        {
            agent = new Agent(TheBus, worker);
            agent.JoinTheParty();
        }

        protected void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(TheBus);
            repositoryTracker.StartListeningForBuildUpdates();
        }

        protected abstract WorkingAreaGoverner SetupWorkingAreaGovernor();

        protected void SetupView()
        {
            report = new ConcurrentDictionary<string, BuildStatus>();
            var statusView = new PipelineStatusView(report);
            TheBus.RegisterHandler<BuildStarted>(statusView.Handle);
            TheBus.RegisterHandler<BuildEnded>(statusView.Handle);
            TheBus.RegisterHandler<BuildUpdated>(statusView.Handle);
            TheBus.RegisterHandler<TerminalUpdate>(statusView.Handle);
        }

        protected abstract PipelineOfTasks GetPipeline();
    }
}