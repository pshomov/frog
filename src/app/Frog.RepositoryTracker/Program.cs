using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.RepositoryTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            var repoTracker = new Domain.RepositoryTracker(SetupBus());
            repoTracker.StartListeningForBuildUpdates();
            while(true)
            {
                repoTracker.CheckForUpdates();
                Thread.Sleep(60*1000);
            }
        }
        static IBus SetupBus()
        {
            return new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_RABBITMQ_SERVER") ?? "localhost");
        }
    }
}
