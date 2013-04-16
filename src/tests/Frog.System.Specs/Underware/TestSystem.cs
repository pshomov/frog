using System;
using System.Collections.Generic;
using System.Threading;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.ProjectRepository;
using Frog.Support;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using NSubstitute;
using SaaS.Engine;
using SimpleCQRS;
using EventStore = SaaS.Wires.EventStore;
using ExecutableTaskGenerator = Frog.Domain.Integration.ExecutableTaskGenerator;
using Task = System.Threading.Tasks.Task;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        List<Message> messages;
        public IBus TheBus;
        List<Agent> agent = new List<Agent>();

        public TaskSource TasksSource;
        Container env;
        CancellationTokenSource cts;
        CqrsEngineHost engine;
        Task task;
        EventsArchiver eventsArchiver;
        EventStore eventStore;
        public IDocumentStore Store;
        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem()
        {
            TheBus = SetupBus();
            messages = new List<Message>();
            SetupAllEventLogging();
        }

        public TestSystem WithRevisionChecker(SourceRepoDriverFactory sourceRepoDriverFactory)
        {
            new RevisionChecker(TheBus, sourceRepoDriverFactory).JoinTheParty();
            return this;
        }

        public TestSystem SetupProjections()
        {
            env = Program.BuildEnvironment(true, @"c:/lokad/system_tests", Config.Env.connection_string);
            eventStore = env.Store;
            cts = new CancellationTokenSource();
            env.ExecuteStartupTasks(cts.Token);
            engine = env.BuildEngine(cts.Token);
            task = engine.Start(cts.Token);
            Store = env.ViewDocs;
            eventsArchiver = new EventsArchiver(TheBus, eventStore);
            eventsArchiver.JoinTheParty();
            return this;
        }


        Pipeline GetPipeline()
        {
                TasksSource = Substitute.For<TaskSource>();
                return new Pipeline(TasksSource,
                                           new ExecutableTaskGenerator(new ExecTaskFactory()));
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
            if (cts != null)
            {
                cts.Cancel();
                if (!task.Wait(5000))
                {
                    Console.WriteLine(@"Terminating");
                }
                engine.Dispose();
                env.Dispose();
            }
            messages.Clear();
        }

        IBus SetupBus()
        {
            return new FakeBus();
        }

        public TestSystem SetupAgent(SourceRepoDriverFactory sourceRepoDriverFactory, WorkingAreaGoverner governer, params string[] capabilities)
        {
            var worker = new Worker(GetPipeline(), governer);
            var agent = new Agent(TheBus, worker, sourceRepoDriverFactory, capabilities);
            agent.JoinTheParty();
            
            return this;
        }

        public TestSystem WithRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(TheBus, new InMemoryProjectsRepository());
            repositoryTracker.JoinTheMessageParty();
            return this;
        }

        public TestSystem AddBuildDispatcher()
        {
            var build_dispatcher = new BuildDispatcher(TheBus);
            build_dispatcher.JoinTheParty();
            return this;
        }
    }

    public class SystemDriver
    {
        readonly TestSystem theTestSystem;

        public SystemDriver(TestSystem system)
        {
            theTestSystem = system;
        }

        public SystemDriver()
        {
            theTestSystem = new TestSystem();
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

        public void Build(string repoUrl, RevisionInfo revision, Guid buildId)
        {
            theTestSystem.TheBus.Send(new Build {RepoUrl = repoUrl, Revision = revision, Id = buildId});
        }

        public void Stop()
        {
            theTestSystem.CleanupTestSystem();
        }

        public TEntity GetView<TKey, TEntity>(TKey id)
        {
            var view = default(TEntity);
            var retry = 0;
            while (!theTestSystem.Store.GetReader<TKey, TEntity>().TryGet(id, out view) && retry < 10)
            {
                Thread.Sleep(1000);
                retry++;
            };
            if (view == null) throw new Exception(string.Format("Could not get entity of type {0} with id {1}", typeof(TEntity).Name, id));
            return view;
        }

        public void BuildRequest(string repoUrl, RevisionInfo revisionInfo, Guid buildId, params string[] capabilities)
        {
            theTestSystem.TheBus.Send(new BuildRequest { RepoUrl = repoUrl, Revision = revisionInfo, Id = buildId, CapabilitiesNeeded = capabilities });
        }
    }

}