using System;
using Frog.Domain.UI;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.TerminalUpdateSpecs
{
    public abstract class TerminalOutputSpecBase : BDD
    {
        protected TerminalOutputEventHandler eventHandler;
        protected TerminalOutputView view;
        protected Guid terminalId;

        protected override void Given()
        {
            terminalId = Guid.NewGuid();
            var eventStore = StoreFactory.WireupEventStore();
            eventHandler = new TerminalOutputEventHandler(eventStore);
            view = new TerminalOutputView(eventStore);
        }
    }
}