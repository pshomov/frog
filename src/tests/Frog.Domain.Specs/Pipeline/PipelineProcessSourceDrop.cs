using Frog.Specs.Support;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs.Pipeline
{
    public class Pipeline_processes_sourceDrop : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                {
                                    _task1 = MockRepository.GenerateMock<ExecTask>("", "");
                                    _task1.Expect(task => task.Perform(null)).IgnoreArguments().Return(
                                        new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
                                    _pipeline = new PipelineOfTasks(new FixedTasksDispenser(_task1));
                                };

        Because of = () => _pipeline.Process(SourceDrop);
        It should_start_first_task = () => _task1.AssertWasCalled(task => task.Perform(SourceDrop));
    }

    public class Pipeline_process_sourceDrop_after_first_task_succeeds : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                {
                                    _task1 = MockRepository.GenerateStub<ExecTask>("", "");
                                    _task1.Expect(task => task.Perform(null)).IgnoreArguments().Return(
                                        new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
                                    _task2 = MockRepository.GenerateMock<ExecTask>("", "");
                                    _task2.Expect(task => task.Perform(null)).IgnoreArguments().Return(
                                        new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));
                                    _pipeline = new PipelineOfTasks(new FixedTasksDispenser(_task1, _task2));
                                };

        Because of = () => _pipeline.Process(SourceDrop);
        It should_perform_later_tasks = () => _task2.AssertWasCalled(task => task.Perform(SourceDrop));
    }

    public class Pipeline_process_sourceDrop_task_failure : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                {
                                    _task1 = MockRepository.GenerateStub<ExecTask>("", "");
                                    _task1.Expect(task => task.Perform(null)).IgnoreArguments().Return(
                                        new ExecTaskResult(ExecTask.ExecutionStatus.Failure, 2));
                                    _task2 = MockRepository.GenerateMock<ExecTask>("", "");
                                    _pipeline = new PipelineOfTasks(new FixedTasksDispenser(_task1, _task2));
                                };

        Because of = () => _pipeline.Process(SourceDrop);
        It should_not_perform_later_tasks = () => _task2.AssertWasNotCalled(task => task.Perform(SourceDrop));
    }

    public class PipelineProcessSourceDropBase
    {
        protected static Domain.Pipeline _pipeline;
        protected static ExecTask _task1;
        protected static ExecTask _task2;
        protected static readonly SourceDrop SourceDrop = new SourceDrop("");
    }
}