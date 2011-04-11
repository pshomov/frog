using System;
using SimpleCQRS;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Text;

namespace Frog.Domain.Integration
{
	public class RabbitMQBus : IBus
	{
		IConnection connection;

		public RabbitMQBus()
		{
			var connectionFactory = new ConnectionFactory ();
			connectionFactory.HostName = "localhost";
			connectionFactory.UserName = "justin";
			connectionFactory.Password = "greatpass!";

			connection = connectionFactory.CreateConnection();
		}

		public void RegisterHandler<T>(Action<T> handler) where T : Message
		{
			IModel channel = connection.CreateModel();
			string queueName = handler.GetType().Name;
			string topicName = typeof(T).Name;
			channel.ExchangeDeclare(topicName, ExchangeType.Fanout, true);
			channel.QueueDeclare(queueName, true, false, false, null);
			channel.QueueBind(queueName, topicName, "", null);

			QueueingBasicConsumer consumer = new QueueingBasicConsumer (channel);
			Thread job = new Thread (() => {
					String consumerTag = channel.BasicConsume(queueName, false, consumer);
					while (true) {
						try {
							RabbitMQ.Client.Events.BasicDeliverEventArgs e = 
							(RabbitMQ.Client.Events.BasicDeliverEventArgs)
							consumer.Queue.Dequeue();
							IBasicProperties props = e.BasicProperties;
							JavaScriptSerializer ser = new JavaScriptSerializer ();
							byte[] body = e.Body;
							var deserialized = ser.Deserialize<T>(Encoding.UTF8.GetString(body));
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
				IModel channel = connection.CreateModel();
				string topicName = typeof(T).Name;
				JavaScriptSerializer ser = new JavaScriptSerializer ();
				var serialized_form = ser.Serialize(@event);
				IBasicProperties basicProperties = channel.CreateBasicProperties();
				channel.BasicPublish(topicName, "", false, false, basicProperties, Encoding.UTF8.GetBytes(serialized_form));
		}

		public void Publish<T>(T @event) where T : Event
		{
			SendMessage(@event);		
		}

		public void Send<T>(T command) where T : Command
		{
			SendMessage(command);
		}
	}
}

