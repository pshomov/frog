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
                                    _task = new ExecTask(_app, "");
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
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
                                    _task = new ExecTask(_app, "");
                                };

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.IsExecuted.ShouldBeTrue();
        It should_match_exit_code_value_from_program = () => _taskResult.ExitCode.ShouldEqual(4);
    }
}