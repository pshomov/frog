namespace Frog.Domain.Integration.UI
{
    public interface TerminalOutputStore
    {
        void RegisterTerminalOutput(TerminalUpdate message);
    }
}