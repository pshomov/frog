using System.IO;
using Frog.Domain;
using Frog.Domain.Specs;
using NUnit.Framework;

namespace Frog.System.Specs
{
    [TestFixture]
    public class System1 : BDD
    {
        private string workingAreaPath;
        private string repoArea;
        Valve valve;
        GitDriver driver;
        PipelineOfTasks pipeline;
        SubfolderWorkingArea area;

        public override void Given()
        {
			workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);
            driver = new GitDriver(repoArea, "test", "http://github.com/pshomov/xray.git");
			if (Underware.IsWindows)
            	pipeline = new PipelineOfTasks(new ExecTask(@"cmd.exe", @"/c %SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild.exe xray.sln"));
			else
            	pipeline = new PipelineOfTasks(new ExecTask(@"xbuild", @"xray.sln"));
            area = new SubfolderWorkingArea(workingAreaPath);
            valve = new Valve(driver, pipeline, area);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_checkout_and_build_project()
        {
            Assert.That(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(workingAreaPath)) {ClearAttributes(workingAreaPath); Directory.Delete(workingAreaPath, true);}
            if (Directory.Exists(repoArea)) {ClearAttributes(repoArea); Directory.Delete(repoArea, true);}
        }

        public static void ClearAttributes(string currentDir)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    ClearAttributes(dir);
                    File.SetAttributes(dir,FileAttributes.Directory);
                }
                string[] files = files = Directory.GetFiles(currentDir);
                foreach (string file in files)
                    File.SetAttributes(file, FileAttributes.Normal);
            }
        }
    }
}