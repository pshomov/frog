using System;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewTaskUpdateSetsTheTerminalId : StatusViewCurrentBuildPublicRepoBase
    {
        private Guid terminalId;
        private Guid terminalId_task1;
        private Guid terminalId_task2;

        protected override void Given()
        {
            base.Given();
            RepoUrl = "http://";
            HandleProjectCheckedOut("");
            terminalId_task1 = Guid.NewGuid();
            terminalId_task2 = Guid.NewGuid();
            HandleBuildStarted(new TaskInfo("t1", terminalId_task1), new TaskInfo("t2", terminalId_task2));
        }

        protected override void When()
        {
            terminalId = Guid.NewGuid();
            HandleBuildUpdated(0, TaskInfo.TaskStatus.Started, terminalId);
        }

        [Test]
        public void should_have_terminalId_as_specified_in_the_message()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks[0].TerminalId, Is.EqualTo(terminalId_task1));
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks[1].TerminalId, Is.EqualTo(terminalId_task2));
        }
    }
}