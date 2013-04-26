using System;
using System.Threading;
using Frog.Domain.Integration;
using Frog.Support;
using Frog.WiredUp;
using SimpleCQRS;

namespace SaaS.Engine
{
    public class Program
    {
        static void Main()
        {
            var bus = SetupBus();
            using (var env = Setup.BuildEnvironment(false, OSHelpers.LokadStorePath(), Config.Env.connection_string, Setup.OpensourceCustomer))
            using (var cts = new CancellationTokenSource())
            {
                env.ExecuteStartupTasks(cts.Token);
                using (var engine = env.BuildEngine(cts.Token))
                {
                    var task = engine.Start(cts.Token);
                    new EventsArchiver(bus, env.Store).JoinTheParty();

                    Console.WriteLine(@"Press enter to stop");
                    Console.ReadLine();
                    cts.Cancel();
                    if (!task.Wait(5000))
                    {
                        Console.WriteLine(@"Terminating");
                    }
                }
            }
        }

        static IBus SetupBus()
        {
            return new RabbitMQBus(OSHelpers.RabbitHost());
        }

    }
}
