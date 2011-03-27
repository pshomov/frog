using System;
using System.Collections.Generic;
using Frog.Specs.Support;
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
            View.Handle(new BuildStarted("http://fle", new PipelineStatus(Guid.NewGuid()) { Tasks = As.List(new TasksInfo(), new TasksInfo()) }));
        }

        protected override void When()
        {
            View.Handle(new TerminalUpdate("content1", 0, 0, "http://fle"));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(BuildStatuses["http://fle"].CombinedTerminalOutput[0].Combined, Is.EqualTo("content1"));
        }
    }

    [TestFixture]
    public class StatusViewAfterTheFirstBuildIsCompleteAndNewOneStarts : StatusViewAfterBuildStarterSpecBase
    {
        protected override void Given()
        {
            base.Given();
            View.Handle(new BuildStarted("http://fle", new PipelineStatus(Guid.NewGuid()) { Tasks = As.List(new TasksInfo(), new TasksInfo()) }));
            View.Handle(new TerminalUpdate("content1", 0, 0, "http://fle"));
            View.Handle(new BuildEnded("http://fle", BuildTotalStatus.Success));
        }

        protected override void When()
        {
            View.Handle(new BuildStarted("http://fle", new PipelineStatus(Guid.NewGuid()) { Tasks = As.List(new TasksInfo(), new TasksInfo()) }));
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(BuildStatuses["http://fle"].CombinedTerminalOutput.Count, Is.EqualTo(0));
        }
    }
}