using System;
using System.Threading;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.RepositoryTracker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var repoTracker = new Domain.RepositoryTracker.RepositoryTracker(SetupBus(),
                                                                             new RiakProjectRepository(
                                                                                 Host(),
                                                                                 Port(),
                                                                                 "projects"));
            repoTracker.JoinTheMessageParty();
            while (true)
            {
                repoTracker.CheckForUpdates();
                Thread.Sleep(60*1000);
            }
        }

        private static int Port()
        {
            return Int32.Parse(
                Environment.GetEnvironmentVariable(
                    "RUNZ_RIAK_PORT") ?? "8087");
        }

        private static string Host()
        {
            return Environment.GetEnvironmentVariable(
                "RUNZ_RIAK_HOST") ?? "localhost";
        }

        private static IBus SetupBus()
        {
            return new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_RABBITMQ_SERVER") ?? "localhost");
        }
    }
}