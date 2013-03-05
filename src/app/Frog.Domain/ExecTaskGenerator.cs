using System;
using System.Collections.Generic;
using Frog.Domain.BuildSystems;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<IExecTask> GimeTasks(Task task);
    }

    public enum OS
    {
        Unix,
        Linux,
        OSX,
        Windows
    };

    public class ExecTaskGenerator : IExecTaskGenerator
    {
        public ExecTaskGenerator(ExecTaskFactory execTaskFactory, OS os)
        {
            this.execTaskFactory = execTaskFactory;
            this.os = os;
        }

        public List<IExecTask> GimeTasks(Task task)
        {
            var result = new List<IExecTask>();
            if (task.GetType() == typeof (MSBuildTask))
            {
                var mstask = (MSBuildTask) task;
                result.Add(execTaskFactory.CreateTask(os == OS.Windows ? ExtensionMethods.format("{0}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe", Environment.GetEnvironmentVariable("SYSTEMROOT")) : "xbuild", mstask.SolutionFile, "build"));
            }
            if (task.GetType() == typeof (NUnitTask))
            {
                var nunit = (NUnitTask) task;
                result.Add(execTaskFactory.CreateTask("nunit", nunit.Assembly, "unit_test"));
            }
            if (task.GetType() == typeof (TestTask))
            {
                result.Add(new TestExecTask((task as TestTask).path, this));
            }
            if (task.GetType() == typeof (FakeTaskDescription))
            {
                result.Add(new FakeExecTask((task as FakeTaskDescription).messages, this));
            }
            if (task.GetType() == typeof (ShellTask))
            {
                result.Add(CreateShellTask((ShellTask) task));
            }
            return result;
        }

        readonly ExecTaskFactory execTaskFactory;
        readonly OS os;

        IExecTask CreateShellTask(ShellTask anyTaskk)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            var command = anyTaskk.cmd + " " + anyTaskk.args;
            args += command.Trim();
            args = args.Trim();
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskFactory.CreateTask(cmd, args, "Shell Task");
            return execTask;
        }
    }
}