using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public class TerminalOutputEventHandler : Handles<TerminalUpdate>
    {
        readonly TerminalOutputStore reg;

        public TerminalOutputEventHandler(TerminalOutputStore reg)
        {
            this.reg = reg;
        }

        public void Handle(TerminalUpdate message)
        {
            reg.RegisterTerminalOutput(message);
        }
    }
}