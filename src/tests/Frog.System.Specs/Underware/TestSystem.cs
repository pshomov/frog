using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.RevisionChecker;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
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
        public RepositoryTracker repositoryTracker { get; private set; }

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runRevisionChecker = true)
        {
            TheBus = SetupBus();

            areaGoverner = governer;
            SetupWorker(GetPipeline());
            SetupRepositoryTracker();
            if (runRevisionChecker) new RevisionChecker(TheBus, sourceRepoDriverFactory).JoinTheParty();
            SetupAgent(sourceRepoDriverFactory);

            Views = new InMemProjectView();
            Setup.SetupView(TheBus, null);

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

        public ProjectView GetView()
        {
            return theTestSystem.Views;
        }

        public void Build(string repoUrl, RevisionInfo revision, Guid buildId)
        {
            theTestSystem.TheBus.Send(new Build {RepoUrl = repoUrl, Revision = revision, Id = buildId});
        }
    }
}