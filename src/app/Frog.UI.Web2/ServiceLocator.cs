using Lokad.Cqrs.AtomicStorage;
using SimpleCQRS;

namespace Frog.UI.Web2
{
    public static class ServiceLocator
    {
        public static IBus Bus { get; set; }
        public static IDocumentStore Store { get; set; }
    }
}