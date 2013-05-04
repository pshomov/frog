using System;
using System.Threading;
using NUnit.Framework;
using Frog.Domain.Integration;
using SimpleCQRS;

namespace Frog.Domain.IntegrationTests.RabbitMQ
{
	[TestFixture]
	public class RabbitMQBasicSingleHandlerTests
	{
	    private const string q1 = "unit_test0";
	    private const string q2 = "unit_test1";
	    private RabbitMQBus bus1;
	    private RabbitMQBus bus2;

        [SetUp]
        public void Setup()
        {
            bus1 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus2 = new RabbitMQBus(Environment.GetEnvironmentVariable("RUNZ_TEST_RABBITMQ_SERVER") ?? "localhost");
            bus1.PurgeQueue(q1);
            bus1.PurgeQueue(q2);
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

        public class MyCommand : Command
        {
            public string Data;
        }

        [Test]
		public void should_send_message_and_only_specified_receiver_receives_it(){
			var msg = "";
			bus1.RegisterDirectHandler<MyCommand>(ev =>  msg += ev.Data, q1);
			bus2.RegisterDirectHandler<MyCommand>(ev =>  msg += ev.Data, q2);
			bus1.Send(new MyCommand(){Data = "fff"}, q1);
            Thread.Sleep(3000);
            Assert.That(msg, Is.Not.EqualTo("ffffff"));
            Assert.That(msg, Is.EqualTo("fff"));
            bus1.UnregisterHandler<MyCommand>(q1);
            bus2.UnregisterHandler<MyCommand>(q2);
        }

	}
}

