using System;

namespace Frog.Domain
{
    public class TerminalUpdate : BuildEvent
    {
        public string Content { get; set; }

        public int ContentSequenceIndex { get; set; }
        public int TaskIndex { get; set; }

        public Guid TerminalId { get; set; }

        public string RepoURL { get; set; }

        public TerminalUpdate()
        {
        }

        public TerminalUpdate(string content, int taskIndex, int contentSequenceIndex, Guid buildId, string repoURL, int sequenceId,
                              Guid terminalId)
            : base(buildId, sequenceId)
        {
            Content = content;
            TaskIndex = taskIndex;
            ContentSequenceIndex = contentSequenceIndex;
            TerminalId = terminalId;
            RepoURL = repoURL;
        }
    }
}