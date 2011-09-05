using System.Collections.Generic;
using System.IO;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.CustomTasks;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;
using Frog.Support;

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
                result.Add(CreateShellTask(new ShellTask {cmd = Path.Combine(Underware.GitProductionScriptsLocation,"ruby_tasks.rb"), args = "rake"}));
            }
            if(task.GetType() == typeof(BundlerTask))
            {
                result.Add(CreateShellTask(new ShellTask { cmd = Path.Combine(Underware.GitProductionScriptsLocation, "ruby_tasks.rb"), args = "bundler" }));
            }
            if(task.GetType() == typeof(ShellTask))
            {
                result.Add(CreateShellTask((ShellTask)task));
            }
            return result;
        }

        private IExecTask CreateShellTask(ShellTask anyTask)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            args += anyTask.cmd + " " + anyTask.args;
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskGenerator.CreateTask(cmd, args, "Shell Task");
            return execTask;
        }
    }
}