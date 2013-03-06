using System.Collections.Generic;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<IExecTask> GimeTasks(ShellTask task);
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
        public ExecTaskGenerator(ExecTaskFactory execTaskFactory)
        {
            this.execTaskFactory = execTaskFactory;
        }

        public List<IExecTask> GimeTasks(ShellTask task)
        {
            return As.List(CreateShellTask(task));
        } 
        public List<IExecTask> GimeTasks(TestTask task)
        {
            return As.List((IExecTask)new TestExecTask(task.Path, this));
        } 
        public List<IExecTask> GimeTasks(FakeTaskDescription task)
        {
            return As.List((IExecTask)new FakeExecTask(task.messages, this));
        } 

        readonly ExecTaskFactory execTaskFactory;

        IExecTask CreateShellTask(ShellTask anyTask)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            var command = anyTask.Command + " " + anyTask.Arguments;
            args += command.Trim();
            args = args.Trim();
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskFactory.CreateTask(cmd, args, "Shell Task");
            return execTask;
        }
    }
}