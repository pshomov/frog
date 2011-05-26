using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Frog.Domain.Integration;
using Frog.Domain.RepositoryTracker;
using SimpleCQRS;

namespace Frog.RepositoryTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            var repoTracker = new Domain.RepositoryTracker.RepositoryTracker(SetupBus(), new RiakProjectRepository(Environment.GetEnvironmentVariable("RUNZ_RIAK_HOST") ?? "localhost", Int32.Parse(Environment.GetEnvironmentVariable("RUNZ_RIAK_PORT") ?? "8087"), "projects"));
            repoTracker.JoinTheMessageParty();
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
