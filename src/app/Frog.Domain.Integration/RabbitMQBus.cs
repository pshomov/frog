using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Frog.Support;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using SimpleCQRS;

namespace Frog.Domain.Integration
{
    public class RabbitMQBus : IBus, IBusDebug, IDisposable, IBusManagement
    {
        private readonly IConnection connection;
        private readonly Dictionary<string, Dictionary<string, Action<BasicDeliverEventArgs>>> queueHandlers = new Dictionary<string, Dictionary<string, Action<BasicDeliverEventArgs>>>();
        private readonly List<Thread> threads = new List<Thread>();
        ConcurrentQueue<Event> eventsBatch;
        Timer timer;

        public RabbitMQBus(string server)
        {
            StopHandling = () => false;
            var connectionFactory = new ConnectionFactory
                                        {
                                            HostName = server,
                                            UserName = "guest",
                                            Password = "guest",
                                            Protocol = Protocols.AMQP_0_9_1
                                        };
            connection = connectionFactory.CreateConnection();
            eventsBatch = new ConcurrentQueue<Event>();
            timer = new Timer(state => PublishPendingEventBatch(), null, 0, 3000);
        }

        public Func<bool> StopHandling { get; set; }

        public void UnRegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            throw new NotImplementedException();
        }

        public void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            var jsonSerializer = new JavaScriptSerializer();
            var topicName = typeof (T).Name;
            var queueName = handlerId;
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, topicName, "", null);
                channel.TxSelect();
            }
            var startThread = false;
            if (!queueHandlers.ContainsKey(queueName))
            {
                queueHandlers.Add(queueName, new Dictionary<string, Action<BasicDeliverEventArgs>>());
                startThread = true;
            }
            queueHandlers[queueName][topicName] = eventArgs =>
                                                       {
                                                           var message =
                                                               jsonSerializer.Deserialize<T>(
                                                                   Encoding.UTF8.GetString(eventArgs.Body));
                                                           message.Timestamp =
                                                               eventArgs.BasicProperties.Timestamp.UnixTime;
                                                           handler(message);
                                                       };
            if (startThread)
            {
                var job = new Thread(() =>
                                         {
                                             using(var channel = connection.CreateModel())
                                             {
                                                 channel.BasicQos(0,1,false);
                                                 var consumer = new QueueingBasicConsumer(channel);
                                                 channel.BasicConsume(queueName, false, consumer);
                                                 while (true)
                                                 {
                                                     BasicDeliverEventArgs e = null;
                                                     try
                                                     {
                                                         e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                                                         queueHandlers[queueName][e.Exchange](e);
                                                         channel.BasicAck(e.DeliveryTag, false);
                                                     }
                                                     catch (EndOfStreamException)
                                                     {
                                                         break;
                                                     }
                                                     catch (OperationInterruptedException)
                                                     {
                                                         break;
                                                     }
                                                     catch (Exception ex)
                                                     {
                                                         HandleBadMessage(e, ex);
                                                         channel.BasicReject(e.DeliveryTag, true);
                                                     }
                                                     if (StopHandling()) break;
                                                 }
                                             }
                                         });
                job.Start();
                threads.Add(job);
            }
        }

        public void RegisterAll(Action<string, string> handler)
        {
            string handlerId = "all_messages";
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(handlerId, false, false, false, null);
                channel.TxSelect();
            }
            var job = new Thread(() =>
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(handlerId, false, consumer);
                    while (true)
                    {
                        BasicDeliverEventArgs e = null;
                        try
                        {
                            e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            handler(Encoding.UTF8.GetString(e.Body), e.Exchange);
                            channel.BasicAck(e.DeliveryTag, false);
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                        catch (OperationInterruptedException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            HandleBadMessage(e, ex);
                            channel.BasicReject(e.DeliveryTag, true);
                        }
                        if (StopHandling()) break;
                    }
                }
            });
            job.Start();
            threads.Add(job);
        }

        public void UnRegisterAll()
        {
            throw new NotImplementedException();
        }


        public void Publish<T>(T @event) where T : Event
        {
            eventsBatch.Enqueue(@event);
        }

        public void Send<T>(T command) where T : Command
        {
            SendMessage(command);
        }

        public void Send<T>(T command, string handlerId) where T : Command
        {
            throw new NotImplementedException();
        }

        public void UnregisterHandler<T>(string handlerId)
        {
            using(IModel channel = connection.CreateModel())
            {
                string topicName = typeof(T).Name;
                string queueName = handlerId;
                //channel.QueueUnbind(queueName, topicName, "", null);
            }
        }

        public void PurgeQueue(string handlerId)
        {
            using(var channel = connection.CreateModel())
            {
				channel.QueueDeclare(handlerId,false, false, false, null);
                channel.QueuePurge(handlerId);
            }
        }

        public event Action<Guid, Message> OnMessage;

        void PublishPendingEventBatch()
        {
            Event newItem;
            var itemsToSend = new List<Event>();
            while (eventsBatch.TryDequeue(out newItem)) itemsToSend.Add(newItem);
            var events = itemsToSend.ToArray();
            if (events.Length > 0)
            {
                SendEvents(events);
            }
        }

        void SendEvents(params object[] events)
        {
            using (Profiler.measure("sending message to Rabbit MQ"))
            {
                var uniqueEventTypes = events.Select(o => o.GetType()).Distinct().ToList();
                using (IModel channel = connection.CreateModel())
                {
                    channel.TxSelect();
                    foreach (var type in uniqueEventTypes)
                    {
                        var topicName = type.Name;
                        channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
                        channel.QueueDeclare("all_messages", false, false, false, null);
                        channel.QueueBind("all_messages", topicName, "", null);
                    }
                    channel.TxCommit();
                }
                using (IModel channel = connection.CreateModel())
                {
                    var serializer = new JavaScriptSerializer();
                    var basicProperties = channel.CreateBasicProperties();
                    foreach (var @event in events)
                    {
                        channel.BasicPublish(@event.GetType().Name, "", false, false, basicProperties,
                                             Encoding.UTF8.GetBytes(serializer.Serialize(@event)));
                    }
                }
            }
        }

        private void HandleBadMessage(BasicDeliverEventArgs basicDeliverEventArgs, Exception exception)
        {
            Console.WriteLine(string.Format(">>> Exception handling message {0}. Exception is {1}",
                                            basicDeliverEventArgs, exception));
        }

        private void SendMessage<T>(T @event)
        {
            using (Profiler.measure("sending message to Rabbit MQ"))
            {
                string topicName = typeof (T).Name;
                using (IModel channel = connection.CreateModel())
                {
            		channel.TxSelect();
                    channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
                    channel.QueueDeclare("all_messages", false, false, false, null);
                    channel.QueueBind("all_messages", topicName, "", null);
                    channel.TxCommit();
                }
                using (IModel channel = connection.CreateModel())
                {
                    var ser = new JavaScriptSerializer();
                    string serializedForm = ser.Serialize(@event);
                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    channel.BasicPublish(topicName, "", false, false, basicProperties,
                                         Encoding.UTF8.GetBytes(serializedForm));
                }
            }
        }

        public void Dispose()
        {
            using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                timer.Dispose(eventWaitHandle);
                eventWaitHandle.WaitOne();
            }
            PublishPendingEventBatch();
            connection.Close();
            connection.Dispose();
        }

        public void WaitForHandlersToFinish(int i)
        {
            threads.ForEach(thread => {if (!thread.Join(i)) throw new TimeoutException("Waiting for message bus handler threads to complete did not finish properly"); });
        }
    }

    public interface IBusManagement
    {
        void UnregisterHandler<T>(string handlerId);
        void PurgeQueue(string handlerId);
    }
}