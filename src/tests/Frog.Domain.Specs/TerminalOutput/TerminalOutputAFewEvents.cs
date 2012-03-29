using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.TerminalUpdateSpecs
{
    class TerminalOutputAFewEvents : TerminalOutputSpecBase
    {
        protected override void When()
        {
            eventHandler.Handle(new TerminalUpdate("1", 0, 0, Guid.NewGuid(), 0, terminalId));
            eventHandler.Handle(new TerminalUpdate("2", 0, 1, Guid.NewGuid(), 1, terminalId));
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminalId), Is.EqualTo("12"));
        }
    }
}