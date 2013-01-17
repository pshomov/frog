using EventStore;
using EventStore.Serialization;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture, Ignore]
    public class EventBasedProjectView : ProjectViewSpecs
    {
        protected override Integration.UI.ProjectView GetProjectView()
        {
            var eventStore = StoreFactory.WireupEventStore();
            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();
            return new Integration.UI.EventBasedProjectView(eventStore);
        }
    }
}