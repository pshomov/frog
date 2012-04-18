using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    [TestFixture]
    public class AddTerminalOutput : BDD
    {
        TerminalOutputRegister register;
        TerminalOutputView view;
        Guid terminal_id;

        protected override void Given()
        {
            register = new TerminalOutputRegister(OSHelpers.TerminalViewConnection());
            view = new TerminalOutputView(OSHelpers.TerminalViewConnection());
        }

        protected override void When()
        {
            terminal_id = Guid.NewGuid();
            register.RegisterTerminalOutput(terminal_id, 0, "c1");
        }

        [Test]
        public void should_have_content_in_view()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.EqualTo("c1"));
        }

    }
}
