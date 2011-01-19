using System;
using System.IO;
using System.Threading;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Support;
using NUnit.Framework;
using SimpleCQRS;
using xray;
using It = NHamcrest.Core;

namespace Frog.System.Specs
{
    [TestFixture]
    public class UpdatingBuildReportStatus : BDD
    {
        private string workingAreaPath;
        private string repoArea;
        Valve valve;
        GitDriver driver;
        PipelineOfTasks pipeline;
        SubfolderWorkingArea area;
        BuildStatus report;

        public override void Given()
        {
            var bus = new FakeBus();
            report = new BuildStatus();
            var statusView = new PipelineStatusView(report);
            bus.RegisterHandler<BuildStarted>(statusView.Handle);
            bus.RegisterHandler<BuildEnded>(statusView.Handle);

            var original_repo = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(original_repo);
            string dummyRepo = GitTestSupport.CreateDummyRepo(original_repo, "test_repo");

            workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);
            driver = new GitDriver(repoArea, "test", dummyRepo);
			if (Underware.IsWindows)
            	pipeline = new PipelineOfTasks(bus, new ExecTask(@"cmd.exe", @"/c %SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild.exe xray.sln"));
			else
            	pipeline = new PipelineOfTasks(bus, new ExecTask(@"xbuild", @"xray.sln"));
            area = new SubfolderWorkingArea(workingAreaPath);
            valve = new Valve(driver, pipeline, area);
        }

        public override void When()
        {
            ThreadPool.QueueUserWorkItem(state => valve.Check());
        }

        [Test]
        public void should_receive_build_started_event()
        {
            var prober = new PollingProber(3000, 100);
            Assert.True(prober.check(Take.Snapshot(() => report.Current).
                Has(x => x, It.Is.EqualTo(BuildStatus.Status.Started))
                ));
            Assert.True(prober.check(Take.Snapshot(() => report.Current).
                Has(x => x, It.Is.EqualTo(BuildStatus.Status.Complete))
                ));
        }

        public override void Cleanup()
        {
            if (Directory.Exists(workingAreaPath)) {ClearAttributes(workingAreaPath); Directory.Delete(workingAreaPath, true);}
            if (Directory.Exists(repoArea)) {ClearAttributes(repoArea); Directory.Delete(repoArea, true);}
        }

        static void ClearAttributes(string currentDir)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    ClearAttributes(dir);
                    File.SetAttributes(dir,FileAttributes.Directory);
                }
                string[] files = Directory.GetFiles(currentDir);
                foreach (string file in files)
                    File.SetAttributes(file, FileAttributes.Normal);
            }
        }
    }

    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>
    {
        readonly BuildStatus report;

        public PipelineStatusView(BuildStatus report)
        {
            this.report = report;
        }

        public void Handle(BuildStarted message)
        {
            report.Current = BuildStatus.Status.Started;
        }

        public void Handle(BuildEnded message)
        {
            report.Current = BuildStatus.Status.Complete;
        }
    }

    public class BuildStatus
    {
        public enum Status {Started, NotStarted, Complete}
        public Status Current { get; set; }
    }
}