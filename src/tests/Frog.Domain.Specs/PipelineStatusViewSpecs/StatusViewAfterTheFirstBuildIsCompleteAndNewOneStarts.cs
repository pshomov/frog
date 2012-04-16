using System.Linq;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterTheFirstBuildIsCompleteAndNewOneStarts : StatusViewCurrentBuildPublicRepoBase
    {
        protected override void Given()
        {
            base.Given();
            RepoUrl = "http://asdasda";
            HandleABuild(BuildTotalEndStatus.Success);
        }

        protected override void When()
        {
            HandleBuildStarted(new TaskInfo(), new TaskInfo(), new TaskInfo());
        }

        [Test]
        public void should_update_terminal_output_for_task_0()
        {
            Assert.That(BuildView.GetBuildStatus(BuildMessage.BuildId).Tasks.Count(), Is.EqualTo(3));
        }
    }
}