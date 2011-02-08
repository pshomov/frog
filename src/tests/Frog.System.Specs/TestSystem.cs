using System.IO;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using Frog.UI.Web;
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

        public TestSystem()
        {
            theBus = new FakeBus();
            var original_repo = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(original_repo);
            string dummyRepo = GitTestSupport.CreateDummyRepo(original_repo, "test_repo");

            workingAreaPath = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            repoArea = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);
            driver = new GitDriver(repoArea, "test", dummyRepo);
            area = new SubfolderWorkingArea(workingAreaPath);

            report = new PipelineStatusView.BuildStatus();
            var statusView = new PipelineStatusView(report);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle);
            theBus.RegisterHandler<BuildEnded>(statusView.Handle);
            theBus.RegisterHandler<TaskStarted>(statusView.Handle);
        }

        public void CleanupTestSystem()
        {
            if (Directory.Exists(workingAreaPath)) { OSHelpers.ClearAttributes(workingAreaPath); Directory.Delete(workingAreaPath, true); }
            if (Directory.Exists(repoArea)) { OSHelpers.ClearAttributes(repoArea); Directory.Delete(repoArea, true); }            
        }
    
        static TestSystem theTestSystem = new TestSystem();

        public static TestSystem TheTestSystem { get { return theTestSystem; } }

    }

    public class SystemDriver
    {
        TestSystem theTestSystem;
        private SystemDriver()
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
    }
}