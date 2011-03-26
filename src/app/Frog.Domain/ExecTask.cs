using System;
using System.ComponentModel;
using Frog.Support;
using SimpleCQRS;

namespace Frog.Domain
{
    public class ExecTaskResult
    {
        public enum Status
        {
            Success,
            Error
        };

        readonly ExecTask.ExecutionStatus executionStatus;
        readonly int exitCode;

        public ExecTaskResult(ExecTask.ExecutionStatus executionStatus, int exitCode)
        {
            this.executionStatus = executionStatus;
            this.exitCode = exitCode;
        }

        public int ExitCode
        {
            get
            {
                if (executionStatus != ExecTask.ExecutionStatus.Success)
                    throw new InvalidOperationException("Task did not execute, so there is no exit code");
                return exitCode;
            }
        }

        public bool HasExecuted
        {
            get { return executionStatus == ExecTask.ExecutionStatus.Success; }
        }

        public Status ExecStatus { get {return HasExecuted && ExitCode == 0 ? Status.Success : Status.Error;} }
    }

    public interface TaskReporter
    {
        void TaskStarted(int pid);
    }


    public class ExecTask
    {
        readonly TaskReporter taskReporter;

        public enum ExecutionStatus
        {
            Success,
            Failure
        }

        readonly string app;
        private readonly string arguments;
        readonly string name;
        public virtual event Action<string> OnTerminalOutputUpdate;

        public ExecTask(string app, string arguments, string name)
        {
            this.app = app;
            this.arguments = arguments;
            this.name = name;
        }

        public ExecTask(string app, string arguments, TaskReporter taskReporter, string name) : this(app, arguments, name)
        {
            this.taskReporter = taskReporter;
        }

        public virtual ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            ProcessWrapper process;
            try
            {
                process = new ProcessWrapper(app, arguments, sourceDrop.SourceDropLocation);
                process.Execute();
                if (taskReporter != null) taskReporter.TaskStarted(process.ProcessInfo.Id);
                var exitcode = process.WaitForProcess(60000);
                if (exitcode == 1) throw new Win32Exception();
            }
            catch (Win32Exception)
            {
                return new ExecTaskResult(ExecutionStatus.Failure, -1);
            }

            if (process.ProcessInfo.HasExited)
            {
                return new ExecTaskResult(ExecutionStatus.Success, process.ProcessInfo.ExitCode);
            }
            return new ExecTaskResult(ExecutionStatus.Failure, -1);
        }

        public string Name
        {
            get { return name; }
        }
    }
}