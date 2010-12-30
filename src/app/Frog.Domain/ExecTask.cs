using System;
using System.ComponentModel;
using System.Diagnostics;

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

    public class ExecTask
    {
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

        public virtual ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            Process process;
            try
            {
                var psi = new ProcessStartInfo(_app, _arguments);
                psi.WorkingDirectory = sourceDrop.SourceDropLocation;
//                psi.UseShellExecute = false;
//                psi.RedirectStandardOutput = true;
                process = Process.Start(psi);
//                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit(30000);
                if (process.HasExited && process.ExitCode == 1) throw new Win32Exception();
            }
            catch (Win32Exception)
            {
                return new ExecTaskResult(ExecutionStatus.Failure, -1);
            }

            if (process.HasExited)
            {
                return new ExecTaskResult(ExecutionStatus.Success, process.ExitCode);
            }
            return new ExecTaskResult(ExecutionStatus.Failure, -1);
        }
    }
}