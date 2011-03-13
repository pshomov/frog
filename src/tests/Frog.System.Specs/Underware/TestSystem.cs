using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class System : SystemBase
    {
        readonly List<Event> events;
        string workingAreaPath;

        public System()
        {
            events = new List<Event>();
            SetupAllEventLogging();
        }

        protected override WorkingArea SetupWorkingArea()
        {
            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingArea(workingAreaPath);
        }

        void SetupAllEventLogging()
        {
            var allmessages = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes()).Where(type => typeof (Event).IsAssignableFrom(type));
            var mthd = theBus.GetType().GetMethod("RegisterHandler");
            Action<Event> eventLogger = @event => events.Add(@event);
            foreach (var msg in allmessages)
            {
                mthd.MakeGenericMethod(msg).Invoke(theBus, new object[] {eventLogger});
            }
        }

        public List<Event> GetEventsSoFar()
        {
            return new List<Event>(events);
        }

        public void CleanupTestSystem()
        {
            events.Clear();
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
            var original_repo = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(original_repo);
            return GitTestSupport.CreateDummyRepo(original_repo, "test_repo");
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

        public List<Event> GetEventsSnapshot()
        {
            return theSystem.GetEventsSoFar();
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