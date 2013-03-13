using System;

namespace Frog.Domain
{
    public class TerminalUpdateInfo
    {
        public string Content { get; private set; }
        public int ContentSequenceIndex { get; private set; }
        public int TaskIndex { get; private set; }
        public Guid TerminalId { get; private set; }

        public TerminalUpdateInfo(int contentSequenceIndex, string content, int taskIndex, Guid terminalId)
        {
            ContentSequenceIndex = contentSequenceIndex;
            Content = content;
            TaskIndex = taskIndex;
            TerminalId = terminalId;
        }
    }
}