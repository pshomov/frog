using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EventStore;
using Frog.Domain;
using Frog.Domain.Integration.UI;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.RevisionChecker;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        readonly List<Message> messages;
        public readonly IBus TheBus;
        readonly WorkingAreaGoverner areaGoverner;
        Agent agent;
        Worker worker;

        public TaskSource TasksSource;
        public ProjectView Views;
        public IStoreEvents Store;
        public ViewForTerminalOutput TerminalStatusView;
        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runRevisionChecker = true)
        {
            TheBus = SetupBus();

            areaGoverner = governer;
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            if (runRevisionChecker) new RevisionChecker(TheBus, sourceRepoDriverFactory).JoinTheParty();
            SetupAgent(sourceRepoDriverFactory);
            Store = WireupEventStore();
            Store.Advanced.Purge();
            Views = new EventBasedProjectView(Store);
            TerminalStatusView = new TerminalOutputView(OSHelpers.TerminalViewConnection());
            Setup.SetupView(TheBus, Store);

            messages = new List<Message>();
            SetupAllEventLogging();
        }

        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                .UsingInMemoryPersistence()
                .InitializeStorageEngine()
                .Build();
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
            var busDebug = (IBusDebug) TheBus;
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
            agent = new Agent(TheBus, worker, sourceRepoDriverFactory);
            agent.JoinTheParty();
        }

        void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(TheBus, new InMemoryProjectsRepository());
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

        public SystemDriver(bool runRevisionChecker = true)
        {
            SourceRepoDriver = Substitute.For<SourceRepoDriver>();
            theTestSystem = new TestSystem(Substitute.For<WorkingAreaGoverner>(), url => SourceRepoDriver, runRevisionChecker);
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

        public ProjectView GetProjectStatusView()
        {
            return theTestSystem.Views;
        }

        public ViewForTerminalOutput GetTerminalStatusView()
        {
            return theTestSystem.TerminalStatusView;
        }

        public void Build(string repoUrl, RevisionInfo revision, Guid buildId)
        {
            theTestSystem.TheBus.Send(new Build {RepoUrl = repoUrl, Revision = revision, Id = buildId});
        }
    }
}