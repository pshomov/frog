using System;
using Frog.Domain.Integration.UI;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    class TerminalOutputAFewEvents<T> : TerminalOutputSpecBase<T> where T : View, new()
    {

        protected override void When()
        {
            TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate("1", 0, 0, Guid.NewGuid(), 0, terminal_id));
            TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate("2", 0, 1, Guid.NewGuid(), 1, terminal_id));
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.EqualTo("12"));
        }
    }
}