using System.Collections.Concurrent;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.Domain
{
    public abstract class SystemBase
    {
        protected IBus theBus;
        readonly WorkingArea area;
        public ConcurrentDictionary<string, PipelineStatusView.BuildStatus> report { get; private set; }
        public RepositoryTracker repositoryTracker { get; private set; }
        Agent agent;
        Worker worker;

        protected SystemBase()
        {
            theBus = SetupBus();

            area = SetupWorkingArea();
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
            worker = new Worker(pipeline, area);
        }

        protected void SetupAgent()
        {
            agent = new Agent(theBus, worker);
            agent.JoinTheParty();
        }

        protected void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(theBus);
            repositoryTracker.StartListeningForBuildUpdates();
        }

        protected abstract WorkingArea SetupWorkingArea();

        protected void SetupView()
        {
            report = new ConcurrentDictionary<string, PipelineStatusView.BuildStatus>();
            var statusView = new PipelineStatusView(report);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle);
            theBus.RegisterHandler<BuildEnded>(statusView.Handle);
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle);
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