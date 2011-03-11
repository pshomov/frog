using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using Frog.Specs.Support;
using NSubstitute;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class TestSystem
    {
        IBus theBus;
        string workingAreaPath;
        SubfolderWorkingArea area;
        PipelineStatusView.BuildStatus report;
        List<Event> events;

        public TestSystem()
        {
            events = new List<Event>();
            theBus = new FakeBus();

            SetupWorkingArea();
            SetupValve(GetPipeline());
            SetupRepositoryTracker();
            SetupAgent();

            SetupView();
            SetupAllEventLogging();
        }


        void SetupValve(PipelineOfTasks pipeline)
        {
            valve = new Valve(pipeline, area);
        }

        void SetupAgent()
        {
            agent = new Agent(theBus, valve);
            agent.JoinTheParty();
        }

        void SetupRepositoryTracker()
        {
            repositoryTracker = new RepositoryTracker(theBus);
            repositoryTracker.StartListeningForBuildUpdates();
        }

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
        }

        PipelineOfTasks GetPipeline()
        {
            var execTask = Substitute.For<ExecTask>("", "", "");
            execTask.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));

            var execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskFactory.CreateTask(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(execTask);

            var fileFinder = new DefaultFileFinder(new PathFinder());
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new MSBuildDetector(fileFinder),
                                           new NUnitTaskDetctor(fileFinder)
                                           ),
                                       new ExecTaskGenerator(execTaskFactory));
        }

        public RepositoryTracker repositoryTracker;

        Agent agent;

        Valve valve;

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

        SystemDriver()
        {
            theTestSystem = new TestSystem();
        }

        public static SystemDriver GetCleanSystem()
        {
            return new SystemDriver();
        }

        public List<Event> GetEventsSnapshot()
        {
            return theTestSystem.GetEventsSoFar();
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