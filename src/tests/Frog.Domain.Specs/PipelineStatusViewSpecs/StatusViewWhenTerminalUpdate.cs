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
            View.Handle(new BuildStarted(BuildMessage.BuildId,
                                         new PipelineStatus()
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo())}, "http://somerepo"));
        }

        protected override void When()
        {
            View.Handle(new TerminalUpdate("content1", 0, 0, BuildMessage.BuildId));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks[0].GetTerminalOutput().Content, Is.EqualTo("content1"));
        }

        [Test]
        public void should_return_empty_output_for_task_1()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks[1].GetTerminalOutput().Content, Is.EqualTo(""));
        }
    }

    [TestFixture]
    public class StatusViewAfterTheFirstBuildIsCompleteAndNewOneStarts : StatusViewAfterBuildStarterSpecBase
    {
        protected override void Given()
        {
            base.Given();
            View.Handle(new BuildStarted(BuildMessage.BuildId,
                                         new PipelineStatus()
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo())}, "http://fle"));
            View.Handle(new TerminalUpdate("content1", 0, 0, BuildMessage.BuildId));
            View.Handle(new BuildEnded(BuildMessage.BuildId, BuildTotalEndStatus.Success));
        }

        protected override void When()
        {
            View.Handle(new BuildStarted(BuildMessage.BuildId,
                                         new PipelineStatus()
                                             {Tasks = As.List(new TaskInfo(), new TaskInfo(), new TaskInfo())}, "http://fle"));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks.Count(), Is.EqualTo(3));
        }
    }
}