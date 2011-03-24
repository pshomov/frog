using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public abstract class TestSystemBase : SystemBase
    {
        public abstract List<Message> GetMessagesSoFar();
    }

    public class TestSystem : TestSystemBase
    {
        readonly List<Message> messages;
        string workingAreaPath;

        public TestSystem()
        {
            messages = new List<Message>();
            SetupAllEventLogging();
        }

        protected override WorkingAreaGoverner SetupWorkingAreaGovernor()
        {
            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingAreaGoverner(workingAreaPath);
        }

        void SetupAllEventLogging()
        {
            IBusDebug busDebug = (IBusDebug) TheBus;
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

        protected override ExecTaskFactory GetExecTaskFactory()
        {
            ExecTask execTask = GetExecTask();
            var execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskFactory.CreateTask(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(execTask);
            return execTaskFactory;
        }

        protected virtual ExecTask GetExecTask()
        {
            var execTask = Substitute.For<ExecTask>("", "", "");
            execTask.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
            return execTask;
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

    public class SystemDriver<TSystem> where  TSystem : TestSystemBase, new()
    {
        readonly TSystem theTestSystem;

        SystemDriver()
        {
            theTestSystem = new TSystem();
        }

        public static SystemDriver<TSystem> GetCleanSystem()
        {
            return new SystemDriver<TSystem>();
        }

        public List<Message> GetEventsSnapshot()
        {
            return theTestSystem.GetMessagesSoFar();
        }

        public void RegisterNewProject(string repoUrl)
        {
            theTestSystem.repositoryTracker.Track(repoUrl);
        }

        public void CheckProjectsForUpdates()
        {
            theTestSystem.repositoryTracker.CheckForUpdates();
        }
    }
}