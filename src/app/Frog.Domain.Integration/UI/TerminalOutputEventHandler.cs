using System;
using System.Linq;
using EventStore;
using MongoDB.Bson;
using MongoDB.Driver;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public class TerminalOutputEventHandler : Handles<TerminalUpdate>
    {
        readonly TerminalOutputRegister reg;

        public TerminalOutputEventHandler(TerminalOutputRegister reg)
        {
            this.reg = reg;
        }

        public void Handle(TerminalUpdate message)
        {
            reg.RegisterTerminalOutput(message.TerminalId, message.SequenceId, message.Content);
        }
    }
}