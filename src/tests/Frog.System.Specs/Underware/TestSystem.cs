using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class System : SystemBase
    {
        readonly List<Message> messages;
        string workingAreaPath;

        public System()
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

        public List<Message> GetMessagesSoFar()
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
            var execTask = Substitute.For<ExecTask>("", "", "");
            execTask.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));

            var execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskFactory.CreateTask(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(execTask);
            return execTaskFactory;
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
        readonly System theSystem;

        SystemDriver()
        {
            theSystem = new System();
        }

        public static SystemDriver GetCleanSystem()
        {
            return new SystemDriver();
        }

        public List<Message> GetEventsSnapshot()
        {
            return theSystem.GetMessagesSoFar();
        }

        public void RegisterNewProject(string repoUrl)
        {
            theSystem.repositoryTracker.Track(repoUrl);
        }

        public void CheckProjectsForUpdates()
        {
            theSystem.repositoryTracker.CheckForUpdates();
        }
    }
}