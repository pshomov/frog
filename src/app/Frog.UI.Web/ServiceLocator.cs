//using Frog.Domain.Integration.UI;
using Lokad.Cqrs.AtomicStorage;
using SimpleCQRS;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static IBus Bus { get; set; }
        public static IDocumentStore Store { get; set; }
    }
}