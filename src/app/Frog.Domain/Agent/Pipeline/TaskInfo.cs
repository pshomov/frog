using System;

namespace Frog.Domain
{
    public class TaskInfo
    {
        public enum TaskStatus
        {
            NotStarted,
            Started,
            FinishedSuccess,
            FinishedError
        }

        public string Name;
        public TaskStatus Status { get; set; }
        public Guid TerminalId { get; set; }

        public TaskInfo()
        {
        }

        public TaskInfo(string name, Guid terminalId)
        {
            TerminalId = terminalId;
            Name = name;
            Status = TaskStatus.NotStarted;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Status {1}, Id {2}", Name, Status, TerminalId);
        }
    }
}