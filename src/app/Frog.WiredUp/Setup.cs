using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Partition;
using Lokad.Cqrs.StreamingStorage;
using Lokad.Cqrs.TapeStorage;
using SaaS;
using SaaS.Client;
using SaaS.Wires;
using Sample.Storage.Sql;

namespace Frog.WiredUp
{
    public sealed class Setup
    {
        static readonly string FunctionalRecorderQueueName = Conventions.FunctionalEventRecorderQueue;
        static readonly string RouterQueueName = Conventions.DefaultRouterQueue;
        static readonly string ErrorsContainerName = Conventions.DefaultErrorsFolder;

        const string EventProcessingQueue = Conventions.Prefix + "-handle-events";
        const string AggregateHandlerQueue = Conventions.Prefix + "-handle-cmd-entity";
        string[] _serviceQueues;
        

        public void ConfigureQueues(int serviceQueueCount, int adapterQueueCount)
        {
            _serviceQueues = Enumerable.Range(0, serviceQueueCount)
                .Select((s, i) => Conventions.Prefix + "-handle-cmd-service-" + i)
                .ToArray();
        }

        public const string TapesContainer = Conventions.Prefix + "-tapes";

        public static readonly EnvelopeStreamer Streamer = Contracts.CreateStreamer();
        public static readonly IDocumentStrategy ViewStrategy = new ViewStrategy();
        public static readonly IDocumentStrategy DocStrategy = new DocumentStrategy();

        public IStreamRoot Streaming;

        public Func<string, IQueueWriter> QueueWriterFactory;
        public Func<string, IQueueReader> QueueReaderFactory;
        public Func<string, IAppendOnlyStore> AppendOnlyStoreFactory;
        public Func<IDocumentStrategy, IDocumentStore> DocumentStoreFactory;

        public Container Build()
        {
            var appendOnlyStore = AppendOnlyStoreFactory(TapesContainer);
            var messageStore = new MessageStore(appendOnlyStore, Streamer.MessageSerializer);

            var sendToRouterQueue = new MessageSender(Streamer, QueueWriterFactory(RouterQueueName));
            var sendToFunctionalRecorderQueue = new MessageSender(Streamer, QueueWriterFactory(FunctionalRecorderQueueName));
            var sendToEventProcessingQueue = new MessageSender(Streamer, QueueWriterFactory(EventProcessingQueue));

            var sendSmart = new TypedMessageSender(sendToRouterQueue, sendToFunctionalRecorderQueue);

            var store = new EventStore(messageStore);

            var quarantine = new EnvelopeQuarantine(Streamer, sendSmart, Streaming.GetContainer(ErrorsContainerName));

            var builder = new CqrsEngineBuilder(Streamer, quarantine);

            var events = new RedirectToDynamicEvent();
//            var commands = new RedirectToCommand();
//            var funcs = new RedirectToCommand();
            

            builder.Handle(QueueReaderFactory(EventProcessingQueue), aem => CallHandlers(events, aem), "watch");
//            builder.Handle(QueueReaderFactory(AggregateHandlerQueue), aem => CallHandlers(commands, aem));
//            builder.Handle(QueueReaderFactory(RouterQueueName), MakeRouter(messageStore), "watch");
            // multiple service queues
//            _serviceQueues.ForEach(s => builder.Handle(QueueReaderFactory(s), aem => CallHandlers(funcs, aem)));

//            builder.Handle(QueueReaderFactory(FunctionalRecorderQueueName), aem => RecordFunctionalEvent(aem, messageStore));
            var viewDocs = DocumentStoreFactory(ViewStrategy);
            var stateDocs = new NuclearStorage(DocumentStoreFactory(DocStrategy));


            var projections = new ProjectionsConsumingOneBoundedContext();
//            var vector = new DomainIdentityGenerator(stateDocs);
//            //var ops = new StreamOps(Streaming);
//            // Domain Bounded Context
//            DomainBoundedContext.EntityApplicationServices(viewDocs, store,vector).ForEach(commands.WireToWhen);
//            DomainBoundedContext.FuncApplicationServices().ForEach(funcs.WireToWhen);
//            DomainBoundedContext.Ports(sendSmart).ForEach(events.WireToWhen);
//            DomainBoundedContext.Tasks(sendSmart, viewDocs, true).ForEach(builder.AddTask);
//            projections.RegisterFactory(DomainBoundedContext.Projections);

            // Client Bounded Context
            projections.RegisterFactory(ClientBoundedContext.Projections);

            // wire all projections
            projections.BuildFor(viewDocs).ForEach(events.WireToWhen);

            // wire in event store publisher
            var publisher = new MessageStorePublisher(messageStore, sendToEventProcessingQueue, stateDocs, DoWePublishThisRecord);
            builder.AddTask(c => Task.Factory.StartNew(() => publisher.Run(c)));

            return new Container
            {
                Builder = builder,
                Setup = this,
                SendToCommandRouter = sendToRouterQueue,
                MessageStore = messageStore,
                ProjectionFactories = projections,
                ViewDocs = viewDocs,
                Publisher = publisher,
                AppendOnlyStore = appendOnlyStore,
                Store = store
            };
        }

        static bool DoWePublishThisRecord(StoreRecord storeRecord)
        {
            return storeRecord.Key != "audit";
        }

