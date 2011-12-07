using System.Linq;
using Frog.Domain.Integration;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.LoadTests
{
    [TestFixture]
    class MessagePublishingAndConsuming
    {
        private RabbitMQBus theBus;

        [SetUp]
        public void Setup()
        {
            theBus = new RabbitMQBus(OSHelpers.RabbitHost());
        }

        public class Event1 : Event
        {
            public string Data;
        }
        public class Event2 : Event
        {
        }

        [Test]
        public void should_publish_1000_messages()
        {
            Enumerable.Range(1,1000).ToList().ForEach(i => theBus.Publish(new Event1 { Data = "unit_test1" }));
        }

        [Test]
        public void should_publish_and_consume_1000_messages()
        {
            int msg = 0;
            theBus.RegisterHandler<Event2>(ev => msg++, "load_test1");
            Enumerable.Range(1, 1000).ToList().ForEach(i => theBus.Publish(new Event2())); 
            AssertionHelpers.WithRetries(() => Assert.That(msg, Is.EqualTo(1000)));
            theBus.UnregisterHandler<Event2>("load_test1");
        }

    }
}
