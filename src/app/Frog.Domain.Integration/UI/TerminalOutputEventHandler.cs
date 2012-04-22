using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public class TerminalOutputEventHandler : Handles<TerminalUpdate>
    {
        readonly RegisterTerminalOutput reg;

        public TerminalOutputEventHandler(RegisterTerminalOutput reg)
        {
            this.reg = reg;
        }

        public void Handle(TerminalUpdate message)
        {
            reg.RegisterTerminalOutput(message);
        }
    }
}