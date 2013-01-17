using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;
using Frog.Domain.IntegrationTests.TerminalView;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.LoadTests
{
    class TerminalOutputProcessing<T> : TerminalOutputSpecBase<T> where T : View, new()
    {
        protected override void When()
        {
            terminal_id = Guid.NewGuid();
            int TERMINAL_UPDATES_COUNT = 300;
            Guid build_id = Guid.NewGuid();
            Enumerable.Range(0, TERMINAL_UPDATES_COUNT).ToList().ForEach(i => TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate(content: "content", taskIndex: 0, contentSequenceIndex: i, buildId: build_id, sequenceId: i + 2, terminalId: terminal_id)));
            TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate(content: "what?", taskIndex: 0, contentSequenceIndex: TERMINAL_UPDATES_COUNT, buildId: build_id, sequenceId: TERMINAL_UPDATES_COUNT + 2, terminalId: terminal_id));
        }

        [Test]
        public void should_have_the_status_done()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.StringEnding("what?"));
        }

    }
}
