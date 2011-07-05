using Frog.Domain.ExecTasks;
using Frog.Support;
using Machine.Specifications;

namespace Frog.Domain.Specs.Task
{
    public class TaskFailsToStart_Spec
    {
        static IExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () => _task = new ExecTask("ad43wsWasdasd", "", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeFalse();

        It should_throw_an_exception_when_trying_to_access_exitcode_value =
            () => Catch.Exception(() => { var a = _taskResult.ExitCode; }).ShouldNotBeNull();

        It should_have_status_set_to_falure = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Error);
    }

    public class TaskStartsButExitsWithNonZero_Spec
    {
        static IExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () => _task = new ExecTask("ruby", @"-e 'exit 4'", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeTrue();
        It should_match_exit_code_value_from_program = () => _taskResult.ExitCode.ShouldEqual(4);
        It should_have_status_set_to_success = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Error);
    }

    public class TaskStartsAndFinishesWithExitCodeZero_Spec
    {
        static IExecTask _task;
        static ExecTaskResult _taskResult;

        Establish context = () => _task = new ExecTask("ruby", @"-e 'exit 0'", "task_name", (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));

        Because of = () => _taskResult = _task.Perform(new SourceDrop(""));
        It should_report_task_execution_status = () => _taskResult.HasExecuted.ShouldBeTrue();
        It should_match_exit_code_value_from_program = () => _taskResult.ExitCode.ShouldEqual(0);
        It should_have_status_set_to_success = () => _taskResult.ExecStatus.ShouldEqual(ExecTaskResult.Status.Success);
    }
}