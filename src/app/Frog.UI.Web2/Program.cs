using System;
using System.Collections.Generic;
using Frog.Domain.Integration;
using Frog.Support;
using Frog.UI.Web2;
using Frog.WiredUp;
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
            ServiceLocator.Bus = new RabbitMQBus(OSHelpers.RabbitHost());
            ServiceLocator.Store = Setup.GetDocumentStore(OSHelpers.LokadStorePath(), Setup.OpensourceCustomer);
        }
    }

}
