﻿using System;
using System.Collections.Generic;
using System.IO;
using Frog.Domain;
using Frog.Domain.BuildSystems.Custom;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Make;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
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
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new CustomTasksDetector(new CustomFileFinder(pathFinder), File.ReadAllText),
                                           new TestTaskDetector(new TestTaskTaskFileFinder(pathFinder)),
                                           new MakeTaskDetector(new MakeFileFinder(pathFinder)),
                                           new RubyTaskDetector(new RakeTaskFileFinder(pathFinder), new BundlerFileFinder(pathFinder)),
                                           new MSBuildDetector(new SolutionTaskFileFinder(pathFinder)),
                                           new NUnitTaskDetector(new NUnitTaskFileFinder(pathFinder))
                                           ),
                                       new ExecTaskGenerator(new ExecTaskFactory(), IsNotWindows() ? OS.Unix : OS.Windows));
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
