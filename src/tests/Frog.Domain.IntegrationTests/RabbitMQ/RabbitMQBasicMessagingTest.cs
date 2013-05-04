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
	    private const string unitTest7 = "unit_test7";
	    private const string unitTest5 = "unit_test5";
	    private const string unitTest4 = "unit_test4";
	    private const string unitTest3 = "unit_test3";
	    private const string unitTest2 = "unit_test2";
	    private const string unitTest1 = "unit_test1";
	    private const string unitTest0 = "unit_test0";
	    private RabbitMQBus bus1;
	    private RabbitMQBus bus2;

        [SetUp]
        public void Setup()
        {
            bus1 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus2 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus1.PurgeQueue(unitTest0);
            bus1.PurgeQueue(unitTest1);
            bus1.PurgeQueue(unitTest2);
            bus1.PurgeQueue(unitTest3);
            bus1.PurgeQueue(unitTest4);
            bus1.PurgeQueue(unitTest5);
            bus1.PurgeQueue(unitTest7);
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
		public void should_send_message_and_receive_it(){
			var msg = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg = ev.Data, unitTest0);
			bus1.Publish(new MyEvent{Data = unitTest1});
            AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo(unitTest1)));
            bus1.UnregisterHandler<MyEvent>(unitTest0);
        }

        [Test]
		public void should_send_message_and_all_queues_should_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, unitTest1);
			bus1.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, unitTest2);
			bus1.Publish(new MyEvent{Data = unitTest2});
            AssertionHelpers.WithRetries(() => Assert.That(msg1, Is.EqualTo(unitTest2)));
            AssertionHelpers.WithRetries(() => Assert.That(msg2, Is.EqualTo(unitTest2)));
            bus1.UnregisterHandler<MyEvent>(unitTest1);
            bus1.UnregisterHandler<MyEvent>(unitTest2);
        }

        [Test]
		public void should_send_mesage_and_only_one_subscriber_of_the_queue_will_receive_it(){
			var msg1 = "";
			var msg2 = "";
			bus1.RegisterHandler<MyEvent>(ev =>  msg1 = ev.Data, unitTest3);
			bus2.RegisterHandler<MyEvent>(ev =>  msg2 = ev.Data, unitTest3);
            bus1.Publish(new MyEvent { Data = unitTest3 });
            Thread.Sleep(3000);
            AssertionHelpers.WithRetries(() => Assert.That(msg1 + msg2, Is.EqualTo(unitTest3)));
            bus1.UnregisterHandler<MyEvent>(unitTest3);
            bus2.UnregisterHandler<MyEvent>(unitTest3);
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
	                                          }, unitTest4);
	        bus1.Publish(new MyEvent {Data = "unit_test4_1"});
	        bus1.Publish(new MyEvent {Data = "unit_test4_2"});
	        AssertionHelpers.WithRetries(() => Assert.That(msg1.Count, Is.EqualTo(2)));
	        Assert.That(msg1[0], Is.EqualTo("unit_test4_1"));
	        Assert.That(msg1[1], Is.EqualTo("unit_test4_2"));
	        Assert.That(exceptions, Is.EqualTo(0));
	        bus1.UnregisterHandler<MyEvent>(unitTest4);
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
            bus1.RegisterHandler<MyEvent>(@event => handler(), unitTest5);
            bus1.RegisterHandler<YourEvent>(@event => handler(), unitTest5);
            bus1.Publish(new MyEvent { Data = "unit_test5_1" });
            Thread.Sleep(50);
            bus1.Publish(new YourEvent { Data = "unit_test5_2" });
            AssertionHelpers.WithRetries(() => Assert.That(messageAlreadyHandled, Is.EqualTo(2)));
            assertOtherMessageHasNotBeenHandledAlready();
            bus1.UnregisterHandler<MyEvent>(unitTest5);
            bus1.UnregisterHandler<YourEvent>(unitTest5);
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
                                              }, unitTest7);
            bus2.RegisterHandler<MyEvent>(@event =>
                                              {
                                                  Thread.Sleep(1000);
                                                  handler2++;
                                              }, unitTest7);
            bus1.Publish(new MyEvent { Data = "" });
            bus1.Publish(new MyEvent { Data = "" });
            AssertionHelpers.WithRetries(() => Assert.That(handler1, Is.EqualTo(1)));
            AssertionHelpers.WithRetries(() => Assert.That(handler2, Is.EqualTo(1)));
            bus1.UnregisterHandler<MyEvent>(unitTest7);
            bus2.UnregisterHandler<MyEvent>(unitTest7);
        }
	}
}

