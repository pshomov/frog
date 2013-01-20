using System;
using Frog.Domain.RepositoryTracker;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    public class AgentChecksForUpdates : AgentSpecsBase
    {
        private Build buildMessage;

        protected override void When()
        {
            buildMessage = new Build { RepoUrl = "http://fle", Revision = new RevisionInfo { Revision = "2" } };
            Agent.Handle(buildMessage);
        }

        [Test]
        public void should_check_for_updates()
        {
            Worker.Received().ExecutePipelineForRevision(Arg.Any<SourceRepoDriver>(), "2");
        }

        [Test]
        public void should_publish_project_checkedout()
        {
            Bus.Received().Publish(
                Arg.Is<ProjectCheckedOut>(
                    found => found.BuildId == buildMessage.Id && found.CheckoutInfo.Comment == "committed"));
        }

        [Test]
        public void should_publish_BuildStarted_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status != null && found.BuildId == buildMessage.Id));
        }

        [Test]
        public void should_have_one_task_with_a_non_null_terminal_id()
        {
            Bus.Received().Publish(
                Arg.Is<BuildStarted>(found => found.Status.Tasks.Count == 1 && found.Status.Tasks[0].TerminalId != Guid.Empty));
        }

        [Test]
        public void should_publish_BuildUpdated_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildUpdated>(found => found.TaskStatus == TaskInfo.TaskStatus.Started && found.TaskIndex == 0 && found.BuildId == buildMessage.Id && found.TerminalId == TerminalId));
        }

        [Test]
        public void should_publish_BuildEnded_event()
        {
            Bus.Received().Publish(
                Arg.Is<BuildEnded>(
                    found => found.TotalStatus == BuildTotalEndStatus.Success && found.BuildId == buildMessage.Id));
        }
    }
}