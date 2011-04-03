using System;
using System.IO;

namespace Frog.Domain.ExecTasks
{
    public class TestExecTask : IExecTask
    {
        readonly string path;

        public TestExecTask(string path)
        {
            this.path = path;
        }

        public event Action<string> OnTerminalOutputUpdate;

        public string Name
        {
            get { return "Test task"; }
        }

        public ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            foreach(var line in File.ReadAllLines(Path.Combine(sourceDrop.SourceDropLocation, path)))
            {
                OnTerminalOutputUpdate("S>" + line+Environment.NewLine);
            }
            return new ExecTaskResult(ExecutionStatus.Success, 0);
        }
    }
}