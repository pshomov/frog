using System.Collections.Concurrent;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.Domain
{
    public abstract class SystemBase
    {
        protected readonly IBus TheBus;
        readonly WorkingAreaGoverner areaGoverner;
        public ConcurrentDictionary<string, PipelineStatusView.BuildStatus> report { get; private set; }
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

        protected virtual FakeBus SetupBus()
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
            report = new ConcurrentDictionary<string, PipelineStatusView.BuildStatus>();
            var statusView = new PipelineStatusView(report);
            TheBus.RegisterHandler<BuildStarted>(statusView.Handle);
            TheBus.RegisterHandler<BuildEnded>(statusView.Handle);
            TheBus.RegisterHandler<BuildUpdated>(statusView.Handle);
        }

        protected PipelineOfTasks GetPipeline()
        {
            ExecTaskFactory execTaskFactory = GetExecTaskFactory();

            var fileFinder = new DefaultFileFinder(new PathFinder());
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new MSBuildDetector(fileFinder),
                                           new NUnitTaskDetctor(fileFinder)
                                           ),
                                       new ExecTaskGenerator(execTaskFactory));
        }

        protected abstract ExecTaskFactory GetExecTaskFactory();
    }
}