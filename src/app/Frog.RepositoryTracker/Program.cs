using System;
using System.Threading;
using Frog.Domain.Integration;
using Frog.Domain.Integration.ProjectRepository;
using Frog.Support;
using SimpleCQRS;

namespace Frog.RepositoryTracker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Profiler.MeasurementsBridge = new Profiler.LogFileLoggingBridge("runz_repo_tracker.log");
            IBus theBus = SetupBus();
            var repoTracker = new Domain.RepositoryTracker(theBus,
                                                                             new RiakProjectRepository(
                                                                                 OSHelpers.RiakHost(),
                                                                                 OSHelpers.RiakPort(),
                                                                                 "projects"));
            repoTracker.JoinTheMessageParty();

            var sleepPeriod = 60 * 1000;
            string mode = Environment.GetEnvironmentVariable("RUNZ_ACCEPTANCE_MODE");
            if (!mode.IsNullOrEmpty() && mode == "ACCEPTANCE") sleepPeriod = 4 * 1000;
            while (true)
            {
                repoTracker.CheckForUpdates();
                Thread.Sleep(sleepPeriod);
            }
        }

        private static IBus SetupBus()
        {
            return new RabbitMQBus(OSHelpers.RabbitHost());
        }

    }
}