using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Frog.Specs.Support;
using NUnit.Framework;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.Domain.IntegrationTests.RabbitMQ
{
	class MyEvent : Event{
		public string Data;
	}
	class YourEvent : Event{
		public string Data;
	}

	[TestFixture]
	public class RabbitMQBasicMessagingTest
	{
	    private RabbitMQBus bus1;
	    private RabbitMQBus bus2;

        [SetUp]
        public void Setup()
        {
            bus1 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus2 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus1.PurgeQueue("unit_test1");
            bus1.PurgeQueue("unit_test2");
            bus1.PurgeQueue("unit_test3");
            bus1.PurgeQueue("unit_test4");
            bus1.PurgeQueue("unit_test5");
            bus1.PurgeQueue("unit_test7");
        }

        [TearDown]
        public void TearDown()
        {
			try {
            bus1.Dispose();
            bus2.Dispose();
			} catch (Exception){
				// there is something wrong with doing the clean up in mono, will fix ASAP
			}
        }

        [Test]
		public void should_send_mesage_and_receive_it(){
			var msg = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg = ev.Data, "unit_test0");
			bus1.Publish(new MyEvent{Data = "unit_test1"});
            AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo("unit_test1")));
            bus1.UnregisterHandler<MyEvent>("unit_test0");
        }

        [Test]
		public void should_send_mesage_and_all_subscribers_should_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test1");
			bus1.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test2");
			bus1.Publish(new MyEvent{Data = "unit_test2"});
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo("unit_test2")));
            AssertionHelpers.WithRetries(() => Assert.That(msg2, Is.EqualTo("unit_test2")));
            bus1.UnregisterHandler<MyEvent>("unit_test1");
            bus1.UnregisterHandler<MyEvent>("unit_test2");
        }

        [Test]
		public void should_send_mesage_and_only_one_subscriber_of_the_queue_will_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, "unit_test3");
			bus2.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, "unit_test3");
            bus1.Publish(new MyEvent { Data = "unit_test3" });
            Thread.Sleep(3000);
            AssertionHelpers.WithRetries(() => Assert.That(msg1 + msg2, Is.EqualTo("unit_test3")));
            bus1.UnregisterHandler<MyEvent>("unit_test3");
        }

	    [Test]
	    public void should_continue_handling_messages_after_particular_message_causes_exception()
	    {
	        var msg1 = new List<string>();
	        var exceptions = 3;
	        bus1.RegisterHandler<MyEvent>(ev =>
	                                          {
	                                              if (ev.Data == "unit_test4_1")
	                                                  if (exceptions > 0)
	                                                  {
	                                                      exceptions--;
	                                                      throw new Exception("ouch");
	                                                  }
	                                                  else msg1.Add(ev.Data);
                                                  else msg1.Add(ev.Data);
	                                          }, "unit_test4");
	        bus1.Publish(new MyEvent {Data = "unit_test4_1"});
	        bus1.Publish(new MyEvent {Data = "unit_test4_2"});
	        AssertionHelpers.WithRetries(() => Assert.That(msg1.Count, Is.EqualTo(2)));
	        Assert.That(msg1[0], Is.EqualTo("unit_test4_1"));
	        Assert.That(msg1[1], Is.EqualTo("unit_test4_2"));
	        Assert.That(exceptions, Is.EqualTo(0));
	        bus1.UnregisterHandler<MyEvent>("unit_test4");
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
            bus1.RegisterHandler<MyEvent>(@event => handler(), "unit_test5");
            bus1.RegisterHandler<YourEvent>(@event => handler(), "unit_test5");
            bus1.Publish(new MyEvent { Data = "unit_test5_1" });
            Thread.Sleep(50);
            bus1.Publish(new YourEvent { Data = "unit_test5_2" });
            AssertionHelpers.WithRetries(() => Assert.That(messageAlreadyHandled, Is.EqualTo(2)));
            assertOtherMessageHasNotBeenHandledAlready();
            bus1.UnregisterHandler<MyEvent>("unit_test5");
            bus1.UnregisterHandler<YourEvent>("unit_test5");
        }

        [Test]
        public void should_hand_over_messages_to_available_queues()
        {
            int handler1 = 0;
            int handler2 = 0;
            bus1.RegisterHandler<MyEvent>(@event =>
                                              {
                                                  Thread.Sleep(1000);
                                                  handler1++;
                                              }, "unit_test7");
            bus2.RegisterHandler<MyEvent>(@event =>
                                              {
                                                  Thread.Sleep(1000);
                                                  handler2++;
                                              }, "unit_test7");
            bus1.Publish(new MyEvent { Data = "" });
            bus1.Publish(new MyEvent { Data = "" });
            AssertionHelpers.WithRetries(() => Assert.That(handler1, Is.EqualTo(1)));
            AssertionHelpers.WithRetries(() => Assert.That(handler2, Is.EqualTo(1)));
        }
	}
}

