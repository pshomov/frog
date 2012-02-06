using System.Collections.Generic;
using System.Linq;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.EventStore.Tests
{
    public abstract class StoreEventsSpecsBase : BDD

    {
        protected EStore store;
        protected MediaStore media;

        protected override void Given()
        {
            media = Substitute.For<MediaStore>();
            store = new EStore(media);
        }
    }

    class AppendEventOnly : StoreEventsSpecsBase
    {
        protected override void When()
        {
            store.AppendEvent("category", new SomeEvent());
        }

        [Test]
        public void should_send_event_to_media()
        {
            media.DidNotReceive().Append("category", Arg.Any<IList<object>>());
        }
    }

    class AppendAndRetrieveEvents : StoreEventsSpecsBase
    {
        private IEnumerable<object> events;

        protected override void When()
        {
            media.GetAll("c1").Returns(new List<object>(){{new SomeEvent()}});
            store.AppendEvent("c1", new SomeEvent());
            events = store.GetEvents("c1");
        }

        [Test]
        public void should_store_events_to_media()
        {
            media.Received().Append("c1", Arg.Any<IList<object>>());
        }

        [Test]
        public void should_retrieve_all_events_from_media()
        {
            media.Received().GetAll("c1");
        }


        [Test]
        public void should_return_the_events_from_the_store()
        {
            Assert.That(events.ToList().Count, Is.EqualTo(1));
        }
    }

    public interface MediaStore
    {
        void Append(string category, IList<object> events);
        IEnumerable<object> GetAll(string category);
    }

    public class SomeEvent
    {
    }

    public class EStore
    {
        private readonly MediaStore mediaStore;
        private Dictionary<string, List<object>> uncommittedEvents = new Dictionary<string, List<object>>();

        public EStore(MediaStore mediaStore)
        {
            this.mediaStore = mediaStore;
        }

        public void AppendEvent(string category, object @event)
        {
            var commits = new List<object>();
            uncommittedEvents.Add(category, commits);
            commits.Add(@event);
        }

        public IEnumerable<object> GetEvents(string category)
        {
            mediaStore.Append(category, uncommittedEvents[category]);
            return mediaStore.GetAll(category);
        }
    }
}
