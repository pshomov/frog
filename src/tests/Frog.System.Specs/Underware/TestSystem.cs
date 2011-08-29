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
    public abstract class TestSystemBase : SystemBase
    {
        protected TestSystemBase(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runAgent) : base(governer, sourceRepoDriverFactory, runAgent: runAgent)
        {
        }

        public abstract List<Message> GetMessagesSoFar();
    }

    public class TestSystem : TestSystemBase
    {
        readonly List<Message> messages;
        string workingAreaPath;
        public IExecTaskGenerator execTaskGenerator;
        public TaskSource tasksSource;

        public TestSystem(WorkingAreaGoverner governer, SourceRepoDriverFactory sourceRepoDriverFactory, bool runAgent = true) : base(governer, sourceRepoDriverFactory, runAgent)
        {
            messages = new List<Message>();
            SetupAllEventLogging();
        }

//        protected override WorkingAreaGoverner SetupWorkingAreaGovernor()
//        {
//            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
//            Directory.CreateDirectory(workingAreaPath);
//            return new SubfolderWorkingAreaGoverner(workingAreaPath);
//        }

        protected override PipelineOfTasks GetPipeline()
        {
            {
                execTaskGenerator = Substitute.For<IExecTaskGenerator>();
                tasksSource = Substitute.For<TaskSource>();
                return new PipelineOfTasks(tasksSource,
                                           new ExecTaskGenerator(new ExecTaskFactory()));
            }
        }

        void SetupAllEventLogging()
        {
            var busDebug = (IBusDebug) TheBus;
            busDebug.OnMessage += msg => messages.Add(msg);
        }

        public override List<Message> GetMessagesSoFar()
        {
            return new List<Message>(messages);
        }

        public void CleanupTestSystem()
        {
            messages.Clear();
            if (Directory.Exists(workingAreaPath))
            {
                OSHelpers.ClearAttributes(workingAreaPath);
                Directory.Delete(workingAreaPath, true);
            }
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
        readonly TestSystemBase theTestSystem;

        SystemDriver(TestSystemBase system)
        {
            theTestSystem = system;
        }

        public static SystemDriver GetCleanSystem(Func<TestSystemBase> factory)
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