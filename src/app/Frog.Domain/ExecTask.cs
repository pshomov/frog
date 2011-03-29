using System;
using System.ComponentModel;
using Frog.Support;

namespace Frog.Domain
{
    public class ExecTaskResult
    {
        public enum Status
        {
            Success,
            Error
        } ;

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

        public Status ExecStatus
        {
            get { return HasExecuted && ExitCode == 0 ? Status.Success : Status.Error; }
        }
    }

    public class ExecTask
    {
        public enum ExecutionStatus
        {
            Success,
            Failure
        }

        readonly string app;
        readonly string arguments;
        readonly string name;
        public event Action<int> OnTaskStarted = pid => {};
        public virtual event Action<string> OnTerminalOutputUpdate = s => {};

        public ExecTask(string app, string arguments, string name)
        {
            this.app = app;
            this.arguments = arguments;
            this.name = name;
        }

        public virtual ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            ProcessWrapper process;
            try
            {
                process = new ProcessWrapper(app, arguments, sourceDrop.SourceDropLocation);
                process.OnErrorOutput += s => { if (s != null) OnTerminalOutputUpdate("E>" + s + Environment.NewLine); };
                process.OnStdOutput += s => { if (s != null) OnTerminalOutputUpdate("S>" + s + Environment.NewLine); };
                process.Execute();
                OnTaskStarted(process.ProcessInfo.Id);
                var exitcode = process.WaitForProcess(60000);
                MonoBugFix(exitcode);
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

        void MonoBugFix(int exitcode)
        {
            if (exitcode == 1) throw new Win32Exception();
        }

        public string Name
        {
            get { return name; }
        }
    }
}