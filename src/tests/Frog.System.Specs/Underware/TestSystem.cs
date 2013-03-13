using System;
using System.Collections.Generic;
using System.Threading;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.ProjectRepository;
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
        readonly List<Message> messages;
        public readonly IBus TheBus;
        readonly WorkingAreaGoverner areaGoverner;
        Agent agent;
        Worker worker;

        public TaskSource TasksSource;
        Container env;
        CancellationTokenSource cts;
        CqrsEngineHost engine;
        Task task;
        EventsArchiver eventsArchiver;
        EventStore eventStore;
        public IDocumentStore Store;
        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runRevisionChecker = true)
        {
            TheBus = SetupBus();
            messages = new List<Message>();
            SetupAllEventLogging();

            areaGoverner = governer;
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            if (runRevisionChecker) new RevisionChecker(TheBus, sourceRepoDriverFactory).JoinTheParty();
            SetupAgent(sourceRepoDriverFactory);
            SetupProjections(TheBus);
        }

        void SetupProjections(IBus bus)
        {
            env = Program.BuildEnvironment(true, @"c:/lokad/system_tests");
            eventStore = env.Store;
            cts = new CancellationTokenSource();
            env.ExecuteStartupTasks(cts.Token);
            engine = env.BuildEngine(cts.Token);
            task = engine.Start(cts.Token);
            Store = env.ViewDocs;
            eventsArchiver = new EventsArchiver(bus, eventStore);
            eventsArchiver.JoinTheParty();
        }


        Pipeline GetPipeline()
        {
            {
                TasksSource = Substitute.For<TaskSource>();
                return new Pipeline(TasksSource,
                                           new ExecutableTaskGenerator(new ExecTaskFactory()));
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
            cts.Cancel();
            if (!task.Wait(5000))
            {
                Console.WriteLine(@"Terminating");
            }
            engine.Dispose();
            env.Dispose();
            messages.Clear();
        }

        IBus SetupBus()
        {
            return new FakeBus();
        }

        void SetupWorker(Pipeline pipeline)
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
    }
}