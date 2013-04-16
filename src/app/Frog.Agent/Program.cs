using System;
using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.TaskSources;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using Frog.Domain.Integration.TaskSources.BuildSystems.DotNet;
using Frog.Domain.Integration.TaskSources.BuildSystems.Test;
using Frog.Support;
using SimpleCQRS;
using ExecutableTaskGenerator = Frog.Domain.Integration.ExecutableTaskGenerator;

namespace Frog.Agent
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker(GetPipeline(), SetupWorkingAreaGovernor());
            var bus = SetupBus();
            SourceRepoDriverFactory sourceRepoDriverFactory = url => new GitDriver(url);
            var agent = new Domain.Agent(bus, worker, sourceRepoDriverFactory, new string[] { });
            var revisionChecker = new RevisionChecker(bus, sourceRepoDriverFactory);

            agent.JoinTheParty();
            revisionChecker.JoinTheParty();
            Console.ReadLine();
        }

        public static Pipeline GetPipeline()
        {
            var pathFinder = new PathFinder();
            var os = IsNotWindows() ? OS.Unix : OS.Windows;
            return new Pipeline(new CompoundTaskSource(
                                           new CustomTasksDetector(new CustomFileFinder(pathFinder), File.ReadAllText),
                                           new TestTaskDetector(new TestTaskTaskFileFinder(pathFinder)),
                                           new MSBuildDetector(new SolutionTaskFileFinder(pathFinder), os),
                                           new NUnitTaskDetector(new NUnitTaskFileFinder(pathFinder))
                                           ),
                                       new ExecutableTaskGenerator(new ExecTaskFactory()));
        }

        public static bool IsNotWindows()
        {
            return As.List(PlatformID.Win32NT, PlatformID.Win32Windows).IndexOf(Environment.OSVersion.Platform) == -1;
        }

        public static IBus SetupBus()
        {
            return new RabbitMQBus(OSHelpers.RabbitHost());
        }

        public static WorkingAreaGoverner SetupWorkingAreaGovernor()
        {
            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingAreaGoverner(workingAreaPath);
        }
    }
}
