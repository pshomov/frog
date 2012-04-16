using System;
using System.Linq;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class TerminalOutputView
    {
        public TerminalOutputView(IStoreEvents eventStore)
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

    public class TerminalOutputA : AggregateRoot
    {
        public override Guid Id
        {
            get { return terminalId; }
        }

        public string Value
        {
            get { return terminalOutput.GetContent(0).Content; }
        }

        public TerminalOutputA(Guid terminalId)
        {
            this.terminalId = terminalId;
            terminalOutput = new TerminalOutput();
        }

        public TerminalOutput.Info Info(int sinceIndex)
        {
            return terminalOutput.GetContent(sinceIndex);
        }

        readonly Guid terminalId;
        readonly TerminalOutput terminalOutput;

        void Apply(TerminalUpdate msg)
        {
            terminalOutput.Add(msg.ContentSequenceIndex, msg.Content);
        }
    }
}