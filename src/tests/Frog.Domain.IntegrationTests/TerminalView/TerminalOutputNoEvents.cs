using System;
using Frog.Domain.Integration.UI;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    public class TerminalOutputNoEvents<T> : TerminalOutputSpecBase<T> where T : View, new()
    {
        protected override void When()
        {
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.EqualTo(""));
        }
    }
}