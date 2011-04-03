using System;
using System.Collections.Generic;
using System.IO;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<IExecTask> GimeTasks(ITask task);
    }

    public class ExecTaskGenerator : IExecTaskGenerator
    {
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(ExecTaskFactory execTaskGenerator)
        {
            this.execTaskGenerator = execTaskGenerator;
        }

        public List<IExecTask> GimeTasks(ITask task)
        {
            var result = new List<IExecTask>();
            if (task.GetType() == typeof (MSBuildTaskDescriptions))
            {
                var mstask = (MSBuildTaskDescriptions) task;
                result.Add(execTaskGenerator.CreateTask("xbuild", mstask.SolutionFile, "build"));
            }
            if (task.GetType() == typeof (NUnitTask))
            {
                var nunit = (NUnitTask) task;
                result.Add(execTaskGenerator.CreateTask("nunit", nunit.Assembly, "unit_test"));
            }
            if(task.GetType() == typeof(TestTaskDescription))
            {
                result.Add(new TestExecTask((task as TestTaskDescription).path));
            }
            return result;
        }
    }

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
            return new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0);
        }
    }
}