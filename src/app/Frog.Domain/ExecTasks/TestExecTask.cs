using System;
using System.Collections.Generic;
using System.IO;
using Frog.Support;

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
            var allLines = File.ReadAllLines(Path.Combine(sourceDrop.SourceDropLocation, path));
            foreach (var line in allLines)
            {
                OnTerminalOutputUpdate("S>" + line + Environment.NewLine);
            }
            var execStatus = ExecutionStatus.Success;
            if (allLines.Length > 0)
            {
                var exits = new Dictionary<string, ExecutionStatus>
                                {
                                    {"OK", ExecutionStatus.Success},
                                    {"ERROR", ExecutionStatus.Failure}
                                };
                var lastItem = allLines.LastItem().ToUpper();
                if (lastItem == "EXCEPTION") 
                    throw new Exception("Bad things happened here");
                if (exits.ContainsKey(lastItem))
                {
                    execStatus = exits[lastItem];
                }
            }
            return new ExecTaskResult(execStatus, 0);
        }
    }
}