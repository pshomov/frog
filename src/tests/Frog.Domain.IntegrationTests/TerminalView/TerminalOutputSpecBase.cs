using System;
using EventStore;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    public interface View
    {
        ViewForTerminalOutput View { get; }
        TerminalOutputStore Store { get; }
    }

    public class EventsView : View
    {
        IStoreEvents event_store;

        public EventsView()
        {
            event_store = StoreFactory.WireupEventStore();
        }

        public ViewForTerminalOutput View
        {
            get
            {
                return new EventBasedViewForTerminalOutput(event_store);
            }
        }

        public TerminalOutputStore Store
        {
            get { return new EventBasedTerminalOutputStore(event_store); }
        }
    }

    public class MongoView : View
    {
        Func<ViewForTerminalOutput> view =
            () => new DocumentBasedViewForTerminalOutput(OSHelpers.TerminalViewConnection());

        Func<TerminalOutputStore> registrar =
            () => new StateBasedTerminalOutputStore(OSHelpers.TerminalViewConnection());

        public ViewForTerminalOutput View
        {
            get { return view(); }
        }

        public TerminalOutputStore Store
        {
            get { return registrar(); }
        }
    }

    [TestFixture(typeof(MongoView))]
    [TestFixture(typeof(EventsView))]
    public abstract class TerminalOutputSpecBase<T> : BDD where T : View, new()
    {
        protected TerminalOutputStore TerminalOutputStore;
        protected ViewForTerminalOutput view;
        protected Guid terminal_id;

        protected override void Given()
        {
            terminal_id = Guid.NewGuid();
            var v = new T();
            TerminalOutputStore = v.Store;
            view = v.View;
        }
    }
}