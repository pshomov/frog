using System;
using System.Threading;
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
	    readonly IBus bus = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");

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

        [Test]
		public void should_send_mesage_and_only_one_subscriber_of_the_queue_will_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test3");
			bus.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test3");
			bus.Publish(new MyEvent{Data = "ff"});
            Thread.Sleep(3000);
		    AssertionHelpers.WithRetries(() => Assert.That(msg1+msg2, Is.EqualTo("ff")));
		}

        [Test]
		public void should_continue_handling_messages_after_particular_message_causes_exception(){
			var msg1 = "";
            bus.RegisterHandler<MyEvent>(ev => { if (ev.Data == "f1") throw new Exception("ouch"); else msg1 = ev.Data; }, "unit_test4");
            bus.Publish(new MyEvent{Data = "f1"});
            bus.Publish(new MyEvent{Data = "fd"});
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("fd")));
            bus.RegisterHandler<MyEvent>(ev => msg1 = ev.Data, "unit_test4");
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("f1")));
        }
	}
}

