using System;
using System.Linq;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public class EventBasedViewForTerminalOutput : ViewForTerminalOutput
    {
        public EventBasedViewForTerminalOutput(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        public TerminalOutput.Info GetTerminalOutput(Guid terminalId, int sinceIndex)
        {
            var commits = eventStore.Advanced.GetFrom(terminalId, Int32.MinValue, Int32.MaxValue);
            var terminalOutputA = new TerminalOutputA(terminalId);
            terminalOutputA.LoadsFromHistory(commits.Select(commit => (Event) commit.Events[0].Body));
            return terminalOutputA.Info(sinceIndex);
        }

        public string GetTerminalOutput(Guid terminalId)
        {
            var commits = eventStore.Advanced.GetFrom(terminalId, Int32.MinValue, Int32.MaxValue);
            var terminalOutputA = new TerminalOutputA(terminalId);
            terminalOutputA.LoadsFromHistory(commits.Select(commit => (Event) commit.Events[0].Body));
            return terminalOutputA.Value;
        }

        readonly IStoreEvents eventStore;
    }
}