using System;
using System.Linq;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class TerminalOutputView
    {
        private readonly IStoreEvents eventStore;

        public TerminalOutputView(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        public TerminalOutput.Info GetTerminalOutput(Guid terminalId, int sinceIndex)
        {
            var commits = eventStore.Advanced.GetFrom(terminalId, Int32.MinValue, Int32.MaxValue);
            var terminalOutputA = new TerminalOutputA(terminalId);
            terminalOutputA.LoadsFromHistory(commits.Select(commit => (Event)commit.Events[0].Body));
            return terminalOutputA.Info(sinceIndex);
        }

        public string GetTerminalOutput(Guid terminalId)
        {
            var commits = eventStore.Advanced.GetFrom(terminalId, Int32.MinValue, Int32.MaxValue);
            var terminalOutputA = new TerminalOutputA(terminalId);
            terminalOutputA.LoadsFromHistory(commits.Select(commit => (Event)commit.Events[0].Body));
            return terminalOutputA.Value;
        }
    }

    public class TerminalOutputA : AggregateRoot
    {
        private readonly Guid terminalId;
        private TerminalOutput terminalOutput;

        public TerminalOutputA(Guid terminalId)
        {
            this.terminalId = terminalId;
            terminalOutput = new TerminalOutput();
        }

        public override Guid Id
        {
            get { return terminalId; }
        }

        void Apply(TerminalUpdate msg)
        {
            terminalOutput.Add(msg.ContentSequenceIndex, msg.Content);
        }

        public string Value { get { return terminalOutput.GetContent(0).Content; } }

        public TerminalOutput.Info Info(int sinceIndex)
        {
            return terminalOutput.GetContent(sinceIndex);
        }
    }
}