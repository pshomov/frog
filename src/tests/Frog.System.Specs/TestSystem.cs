using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Domain.UI;
using Frog.Specs.Support;
using SimpleCQRS;

namespace Frog.System.Specs
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

        public TestSystem()
        {
            events = new List<Event>();
            theBus = new FakeBus();
            string dummyRepo = SetupDummyRepo();

            SetupRepoClone(dummyRepo);
            SetupWorkingArea();

            SetupView();
            SetupAllEventLogging();
        }

        void SetupRepoClone(string dummyRepo)
        {
            repoArea = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            driver = new GitDriver(repoArea, "test", dummyRepo);
        }

        void SetupWorkingArea()
        {
            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            area = new SubfolderWorkingArea(workingAreaPath);
        }

        string SetupDummyRepo()
        {
            var original_repo = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(original_repo);
            return GitTestSupport.CreateDummyRepo(original_repo, "test_repo");
        }

        void SetupView()
        {
            report = new PipelineStatusView.BuildStatus();
            var statusView = new PipelineStatusView(report);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle);
            theBus.RegisterHandler<BuildEnded>(statusView.Handle);
            theBus.RegisterHandler<TaskStarted>(statusView.Handle);
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

        static readonly TestSystem theTestSystem = new TestSystem();

        public static TestSystem TheTestSystem
        {
            get { return theTestSystem; }
        }
    }

    public class SystemDriver
    {
        readonly TestSystem theTestSystem;

        SystemDriver()
        {
            theTestSystem = new TestSystem();
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

        public static SystemDriver GetCleanSystem()
        {
            return new SystemDriver();
        }

        public void ResetSystem()
        {
            theTestSystem.CleanupTestSystem();
        }

        public List<Event> GetEventsSnapshot()
        {
            return theTestSystem.GetEventsSoFar();
        }
    }
}