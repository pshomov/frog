using System;
using System.ComponentModel;
using System.Diagnostics;
using Frog.Support;

namespace Frog.Domain.Specs
{
    public class ExecTaskResult
    {
        public enum Status
        {
            Success,
            Error
        };

        readonly ExecTask.ExecutionStatus _executionStatus;
        readonly int _exitCode;

        public ExecTaskResult(ExecTask.ExecutionStatus executionStatus, int exitCode)
        {
            _executionStatus = executionStatus;
            _exitCode = exitCode;
        }

        public int ExitCode
        {
            get
            {
                if (_executionStatus != ExecTask.ExecutionStatus.Success)
                    throw new InvalidOperationException("Task did not execute, so there is no exit code");
                return _exitCode;
            }
        }

        public bool IsExecuted
        {
            get { return _executionStatus == ExecTask.ExecutionStatus.Success; }
        }

        public Status ExecStatus { get {return IsExecuted && ExitCode == 0 ? Status.Success : Status.Error;} }
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

        readonly string _app;
        private readonly string _arguments;

        public ExecTask(string app, string arguments)
        {
            _app = app;
            _arguments = arguments;
        }

        public ExecTask(string app, string arguments, TaskReporter taskReporter) : this(app, arguments)
        {
            this.taskReporter = taskReporter;
        }

        public virtual ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            ProcessWrapper process;
            try
            {
                process = new ProcessWrapper(_app, _arguments, sourceDrop.SourceDropLocation);
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
    }
}