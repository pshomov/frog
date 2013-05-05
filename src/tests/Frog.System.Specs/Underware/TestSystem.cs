using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.ProjectRepository;
using Frog.Domain.Integration.Projections;
using Frog.Support;
using Frog.WiredUp;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using NSubstitute;
using SimpleCQRS;
using EventStore = SaaS.Wires.EventStore;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        public TestSystem()
        {
            TheBus = SetupBus();
            messages = new List<Tuple<Guid, Message>>();
            SetupAllEventLogging();
            env = Setup.BuildEnvironment(true, string.Format(@"c:/lokad/system_tests/{0}", Guid.NewGuid()), Config.Env.connection_string, Guid.NewGuid());
            eventStore = env.Store;
        }

        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem WithRevisionChecker(SourceRepoDriverFactory sourceRepoDriverFactory)
        {
            new RevisionChecker(TheBus, sourceRepoDriverFactory).JoinTheParty();
            return this;
        }

        public TestSystem WithProjections()
        {
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
            busDebug.OnMessage += (id,msg) => messages.Add(new Tuple<Guid, Message>(id,msg));
        }

        public List<Tuple<Guid, Message>> GetMessagesSoFar()
        {
            return new List<Tuple<Guid,Message>>(messages);
        }

        public void CleanupTestSystem()
        {
            if (cts != null)
            {
                eventsArchiver.LeaveTheParty();
                agents.ForEach(agent => agent.LeaveTheParty());
                using (cts)
                {
                    cts.Cancel();
                    using (task)
                    {
                        if (!task.Wait(5000))
                        {
                            Console.WriteLine(@"Terminating");
                        }
                        using (env)
                        using (engine)
                        {
                            engine = null;
                            env = null;
                        }
                        task = null;
                    }
                    cts = null;
                }
            }
            messages.Clear();
        }

        IBus SetupBus()
        {
            return new FakeBus();
        }

        public TestSystem AddAgent(SourceRepoDriverFactory sourceRepoDriverFactory, WorkingAreaGoverner governer, Guid agentId, params string[] capabilities)
        {
            var worker = new Worker(GetPipeline(), governer);
            var agent = new Agent(TheBus, worker, sourceRepoDriverFactory, capabilities, agentId);
            agent.JoinTheParty();
            agents.Add(agent);
            
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
            var build_dispatcher = new BuildDispatcher(TheBus, new AgentStatuses(Store));
            build_dispatcher.JoinTheParty();
            return this;
        }

        public IDocumentStore Store;
        public TaskSource TasksSource;
        public IBus TheBus;
        List<Agent> agents = new List<Agent>();
        CancellationTokenSource cts;
        CqrsEngineHost engine;
        Container env;
        EventStore eventStore;
        EventsArchiver eventsArchiver;
        List<Tuple<Guid,Message>> messages;
        Task task;
    }

    public class SystemDriver
    {
        public SystemDriver(TestSystem system)
        {
            theTestSystem = system;
        }

        public SystemDriver()
        {
            theTestSystem = new TestSystem();
        }

        public List<Message> GetEventsSnapshot(Guid id)
        {
            return theTestSystem.GetMessagesSoFar().Where(tuple => id == Guid.Empty || tuple.Item1 == id).Select(tuple => tuple.Item2).ToList();
        }

        public List<Message> GetEventsSnapshot()
        {
            return theTestSystem.GetMessagesSoFar().Select(tuple => tuple.Item2).ToList();
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

        readonly TestSystem theTestSystem;
    }

}