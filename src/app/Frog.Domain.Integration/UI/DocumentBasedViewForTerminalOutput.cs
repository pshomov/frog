using System;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
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