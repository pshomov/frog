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
        RegisterTerminalOutput Registrar { get; }
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
                return new ViewForTerminalOutputImpl(event_store);
            }
        }

        public RegisterTerminalOutput Registrar
        {
            get { return new EventBasedRegistrar(event_store); }
        }
    }

    public class MongoView : View
    {
        Func<ViewForTerminalOutput> view =
            () => new TerminalOutputView(OSHelpers.TerminalViewConnection());

        Func<RegisterTerminalOutput> registrar =
            () => new TerminalOutputRegister(OSHelpers.TerminalViewConnection());

        public ViewForTerminalOutput View
        {
            get { return view(); }
        }

        public RegisterTerminalOutput Registrar
        {
            get { return registrar(); }
        }
    }

    [TestFixture(typeof(MongoView))]
    [TestFixture(typeof(EventsView))]
    public abstract class TerminalOutputSpecBase<T> : BDD where T : View, new()
    {
        protected RegisterTerminalOutput register;
        protected ViewForTerminalOutput view;
        protected Guid terminal_id;

        protected override void Given()
        {
            terminal_id = Guid.NewGuid();
            var v = new T();
            register = v.Registrar;
            view = v.View;
        }
    }
}