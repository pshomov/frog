﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Domain.UI;
using Frog.Specs.Support;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        public FakeBus theBus;
        string workingAreaPath;
        string repoArea;
        public GitDriver driver;
        public SubfolderWorkingArea area;
        public PipelineStatusView.BuildStatus report;
        public List<Event> events;

        public TestSystem(PipelineOfTasks pipeline)
        {
            events = new List<Event>();
            theBus = new FakeBus();

            SetupValve(pipeline);
            SetupWorkingArea();
            SetupRepositoryTracker();
            SetupAgent();

            SetupView();
            SetupAllEventLogging();
        }

        void SetupValve(PipelineOfTasks pipeline)
        {
            driver = new GitDriver(null, null, null);
            valve = new Valve(driver, pipeline, area);
        }

        void SetupAgent()
        {
            agent = new Agent(theBus, valve);
            agent.JoinTheParty();
        }

        void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(theBus);
        }

//        public void SetupRepoClone(string dummyRepo)
//        {
//            repoArea = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
//            Directory.CreateDirectory(repoArea);
//            driver = new GitDriver(repoArea, "test", dummyRepo);
//        }

        void SetupWorkingArea()
        {
            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            area = new SubfolderWorkingArea(workingAreaPath);
        }

        void SetupView()
        {
            report = new PipelineStatusView.BuildStatus();
            var statusView = new PipelineStatusView(report);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle);
            theBus.RegisterHandler<BuildEnded>(statusView.Handle);
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle);
        }

        void SetupAllEventLogging()
        {
            var allmessages = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes()).Where(type => typeof (Event).IsAssignableFrom(type));
            var mthd = theBus.GetType().GetMethod("RegisterHandler");
            Action<Event> event_logger = @event => events.Add(@event);
            foreach (var msg in allmessages)
            {
                mthd.MakeGenericMethod(msg).Invoke(theBus, new object[] {event_logger});
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
            if (Directory.Exists(repoArea))
            {
                OSHelpers.ClearAttributes(repoArea);
                Directory.Delete(repoArea, true);
            }
        }

        public RepositoryTracker repositoryTracker;
        public Agent agent;
        public Valve valve;

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
            get {
                return repoPath;
            }
        }

        public static RepositoryDriver GetNewRepository()
        {
            return new RepositoryDriver(SetupDummyRepo());
        }

        public void CommitDirectoryTree(string treeRoot)
        {
            GitTestSupport.CommitChangeFiles(repoPath, treeRoot);
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
        readonly TestSystem theTestSystem;

        SystemDriver(PipelineOfTasks pipeline)
        {
            theTestSystem = new TestSystem(pipeline);
        }

        public FakeBus Bus
        {
            get { return theTestSystem.theBus; }
        }

        public SourceRepoDriver Git
        {
            get { return theTestSystem.driver; }
        }

        public WorkingArea WorkingArea
        {
            get { return theTestSystem.area; }
        }

        public PipelineStatusView.BuildStatus CurrentReport
        {
            get { return theTestSystem.report; }
        }

        public static SystemDriver GetCleanSystem(PipelineOfTasks pipeline)
        {
            return new SystemDriver(pipeline);
        }

        public void ResetSystem()
        {
            theTestSystem.CleanupTestSystem();
        }

        public List<Event> GetEventsSnapshot()
        {
            return theTestSystem.GetEventsSoFar();
        }

//        public void MonitorRepository(string repoUrl)
//        {
//            theTestSystem.SetupRepoClone(repoUrl);
//        }

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