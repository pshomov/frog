using System;
using System.Collections.Generic;
using Frog.Domain.Integration;
using Frog.Support;
using Frog.UI.Web2;
using Lokad.Cqrs;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Nancy.Responses;
using Nancy.TinyIoc;
using SaaS.Wires;

namespace Frog.UI.Web2
{
    class Program
    {
        static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new HostConfiguration(){RewriteLocalhost = true}, new Uri("http://localhost:8888/"));
            nancyHost.Start();
            Console.WriteLine("Nancy now listening - Press enter to stop");
            Console.ReadKey();
            nancyHost.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }

    public class Fle : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            WireUpUIModelInfrastructure();
        }

        static void WireUpUIModelInfrastructure()
        {
            var theBus = new RabbitMQBus(OSHelpers.RabbitHost());
            var config = FileStorage.CreateConfig(OSHelpers.LokadStorePath());

            ServiceLocator.Bus = theBus;
            ServiceLocator.Store = config.CreateDocumentStore(new ViewStrategy());
        }
    }

}
