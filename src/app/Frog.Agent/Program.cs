using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Make;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.Integration;
using Frog.Domain.RevisionChecker;
using Frog.Domain.TaskSources;
using SimpleCQRS;

namespace Frog.Agent
{
    class Program
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

        static PipelineOfTasks GetPipeline()
        {
            var pathFinder = new PathFinder();
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new TestTaskDetector(new TestTaskTaskFileFinder(pathFinder)),
                                           new MakeTaskDetector(new MakeFileFinder(pathFinder)),
                                           new RubyTaskDetector(new RakeTaskFileFinder(pathFinder), new BundlerFileFinder(pathFinder)),
                                           new MSBuildDetector(new SolutionTaskFileFinder(pathFinder)),
                                           new NUnitTaskDetector(new NUnitTaskFileFinder(pathFinder))
                                           ),
                                       new ExecTaskGenerator(new ExecTaskFactory()));
        }
        static IBus SetupBus()
        {
            return new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_RABBITMQ_SERVER") ?? "localhost");
        }

        static WorkingAreaGoverner SetupWorkingAreaGovernor()
        {
            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingAreaGoverner(workingAreaPath);
        }
    }
}
