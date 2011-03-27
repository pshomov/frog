using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TerminalOutputConsolidationMethod
    {
        [Test]
        public void should_combine_all_terminal_input_and_return_result()
        {
            var terminalOutput = new PipelineStatusView.TerminalOutput();
            terminalOutput.Add(sequnceIndex: 0, content: "cont1");
            terminalOutput.Add(sequnceIndex: 1, content: "cont2");

            Assert.That(terminalOutput.Combined, Is.EqualTo("cont1cont2"));
        }

        [Test]
        public void should_order_pieces_by_content_sequnece_number()
        {
            var terminalOutput = new PipelineStatusView.TerminalOutput();
            terminalOutput.Add(sequnceIndex: 2, content: "cont2");
            terminalOutput.Add(sequnceIndex: 0, content: "cont0");
            terminalOutput.Add(sequnceIndex: 1, content: "cont1");

            Assert.That(terminalOutput.Combined, Is.EqualTo("cont0cont1cont2"));
        }

        [Test]
        public void should_combine_only_the_available_consequitive_pieces()
        {
            var terminalOutput = new PipelineStatusView.TerminalOutput();
            terminalOutput.Add(sequnceIndex: 2, content: "cont1");
            terminalOutput.Add(sequnceIndex: 0, content: "cont2");

            Assert.That(terminalOutput.Combined, Is.EqualTo("cont2"));
        }

    }
}