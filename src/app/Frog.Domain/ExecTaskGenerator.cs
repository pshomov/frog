using System.Collections.Generic;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<ExecTask> GimeTasks(ShellTaskDescription taskDescription);
        List<ExecTask> GimeTasks(TestTaskDescription taskDescription);
        List<ExecTask> GimeTasks(FakeTaskDescription task);
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

        public List<ExecTask> GimeTasks(ShellTaskDescription taskDescription)
        {
            return As.List(CreateShellTask(taskDescription));
        } 
        public List<ExecTask> GimeTasks(TestTaskDescription taskDescription)
        {
            return As.List((ExecTask)new TestExecTask(taskDescription.Path, this));
        } 
        public List<ExecTask> GimeTasks(FakeTaskDescription task)
        {
            return As.List((ExecTask)new FakeExecTask(task.messages, this));
        } 

        readonly ExecTaskFactory execTaskFactory;

        ExecTask CreateShellTask(ShellTaskDescription anyTaskDescription)
        {
            var cmd = Os.IsUnix ? "/bin/bash" : "cmd.exe";
            var args = Os.IsUnix ? "-c \"" : "/c ";
            var command = anyTaskDescription.Command + " " + anyTaskDescription.Arguments;
            args += command.Trim();
            args = args.Trim();
            if (Os.IsUnix) args += "\"";

            var execTask = execTaskFactory.CreateOSExecutableTask(cmd, args, anyTaskDescription.Name.IsNullOrEmpty() ? cmd + " "+args : anyTaskDescription.Name);
            return execTask;
        }
    }
}