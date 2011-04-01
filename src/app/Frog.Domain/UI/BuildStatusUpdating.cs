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

        public void BuildEnded(BuildTotalStatus overall)
        {
            Overall = overall;
        }
    }

    public class TaskZtate
    {
        public string Name { get; private set; }
        PipelineStatusView.TerminalOutput terminalOutput;
        TaskInfo.TaskStatus status;

        public string TerminalOutput
        {
            get { return terminalOutput.Combined; }
        }

        public TaskInfo.TaskStatus Status
        {
            get {
                return status;
            }
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