using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewTaskUpdateSetsTheTerminalId : StatusViewCurrentBuildPublicRepoBase
    {
        private Guid terminal_id_task1;
        private Guid terminal_id_task2;

        protected override void Given()
        {
            base.Given();
            RepoUrl = "http://";
            HandleProjectCheckedOut("");
            terminal_id_task1 = Guid.NewGuid();
            terminal_id_task2 = Guid.NewGuid();
        }

        protected override void When()
        {
            HandleBuildStarted(new TaskInfo("t1", terminal_id_task1), new TaskInfo("t2", terminal_id_task2));
        }

        [Test]
        public void should_have_terminalId_as_specified_in_the_message()
        {
            Assert.That(BuildView.GetBuildStatus(BuildMessage.BuildId).Tasks[0].TerminalId, Is.EqualTo(terminal_id_task1));
            Assert.That(BuildView.GetBuildStatus(BuildMessage.BuildId).Tasks[1].TerminalId, Is.EqualTo(terminal_id_task2));
        }
    }
}