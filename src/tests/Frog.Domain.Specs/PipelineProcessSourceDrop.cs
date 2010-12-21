using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Frog.Domain.Specs
{
    public class PipelineProcessSourceDrop : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                        {
                                            _task1 = MockRepository.GenerateMock<Task>();
                                            _pipeline = new PipelineOfTasks(_task1);
                                        };
        Because of = () => _pipeline.Process(SourceDrop);
        It should_start_first_task = () => _task1.AssertWasCalled(task => task.Perform(SourceDrop));
    }

    public class PipelineProcessSourceDropAndFirstTaskSucceeds : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                        {
                                            _task1 = MockRepository.GenerateStub<Task>();
                                            _task1.Expect(task => task.Perform(null)).IgnoreArguments().Return(TaskResult.Success);
                                            _task2 = MockRepository.GenerateMock<Task>();
                                            _pipeline = new PipelineOfTasks(_task1, _task2);
                                        };
        Because of = () => _pipeline.Process(SourceDrop);
        It should_perform_later_tasks = () => _task2.AssertWasCalled(task => task.Perform(SourceDrop));
    }

    public class PipelineProcessSourceDrop_TaskFailureStopsThePipeline : PipelineProcessSourceDropBase
    {
        Establish context = () =>
                                        {
                                            _task1 = MockRepository.GenerateStub<Task>();
                                            _task1.Expect(task => task.Perform(null)).IgnoreArguments().Return(TaskResult.Error);
                                            _task2 = MockRepository.GenerateMock<Task>();
                                            _pipeline = new PipelineOfTasks(_task1, _task2);
                                        };
        Because of = () => _pipeline.Process(SourceDrop);
        It should_not_perform_later_tasks = () => _task2.AssertWasNotCalled(task => task.Perform(SourceDrop));
    }

    public class PipelineProcessSourceDropBase
    {
        protected static Pipeline _pipeline;
        protected static Task _task1;
        protected static Task _task2;
        protected static readonly SourceDrop SourceDrop = new SourceDrop("src_id", 1, "/asdfasdf/asfdasdf");
    }
}
