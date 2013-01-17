using System;
using System.Threading;
using EventStore;
using EventStore.Serialization;
using Frog.Domain.Integration;
using Frog.Domain.Integration.UI;
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
            var repoTracker = new Domain.RepositoryTracker.RepositoryTracker(theBus,
                                                                             new RiakProjectRepository(
                                                                                 OSHelpers.RiakHost(),
                                                                                 OSHelpers.RiakPort(),
                                                                                 "projects"));
            repoTracker.JoinTheMessageParty();

            Setup.SetupView(theBus, Config.WireupEventStore());
			
			
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