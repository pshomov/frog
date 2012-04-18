using System;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;
using Frog.Support;

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
            eventHandler = new TerminalOutputEventHandler(new TerminalOutputRegister(OSHelpers.TerminalViewConnection()));
            view = new TerminalOutputView(OSHelpers.TerminalViewConnection());
        }
    }
}