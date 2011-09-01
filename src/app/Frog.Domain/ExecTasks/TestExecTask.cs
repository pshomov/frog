using System;
using System.Collections.Generic;
using System.IO;
using Frog.Support;

namespace Frog.Domain.ExecTasks
{
    public abstract class TestTaskBase : IExecTask
    {
        public event Action<string> OnTerminalOutputUpdate = s => {};

        public string Name
        {
            get { return "Test task"; }
        }

        public ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            var allLines = ReadAllLines(sourceDrop);
            foreach (var line in allLines)
            {
                OnTerminalOutputUpdate("S>" + line + "\r\n");
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

        protected abstract string[] ReadAllLines(SourceDrop sourceDrop);
    }

    public class TestExecTask : TestTaskBase
    {
        readonly string path;

        public TestExecTask(string path)
        {
            this.path = path;
        }

        protected override string[] ReadAllLines(SourceDrop sourceDrop)
        {
            return File.ReadAllLines(Path.Combine(sourceDrop.SourceDropLocation, path));
        }
    }

    class FakeExecTask : TestTaskBase
    {
        private readonly string[] tasks;

        public FakeExecTask(string[] tasks)
        {
            this.tasks = tasks;
        }

        protected override string[] ReadAllLines(SourceDrop sourceDrop)
        {
            return tasks;
        }
    }
}