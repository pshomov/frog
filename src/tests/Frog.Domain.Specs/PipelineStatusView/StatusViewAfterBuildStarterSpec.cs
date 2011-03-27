using System;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusView
{
    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.NotStarted}
                                         }
                                 };
            view.Handle(new BuildStarted("http://repo", new PipelineStatus(pipelineStatus)));
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(buildStatuses["http://repo"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.PipelineStarted));
        }

        [Test]
        public void should_set_status_as_as_in_message()
        {
            Assert.True(new CompareObjects().Compare(buildStatuses["http://repo"].PipelineStatus, pipelineStatus));
        }
    }
}