using System;
using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.BuildSystems.Custom;
using Frog.Domain.BuildSystems.DotNet;
using Frog.Domain.BuildSystems.Test;
using Frog.Domain.Integration;
using Frog.Domain.RevisionChecker;
using Frog.Domain.TaskSources;
using Frog.Support;
using SimpleCQRS;

namespace Frog.Agent
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker(GetPipeline(), SetupWorkingAreaGovernor());
            var bus = SetupBus();
            SourceRepoDriverFactory sourceRepoDriverFactory = url => new GitDriver(url);
            var agent = new Domain.Agent(bus, worker, sourceRepoDriverFactory);
            var revisionChecker = new RevisionChecker(bus, sourceRepoDriverFactory);

            agent.JoinTheParty();
            revisionChecker.JoinTheParty();
            Console.ReadLine();
        }

        public static PipelineOfTasks GetPipeline()
        {
            var pathFinder = new PathFinder();
            var os = IsNotWindows() ? OS.Unix : OS.Windows;
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new CustomTasksDetector(new CustomFileFinder(pathFinder), File.ReadAllText),
                                           new TestTaskDetector(new TestTaskTaskFileFinder(pathFinder)),
                                           new MSBuildDetector(new SolutionTaskFileFinder(pathFinder), os),
                                           new NUnitTaskDetector(new NUnitTaskFileFinder(pathFinder))
                                           ),
                                       new ExecTaskGenerator(new ExecTaskFactory()));
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
