using System;
using RabbitMQ.Client.Events;
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

		public RabbitMQBus(string server)
		{
			var connectionFactory = new ConnectionFactory {HostName = server, UserName = "guest", Password = "guest",Protocol = Protocols.AMQP_0_9_1};
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
			        var jsonSerializer = new JavaScriptSerializer();
			        while (true)
			        {
			            var e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
			            try {
					        var deserialized = jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(e.Body));
							handler(deserialized);
							channel.BasicAck(e.DeliveryTag, false);
						} catch (OperationInterruptedException) {
							break;
						} catch(Exception ex)
						{
						   HandleBadMessage(e, ex);
                           channel.BasicNack(e.DeliveryTag, false, true);
                        }
			        }
			});
			job.Start();
		}

	    private void HandleBadMessage(BasicDeliverEventArgs basicDeliverEventArgs, Exception exception)
	    {    
	        Console.WriteLine(string.Format(">>> Exception handling message {0}. Exception is {1}", basicDeliverEventArgs, exception));
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

