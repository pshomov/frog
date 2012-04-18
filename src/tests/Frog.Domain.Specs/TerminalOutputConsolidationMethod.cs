using Frog.Domain.Integration.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TerminalOutputConsolidationMethod
    {
        [Test]
        public void should_combine_all_terminal_input_and_return_result()
        {
            var terminalOutput = new TerminalOutput();
            terminalOutput.Add(sequnceIndex: 0, content: "cont1");
            terminalOutput.Add(sequnceIndex: 1, content: "cont2");

            Assert.That(terminalOutput.GetContent(0).Content, Is.EqualTo("cont1cont2"));
        }

        [Test]
        public void should_order_pieces_by_content_sequnece_number()
        {
            var terminalOutput = new TerminalOutput();
            terminalOutput.Add(sequnceIndex: 2, content: "cont2");
            terminalOutput.Add(sequnceIndex: 0, content: "cont0");
            terminalOutput.Add(sequnceIndex: 1, content: "cont1");

            Assert.That(terminalOutput.GetContent(0).Content, Is.EqualTo("cont0cont1cont2"));
        }

        [Test]
        public void should_combine_only_the_available_consequitive_pieces()
        {
            var terminalOutput = new TerminalOutput();
            terminalOutput.Add(sequnceIndex: 2, content: "cont1");
            terminalOutput.Add(sequnceIndex: 0, content: "cont2");

            Assert.That(terminalOutput.GetContent(0).Content, Is.EqualTo("cont2"));
            Assert.That(terminalOutput.GetContent(0).LastChunkIndex, Is.EqualTo(1));
        }
        [Test]
        public void should_combine_only_the_available_consequitive_pieces_after_the_specified_index()
        {
            var terminalOutput = new TerminalOutput();
            terminalOutput.Add(sequnceIndex: 4, content: "cont4");
            terminalOutput.Add(sequnceIndex: 2, content: "cont2");
            terminalOutput.Add(sequnceIndex: 1, content: "cont1");
            terminalOutput.Add(sequnceIndex: 0, content: "cont0");

            var content = terminalOutput.GetContent(sinceIndex:1);
            Assert.That(content.Content, Is.EqualTo("cont1cont2"));
            Assert.That(content.LastChunkIndex, Is.EqualTo(3));
        }

        [Test]
        public void should_return_same_task_and_content_index_if_no_new_content_has_been_added()
        {
            var terminalOutput = new TerminalOutput();
            terminalOutput.Add(sequnceIndex: 4, content: "cont4");
            terminalOutput.Add(sequnceIndex: 2, content: "cont2");
            terminalOutput.Add(sequnceIndex: 1, content: "cont1");
            terminalOutput.Add(sequnceIndex: 0, content: "cont0");

            var content = terminalOutput.GetContent(0);
            var content2 = terminalOutput.GetContent(content.LastChunkIndex);
            Assert.That(content2.Content, Is.EqualTo(""));
            Assert.That(content2.LastChunkIndex, Is.EqualTo(content.LastChunkIndex));
        }

    }
}