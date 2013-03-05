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
        List<IExecTask> GimeTasks(ShellTaskk task);
        List<IExecTask> GimeTasks(TestTask task);
        List<IExecTask> GimeTasks(FakeTaskDescription task);
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

        public List<IExecTask> GimeTasks(ShellTaskk task)
        {
            return As.List(CreateShellTask(task));
        } 
        public List<IExecTask> GimeTasks(TestTask task)
        {
            return As.List((IExecTask)new TestExecTask(task.path, this));
        } 
        public List<IExecTask> GimeTasks(FakeTaskDescription task)
        {
            return As.List((IExecTask)new FakeExecTask(task.messages, this));
        } 

        readonly ExecTaskFactory execTaskFactory;
        readonly OS os;

        IExecTask CreateShellTask(ShellTaskk anyTaskk)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            var command = anyTaskk.Command + " " + anyTaskk.Arguments;
            args += command.Trim();
            args = args.Trim();
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskFactory.CreateTask(cmd, args, "Shell Task");
            return execTask;
        }
    }
}