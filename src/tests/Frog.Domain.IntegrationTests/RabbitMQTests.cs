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
        IBus bus = new RabbitMQBus();

        [Test]
		public void should_send_mesage_and_receive_it(){
			var msg = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg = ev.Data, "unit_test0");
			bus.Publish(new MyEvent{Data = "ff"});
		    AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo("ff")));
		}

        [Test]
		public void should_send_mesage_and_all_subscribers_should_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test1");
			bus.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test2");
			bus.Publish(new MyEvent{Data = "ff"});
		    AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("ff")));
		    AssertionHelpers.WithRetries(() => Assert.That(msg2, Is.EqualTo("ff")));
		}
	}
}

