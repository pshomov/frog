using System;
using System.Diagnostics;
using System.Threading;
using Frog.Domain.Integration;
using Frog.Support;
using Lokad.Cqrs;
using Lokad.Cqrs.Evil;
using SaaS.Wires;
using SimpleCQRS;

namespace SaaS.Engine
{
    public class Program
    {
        static void Main()
        {
            var bus = SetupBus();
            using (var env = BuildEnvironment(false, OSHelpers.LokadStorePath()))
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

        static void ConfigureObserver()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var observer = new ConsoleObserver();
            SystemObserver.Swap(observer);
            Context.SwapForDebug(s => SystemObserver.Notify(s));
        }

        public static Container BuildEnvironment(bool reset_store, string storePath)
        {
            ConfigureObserver();
            var setup = new Setup();

            var lokadStorePath = storePath;
            SystemObserver.Notify("Using store : {0}", lokadStorePath);

            var config = FileStorage.CreateConfig(lokadStorePath, reset: reset_store);

            setup.Streaming = config.CreateStreaming();
            setup.DocumentStoreFactory = config.CreateDocumentStore;
            setup.QueueReaderFactory = s => config.CreateInbox(s, DecayEvil.BuildExponentialDecay(500));
            setup.QueueWriterFactory = config.CreateQueueWriter;
            setup.AppendOnlyStoreFactory = config.CreateAppendOnlyStore;

            setup.ConfigureQueues(1, 1);

            return setup.Build();
        }
    }
}
