using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using Frog.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        readonly List<Message> messages;
        public TaskSource TasksSource;
        protected IBus TheBus;
        private readonly WorkingAreaGoverner areaGoverner;
        private Agent agent;
        private Worker worker;

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runAgent = true)
        {
            TheBus = SetupBus();

            areaGoverner = governer;
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            if (runAgent) SetupAgent(sourceRepoDriverFactory);

            SetupView();

            messages = new List<Message>();
            SetupAllEventLogging();
        }

        public ConcurrentDictionary<string, BuildStatus> report { get; private set; }
        public RepositoryTracker repositoryTracker { get; private set; }

        protected  PipelineOfTasks GetPipeline()
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

        public  List<Message> GetMessagesSoFar()
        {
            return new List<Message>(messages);
        }

        public void CleanupTestSystem()
        {
            messages.Clear();
        }

        protected virtual IBus SetupBus()
        {
            return new FakeBus();
        }

        protected virtual void SetupWorker(PipelineOfTasks pipeline)
        {
            worker = new Worker(pipeline, areaGoverner);
        }

        protected virtual void SetupAgent(SourceRepoDriverFactory sourceRepoDriverFactory)
        {
            agent = new Agent(TheBus, worker, sourceRepoDriverFactory);
            agent.JoinTheParty();
        }

        protected virtual void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(TheBus, new InMemoryProjectsRepository());
            repositoryTracker.JoinTheMessageParty();
        }

        protected void SetupView()
        {
            report = new ConcurrentDictionary<string, BuildStatus>();
            var statusView = new PipelineStatusView(report);
            TheBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            TheBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            TheBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            TheBus.RegisterHandler<TerminalUpdate>(statusView.Handle, "UI");
        }
    }

    public class RepositoryDriver
    {
        readonly string repoPath;

        RepositoryDriver(string repoPath)
        {
            this.repoPath = repoPath;
        }

        public string Url
        {
            get { return repoPath; }
        }

        public static RepositoryDriver GetNewRepository()
        {
            return new RepositoryDriver(SetupDummyRepo());
        }

        static string SetupDummyRepo()
        {
            var originalRepo = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(originalRepo);
            return GitTestSupport.CreateDummyRepo(originalRepo, "test_repo");
        }

        public void Cleanup()
        {
            OSHelpers.ClearAttributes(repoPath);
            Directory.Delete(repoPath, true);
        }
    }

    public class SystemDriver
    {
        readonly TestSystem theTestSystem;

        SystemDriver(TestSystem system)
        {
            theTestSystem = system;
        }

        public static SystemDriver GetCleanSystem(Func<TestSystem> factory)
        {
            return new SystemDriver(factory());
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