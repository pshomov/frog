using System;
using Frog.Specs.Support;
using NUnit.Framework;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.Domain.IntegrationTests
{
	class MyEvent : Event{
		public string Data;
	}

	[TestFixture]
	public class RabbitMQTests
	{
		[Test]
		public void should_send_mesage_and_receive_it(){
			var msg = "";
			var bus = new RabbitMQBus();
			bus.RegisterHandler<MyEvent>(ev =>  msg = ev.Data, "unit_test");
			bus.Publish(new MyEvent{Data = "ff"});
		    AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo("ff")));
		}
	}
}

