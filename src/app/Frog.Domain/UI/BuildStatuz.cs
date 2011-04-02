using System.Collections.Generic;
using System.Linq;

namespace Frog.Domain.UI
{
    public class BuildStatuz
    {
        public void BuildStarted(IEnumerable<TaskInfo> taskInfos)
        {
            tasks = new List<TaskZtate>(from taskInfo in taskInfos select new TaskZtate(taskInfo.Name));
        }

        List<TaskZtate> tasks;
        public BuildTotalStatus Overall { get; private set; }

        public IEnumerable<TaskZtate> Tasks
        {
            get { return tasks; }
        }

        public void BuildUpdated(int taskIndex, TaskInfo.TaskStatus status)
        {
            tasks[taskIndex].UpdateTaskStatus(status);
        }

        public void BuildEnded(BuildTotalEndStatus overall)
        {
            Overall = overall  == BuildTotalEndStatus.Success ? BuildTotalStatus.BuildEndedSuccess : BuildTotalStatus.BuildEndedError;
        }
    }

    public enum BuildTotalStatus
    {
        BuildStarted,
        BuildEndedError,
        BuildEndedSuccess
    }

    public class TaskZtate
    {
        public string Name { get; private set; }
        readonly PipelineStatusView.TerminalOutput terminalOutput;
        TaskInfo.TaskStatus status;

        public string TerminalOutput
        {
            get { return terminalOutput.Combined; }
        }

        public void AddTerminalOutput(int sequence, string content)
        {
            terminalOutput.Add(sequence, content);
        }

        public TaskInfo.TaskStatus Status
        {
            get { return status; }
        }

        public void UpdateTaskStatus(TaskInfo.TaskStatus status)
        {
            this.status = status;
        }

        public TaskZtate(string name)
        {
            Name = name;
            terminalOutput = new PipelineStatusView.TerminalOutput();
            status = TaskInfo.TaskStatus.NotStarted;
        }
    }
}