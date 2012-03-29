using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.TerminalUpdateSpecs
{
    [TestFixture]
    public class TerminalOutputNoEvents : TerminalOutputSpecBase
    {
        protected override void When()
        {
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminalId), Is.EqualTo(""));
        }
    }
}