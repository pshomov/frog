using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.ProjectRepository;
using Frog.Domain.Integration.Projections;
using Frog.Domain.Integration.TaskSources;
using Frog.Domain.Integration.TaskSources.BuildSystems.Custom;
using Frog.Domain.Integration.TaskSources.BuildSystems.DotNet;
using Frog.Domain.Integration.TaskSources.BuildSystems.Test;
using Frog.Support;
using Lokad.Cqrs.Build;
using SimpleCQRS;

namespace Frog.WiredUp
{
    public static class ProductionInfrastructure
    {
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

    public class AgentDeploumentWireUp : IDisposable
    {
        CqrsEngineHost engine;
        CancellationTokenSource cts;
        Container env;
        Task task;
        Thread repotrackingJob;

        public void Start(Guid agentId)
        {
            var worker = new Worker(ProductionInfrastructure.GetPipeline(), ProductionInfrastructure.SetupWorkingAreaGovernor());
            var bus = ProductionInfrastructure.SetupBus();
            env = Setup.BuildEnvironment(false, OSHelpers.LokadStorePath(), Config.Env.connection_string, Setup.OpensourceCustomer);
            cts = new CancellationTokenSource();
            env.ExecuteStartupTasks(cts.Token);
            engine = env.BuildEngine(cts.Token);
            task = engine.Start(cts.Token);

            SourceRepoDriverFactory source_repo_driver_factory = url => new GitDriver(url);
            var agent = new Agent(bus, worker, source_repo_driver_factory, new string[] { }, agentId);
            var revision_checker = new RevisionChecker(bus, source_repo_driver_factory);
            var repository_tracker = new RepositoryTracker(bus, new RiakProjectRepository(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "projects"));

//            var events_archiver = new EventsArchiver(bus, env.Store);
            var build_dispatcher = new BuildDispatcher(bus, new AgentStatuses(env.ViewDocs));

//            events_archiver.JoinTheParty();
            build_dispatcher.JoinTheParty();
            agent.JoinTheParty();
            revision_checker.JoinTheParty();
            repository_tracker.JoinTheMessageParty();

            repotrackingJob = new Thread(() =>
                {
                    var sleepPeriod = 60*1000;
                    string mode = Environment.GetEnvironmentVariable("RUNZ_ACCEPTANCE_MODE");
                    if (!mode.IsNullOrEmpty() && mode == "ACCEPTANCE") sleepPeriod = 4*1000;
                    while (true)
                    {
                        repository_tracker.CheckForUpdates();
                        Thread.Sleep(sleepPeriod);
                    }
                });
            repotrackingJob.Start();
        }
        public void Dispose()
        {
            repotrackingJob.Abort();
            cts.Cancel();
            if (!task.Wait(5000))
            {
                Console.WriteLine(@"Terminating");
            }
            engine.Dispose();
            cts.Dispose();
            env.Dispose();
        }
    }
}
