using System;
using Frog.Domain.Integration;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.RabbitMQ
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
            bus.WaitForHandlersToFinish(7000);
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
            bus.WaitForHandlersToFinish(7000);
            Assert.That(messagesHandled[0], Is.EqualTo(2));
        }

        private void PurgeMessages(string queue)
        {
            IBusManagement busManagement = bus;
            try
            {
                busManagement.PurgeQueue(queue);
            }
            catch
            {
                // if it fails, it is not a big deal. Probably means queue did not exist
            }
        }
    }
}
