using System;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    public class TerminalOutputOneUpdateSpec<T> : TerminalOutputSpecBase<T> where T : View, new()
    {
        protected override void When()
        {
            TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate("1", 0, 0, Guid.NewGuid(), 0, terminal_id));
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.EqualTo("1"));
        }
    }
}
