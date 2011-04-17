using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	class YourEvent : Event{
		public string Data;
	}

	[TestFixture]
	public class RabbitMQTests
	{
	    private RabbitMQBus bus;

        [SetUp]
        public void Setup()
        {
            bus = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
        }

        [TearDown]
        public void TearDown()
        {
            bus.Dispose();
        }

        [Test]
		public void should_send_mesage_and_receive_it(){
			var msg = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg = ev.Data, "unit_test0");
			bus.Publish(new MyEvent{Data = "unit_test1"});
            AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo("unit_test1")));
		}

        [Test]
		public void should_send_mesage_and_all_subscribers_should_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test1");
			bus.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test2");
			bus.Publish(new MyEvent{Data = "unit_test2"});
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("unit_test2")));
            AssertionHelpers.WithRetries(() => Assert.That(msg2, Is.EqualTo("unit_test2")));
		}

        [Test]
		public void should_send_mesage_and_only_one_subscriber_of_the_queue_will_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test3");
			bus.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test3");
            bus.Publish(new MyEvent { Data = "unit_test3" });
            Thread.Sleep(3000);
            AssertionHelpers.WithRetries(() => Assert.That(msg1 + msg2, Is.EqualTo("unit_test3")));
		}

        [Test]
		public void should_continue_handling_messages_after_particular_message_causes_exception(){
			var msg1 = "";
            bus.RegisterHandler<MyEvent>(ev => { if (ev.Data == "unit_test4_1") throw new Exception("ouch"); else msg1 = ev.Data; }, "unit_test4");
            bus.Publish(new MyEvent { Data = "unit_test4_1" });
            bus.Publish(new MyEvent { Data = "unit_test4_2" });
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("unit_test4_2")));
            bus.RegisterHandler<MyEvent>(ev => msg1 = ev.Data, "unit_test4");
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("unit_test4_1")));
        }

        [Test]
        public void should_handle_messages_sequentially_when_they_are_in_the_same_queue()
        {
            int messageCount = 0;
            var messageAlreadyHandled = 0;
            Action assertOtherMessageHasNotBeenHandledAlready = () => Assert.Fail("what the ..."); // weirdness so that we do not make assertions on a different thread
            Action handler = () =>
                                 {
                                     if (messageCount++ == 0)
                                     {
                                         Thread.Sleep(1000);
                                         var msg = messageAlreadyHandled;
                                         assertOtherMessageHasNotBeenHandledAlready = () => Assert.That(msg, Is.EqualTo(0));
                                     }
                                     Interlocked.Increment(ref messageAlreadyHandled);
                                 };
            bus.RegisterHandler<MyEvent>(@event => handler(), "unit_test5");
            bus.RegisterHandler<YourEvent>(@event => handler(), "unit_test5");
            bus.Publish(new MyEvent { Data = "unit_test5_1" });
            Thread.Sleep(50);
            bus.Publish(new YourEvent { Data = "unit_test5_2" });
            AssertionHelpers.WithRetries(() => Assert.That(messageAlreadyHandled, Is.EqualTo(2)));
            assertOtherMessageHasNotBeenHandledAlready();
        }
	}
}

