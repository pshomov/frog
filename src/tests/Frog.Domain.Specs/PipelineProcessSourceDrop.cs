using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class PipelineProcessSourceDrop
    {
        private static Pipeline _pipeline;
        private static Task _task1;
        private static Task _task2;
        private static readonly SourceDrop SourceDrop = new SourceDrop("src_id", 1, "/asdfasdf/asfdasdf");

        Establish context = () =>
                                        {
                                            _task1 = MockRepository.GenerateMock<Task>();
                                            _task2 = MockRepository.GenerateMock<Task>();
                                            _pipeline = new PipelineOfTasks(_task1, _task2);
                                        };
        Because of = () => _pipeline.Process(SourceDrop);
        It should_flow_downstream = () => _task1.AssertWasCalled(task => task.Perform(SourceDrop));
    }
}
