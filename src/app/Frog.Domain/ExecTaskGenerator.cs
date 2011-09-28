using System.Collections.Generic;
using System.IO;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Make;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;
using Frog.Support;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<IExecTask> GimeTasks(Task task);
    }

    public class ExecTaskGenerator : IExecTaskGenerator
    {
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(ExecTaskFactory execTaskGenerator)
        {
            this.execTaskGenerator = execTaskGenerator;
        }

        public List<IExecTask> GimeTasks(Task task)
        {
            var result = new List<IExecTask>();
            if (task.GetType() == typeof (MSBuildTask))
            {
                var mstask = (MSBuildTask) task;
                result.Add(execTaskGenerator.CreateTask("xbuild", mstask.SolutionFile, "build"));
            }
            if (task.GetType() == typeof (NUnitTask))
            {
                var nunit = (NUnitTask) task;
                result.Add(execTaskGenerator.CreateTask("nunit", nunit.Assembly, "unit_test"));
            }
            if (task.GetType() == typeof(TestTaskDescription))
            {
                result.Add(new TestExecTask((task as TestTaskDescription).path, this));
            }
            if (task.GetType() == typeof(FakeTaskDescription))
            {
                result.Add(new FakeExecTask((task as FakeTaskDescription).messages, this));
            }
            if (task.GetType() == typeof(RakeTask))
            {
                result.Add(CreateShellTask(new ShellTask {cmd = "rake", args = ""}));
            }
            if(task.GetType() == typeof(BundlerTask))
            {
                result.Add(CreateShellTask(new ShellTask { cmd = "bundle", args = "install --path ~/.gem" }));
            }
            if(task.GetType() == typeof(ShellTask))
            {
                result.Add(CreateShellTask((ShellTask)task));
            }
            if(task.GetType() == typeof(MakeTask))
            {
                result.Add(execTaskGenerator.CreateTask("make", null, "Make task"));
            }
            return result;
        }

        private IExecTask CreateShellTask(ShellTask anyTask)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            args += anyTask.cmd + " " + anyTask.args;
            args = args.Trim();
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskGenerator.CreateTask(cmd, args, "Shell Task");
            return execTask;
        }
    }
}