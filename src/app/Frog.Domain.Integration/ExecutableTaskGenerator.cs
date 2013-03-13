using System.Collections.Generic;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Domain.Integration
{
    public enum OS
    {
        Unix,
        Linux,
        OSX,
        Windows
    };

    public class ExecutableTaskGenerator : ExecTaskGenerator
    {
        public ExecutableTaskGenerator(ExecTaskFactory execTaskFactory)
        {
            this.execTaskFactory = execTaskFactory;
        }

        public List<ExecutableTask> GimeTasks(ShellTaskDescription taskDescription)
        {
            return As.List(CreateShellTask(taskDescription));
        } 
        public List<ExecutableTask> GimeTasks(TestTaskDescription taskDescription)
        {
            return As.List((ExecutableTask)new TestExecTask(taskDescription.Path, this));
        } 
        public List<ExecutableTask> GimeTasks(FakeTaskDescription taskDescription)
        {
            return As.List((ExecutableTask)new FakeExecTask(taskDescription.messages, this));
        } 

        readonly ExecTaskFactory execTaskFactory;

        ExecutableTask CreateShellTask(ShellTaskDescription anyTaskDescription)
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