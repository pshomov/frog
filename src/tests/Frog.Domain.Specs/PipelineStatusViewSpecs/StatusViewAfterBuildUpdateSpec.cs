using System.Linq;
using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void Given()
        {
            base.Given();
            RepoUrl = "http://";
            HandleBuildStarted(DefaultTask);
        }

        protected override void When()
        {
            View.Handle(new BuildUpdated(BuildMessage.BuildId, 0, TaskInfo.TaskStatus.FinishedError));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Overall,
                        Is.EqualTo(BuildTotalStatus.BuildStarted));
        }

        [Test]
        public void should_set_task_status_as_as_in_message()
        {
            Assert.That(ProjectView.GetBuildStatus(BuildMessage.BuildId).Tasks[0].Status,
                        Is.EqualTo(TaskInfo.TaskStatus.FinishedError));
        }
    }
}