using System;
using NUnit.Framework;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.Domain.IntegrationTests
{
	class MyEvent : Event{
		public string data;
	}
	[TestFixture]
	public class RabbitMQTests
	{
		[Test]
		public void should_send_mesages(){
			var msg = "";
			var bus = new RabbitMQBus();
			bus.RegisterHandler<MyEvent>((ev) =>  msg = ev.data);
			bus.Publish(new MyEvent{data = "ff"});
			Assert.That(msg, Is.EqualTo("ff"));
		}
	}
}

