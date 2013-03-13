using System;

namespace Frog.Domain
{
    public class BuildUpdated : BuildEvent
    {
        public int TaskIndex { get; set; }

        public TaskInfo.TaskStatus TaskStatus { get; set; }

        public Guid TerminalId { get;  set; }
        public string RepoURL { get;  set; }

        public BuildUpdated()
        {
        }

        public BuildUpdated(Guid buildId, string repoUrl, int taskIndex, TaskInfo.TaskStatus newStatus, int sequenceId, Guid terminalId)
            : base(buildId, sequenceId)
        {
            TaskIndex = taskIndex;
            TaskStatus = newStatus;
            TerminalId = terminalId;
            RepoURL = repoUrl;
        }
    }
}