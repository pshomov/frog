using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class Pipeline_Specs
    {
        private static Pipeline _pipeline;
        private static Task _task1;
        private Task _task2;
        private static readonly SourceDrop SourceDrop = new SourceDrop("src_id", 1, "/asdfasdf/asfdasdf");
        private Establish context = () => _task1 = MockRepository.GenerateMock<Task>();
        Because of = () => _pipeline.Process(SourceDrop);
        private It should_flow_downstream = () => _task1.AssertWasCalled(task => task.Perform(SourceDrop));
    }

    internal class Task
    {
        public void Perform(SourceDrop sourceDrop)
        {
            throw new NotImplementedException();
        }
    }
}
