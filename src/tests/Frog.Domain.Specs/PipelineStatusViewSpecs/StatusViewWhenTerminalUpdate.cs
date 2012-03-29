using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewWhenTerminalUpdate : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void Given()
        {
            base.Given();
            RepoUrl = "http://";
            HandleProjectCheckedOut("");
            HandleBuildStarted(new TaskInfo(), new TaskInfo());
        }

        protected override void When()
        {
            EventHandler.Handle(new TerminalUpdate("content1", 0, 0, BuildMessage.BuildId, 0, Guid.NewGuid()));
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
}