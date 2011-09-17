using System;
using System.Linq;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewWhenTerminalUpdate : StatusViewAfterBuildStarterSpecBase
    {
        protected override void Given()
        {
            base.Given();
            View.Handle(new BuildStarted(BuildMessage.Id,
                                         new PipelineStatus(Guid.NewGuid())
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo())}));
        }

        protected override void When()
        {
            View.Handle(new TerminalUpdate("content1", 0, 0, BuildMessage.Id));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(BuildStatuses[BuildMessage.Id].Tasks[0].GetTerminalOutput().Content, Is.EqualTo("content1"));
        }

        [Test]
        public void should_return_empty_output_for_task_1()
        {
            Assert.That(BuildStatuses[BuildMessage.Id].Tasks[1].GetTerminalOutput().Content, Is.EqualTo(""));
        }
    }

    [TestFixture]
    public class StatusViewAfterTheFirstBuildIsCompleteAndNewOneStarts : StatusViewAfterBuildStarterSpecBase
    {
        protected override void Given()
        {
            base.Given();
            View.Handle(new BuildStarted(BuildMessage.Id,
                                         new PipelineStatus(Guid.NewGuid())
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo())}));
            View.Handle(new TerminalUpdate("content1", 0, 0, BuildMessage.Id));
            View.Handle(new BuildEnded(BuildMessage.Id, BuildTotalEndStatus.Success));
        }

        protected override void When()
        {
            View.Handle(new BuildStarted(BuildMessage.Id,
                                         new PipelineStatus(Guid.NewGuid())
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo(), new TaskInfo())}));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(BuildStatuses[BuildMessage.Id].Tasks.Count(), Is.EqualTo(3));
        }
    }
}