        static void RecordFunctionalEvent(ImmutableEnvelope envelope, MessageStore store)
        {
            if (envelope.Message is IFuncEvent) store.RecordMessage("func", envelope);
            else throw new InvalidOperationException("Non-func event {0} landed to queue for tracking stateless events");
        }

        Action<ImmutableEnvelope> MakeRouter(MessageStore tape)
        {
            var entities = QueueWriterFactory(AggregateHandlerQueue);
            var processing = _serviceQueues.Select(QueueWriterFactory).ToArray();
            
            return envelope =>
            {
                var message = envelope.Message;
                if (message is ICommand)
                {
                    // all commands are recorded to audit stream, as they go through router
                    tape.RecordMessage("audit", envelope);
                }

                if (message is IEvent)
                {
                    throw new InvalidOperationException("Events are not expected in command router queue");
                }

                var data = Streamer.SaveEnvelopeData(envelope);

                if (message is ICommand<IIdentity>)
                {
                    entities.PutMessage(data);
                    return;
                }
                if (message is IFuncCommand)
                {
                    // randomly distribute between queues
                    var i = Environment.TickCount % processing.Length;
                    processing[i].PutMessage(data);
                    return;
                }
                throw new InvalidOperationException("Unknown queue format");
            };


        }

        static void ConfigureObserver()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var observer = new ConsoleObserver();
            SystemObserver.Swap(observer);
            Context.SwapForDebug(s => SystemObserver.Notify(s));
        }

        public static IDocumentStore GetDocumentStore(string storePath, Guid customerId)
        {
            var config = FileStorage.CreateConfig(Path.Combine(storePath, customerId.ToString()), reset: false);
            return config.CreateDocumentStore(new ViewStrategy());
        }

        public static Container BuildEnvironment(bool reset_store, string storePath, string eventStoreConnection, Guid customerId)
        {
            ConfigureObserver();
            var setup = new Setup();

            var customer_store_path = Path.Combine(storePath, customerId.ToString());
            SystemObserver.Notify("Using store : {0}", customer_store_path);

            var config = FileStorage.CreateConfig(customer_store_path, reset: reset_store);

            setup.Streaming = config.CreateStreaming();
            setup.DocumentStoreFactory = config.CreateDocumentStore;
            setup.QueueReaderFactory = s => config.CreateInbox(s, DecayEvil.BuildExponentialDecay(500));
            setup.QueueWriterFactory = config.CreateQueueWriter;
            setup.AppendOnlyStoreFactory = (name) =>
            {
                var sql_event_store = new SqlEventStore(eventStoreConnection, customerId);
                sql_event_store.Initialize();
                if (reset_store) sql_event_store.ResetStore();
                return sql_event_store;
            };

            setup.ConfigureQueues(1, 1);

            return setup.Build();
        }


        /// <summary>
        /// Helper class that merely makes the concept explicit
        /// </summary>
        public sealed class ProjectionsConsumingOneBoundedContext
        {
            public delegate IEnumerable<object> FactoryForWhenProjections(IDocumentStore store);

            readonly IList<FactoryForWhenProjections> _factories = new List<FactoryForWhenProjections>();

            public void RegisterFactory(FactoryForWhenProjections factory)
            {
                _factories.Add(factory);
            }

            public IEnumerable<object> BuildFor(IDocumentStore store)
            {
                return _factories.SelectMany(factory => factory(store));
            }
        }

        static void CallHandlers(RedirectToDynamicEvent functions, ImmutableEnvelope aem)
        {
            var e = aem.Message as IEvent;

            if (e != null)
            {
                functions.InvokeEvent(e);
            }
        }

        static void CallHandlers(RedirectToCommand serviceCommands, ImmutableEnvelope aem)
        {

            var content = aem.Message;
            var watch = Stopwatch.StartNew();
            serviceCommands.Invoke(content);
            watch.Stop();

            var seconds = watch.Elapsed.TotalSeconds;
            if (seconds > 10)
            {
                SystemObserver.Notify("[Warn]: {0} took {1:0.0} seconds", content.GetType().Name, seconds);
            }
        }

        public static Guid OpensourceCustomer = Guid.Parse("8b4f3240-c41f-4eed-9129-d414d2c01645");
    }

    public sealed class Container : IDisposable
    {
        public Setup Setup;
        public CqrsEngineBuilder Builder;
        public MessageSender SendToCommandRouter;
        public MessageStore MessageStore;
        public IAppendOnlyStore AppendOnlyStore;
        public IDocumentStore ViewDocs;
        public Setup.ProjectionsConsumingOneBoundedContext ProjectionFactories;
        public MessageStorePublisher Publisher;

        public EventStore Store;

        public CqrsEngineHost BuildEngine(CancellationToken token)
        {
            return Builder.Build(token);
        }

        public void ExecuteStartupTasks(CancellationToken token)
        {
            Publisher.VerifyEventStreamSanity();

            // we run S2 projections from 3 different BCs against one domain log
            StartupProjectionRebuilder.Rebuild(
                token,
                ViewDocs,
                MessageStore,
                store => ProjectionFactories.BuildFor(store));
        }

        public void Dispose()
        {
            using (AppendOnlyStore)
            {
                AppendOnlyStore = null;
            }
        }
    }

    public static class ExtendArrayEvil
    {
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var variable in self)
            {
                action(variable);
            }
        }
    }
}