using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        readonly List<Message> messages;
        readonly IBus theBus;
        readonly WorkingAreaGoverner areaGoverner;
        Agent agent;
        Worker worker;

        public ConcurrentDictionary<string, BuildStatus> report { get; private set; }
        public TaskSource TasksSource;
        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runAgent = true)
        {
            theBus = SetupBus();

            areaGoverner = governer;
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            if (runAgent) SetupAgent(sourceRepoDriverFactory);

            report = Setup.SetupView(theBus);

            messages = new List<Message>();
            SetupAllEventLogging();
        }

        PipelineOfTasks GetPipeline()
        {
            {
                TasksSource = Substitute.For<TaskSource>();
                return new PipelineOfTasks(TasksSource,
                                           new ExecTaskGenerator(new ExecTaskFactory()));
            }
        }

        void SetupAllEventLogging()
        {
            var busDebug = (IBusDebug) theBus;
            busDebug.OnMessage += msg => messages.Add(msg);
        }

        public List<Message> GetMessagesSoFar()
        {
            return new List<Message>(messages);
        }

        public void CleanupTestSystem()
        {
            messages.Clear();
        }

        IBus SetupBus()
        {
            return new FakeBus();
        }

        void SetupWorker(PipelineOfTasks pipeline)
        {
            worker = new Worker(pipeline, areaGoverner);
        }

        void SetupAgent(SourceRepoDriverFactory sourceRepoDriverFactory)
        {
            agent = new Agent(theBus, worker, sourceRepoDriverFactory);
            agent.JoinTheParty();
        }

        void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(theBus, new InMemoryProjectsRepository());
            repositoryTracker.JoinTheMessageParty();
        }
    }

    public class SystemDriver
    {
        readonly TestSystem theTestSystem;
        public SourceRepoDriver SourceRepoDriver;

        public SystemDriver(TestSystem system)
        {
            theTestSystem = system;
        }

        public SystemDriver(bool runAgent = true)
        {
            SourceRepoDriver = Substitute.For<SourceRepoDriver>();
            theTestSystem = new TestSystem(Substitute.For<WorkingAreaGoverner>(), url => SourceRepoDriver, runAgent);
        }

        public List<Message> GetEventsSnapshot()
        {
            return theTestSystem.GetMessagesSoFar();
        }

        public void RegisterNewProject(string repoUrl)
        {
            theTestSystem.repositoryTracker.Handle(new RegisterRepository{Repo = repoUrl});
        }

        public void CheckProjectsForUpdates()
        {
            theTestSystem.repositoryTracker.CheckForUpdates();
        }

        public Dictionary<string, BuildStatus> GetView()
        {
            return new Dictionary<string, BuildStatus>(theTestSystem.report);
        }
    }
}