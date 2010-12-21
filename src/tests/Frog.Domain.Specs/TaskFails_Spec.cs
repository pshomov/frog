using System;
using System.ComponentModel;
using System.Diagnostics;
using Machine.Specifications;

namespace Frog.Domain.Specs
{
    public class TaskFails_Spec
    {
        static ExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () =>
                                {
                                    string _app = "adasdasd";
                                    if (Underware.IsWindows) _app = "callme_wrong.bat";
                                    if (Underware.IsUnix) _app = "./callme_wrong.sh";
                                    _task = new ExecTask(_app);
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop("", 1, ""));
        It should_report_task_execution_status = () => _taskResult.IsExecuted.ShouldBeFalse();

        It should_throw_an_exception_when_trying_to_access_exitcode_value =
            () => Catch.Exception(() => { int a = _taskResult.ExitCode; }).ShouldNotBeNull();
    }

    public class TaskSucceeds_Spec
    {
        static ExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () =>
                                {
                                    string _app = "adasdasd";
                                    if (Underware.IsWindows) _app = "callme.bat";
                                    if (Underware.IsUnix) _app = "./callme.sh";
                                    _task = new ExecTask(_app);
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop("", 1, ""));
        It should_report_task_execution_status = () => _taskResult.IsExecuted.ShouldBeTrue();
        It should_throw_an_exception_when_trying_to_access_exitcode_value = () => _taskResult.ExitCode.ShouldEqual(4);
    }



    public class ExecTaskResult : TaskResult
    {
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
    }

    public class ExecTask
    {
        public enum ExecutionStatus
        {
            Success,
            Failure
        }

        readonly string _app;

        public ExecTask(string app)
        {
            _app = app;
        }

        public ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            Process process;
            try
            {
                process = Process.Start(_app);
                process.WaitForExit(3000);
            }
            catch (Win32Exception e)
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