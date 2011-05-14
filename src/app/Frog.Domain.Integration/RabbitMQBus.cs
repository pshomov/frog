using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
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
            
        public RabbitMQBus(string server)
        {
            var connectionFactory = new ConnectionFactory
                                        {
                                            HostName = server,
                                            UserName = "guest",
                                            Password = "guest",
                                            Protocol = Protocols.AMQP_0_9_1
                                        };
            connection = connectionFactory.CreateConnection();
        }

        public void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            var jsonSerializer = new JavaScriptSerializer();
            var channel = connection.CreateModel();
            var topicName = typeof (T).Name;
            var queueName = handlerId;
            channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, topicName, "", null);
            var startThread = false;
            if (!queueHandlers.ContainsKey(queueName))
            {
                queueHandlers.Add(queueName, new Dictionary<string, Action<BasicDeliverEventArgs>>());
                startThread = true;
            }
            queueHandlers[queueName][topicName] = eventArgs =>
                                                       {
                                                           var deserialized =
                                                               jsonSerializer.Deserialize<T>(
                                                                   Encoding.UTF8.GetString(eventArgs.Body));
                                                           handler(deserialized);
                                                       };
            if (startThread)
            {
                var consumer = new QueueingBasicConsumer(channel);
                var job = new Thread(() =>
                                         {
                                             channel.BasicConsume(queueName, false, consumer);
                                             while (true)
                                             {
                                                 BasicDeliverEventArgs e = null;
                                                 try
                                                 {
                                                     e = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
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
                                             }
                                         });
                job.Start();
            }
        }

        public void Publish<T>(T @event) where T : Event
        {
            SendMessage(@event);
        }

        public void Send<T>(T command) where T : Command
        {
            SendMessage(command);
        }

        public void UnregisterHandler<T>(string handlerId)
        {
            using(IModel channel = connection.CreateModel())
            {
                string topicName = typeof(T).Name;
                string queueName = handlerId;
                channel.QueueUnbind(queueName, topicName, "", null);
            }
        }

        public event Action<Message> OnMessage;

        private void HandleBadMessage(BasicDeliverEventArgs basicDeliverEventArgs, Exception exception)
        {
            Console.WriteLine(string.Format(">>> Exception handling message {0}. Exception is {1}",
                                            basicDeliverEventArgs, exception));
        }

        private void SendMessage<T>(T @event)
        {
            using (IModel channel = connection.CreateModel())
            {
                string topicName = typeof(T).Name;
                var ser = new JavaScriptSerializer();
                string serializedForm = ser.Serialize(@event);
                IBasicProperties basicProperties = channel.CreateBasicProperties();
                channel.BasicPublish(topicName, "", false, false, basicProperties, Encoding.UTF8.GetBytes(serializedForm));
            }
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }
    }

    public interface IBusManagement
    {
        void UnregisterHandler<T>(string handlerId);
    }
}