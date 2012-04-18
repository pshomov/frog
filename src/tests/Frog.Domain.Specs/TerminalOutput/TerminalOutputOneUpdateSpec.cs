using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.TerminalUpdateSpecs
{
    [TestFixture]
    public class TerminalOutputOneUpdateSpec : TerminalOutputSpecBase
    {
        protected override void When()
        {
            eventHandler.Handle(new TerminalUpdate("1", 0, 0, Guid.NewGuid(), 0, terminalId));
        }

        [Test]
        public void should_have_content_as_is()
        {
            Assert.That(view.GetTerminalOutput(terminalId), Is.EqualTo("1"));
        }
    }
}
