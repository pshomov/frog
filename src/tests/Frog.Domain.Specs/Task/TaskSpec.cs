using Frog.Support;
using Machine.Specifications;

namespace Frog.Domain.Specs.Task
{
    public class TaskFailsToStart_Spec
    {
        static ExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () =>
                                {
                                    string _app = "adasdasd";
                                    if (Underware.IsWindows) _app = "callme_wrong.bat";
                                    if (Underware.IsUnix) _app = "./callme_wrong.sh";
                                    _task = new ExecTask(_app, "");
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeFalse();

        It should_throw_an_exception_when_trying_to_access_exitcode_value =
            () => Catch.Exception(() => { int a = _taskResult.ExitCode; }).ShouldNotBeNull();

        It should_have_status_set_to_falure = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Error);
    }

    public class TaskStartsButExitsWithNonZero_Spec
    {
        static ExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () =>
                                {
                                    if (Underware.IsWindows) _task = new ExecTask("cmd.exe", @"/c exit /b 4");
                                    if (Underware.IsUnix) _task = new ExecTask("./callme.sh", "");
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeTrue();
        It should_match_exit_code_value_from_program = () => _taskResult.ExitCode.ShouldEqual(4);
        It should_have_status_set_to_success = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Error);
    }

    public class TaskStartsAndFinishesWithExitCodeZero_Spec
    {
        static ExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () =>
                                {
                                    if (Underware.IsWindows) _task = new ExecTask("cmd.exe", @"/c echo buy buy");
                                    if (Underware.IsUnix) _task = new ExecTask("./callme.sh", "");
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeTrue();
        It should_match_exit_code_value_from_program = () => _taskResult.ExitCode.ShouldEqual(0);
        It should_have_status_set_to_success = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Success);
    }
}