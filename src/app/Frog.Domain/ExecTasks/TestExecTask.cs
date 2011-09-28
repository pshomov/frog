using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Frog.Domain.BuildSystems.Rake;
using Frog.Support;

namespace Frog.Domain.ExecTasks
{
    public abstract class TestTaskBase : IExecTask
    {
        private readonly IExecTaskGenerator execTaskGenerator;
        public event Action<string> OnTerminalOutputUpdate = s => {};

        protected TestTaskBase(IExecTaskGenerator execTaskGenerator)
        {
            this.execTaskGenerator = execTaskGenerator;
        }

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
                allLines.Where(s => s.StartsWith("exec")).ToList().ForEach(s =>
                                                                               {
                                                                                   var parsed = Regex.Match(s, @"^exec (\S+) (.*)$");
                                                                                   var tasks = execTaskGenerator.GimeTasks(
                                                                                       new ShellTask
                                                                                           {
                                                                                               cmd = parsed.Groups[1].Value,
                                                                                               args = parsed.Groups[2].Value
                                                                                           });
                                                                                   var task = tasks[0];
                                                                                   task.OnTerminalOutputUpdate +=
                                                                                       OnTaskOnOnTerminalOutputUpdate;
                                                                                   task.Perform(sourceDrop);
                                                                                   task.OnTerminalOutputUpdate -=
                                                                                       OnTaskOnOnTerminalOutputUpdate;
                                                                               });
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

        private void OnTaskOnOnTerminalOutputUpdate(string s1)
        {
            OnTerminalOutputUpdate(s1);
        }

        protected abstract string[] ReadAllLines(SourceDrop sourceDrop);
    }

    public class TestExecTask : TestTaskBase
    {
        readonly string path;

        public TestExecTask(string path, IExecTaskGenerator execTaskGenerator) : base(execTaskGenerator)
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

        public FakeExecTask(string[] tasks, IExecTaskGenerator execTaskGenerator): base(execTaskGenerator)
        {
            this.tasks = tasks;
        }

        protected override string[] ReadAllLines(SourceDrop sourceDrop)
        {
            return tasks;
        }
    }
}