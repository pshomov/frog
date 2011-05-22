using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Frog.Domain.Integration;
using Frog.Specs.Support;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.IntegrationTests
{
    [TestFixture]
    class MessageBusChecksIfShouldContinue
    {
        RabbitMQBus bus;

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
        public void should_ask_wether_to_continue_messaging_after_each_handled_message_in_single_queue()
        {
            PurgeMessages("should_continue");
            bus.StopHandling = () => true;
            int[] messagesHandled = {0};
            bus.RegisterHandler<MyEvent>(ev => { messagesHandled[0]++; }, "should_continue");
            bus.Publish(new MyEvent());
            bus.Publish(new MyEvent());
            bus.WaitForHandlersToFinish(1000);
            Assert.That(messagesHandled[0], Is.EqualTo(1));
        }

        [Test]
        public void should_ask_wether_to_continue_messaging_after_each_handled_message_in_multiple_queues()
        {
            PurgeMessages("should_continue_1");
            PurgeMessages("should_continue_2");
            bus.StopHandling = () => true;
            int[] messagesHandled = {0};
            bus.RegisterHandler<MyEvent>(ev => { messagesHandled[0]++; }, "should_continue_1");
            bus.RegisterHandler<MyEvent>(ev => { messagesHandled[0]++; }, "should_continue_2");
            bus.Publish(new MyEvent());
            bus.Publish(new MyEvent());
            bus.WaitForHandlersToFinish(1000);
            Assert.That(messagesHandled[0], Is.EqualTo(2));
        }

        private void PurgeMessages(string queue)
        {
            IBusManagement busManagement = bus;
            busManagement.PurgeQueue(queue);
        }
    }
}
