using System.IO;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using SimpleCQRS;

namespace Frog.System.Specs
{
    public static class TestSystem
    {
        public static FakeBus theBus;
        static string workingAreaPath;
        static string repoArea;
        public static GitDriver driver;
        public static SubfolderWorkingArea area;

        public static void CleanTestSystem()
        {
            theBus = new FakeBus();
            var original_repo = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(original_repo);
            string dummyRepo = GitTestSupport.CreateDummyRepo(original_repo, "test_repo");

            workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);
            driver = new GitDriver(repoArea, "test", dummyRepo);
            area = new SubfolderWorkingArea(workingAreaPath);
        }

        public static void CleanupTestSystem()
        {
            if (Directory.Exists(workingAreaPath)) { OSHelpers.ClearAttributes(workingAreaPath); Directory.Delete(workingAreaPath, true); }
            if (Directory.Exists(repoArea)) { OSHelpers.ClearAttributes(repoArea); Directory.Delete(repoArea, true); }            
        }
    }
}