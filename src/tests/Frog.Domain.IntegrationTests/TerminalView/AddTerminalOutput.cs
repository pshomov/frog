using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.TerminalView
{
    public class AddTerminalOutput<T> : TerminalOutputSpecBase<T> where T : View, new()
    {
        protected override void When()
        {
            TerminalOutputStore.RegisterTerminalOutput(new TerminalUpdate {TerminalId = terminal_id, ContentSequenceIndex = 0, Content = "c1"});
        }

        [Test]
        public void should_have_content_in_view()
        {
            Assert.That(view.GetTerminalOutput(terminal_id), Is.EqualTo("c1"));
        }

    }
}
