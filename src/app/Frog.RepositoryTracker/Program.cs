using System;
using System.Threading;
using EventStore;
using EventStore.Serialization;
using Frog.Domain.Integration;
using Frog.Support;
using SimpleCQRS;
using Frog.Domain.UI;

namespace Frog.RepositoryTracker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Profiler.MeasurementsBridge = new Profiler.LogFileLoggingBridge();
            IBus theBus = SetupBus();
            var repoTracker = new Domain.RepositoryTracker.RepositoryTracker(theBus,
                                                                             new RiakProjectRepository(
                                                                                 OSHelpers.RiakHost(),
                                                                                 OSHelpers.RiakPort(),
                                                                                 "projects"));
            repoTracker.JoinTheMessageParty();

            var views = new PersistentProjectView(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "buildsIds", "reposBucket");
            Setup.SetupView(theBus, WireupEventStore());
			
			
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
        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                .LogToOutputWindow()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                .InitializeStorageEngine()
                .Build();
        }

    }
}