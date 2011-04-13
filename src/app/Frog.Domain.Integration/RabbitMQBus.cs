using System;
using SimpleCQRS;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Text;

namespace Frog.Domain.Integration
{
	public class RabbitMQBus : IBus, IBusDebug
	{
		IConnection connection;

		public RabbitMQBus()
		{
			var connectionFactory = new ConnectionFactory {HostName = "192.168.0.2", UserName = "guest", Password = "guest"};
		    connection = connectionFactory.CreateConnection();
		}

		public void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
		{
			var channel = connection.CreateModel();
		    var topicName = typeof(T).Name;
		    var queueName = string.Format("{0}_{1}", topicName, handlerId);
		    channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
			channel.QueueDeclare(queueName, false, false, false, null);
			channel.QueueBind(queueName, topicName, "", null);

			var consumer = new QueueingBasicConsumer (channel);
			var job = new Thread (() => {
					var consumerTag = channel.BasicConsume(queueName, false, consumer);
			        var jsonSerializer = new JavaScriptSerializer ();
			        while (true)
					{
					    try {
							var e = (RabbitMQ.Client.Events.BasicDeliverEventArgs)consumer.Queue.Dequeue();
					        var deserialized = jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(e.Body));
							handler(deserialized);
							channel.BasicAck(e.DeliveryTag, false);
						} catch (OperationInterruptedException) {
							break;
						}
					}
			});
			job.Start();
		}
		
		void SendMessage<T>(T @event)
		{
				var channel = connection.CreateModel();
				var topicName = typeof(T).Name;
				var ser = new JavaScriptSerializer ();
				var serializedForm = ser.Serialize(@event);
				var basicProperties = channel.CreateBasicProperties();
				channel.BasicPublish(topicName, "", false, false, basicProperties, Encoding.UTF8.GetBytes(serializedForm));
		}

		public void Publish<T>(T @event) where T : Event
		{
			SendMessage(@event);		
		}

		public void Send<T>(T command) where T : Command
		{
			SendMessage(command);
		}

	    public event Action<Message> OnMessage;
	}
}

