using System;

namespace Frog.Domain.Integration.UI
{
    public interface ViewForTerminalOutput
    {
        TerminalOutput.Info GetTerminalOutput(Guid terminalId, int sinceIndex);
        string GetTerminalOutput(Guid terminalId);
    }
}